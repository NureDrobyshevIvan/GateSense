using System;
using System.Linq;
using Domain.Models.DTOS.Gates;
using Domain.Models.Garages;
using Domain.Models.Gates;
using GateSense.Application.Gates.Interfaces;
using Infrastructure.Common.Errors;
using Infrastructure.Common.ResultPattern;
using Infrastructure.Repository.Interfaces;

namespace GateSense.Application.Gates.Services;

public class GateService : IGateService
{
    private readonly IGenericRepository<Garage> _garageRepository;
    private readonly IGenericRepository<AccessKey> _accessKeyRepository;
    private readonly IGenericRepository<GateEvent> _gateEventRepository;
    private readonly IGenericRepository<GarageAccess> _garageAccessRepository;

    private static readonly Error GarageNotFound =
        Error.NotFound("garage.NOT_FOUND", "Garage not found");

    private static readonly Error ForbiddenGarageAccess =
        Error.Forbidden("garage.FORBIDDEN", "You do not have access to this garage");

    private static readonly Error AccessKeyInvalid =
        Error.Unauthorized("gate.ACCESS_KEY_INVALID", "Invalid or expired access key");

    public GateService(
        IGenericRepository<Garage> garageRepository,
        IGenericRepository<AccessKey> accessKeyRepository,
        IGenericRepository<GateEvent> gateEventRepository,
        IGenericRepository<GarageAccess> garageAccessRepository)
    {
        _garageRepository = garageRepository;
        _accessKeyRepository = accessKeyRepository;
        _gateEventRepository = gateEventRepository;
        _garageAccessRepository = garageAccessRepository;
    }

    public async Task<Result> OpenGateAsync(int garageId, GateCommandRequest request, int userId)
    {
        return await ExecuteGateCommand(garageId, request, userId, GateAction.Open);
    }

    public async Task<Result> CloseGateAsync(int garageId, GateCommandRequest request, int userId)
    {
        return await ExecuteGateCommand(garageId, request, userId, GateAction.Close);
    }

    public async Task<Result<GateStateResponse>> GetGateStateAsync(int garageId, int userId)
    {
        var guard = await EnsureGarageAccess(garageId, userId);
        if (!guard.IsSuccess)
        {
            return Result<GateStateResponse>.Failure(guard.Errors);
        }

        var eventsResult = await _gateEventRepository.GetListByConditionAsync(e => e.GarageId == garageId);

        if (!eventsResult.IsSuccess)
        {
            return Result<GateStateResponse>.Failure(eventsResult.Errors);
        }

        var lastEvent = eventsResult.Value
            .OrderByDescending(e => e.CreatedOn)
            .FirstOrDefault();

        string state;
        if (lastEvent == null)
        {
            state = "Unknown";
        }
        else if (lastEvent.Result == GateActionResult.Success)
        {
            state = lastEvent.Action == GateAction.Open ? "Open" : "Closed";
        }
        else
        {
            // If last action failed, state is unknown
            state = "Unknown";
        }

        var response = new GateStateResponse
        {
            GarageId = garageId,
            State = state,
            LastAction = lastEvent?.Action.ToString(),
            LastActionTime = lastEvent?.CreatedOn
        };

        return Result<GateStateResponse>.Success(response);
    }

    private async Task<Result> ExecuteGateCommand(int garageId, GateCommandRequest request, int userId, GateAction action)
    {
        var guard = await EnsureGarageAccess(garageId, userId);
        if (!guard.IsSuccess)
        {
            return guard;
        }

        // Determine trigger source based on user's access
        var accessCheck = await CheckUserAccess(garageId, userId);
        GateTriggerSource trigger = accessCheck.IsOwner 
            ? GateTriggerSource.Owner 
            : accessCheck.IsFamily 
                ? GateTriggerSource.Family 
                : GateTriggerSource.Guest;

        AccessKey? accessKey = null;

        if (!string.IsNullOrEmpty(request.AccessKeyToken))
        {
            var keyResult = await _accessKeyRepository.GetSingleByConditionAsync(k => k.Token == request.AccessKeyToken);
            if (!keyResult.IsSuccess || keyResult.Value.Status != AccessKeyStatus.Active || (keyResult.Value.ExpiresOn.HasValue && keyResult.Value.ExpiresOn < DateTimeOffset.UtcNow))
            {
                return Result.Failure(AccessKeyInvalid);
            }

            if (keyResult.Value.GarageId != garageId)
            {
                return Result.Failure(ForbiddenGarageAccess);
            }

            accessKey = keyResult.Value;
            trigger = keyResult.Value.KeyType == AccessKeyType.Family ? GateTriggerSource.Family : GateTriggerSource.Guest;
        }

        var gateEvent = new GateEvent
        {
            GarageId = garageId,
            InitiatorUserId = userId,
            AccessKeyId = accessKey?.Id,
            TriggerSource = trigger,
            Action = action,
            Result = GateActionResult.Success,
            FailureReason = null
        };

        // TODO: integrate with actual IoT command dispatch; for now record the event as success
        var addResult = await _gateEventRepository.AddAsync(gateEvent);
        return addResult.IsSuccess ? Result.Success() : Result.Failure(addResult.Errors);
    }

    private async Task<Result> EnsureGarageAccess(int garageId, int userId)
    {
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId);
        if (!garageResult.IsSuccess)
        {
            return Result.Failure(GarageNotFound);
        }

        // Check if user is owner
        if (garageResult.Value.OwnerId == userId)
        {
            return Result.Success();
        }

        // Check if user has access through GarageAccess
        var accessResult = await _garageAccessRepository.GetSingleByConditionAsync(
            a => a.GarageId == garageId && a.UserId == userId);
        
        if (!accessResult.IsSuccess)
        {
            return Result.Failure(ForbiddenGarageAccess);
        }

        var access = accessResult.Value;
        
        // Check if access has expired
        if (access.ExpiresOn.HasValue && access.ExpiresOn.Value < DateTimeOffset.UtcNow)
        {
            return Result.Failure(ForbiddenGarageAccess);
        }

        return Result.Success();
    }

    private async Task<(bool IsOwner, bool IsFamily, bool IsGuest)> CheckUserAccess(int garageId, int userId)
    {
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId);
        if (garageResult.IsSuccess && garageResult.Value.OwnerId == userId)
        {
            return (true, false, false);
        }

        var accessResult = await _garageAccessRepository.GetSingleByConditionAsync(
            a => a.GarageId == garageId && a.UserId == userId);
        
        if (accessResult.IsSuccess)
        {
            var access = accessResult.Value;
            if (!access.ExpiresOn.HasValue || access.ExpiresOn.Value >= DateTimeOffset.UtcNow)
            {
                bool isFamily = access.AccessLevel == AccessLevel.Family;
                return (false, isFamily, !isFamily);
            }
        }

        return (false, false, false);
    }
}


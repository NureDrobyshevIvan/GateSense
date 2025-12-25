using Domain.Models.Auth;
using Domain.Models.DTOS.Access;
using Domain.Models.Garages;
using GateSense.Application.Access.Interfaces;
using Infrastructure.Common.Errors;
using Infrastructure.Common.ResultPattern;
using Infrastructure.Data.UnitOfWork;
using Infrastructure.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GateSense.Application.Access.Services;

public class AccessService : IAccessService
{
    private readonly IGenericRepository<Garage> _garageRepository;
    private readonly IGenericRepository<GarageAccess> _garageAccessRepository;
    private readonly IGenericRepository<AccessKey> _accessKeyRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly Error GarageNotFound =
        Error.NotFound("garage.NOT_FOUND", "Garage not found");

    private static readonly Error ForbiddenGarageAccess =
        Error.Forbidden("garage.FORBIDDEN", "You do not have access to this garage");

    private static readonly Error UserNotFound =
        Error.NotFound("user.NOT_FOUND", "User not found");

    private static readonly Error AccessExists =
        Error.Conflict("access.ALREADY_EXISTS", "Access for this user already exists");

    private static readonly Error AccessNotFound =
        Error.NotFound("access.NOT_FOUND", "Access not found");

    public AccessService(
        IGenericRepository<Garage> garageRepository,
        IGenericRepository<GarageAccess> garageAccessRepository,
        IGenericRepository<AccessKey> accessKeyRepository,
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork)
    {
        _garageRepository = garageRepository;
        _garageAccessRepository = garageAccessRepository;
        _accessKeyRepository = accessKeyRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<GarageAccess>>> GetMembersAsync(int garageId, int userId)
    {
        var guard = await EnsureGarageOwned(garageId, userId);
        if (!guard.IsSuccess)
        {
            return Result<IEnumerable<GarageAccess>>.Failure(guard.Errors);
        }

        var includes = new List<Func<IQueryable<GarageAccess>, IQueryable<GarageAccess>>>
        {
            q => q.Include(g => g.User)
        };

        return await _garageAccessRepository.GetListByConditionAsync(g => g.GarageId == garageId, includes);
    }

    public async Task<Result> AssignFamilyAccessAsync(AssignFamilyAccessRequest request, int userId)
    {
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == request.GarageId);
        if (!garageResult.IsSuccess)
        {
            return Result.Failure(GarageNotFound);
        }

        var garage = garageResult.Value;

        // If garage has no owner, assign the current user as owner
        if (garage.OwnerId == null)
        {
            garage.OwnerId = userId;
            var updateResult = await _garageRepository.UpdateAsync(garage);
            if (!updateResult.IsSuccess)
            {
                return updateResult;
            }
        }
        else
        {
            // If garage has owner, check if current user is the owner
            var guard = await EnsureGarageOwned(request.GarageId, userId);
            if (!guard.IsSuccess)
            {
                return guard;
            }
        }

        var familyUser = await _userManager.FindByEmailAsync(request.Email);
        if (familyUser == null)
        {
            return Result.Failure(UserNotFound);
        }

        // prevent duplicate access
        var existing = await _garageAccessRepository.GetSingleByConditionAsync(
            g => g.GarageId == request.GarageId && g.UserId == familyUser.Id);
        if (existing.IsSuccess)
        {
            return Result.Failure(AccessExists);
        }

        var garageAccess = new GarageAccess
        {
            GarageId = request.GarageId,
            UserId = familyUser.Id,
            AccessLevel = AccessLevel.Family,
            ExpiresOn = null
        };

        var accessKey = new AccessKey
        {
            GarageId = request.GarageId,
            IssuedByUserId = userId,
            KeyType = AccessKeyType.Family,
            Status = AccessKeyStatus.Active,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresOn = null
        };

        await _unitOfWork.BeginTransactionAsync();

        var addAccessResult = await _garageAccessRepository.AddAsync(garageAccess);
        if (!addAccessResult.IsSuccess)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return addAccessResult;
        }

        var addKeyResult = await _accessKeyRepository.AddAsync(accessKey);
        if (!addKeyResult.IsSuccess)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Failure(addKeyResult.Errors);
        }

        await _unitOfWork.CommitTransactionAsync();
        return Result.Success();
    }

    public async Task<Result<CreateGuestAccessResponse>> CreateGuestAccessAsync(CreateGuestAccessRequest request, int userId)
    {
        var guard = await EnsureGarageOwned(request.GarageId, userId);
        if (!guard.IsSuccess)
        {
            return Result<CreateGuestAccessResponse>.Failure(guard.Errors);
        }

        var token = Guid.NewGuid().ToString("N");
        var accessKey = new AccessKey
        {
            GarageId = request.GarageId,
            IssuedByUserId = userId,
            KeyType = AccessKeyType.Guest,
            Status = AccessKeyStatus.Active,
            Token = token,
            ExpiresOn = request.ExpiresOn
        };

        var addResult = await _accessKeyRepository.AddAsync(accessKey);
        if (!addResult.IsSuccess)
        {
            return Result<CreateGuestAccessResponse>.Failure(addResult.Errors);
        }

        var response = new CreateGuestAccessResponse
        {
            Id = addResult.Value,
            Token = token,
            ExpiresOn = request.ExpiresOn
        };

        return Result<CreateGuestAccessResponse>.Success(response);
    }

    public async Task<Result> RevokeAccessAsync(int accessId, int userId)
    {
        // Try revoke AccessKey first
        var keyResult = await _accessKeyRepository.GetSingleByConditionAsync(a => a.Id == accessId);
        if (keyResult.IsSuccess)
        {
            var guard = await EnsureGarageOwned(keyResult.Value.GarageId, userId);
            if (!guard.IsSuccess)
            {
                return guard;
            }

            keyResult.Value.Status = AccessKeyStatus.Revoked;
            return await _accessKeyRepository.UpdateAsync(keyResult.Value);
        }

        // Try delete GarageAccess
        var accessResult = await _garageAccessRepository.GetSingleByConditionAsync(a => a.Id == accessId);
        if (!accessResult.IsSuccess)
        {
            return Result.Failure(AccessNotFound);
        }

        var guardAccess = await EnsureGarageOwned(accessResult.Value.GarageId, userId);
        if (!guardAccess.IsSuccess)
        {
            return guardAccess;
        }

        return await _garageAccessRepository.DeleteAsync(a => a.Id == accessId);
    }

    private async Task<Result> EnsureGarageOwned(int garageId, int userId)
    {
        var garageResult = await _garageRepository.GetSingleByConditionAsync(g => g.Id == garageId);
        if (!garageResult.IsSuccess)
        {
            return Result.Failure(GarageNotFound);
        }

        if (garageResult.Value.OwnerId != userId)
        {
            return Result.Failure(ForbiddenGarageAccess);
        }

        return Result.Success();
    }
}


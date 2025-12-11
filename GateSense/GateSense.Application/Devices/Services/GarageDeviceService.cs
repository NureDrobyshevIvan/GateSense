using System;
using Domain.Models.Devices;
using Domain.Models.DTOS.Devices;
using GateSense.Application.Devices.Interfaces;
using Infrastructure.Common.Errors;
using Infrastructure.Common.ResultPattern;
using Infrastructure.Data.UnitOfWork;
using Infrastructure.Repository.Interfaces;

namespace GateSense.Application.Devices.Services;

public class GarageDeviceService : IGarageDeviceService
{
    private readonly IGenericRepository<IoTDevice> _deviceRepository;
    private readonly IGenericRepository<Domain.Models.Garages.Garage> _garageRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly Error GarageNotFound =
        Error.NotFound("garage.NOT_FOUND", "Garage not found");

    private static readonly Error ForbiddenGarageAccess =
        Error.Forbidden("garage.FORBIDDEN", "You do not have access to this garage");

    private static readonly Error DeviceNotFound =
        Error.NotFound("device.NOT_FOUND", "Device not found");

    private static readonly Error InvalidDeviceType =
        Error.Validation("device.INVALID_TYPE", "Unsupported device type");

    public GarageDeviceService(
        IGenericRepository<IoTDevice> deviceRepository,
        IGenericRepository<Domain.Models.Garages.Garage> garageRepository,
        IUnitOfWork unitOfWork)
    {
        _deviceRepository = deviceRepository;
        _garageRepository = garageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<IoTDevice>>> GetDevicesAsync(int garageId, int userId)
    {
        var garageCheck = await EnsureGarageOwned(garageId, userId);
        if (!garageCheck.IsSuccess)
        {
            return Result<IEnumerable<IoTDevice>>.Failure(garageCheck.Errors);
        }

        return await _deviceRepository.GetListByConditionAsync(d => d.GarageId == garageId);
    }

    public async Task<Result<int>> RegisterDeviceAsync(int garageId, RegisterDeviceRequest request, int userId)
    {
        var garageCheck = await EnsureGarageOwned(garageId, userId);
        if (!garageCheck.IsSuccess)
        {
            return Result<int>.Failure(garageCheck.Errors);
        }

        if (!Enum.TryParse<DeviceType>(request.DeviceType, true, out var deviceType))
        {
            return Result<int>.Failure(InvalidDeviceType);
        }

        var device = new IoTDevice
        {
            GarageId = garageId,
            SerialNumber = request.SerialNumber,
            FirmwareVersion = request.FirmwareVersion,
            DeviceType = deviceType,
            Status = DeviceStatus.Online
        };

        await _unitOfWork.BeginTransactionAsync();

        var addResult = await _deviceRepository.AddAsync(device);
        if (!addResult.IsSuccess)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return addResult;
        }

        await _unitOfWork.CommitTransactionAsync();
        return addResult;
    }

    public async Task<Result> RemoveDeviceAsync(int garageId, int deviceId, int userId)
    {
        var garageCheck = await EnsureGarageOwned(garageId, userId);
        if (!garageCheck.IsSuccess)
        {
            return Result.Failure(garageCheck.Errors);
        }

        var deviceResult = await _deviceRepository.GetSingleByConditionAsync(d => d.Id == deviceId);
        if (!deviceResult.IsSuccess)
        {
            return Result.Failure(DeviceNotFound);
        }

        if (deviceResult.Value.GarageId != garageId)
        {
            return Result.Failure(ForbiddenGarageAccess);
        }

        return await _deviceRepository.DeleteAsync(d => d.Id == deviceId && d.GarageId == garageId);
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


using Domain.Models.DTOS.Gates;
using Domain.Models.DTOS.Sensors;
using Domain.Models.Devices;
using Domain.Models.Gates;
using Domain.Models.Sensors;
using GateSense.Application.IoT.Interfaces;
using Infrastructure.Common.Errors;
using Infrastructure.Common.ResultPattern;
using Infrastructure.Repository.Interfaces;

namespace GateSense.Application.IoT.Services;

public class IoTService : IIoTService
{
    private readonly IGenericRepository<IoTDevice> _deviceRepository;
    private readonly IGenericRepository<SensorReading> _sensorReadingRepository;
    private readonly IGenericRepository<GateEvent> _gateEventRepository;

    private static readonly Error DeviceNotFound =
        Error.NotFound("iot.DEVICE_NOT_FOUND", "Device with the specified serial number was not found");

    private static readonly Error InvalidSensorType =
        Error.Validation("iot.INVALID_SENSOR_TYPE", "Invalid sensor type");

    public IoTService(
        IGenericRepository<IoTDevice> deviceRepository,
        IGenericRepository<SensorReading> sensorReadingRepository,
        IGenericRepository<GateEvent> gateEventRepository)
    {
        _deviceRepository = deviceRepository;
        _sensorReadingRepository = sensorReadingRepository;
        _gateEventRepository = gateEventRepository;
    }

    public async Task<Result> SubmitSensorDataAsync(SensorDataSubmissionRequest request)
    {
        // Find device by serial number
        var deviceResult = await _deviceRepository.GetSingleByConditionAsync(
            d => d.SerialNumber == request.SerialNumber);

        if (!deviceResult.IsSuccess)
        {
            return Result.Failure(DeviceNotFound);
        }

        var device = deviceResult.Value;

        // Validate and parse sensor type
        if (!Enum.TryParse<SensorType>(request.SensorType, ignoreCase: true, out var sensorType))
        {
            return Result.Failure(InvalidSensorType);
        }

        // Create sensor reading
        var sensorReading = new SensorReading
        {
            DeviceId = device.Id,
            SensorType = sensorType,
            Value = request.Value,
            Unit = request.Unit,
            RecordedOn = request.RecordedOn
        };

        // Update device heartbeat (device is active if sending data)
        device.LastHeartbeatOn = DateTimeOffset.UtcNow;
        device.Status = DeviceStatus.Online;

        // Save sensor reading and update device
        var addResult = await _sensorReadingRepository.AddAsync(sensorReading);
        if (!addResult.IsSuccess)
        {
            return addResult;
        }

        var updateResult = await _deviceRepository.UpdateAsync(device);
        if (!updateResult.IsSuccess)
        {
            return updateResult;
        }

        return Result.Success();
    }

    public async Task<Result> SendHeartbeatAsync(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return Result.Failure(Error.Validation("iot.INVALID_SERIAL_NUMBER", "Serial number cannot be empty"));
        }

        // Find device by serial number
        var deviceResult = await _deviceRepository.GetSingleByConditionAsync(
            d => d.SerialNumber == serialNumber);

        if (!deviceResult.IsSuccess)
        {
            return Result.Failure(DeviceNotFound);
        }

        var device = deviceResult.Value;

        // Update heartbeat timestamp and status
        device.LastHeartbeatOn = DateTimeOffset.UtcNow;
        device.Status = DeviceStatus.Online;

        var updateResult = await _deviceRepository.UpdateAsync(device);
        if (!updateResult.IsSuccess)
        {
            return updateResult;
        }

        return Result.Success();
    }

    public async Task<Result<GateStateResponse>> GetGateStateBySerialNumberAsync(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return Result<GateStateResponse>.Failure(
                Error.Validation("iot.INVALID_SERIAL_NUMBER", "Serial number cannot be empty"));
        }

        var deviceResult = await _deviceRepository.GetSingleByConditionAsync(
            d => d.SerialNumber == serialNumber);

        if (!deviceResult.IsSuccess)
        {
            return Result<GateStateResponse>.Failure(DeviceNotFound);
        }

        var device = deviceResult.Value;
        int garageId = device.GarageId;

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
}


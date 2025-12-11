using Domain.Models.Devices;
using Domain.Models.DTOS.Devices;
using Infrastructure.Common.ResultPattern;

namespace GateSense.Application.Devices.Interfaces;

public interface IGarageDeviceService
{
    Task<Result<IEnumerable<IoTDevice>>> GetDevicesAsync(int garageId, int userId);

    Task<Result<int>> RegisterDeviceAsync(int garageId, RegisterDeviceRequest request, int userId);

    Task<Result> RemoveDeviceAsync(int garageId, int deviceId, int userId);
}


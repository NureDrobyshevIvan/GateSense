using System.Security.Claims;
using Domain.Models.DTOS.Devices;
using GateSense.Application.Devices.Interfaces;
using GetSense.API.ApiResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize]
[ApiController]
[Route("garages/{garageId:int}/devices")]
public class GarageDevicesController : ControllerBase
{
    private readonly IGarageDeviceService _deviceService;

    public GarageDevicesController(IGarageDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDevices(int garageId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _deviceService.GetDevicesAsync(garageId, userId.Value);
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost]
    public async Task<IActionResult> RegisterDevice(int garageId, [FromBody] RegisterDeviceRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _deviceService.RegisterDeviceAsync(garageId, request, userId.Value);
        return result.Match(
            successStatusCode: StatusCodes.Status201Created,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpDelete("{deviceId:int}")]
    public async Task<IActionResult> RemoveDevice(int garageId, int deviceId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _deviceService.RemoveDeviceAsync(garageId, deviceId, userId.Value);
        return result.MatchNoData(
            successStatusCode: StatusCodes.Status204NoContent,
            failure: ApiResults.ToProblemDetails
        );
    }

    private int? GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var userId) ? userId : null;
    }
}


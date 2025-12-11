using System.Security.Claims;
using Domain.Models.DTOS.Sensors;
using GateSense.Application.Sensors.Interfaces;
using GetSense.API.ApiResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize]
[ApiController]
[Route("garages/{garageId:int}/sensors")]
public class SensorsController : ControllerBase
{
    private readonly ISensorService _sensorService;

    public SensorsController(ISensorService sensorService)
    {
        _sensorService = sensorService;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestReadings(int garageId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _sensorService.GetLatestReadingsAsync(garageId, userId.Value);
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetSensorHistory(int garageId, [FromQuery] SensorHistoryQuery query)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _sensorService.GetSensorHistoryAsync(garageId, userId.Value, query);
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetActiveAlerts(int garageId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _sensorService.GetActiveAlertsAsync(garageId, userId.Value);
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    private int? GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var userId) ? userId : null;
    }
}


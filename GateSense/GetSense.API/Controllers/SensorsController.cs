using Domain.Models.DTOS.Sensors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize]
[ApiController]
[Route("garages/{garageId:int}/sensors")]
public class SensorsController : ControllerBase
{
    [HttpGet("latest")]
    public IActionResult GetLatestReadings(int garageId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpGet("history")]
    public IActionResult GetSensorHistory(int garageId, [FromQuery] SensorHistoryQuery query)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpGet("alerts")]
    public IActionResult GetActiveAlerts(int garageId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}


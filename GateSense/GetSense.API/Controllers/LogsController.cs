using Domain.Models.DTOS.Gates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize]
[ApiController]
[Route("garages/{garageId:int}/logs")]
public class LogsController : ControllerBase
{
    [HttpGet("gate")]
    public IActionResult GetGateLogs(int garageId, [FromQuery] GateLogQuery query)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}


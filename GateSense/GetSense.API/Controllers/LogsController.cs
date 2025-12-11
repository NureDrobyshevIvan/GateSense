using System.Security.Claims;
using Domain.Models.DTOS.Gates;
using GateSense.Application.Logs.Interfaces;
using GetSense.API.ApiResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize]
[ApiController]
[Route("garages/{garageId:int}/logs")]
public class LogsController : ControllerBase
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    [HttpGet("gate")]
    public async Task<IActionResult> GetGateLogs(int garageId, [FromQuery] GateLogQuery query)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _logService.GetGateLogsAsync(garageId, userId.Value, query);
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


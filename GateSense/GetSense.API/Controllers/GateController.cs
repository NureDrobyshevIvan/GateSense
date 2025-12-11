using System.Security.Claims;
using Domain.Models.DTOS.Gates;
using GateSense.Application.Gates.Interfaces;
using GetSense.API.ApiResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize]
[ApiController]
[Route("garages/{garageId:int}/gate")]
public class GateController : ControllerBase
{
    private readonly IGateService _gateService;

    public GateController(IGateService gateService)
    {
        _gateService = gateService;
    }

    [HttpPost("open")]
    public async Task<IActionResult> OpenGate(int garageId, [FromBody] GateCommandRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _gateService.OpenGateAsync(garageId, request, userId.Value);
        return result.MatchNoData(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost("close")]
    public async Task<IActionResult> CloseGate(int garageId, [FromBody] GateCommandRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _gateService.CloseGateAsync(garageId, request, userId.Value);
        return result.MatchNoData(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("state")]
    public async Task<IActionResult> GetGateState(int garageId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _gateService.GetGateStateAsync(garageId, userId.Value);
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


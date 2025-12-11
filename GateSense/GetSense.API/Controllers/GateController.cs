using Domain.Models.DTOS.Gates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize]
[ApiController]
[Route("garages/{garageId:int}/gate")]
public class GateController : ControllerBase
{
    [HttpPost("open")]
    public IActionResult OpenGate(int garageId, [FromBody] GateCommandRequest request)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("close")]
    public IActionResult CloseGate(int garageId, [FromBody] GateCommandRequest request)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpGet("state")]
    public IActionResult GetGateState(int garageId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}


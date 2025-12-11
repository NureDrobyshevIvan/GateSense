using Domain.Models.DTOS.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize]
[ApiController]
[Route("access")]
public class AccessController : ControllerBase
{
    [HttpGet("garages/{garageId:int}/members")]
    public IActionResult GetGarageMembers(int garageId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("family")]
    public IActionResult AssignFamilyAccess([FromBody] AssignFamilyAccessRequest request)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("guest")]
    public IActionResult CreateGuestAccess([FromBody] CreateGuestAccessRequest request)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpDelete("{accessId:int}")]
    public IActionResult RevokeAccess(int accessId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}


using System.Security.Claims;
using Domain.Models.DTOS.Access;
using GateSense.Application.Access.Interfaces;
using GetSense.API.ApiResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize]
[ApiController]
[Route("access")]
public class AccessController : ControllerBase
{
    private readonly IAccessService _accessService;

    public AccessController(IAccessService accessService)
    {
        _accessService = accessService;
    }

    [HttpGet("garages/{garageId:int}/members")]
    public async Task<IActionResult> GetGarageMembers(int garageId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _accessService.GetMembersAsync(garageId, userId.Value);
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost("family")]
    public async Task<IActionResult> AssignFamilyAccess([FromBody] AssignFamilyAccessRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _accessService.AssignFamilyAccessAsync(request, userId.Value);
        return result.MatchNoData(
            successStatusCode: StatusCodes.Status201Created,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost("guest")]
    public async Task<IActionResult> CreateGuestAccess([FromBody] CreateGuestAccessRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _accessService.CreateGuestAccessAsync(request, userId.Value);
        return result.Match(
            successStatusCode: StatusCodes.Status201Created,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpDelete("{accessId:int}")]
    public async Task<IActionResult> RevokeAccess(int accessId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _accessService.RevokeAccessAsync(accessId, userId.Value);
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


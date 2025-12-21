using System.Security.Claims;
using Domain.Models.DTOS.Garages;
using GateSense.Application.Garages.Interfaces;
using GetSense.API.ApiResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize]
[ApiController]
[Route("garages")]
public class GaragesController : ControllerBase
{
    private readonly IGarageService _garageService;

    public GaragesController(IGarageService garageService)
    {
        _garageService = garageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserGarages()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _garageService.GetUserGaragesAsync(userId.Value);
        
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("{garageId:int}")]
    public async Task<IActionResult> GetGarageDetails(int garageId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _garageService.GetGarageAsync(garageId, userId.Value);
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost]
    public async Task<IActionResult> CreateGarage([FromBody] CreateGarageRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _garageService.CreateGarageAsync(request, userId.Value);
        return result.Match(
            successStatusCode: StatusCodes.Status201Created,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPut("{garageId:int}")]
    public async Task<IActionResult> UpdateGarage(int garageId, [FromBody] UpdateGarageRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _garageService.UpdateGarageAsync(garageId, request, userId.Value);
        return result.MatchNoData(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpDelete("{garageId:int}")]
    public async Task<IActionResult> DeleteGarage(int garageId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _garageService.DeleteGarageAsync(garageId, userId.Value);
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


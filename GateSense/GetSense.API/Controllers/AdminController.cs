using Domain.Models.DTOS.Admin;
using Domain.Models.DTOS.Garages;
using GateSense.Application.Admin.Interfaces;
using GetSense.API.ApiResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _adminService.GetAllUsersAsync();
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("users/{userId:int}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var result = await _adminService.GetUserByIdAsync(userId);
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPut("users/{userId:int}")]
    public async Task<IActionResult> UpdateUser(int userId, [FromBody] AdminUpdateUserRequest request)
    {
        var result = await _adminService.UpdateUserAsync(userId, request);
        return result.MatchNoData(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpDelete("users/{userId:int}")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var result = await _adminService.DeleteUserAsync(userId);
        return result.MatchNoData(
            successStatusCode: StatusCodes.Status204NoContent,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("garages")]
    public async Task<IActionResult> GetAllGarages()
    {
        var result = await _adminService.GetAllGaragesAsync();
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("garages/{garageId:int}")]
    public async Task<IActionResult> GetGarageById(int garageId)
    {
        var result = await _adminService.GetGarageByIdAsync(garageId);
        return result.Match(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPut("garages/{garageId:int}")]
    public async Task<IActionResult> UpdateGarage(int garageId, [FromBody] UpdateGarageRequest request)
    {
        var result = await _adminService.UpdateGarageAsync(garageId, request);
        return result.MatchNoData(
            successStatusCode: StatusCodes.Status200OK,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpDelete("garages/{garageId:int}")]
    public async Task<IActionResult> DeleteGarage(int garageId)
    {
        var result = await _adminService.DeleteGarageAsync(garageId);
        return result.MatchNoData(
            successStatusCode: StatusCodes.Status204NoContent,
            failure: ApiResults.ToProblemDetails
        );
    }
}


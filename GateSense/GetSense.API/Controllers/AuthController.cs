using System.Security.Claims;
using GetSense.API.ApiResult;
using GateSense.Application.Auth.Interfaces;
using Domain.Models.DTOS.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
    {
        var result = await _authService.RegisterAsync(model);

        return result.MatchNoData(
            successStatusCode: 201,
            failure: ApiResults.ToProblemDetails
        );
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginByCredentialsAsync([FromBody] LoginModel model)
    {
        var result = await _authService.LoginAsync(model);

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> LoginProfile()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var result = await _authService.GetUserProfile(email);

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [AllowAnonymous]
    [HttpGet("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var result = await _authService.RefreshToken();

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails);
    }

    [Authorize]
    [HttpGet("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        var result = await _authService.LogoutAsync();

        return result.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

}
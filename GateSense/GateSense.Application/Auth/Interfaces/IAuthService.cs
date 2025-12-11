using Domain.Models.DTOS.Auth.Models;
using Domain.Models.DTOS.Auth.Responses;
using Infrastructure.Common.ResultPattern;

namespace GateSense.Application.Auth.Interfaces;

public interface IAuthService
{
    public Task<Result<int>> GetIdByEmail(string email);

    public Task<Result<IEnumerable<string>>> GetRolesByEmail(string email);

    public Task<Result<LoginResponse>> GetUserProfile(string? email);

    public Task<Result<LoginResponse>> RefreshToken();

    public Task<Result> RegisterAsync(RegisterModel registerModel);

    public Task<Result<LoginResponse>> LoginAsync(LoginModel loginModel);

    public Task<Result> LogoutAsync();

}
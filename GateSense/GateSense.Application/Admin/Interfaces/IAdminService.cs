using Domain.Models.Auth;
using Domain.Models.DTOS.Admin;
using Domain.Models.Garages;
using Infrastructure.Common.ResultPattern;

namespace GateSense.Application.Admin.Interfaces;

public interface IAdminService
{
    Task<Result<IEnumerable<AdminUserResponse>>> GetAllUsersAsync();
    Task<Result<AdminUserResponse>> GetUserByIdAsync(int userId);
    Task<Result> UpdateUserAsync(int userId, AdminUpdateUserRequest request);
    Task<Result> DeleteUserAsync(int userId);
    
    Task<Result<IEnumerable<AdminGarageResponse>>> GetAllGaragesAsync();
    Task<Result<AdminGarageResponse>> GetGarageByIdAsync(int garageId);
    Task<Result> UpdateGarageAsync(int garageId, Domain.Models.DTOS.Garages.UpdateGarageRequest request);
    Task<Result> DeleteGarageAsync(int garageId);
}


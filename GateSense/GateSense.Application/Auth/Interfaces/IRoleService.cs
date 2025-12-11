using Domain.Models.Auth;

namespace GateSense.Application.Auth.Interfaces;

public interface IRoleService
{
    public Task<bool> AddToRolesAsync(ApplicationUser user, string role);
}
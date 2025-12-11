using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly IServiceScope _scope;

    public DatabaseSeeder(IServiceScope scope)
    {
        _scope = scope;
    }
    
    public async Task SeedAsync()
    {
        var scopedServices = _scope.ServiceProvider;

        var context = scopedServices.GetRequiredService<ApplicationDbContext>();
        var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole<int>>>();

        await SeedRolesAsync(roleManager);

        await context.SaveChangesAsync();
    }
    private async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(UserRoles.Admin));
        }
        
        if (!await roleManager.RoleExistsAsync(UserRoles.User))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(UserRoles.User));
        }
    }
}
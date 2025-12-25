using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GetSense.API.Configurations;

public static class ConfigureApp
{
    public static async Task Configure(this WebApplication app)
    {
        var config = app.Configuration;

        app.UseExceptionHandler();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "GateSense API v1");
            c.RoutePrefix = "swagger";
        });

        using var scope = app.Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();

        var seeder = new DatabaseSeeder(scope);
        await seeder.SeedAsync();

        if (!app.Environment.IsDevelopment())
        {
            app.UseForwardedHeaders();
        }
        
        app.UseHttpsRedirection();
        
        app.UseRouting();
        
        var frontEndUrl = config.GetSection("ApplicationUrls")["FrontEnd"];
        if (!string.IsNullOrEmpty(frontEndUrl))
        {
            app.UseCors(options =>
            {
                options
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithOrigins(frontEndUrl);
            });
        }
        else
        {
            app.UseCors(options =>
            {
                options
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin();
            });
        }
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }
}

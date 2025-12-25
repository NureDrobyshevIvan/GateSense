using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        var logger = scopedServices.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var context = scopedServices.GetRequiredService<ApplicationDbContext>();
            logger.LogInformation("Starting database migration...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration completed successfully");

            var seeder = new DatabaseSeeder(scope);
            logger.LogInformation("Starting database seeding...");
            await seeder.SeedAsync();
            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database migration or seeding. Application will continue to start.");
        }

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

using Domain.Models.Auth;
using Domain.Models.Devices;
using Domain.Models.Garages;
using Domain.Models.Gates;
using Domain.Models.Sensors;
using Infrastructure.Data.Configurations.Auth;
using Infrastructure.Data.Configurations.Garages;
using Infrastructure.Data.Configurations.Devices;
using Infrastructure.Data.Configurations.Gates;
using Infrastructure.Data.Configurations.Sensors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    public DbSet<Garage> Garages { get; set; } = null!;

    public DbSet<GarageAccess> GarageAccesses { get; set; } = null!;

    public DbSet<AccessKey> AccessKeys { get; set; } = null!;

    public DbSet<IoTDevice> Devices { get; set; } = null!;

    public DbSet<SensorReading> SensorReadings { get; set; } = null!;

    public DbSet<GateEvent> GateEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new GarageConfiguration());
        modelBuilder.ApplyConfiguration(new GarageAccessConfiguration());
        modelBuilder.ApplyConfiguration(new AccessKeyConfiguration());
        modelBuilder.ApplyConfiguration(new IoTDeviceConfiguration());
        modelBuilder.ApplyConfiguration(new SensorReadingConfiguration());
        modelBuilder.ApplyConfiguration(new GateEventConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
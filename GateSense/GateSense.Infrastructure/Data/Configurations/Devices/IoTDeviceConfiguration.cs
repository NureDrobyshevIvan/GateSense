using Domain.Models.Devices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Devices;

public class IoTDeviceConfiguration : IEntityTypeConfiguration<IoTDevice>
{
    public void Configure(EntityTypeBuilder<IoTDevice> builder)
    {
        builder.HasIndex(d => d.SerialNumber).IsUnique();

        builder.Property(d => d.SerialNumber)
            .HasMaxLength(64);

        builder.Property(d => d.FirmwareVersion)
            .HasMaxLength(32);

        builder.Property(d => d.DeviceType)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(d => d.CreatedOn)
            .HasDefaultValueSql("NOW()");

        builder.Property(d => d.LastModifiedOn)
            .HasDefaultValueSql("NOW()");
    }
}


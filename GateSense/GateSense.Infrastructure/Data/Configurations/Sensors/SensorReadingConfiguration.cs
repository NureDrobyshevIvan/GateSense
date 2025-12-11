using Domain.Models.Sensors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Sensors;

public class SensorReadingConfiguration : IEntityTypeConfiguration<SensorReading>
{
    public void Configure(EntityTypeBuilder<SensorReading> builder)
    {
        builder.Property(s => s.SensorType)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(s => s.Unit)
            .HasMaxLength(16);

        builder.Property(s => s.Value)
            .HasColumnType("decimal(10,2)");

        builder.Property(s => s.CreatedOn)
            .HasDefaultValueSql("NOW()");

        builder.Property(s => s.LastModifiedOn)
            .HasDefaultValueSql("NOW()");
    }
}


using Domain.Models.Garages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Garages;

public class GarageAccessConfiguration : IEntityTypeConfiguration<GarageAccess>
{
    public void Configure(EntityTypeBuilder<GarageAccess> builder)
    {
        builder.HasIndex(x => new { x.GarageId, x.UserId }).IsUnique();

        builder.Property(x => x.AccessLevel)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.CreatedOn)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(x => x.LastModifiedOn)
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}


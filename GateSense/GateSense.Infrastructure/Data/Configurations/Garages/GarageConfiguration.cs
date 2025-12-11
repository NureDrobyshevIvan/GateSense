using Domain.Models.Garages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Garages;

public class GarageConfiguration : IEntityTypeConfiguration<Garage>
{
    public void Configure(EntityTypeBuilder<Garage> builder)
    {
        builder.Property(g => g.Name)
            .HasMaxLength(128);

        builder.Property(g => g.Address)
            .HasMaxLength(256);

        builder.Property(g => g.TimeZone)
            .HasMaxLength(64);

        builder.HasOne(g => g.Owner)
            .WithMany()
            .HasForeignKey(g => g.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(g => g.CreatedOn)
            .HasDefaultValueSql("NOW()");

        builder.Property(g => g.LastModifiedOn)
            .HasDefaultValueSql("NOW()");
    }
}


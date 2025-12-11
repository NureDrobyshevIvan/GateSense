using Domain.Models.Garages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Garages;

public class AccessKeyConfiguration : IEntityTypeConfiguration<AccessKey>
{
    public void Configure(EntityTypeBuilder<AccessKey> builder)
    {
        builder.HasIndex(x => x.Token).IsUnique();

        builder.Property(x => x.KeyType)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Token)
            .HasMaxLength(256);

        builder.Property(x => x.CreatedOn)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(x => x.LastModifiedOn)
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}


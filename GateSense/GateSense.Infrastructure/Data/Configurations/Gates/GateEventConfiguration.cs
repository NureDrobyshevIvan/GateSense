using Domain.Models.Gates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Gates;

public class GateEventConfiguration : IEntityTypeConfiguration<GateEvent>
{
    public void Configure(EntityTypeBuilder<GateEvent> builder)
    {
        builder.Property(e => e.TriggerSource)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(e => e.Action)
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(e => e.Result)
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(e => e.FailureReason)
            .HasMaxLength(256);

        builder.HasOne(e => e.AccessKey)
            .WithMany(k => k.GateEvents)
            .HasForeignKey(e => e.AccessKeyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.InitiatorUser)
            .WithMany()
            .HasForeignKey(e => e.InitiatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.CreatedOn)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(e => e.LastModifiedOn)
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}


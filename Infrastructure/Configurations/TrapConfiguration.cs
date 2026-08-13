using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace RodentTrap.Infrastructure.Data.Configurations;

public class TrapConfiguration : IEntityTypeConfiguration<Trap>
{
    public void Configure(EntityTypeBuilder<Trap> builder)
    {
        builder.ToTable("Traps");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TrapNumber).HasMaxLength(50).IsRequired();
        builder.Property(t => t.TrapGroup).HasMaxLength(50).IsRequired();
        builder.Property(t => t.BatteryPercentage).IsRequired();
          builder.Property(t => t.IndicatorStatus).HasConversion<string>().HasMaxLength(20);
        
       

    }
}
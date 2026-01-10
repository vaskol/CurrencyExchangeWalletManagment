using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adapter.SQL.Configuration;

public class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRate>
{
    public void Configure(EntityTypeBuilder<CurrencyRate> builder)
    {
        builder.ToTable("CurrencyRate");
        builder.HasKey(r => new { r.Currency, r.RateDate });

        builder.Property(r => r.Currency).IsRequired().HasMaxLength(3);
        builder.Property(r => r.Rate).IsRequired().HasPrecision(18, 6);
    }
}

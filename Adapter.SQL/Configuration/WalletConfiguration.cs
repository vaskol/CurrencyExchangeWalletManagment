using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adapter.SQL.Configuration;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallet");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id).ValueGeneratedNever();
        builder.Property(w => w.Currency).IsRequired().HasMaxLength(3);
        builder.Property(w => w.Balance).HasPrecision(18, 4);
    }
}

using Adapter.SQL.Data;
using Core.Entities;
using Core.Ports;

namespace Adapter.SQL.Repositories;

public class WalletRepository(AppDbContext dbContext) : IWalletRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<Wallet?> GetByIdAsync(long walletId)
    {
        return await _dbContext.Wallets.FindAsync(walletId);
    }

    public async Task AddAsync(Wallet wallet)
    {
        await _dbContext.Wallets.AddAsync(wallet);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Wallet wallet)
    {
        _dbContext.Wallets.Update(wallet);
        await _dbContext.SaveChangesAsync();
    }
}

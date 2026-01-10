using Core.Entities;
namespace Core.Ports;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(long walletId);
    Task AddAsync(Wallet wallet);
    Task UpdateAsync(Wallet wallet);
}

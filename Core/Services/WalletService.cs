using Core.Entities;
using Core.Strategies;
using Core.Ports;
using Core.Utilities;
namespace Core.Services;

public class WalletService(IWalletRepository walletRepository)
{
    private readonly IWalletRepository _walletRepository = walletRepository;

    public virtual async Task<Wallet> CreateWalletAsync(long id, string currency, decimal initialBalance = 0)
    {
       //var initBalance = NumberFormattingHelper.ToPrettyAmount(initialBalance); 
        Wallet wallet = new Wallet(id, currency, initialBalance);
        await _walletRepository.AddAsync(wallet);
        return wallet;
    }

    public virtual async Task<Wallet?> GetWalletAsync(long walletId)
    {
        return await _walletRepository.GetByIdAsync(walletId);
    }

    public virtual async Task AdjustWalletBalanceAsync(long walletId, decimal amount, IWalletAdjustmentStrategy strategy)
    {
        var wallet = await _walletRepository.GetByIdAsync(walletId);
        if (wallet == null)
            throw new InvalidOperationException($"Wallet {walletId} not found");

        //var amt = NumberFormattingHelper.ToPrettyAmount(amount);
        wallet.AdjustBalance(amount, strategy);
        await _walletRepository.UpdateAsync(wallet);
    }

}

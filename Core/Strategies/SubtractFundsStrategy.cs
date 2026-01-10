using Core.Exceptions;

namespace Core.Strategies;

public class SubtractFundsStrategy : IWalletAdjustmentStrategy
{
    public decimal Adjust(decimal currentBalance, decimal amount)
    {
        if (currentBalance < amount)
            throw new InsufficientFundsException("Wallet has insufficient funds.");

        return currentBalance - amount;
    }
}

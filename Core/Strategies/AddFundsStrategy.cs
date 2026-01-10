namespace Core.Strategies;

public class AddFundsStrategy : IWalletAdjustmentStrategy
{
    public decimal Adjust(decimal currentBalance, decimal amount)
    {
        return currentBalance + amount;
    }
}

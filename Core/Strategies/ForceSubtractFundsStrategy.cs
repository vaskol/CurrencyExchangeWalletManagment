namespace Core.Strategies;

public class ForceSubtractFundsStrategy : IWalletAdjustmentStrategy
{
    public decimal Adjust(decimal currentBalance, decimal amount)
    {
        return currentBalance - amount;
    }
}

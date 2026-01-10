namespace Core.Strategies;

public interface IWalletAdjustmentStrategy
{
    decimal Adjust(decimal currentBalance, decimal amount);
}

namespace Core.Entities;

using Core.Strategies;

public class Wallet(long id, string currency, decimal initialBalance = 0)
{
    public long Id { get; private set; } = id;
    public string Currency { get; private set; } = currency;
    public decimal Balance { get; private set; } = initialBalance;

    public void AdjustBalance(decimal amount, IWalletAdjustmentStrategy strategy)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        Balance = strategy.Adjust(Balance, amount);
    }

    // todo
    private Wallet() : this(0, string.Empty, 0) { }

}
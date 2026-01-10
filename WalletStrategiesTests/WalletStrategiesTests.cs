using Core.Exceptions;
using Core.Strategies;

namespace WalletStrategyTests;

public class WalletStrategyTests
{
    [Fact]
    public void AddFunds_AddsAmount()
    {
        var strategy = new AddFundsStrategy();
        var result = strategy.Adjust(100m, 50m);
        Assert.Equal(150m, result);
    }
    [Fact]
    public void SubtractFunds_SubstractsAmount()
    {
        var strategy = new SubtractFundsStrategy();
        var result = strategy.Adjust(100m, 50m);
        Assert.Equal(50m, result);
    }
    [Fact]
    public void SubtractFunds_Throws_WhenInsufficient()
    {
        var strategy = new SubtractFundsStrategy();
        Assert.Throws<InsufficientFundsException>(() =>
            strategy.Adjust(50m, 100m));
    }

    [Fact]
    public void ForceSubtract_AllowsNegative()
    {
        var strategy = new ForceSubtractFundsStrategy();
        var result = strategy.Adjust(50m, 100m);
        Assert.Equal(-50m, result);
    }
}


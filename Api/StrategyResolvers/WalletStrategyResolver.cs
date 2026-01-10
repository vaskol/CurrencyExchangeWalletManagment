using Core.Strategies;

namespace Api.StrategyResolvers
{
    public static class WalletStrategyResolver
    {
        private static readonly Dictionary<string, IWalletAdjustmentStrategy> _strategies =
            new()
            {
                { "AddFundsStrategy", new AddFundsStrategy() },
                { "SubtractFundsStrategy", new SubtractFundsStrategy() },
                { "ForceSubtractFundsStrategy", new ForceSubtractFundsStrategy() }
            };

        public static IWalletAdjustmentStrategy Resolve(string strategy)
        {
            if (_strategies.TryGetValue(strategy, out var selectedStrategy))
                return selectedStrategy;

            throw new ArgumentException($"Unknown wallet adjustment strategy: {strategy}");
        }
    }
}

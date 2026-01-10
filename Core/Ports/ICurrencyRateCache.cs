using Core.Entities;

namespace Core.Ports;

public interface ICurrencyRateCache
{
    Task<decimal?> GetLatestRateFromCacheAsync(string currency, DateTime date);
    Task SetLatestRatesToCacheAsync(List<CurrencyRate> rates);
}

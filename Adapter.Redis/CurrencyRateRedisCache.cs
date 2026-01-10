using Core.Entities;
using Core.Ports;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Adapter.Redis;

public class CurrencyRateRedisCache(IDistributedCache cache) : ICurrencyRateCache
{
    private readonly IDistributedCache _cache = cache;
    private static string Key(DateTime date) => $"latest:{date:yyyy-MM-dd}";

    public async Task<decimal?> GetLatestRateFromCacheAsync(string currency, DateTime date)
    {
        var json = await _cache.GetStringAsync(Key(date));
        if (json == null)
            return null;

        var rates = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);
        return rates?.GetValueOrDefault(currency);
    }

    public async Task SetLatestRatesToCacheAsync(List<CurrencyRate> rates)
    {
        if (rates.Count == 0) return;

        var date = rates.Max(r => r.RateDate).Date;

        var dict = rates
            .GroupBy(r => r.Currency)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.RateDate).First().Rate);

        await _cache.SetStringAsync(
            Key(date),
            JsonSerializer.Serialize(dict),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });
    }
}

using Core.Entities;
using Core.Ports;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Adapter.Redis;

public class CurrencyRateRedisCache(IDistributedCache cache, ILoggerFactory loggerFactory) : ICurrencyRateCache
{
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger _logger = loggerFactory.CreateLogger<CurrencyRateRedisCache>();
    private static string Key(DateTime date) => $"latest:{date:yyyy-MM-dd}";

    public async Task<decimal?> GetLatestRateFromCacheAsync(string currency, DateTime date)
    {
        try
        {
            var json = await _cache.GetStringAsync(Key(date));
            if (json is null)
                return null;

            var rates = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);
            if (rates is null || rates.Count < 2)
                return null; // partial / invalid snapshot
            if (!rates.TryGetValue(currency, out var rate) || rate <= 0)
                return null;
            return rate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get latest currency rate from cache for {Currency} on {Date}", currency, date);
            return null;
        }
    }

    public async Task SetLatestRatesToCacheAsync(List<CurrencyRate> rates)
    {
        if (rates.Count == 0) return;
        try
        {
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving currency rates to Redis cache for date {Date}", rates.Max(r => r.RateDate));
        }
    }
}

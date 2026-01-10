using Core.Entities;
using Core.Ports;

namespace Core.Services;

public class CurrencyRateService(
    ICurrencyRateProvider rateProvider,
    ICurrencyRateCache cache,
    ICurrencyRateRepository rateRepository)
{
    private readonly ICurrencyRateProvider _rateProvider = rateProvider;
    private readonly ICurrencyRateRepository _rateRepository = rateRepository;
    private readonly ICurrencyRateCache _cache = cache;
    public virtual async Task UpdateRatesAsync()
    {
        var rates = await _rateProvider.GetLatestRatesFromEcbAsync();
        var rowsAffected = await _rateRepository.UpsertRatesAsync(rates);
        //var today = DateTime.UtcNow;

        //CHECK THIS await _rateRepository.UpsertRatesAsync(rates);

        if (rowsAffected > 0)
        {
            await _cache.SetLatestRatesToCacheAsync(rates);
        }
    }
    public virtual async Task<decimal> GetLatestRateAsync(string currency, DateTime date)
    {
        // Redis first
        var cached = await _cache.GetLatestRateFromCacheAsync(currency, date);
        if (cached.HasValue)
            return cached.Value;

        // Fallback to SQL
        var dbRate = await _rateRepository.GetLatestRateAsync(currency, date);
        if (!dbRate.HasValue)
            throw new Exception("Rate not found");

        await _cache.SetLatestRatesToCacheAsync(new List<CurrencyRate>
        {
            new() {
                Currency = currency,
                RateDate = date,
                Rate = dbRate.Value
            }
        });

        return dbRate.Value;
    }

    public virtual async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow.Date;

        // Use Redis + SQL fallback
        var fromRate = await GetLatestRateAsync(fromCurrency, targetDate);
        var toRate = await GetLatestRateAsync(toCurrency, targetDate);

        // Conversion formula: amount * (toRate / fromRate)
        return amount * ((decimal)toRate / fromRate);
    }
}

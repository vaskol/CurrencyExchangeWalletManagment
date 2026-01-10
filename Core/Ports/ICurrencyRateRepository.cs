using Core.Entities;

namespace Core.Ports;

public interface ICurrencyRateRepository
{
    Task<int> UpsertRatesAsync(List<CurrencyRate> rates);
    Task<decimal?> GetLatestRateAsync(string currency, DateTime targetDate);
}

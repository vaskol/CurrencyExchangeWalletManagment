using Core.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core.Ports;

public interface ICurrencyRateRepository
{
    Task<int> UpsertRatesAsync(List<CurrencyRate> rates);
    Task<decimal?> GetLatestRateAsync(string currency, DateTime targetDate);
}

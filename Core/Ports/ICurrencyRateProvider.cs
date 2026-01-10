using Core.Entities;

namespace Core.Ports;

public interface ICurrencyRateProvider
{
    Task<List<CurrencyRate>> GetLatestRatesFromEcbAsync();
}

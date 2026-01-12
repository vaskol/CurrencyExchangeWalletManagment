using Core.Entities;
using Core.Ports;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Xml.Linq;
namespace Adapter.ECB
{
    public class EcbCurrencyRateProvider(HttpClient httpClient, ILoggerFactory loggerFactory) : ICurrencyRateProvider
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger _logger = loggerFactory.CreateLogger<EcbCurrencyRateProvider>();
        public async Task<List<CurrencyRate>> GetLatestRatesFromEcbAsync()
        {
            try
            {
                var url = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";
                var xmlString = await _httpClient.GetStringAsync(url);
                var doc = XDocument.Parse(xmlString);
                var ns = doc.Root!.GetDefaultNamespace();

                var dailyCube = doc.Descendants(ns + "Cube").FirstOrDefault(x => x.Attribute("time") != null);
                if (dailyCube is null)
                    throw new InvalidOperationException("ECB XML does not contain a daily Cube element");

                var rateDate = DateTime.ParseExact(dailyCube.Attribute("time")!.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                var rates = dailyCube.Elements(ns + "Cube")
                     .Select(x => new CurrencyRate(
                         x.Attribute("currency")!.Value,
                         decimal.Parse(
                             x.Attribute("rate")!.Value,
                             NumberStyles.Any,
                             CultureInfo.InvariantCulture
                         ),
                         rateDate
                     ))
                     .ToList();

                // Add EUR manually, which is the base currency
                rates.Add(new CurrencyRate("EUR", 1m, rateDate));

                return rates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch or parse ECB rates");
                throw;
            }
        }
    }
}

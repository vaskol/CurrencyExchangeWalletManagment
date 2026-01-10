using System.Globalization;
using System.Xml.Linq;
using Core.Ports;
using Core.Entities;
namespace Adapter.ECB
{
    public class EcbCurrencyRateProvider(HttpClient httpClient) : ICurrencyRateProvider
    {
        private readonly HttpClient _httpClient = httpClient;
        public async Task<List<CurrencyRate>> GetLatestRatesFromEcbAsync()
        {
            var url = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";
            var xmlString = await _httpClient.GetStringAsync(url);
            var doc = XDocument.Parse(xmlString);
            var ns = doc.Root!.GetDefaultNamespace();

            var dailyCube = doc.Descendants(ns + "Cube").FirstOrDefault(x => x.Attribute("time") != null);
            if (dailyCube is null)
                throw new InvalidOperationException("ECB XML does not contain a daily Cube element");

            var rateDate = DateTime.ParseExact(dailyCube.Attribute("time")!.Value,"yyyy-MM-dd",CultureInfo.InvariantCulture);

            var rates = dailyCube.Elements(ns + "Cube")
                 .Select(x => new CurrencyRate
                 {
                     Currency = x.Attribute("currency")!.Value,
                     Rate = decimal.Parse(
                         x.Attribute("rate")!.Value,
                         NumberStyles.Any,
                         CultureInfo.InvariantCulture
                     ),
                     RateDate = rateDate
                 })
                 .ToList();

            // Add EUR manually, which is the base currency
            rates.Add(new CurrencyRate
            {
                Currency = "EUR",
                Rate = 1m,
                RateDate = rateDate
            });

            return rates;
        }
    }
}

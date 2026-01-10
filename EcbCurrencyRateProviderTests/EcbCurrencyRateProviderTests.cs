using Adapter.ECB;
namespace EcbCurrencyRateProviderTests;
public class EcbCurrencyRateProviderTests
{
    [Fact]
    public async Task ShouldFetchRatesFromEcb()
    {
        var provider = new EcbCurrencyRateProvider(new HttpClient());
        var rates = await provider.GetLatestRatesFromEcbAsync();

        Assert.NotNull(rates);
        Assert.NotEmpty(rates);
        Assert.True(rates.Any());
    }
}

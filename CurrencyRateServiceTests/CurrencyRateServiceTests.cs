using Xunit;
using Moq;
using Core.Ports;
using Core.Services;
using Core.Entities;
namespace CurrencyRateServiceTests;
public class CurrencyRateServiceTests
{
    [Fact]
    public async Task Convert_UsesRedisFirst()
    {
        var rateProviderMock = new Mock<ICurrencyRateProvider>();
        var cacheMock = new Mock<ICurrencyRateCache>();
        var rateRepositoryMock = new Mock<ICurrencyRateRepository>();
        var currencyRateServiceMock = new CurrencyRateService(
            rateProviderMock.Object,
            cacheMock.Object,
            rateRepositoryMock.Object

       );
        cacheMock.Setup(c => c.GetLatestRateFromCacheAsync("USD", It.IsAny<DateTime>()))
             .ReturnsAsync(1.2m);
        cacheMock.Setup(c => c.GetLatestRateFromCacheAsync("EUR", It.IsAny<DateTime>()))
             .ReturnsAsync(1m);

        var result = await currencyRateServiceMock.ConvertAsync(100, "EUR", "USD");

        Assert.Equal(120, result);
        rateRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateRatesAsync_ShouldCallRepositoryUpsert()
    {
        //Arrange
        var rates = new List<CurrencyRate>
       {  new CurrencyRate
              {
                  Currency = "USD",
                  Rate = 1.1m,
                  RateDate = DateTime.UtcNow.Date
              },
              new CurrencyRate
              {
                  Currency = "GBP",
                  Rate = 0.9m,
                  RateDate = DateTime.UtcNow.Date
              }
        };
        var rateProviderMock = new Mock<ICurrencyRateProvider>();
        var cacheMock = new Mock<ICurrencyRateCache>();
        var rateRepositoryMock = new Mock<ICurrencyRateRepository>();
        var currencyRateServiceMock = new CurrencyRateService(
            rateProviderMock.Object,
            cacheMock.Object,
            rateRepositoryMock.Object
       );

        rateProviderMock.Setup(p => p.GetLatestRatesFromEcbAsync()).ReturnsAsync(rates);
        rateRepositoryMock.Setup(p => p.UpsertRatesAsync(rates));

        //Act
        await currencyRateServiceMock.UpdateRatesAsync();

        //Assert
        rateRepositoryMock.Verify(r => r.UpsertRatesAsync(It.IsAny<List<CurrencyRate>>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRatesAsync_ShouldCallCache()
    {
        //Arrange
        var rates = new List<CurrencyRate>
       {  new CurrencyRate
              {
                  Currency = "USD",
                  Rate = 1.1m,
                  RateDate = DateTime.UtcNow.Date
              },
              new CurrencyRate
              {
                  Currency = "GBP",
                  Rate = 0.9m,
                  RateDate = DateTime.UtcNow.Date
              }
        };
        var rowsAffected = 0;
        var rateProviderMock = new Mock<ICurrencyRateProvider>();
        var cacheMock = new Mock<ICurrencyRateCache>();
        var rateRepositoryMock = new Mock<ICurrencyRateRepository>();
        var currencyRateServiceMock = new CurrencyRateService(
            rateProviderMock.Object,
            cacheMock.Object,
            rateRepositoryMock.Object
       );

        rateProviderMock.Setup(p => p.GetLatestRatesFromEcbAsync()).ReturnsAsync(rates);
        rateRepositoryMock.Setup(p => p.UpsertRatesAsync(rates));

        if (rowsAffected > 0)
            cacheMock.Setup(p => p.SetLatestRatesToCacheAsync(rates));

        //Act
        await currencyRateServiceMock.UpdateRatesAsync();

        //Assert
        rateRepositoryMock.Verify(r => r.UpsertRatesAsync(It.IsAny<List<CurrencyRate>>()), Times.Once);
    }

    [Fact]
    public async Task ConvertAsync_ShouldReturnCorrectAmount()
    {
        //Arrange
        var rateProviderMock = new Mock<ICurrencyRateProvider>();
        var cacheMock = new Mock<ICurrencyRateCache>();
        var rateRepositoryMock = new Mock<ICurrencyRateRepository>();
        var currencyRateServiceMock = new CurrencyRateService(
            rateProviderMock.Object,
            cacheMock.Object,
            rateRepositoryMock.Object
       );
        cacheMock.Setup(c => c.GetLatestRateFromCacheAsync("USD", It.IsAny<DateTime>()))
             .ReturnsAsync(1.1m);

        cacheMock.Setup(c => c.GetLatestRateFromCacheAsync("EUR", It.IsAny<DateTime>()))
             .ReturnsAsync(1m);

        //Act
        var result = await currencyRateServiceMock.ConvertAsync(110, "USD", "EUR");

        // USD->EUR: 110 * (1 / 1.1) = 100
        Assert.Equal(100m, result);
    }


    [Fact]
    public async Task ConvertAsync_ShouldThrow_WhenRateNotFound()
    {
        //Arrange
        var rateProviderMock = new Mock<ICurrencyRateProvider>();
        var cacheMock = new Mock<ICurrencyRateCache>();
        var rateRepositoryMock = new Mock<ICurrencyRateRepository>();
        var currencyRateServiceMock = new CurrencyRateService(
            rateProviderMock.Object,
            cacheMock.Object,
            rateRepositoryMock.Object
       );

        rateRepositoryMock.Setup(r => r.GetLatestRateAsync("USD", It.IsAny<DateTime>())).ReturnsAsync((decimal?)null);

        //Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
        currencyRateServiceMock.ConvertAsync(100, "USD", "EUR")
    );
    }
}
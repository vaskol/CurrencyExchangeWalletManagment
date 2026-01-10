using Adapter.SQL;
using Api.Contracts;
using Api.Controllers;
using Azure.Core;
using Core.Entities;
using Core.Ports;
using Core.Services;
using Core.Strategies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
namespace WalletControllerTests;
public class WalletControllerTests
{
    [Fact]
    public async Task Successful_WalletCreation()
    {
        // Arrange
        var rateProviderMock = new Mock<ICurrencyRateProvider>();
        var cacheMock = new Mock<ICurrencyRateCache>();
        var rateRepositoryMock = new Mock<ICurrencyRateRepository>();
        var walletRepositoryMock = new Mock<IWalletRepository>();

        var walletService = new WalletService(walletRepositoryMock.Object);

        var currencyRateService = new CurrencyRateService(
            rateProviderMock.Object,   
            cacheMock.Object,          
            rateRepositoryMock.Object  
        );
        var inMemoryConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AllowedCurrencies:0", "EUR" },
                { "AllowedCurrencies:1", "USD" }
            })
            .Build();

        var controller = new WalletsController(
            walletService,
            currencyRateService,
            inMemoryConfig
        );

        var request = new CreateWalletRequest
        {
            Id = 1,
            Currency = "EUR",
            InitialBalance = 100
        };

        // Act
        var result = await controller.CreateWallet(request);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<WalletResponse>(created.Value);

        Assert.Equal(1, response.Id);
        Assert.Equal("EUR", response.Currency);
        Assert.Equal(100, response.Balance);
    }

    [Fact]
    public async Task Not_Successful_WalletCreation()
    {
        // Arrange
        var rateProviderMock = new Mock<ICurrencyRateProvider>();
        var cacheMock = new Mock<ICurrencyRateCache>();
        var rateRepositoryMock = new Mock<ICurrencyRateRepository>();
        var walletRepositoryMock = new Mock<IWalletRepository>();

        var walletService = new WalletService(walletRepositoryMock.Object);

        var currencyRateService = new CurrencyRateService(
            rateProviderMock.Object,
            cacheMock.Object,
            rateRepositoryMock.Object
        );
        var inMemoryConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AllowedCurrencies:0", "EUR" },
                { "AllowedCurrencies:1", "USD" }
            })
            .Build();


        var controller = new WalletsController(
            walletService,
            currencyRateService,
            inMemoryConfig
        );

        var request = new CreateWalletRequest
        {
            Id = 1,
            Currency = "Eru",
            InitialBalance = 100
        };

        // Act
        var result = await controller.CreateWallet(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorMessage = Assert.IsType<string>(badRequest.Value);

        Assert.Equal("Invalid currency. Allowed currencies: EUR, USD", errorMessage);
    }

    [Fact]
    public async Task Successful_WalletRetrieval()
    {
        // Arrange
        var rateProviderMock = new Mock<ICurrencyRateProvider>();
        var cacheMock = new Mock<ICurrencyRateCache>();
        var rateRepositoryMock = new Mock<ICurrencyRateRepository>();
        var walletRepositoryMock = new Mock<IWalletRepository>();
        var walletServiceMock = new Mock<WalletService>(walletRepositoryMock.Object);
        var currencyRateServiceMock = new Mock<CurrencyRateService>(
            rateProviderMock.Object,
            cacheMock.Object,
            rateRepositoryMock.Object
        );

        var inMemoryConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AllowedCurrencies:0", "EUR" },
                { "AllowedCurrencies:1", "USD" }
            })
            .Build();

        var controller = new WalletsController(
            walletServiceMock.Object,
            currencyRateServiceMock.Object,
            inMemoryConfig
        );
        var wallet = new Wallet(1, "EUR", 100);
        var currency = "USD";

        walletServiceMock.Setup(x => x.GetWalletAsync(wallet.Id)).ReturnsAsync(wallet);
       
        currencyRateServiceMock.Setup(x => x.ConvertAsync(wallet.Balance, wallet.Currency, currency, It.IsAny<DateTime>()))
            .ReturnsAsync(110); 

        // Act
        var result = await controller.GetWallet(wallet.Id, currency);

        // Assert
        var response = Assert.IsType<WalletResponse>(result.Value);

        Assert.Equal(wallet.Id, response.Id);
        Assert.Equal(currency, response.Currency);
        // USD->EUR: 110 * (1 / 1.1) = 100
        Assert.Equal(110, response.Balance);
    }

    [Fact]
    public async Task Successful_StrategyEnforcement_type1()
    {
        // Arrange
        var rateProviderMock = new Mock<ICurrencyRateProvider>();
        var cacheMock = new Mock<ICurrencyRateCache>();
        var rateRepositoryMock = new Mock<ICurrencyRateRepository>();
        var walletRepositoryMock = new Mock<IWalletRepository>();
        var walletServiceMock = new Mock<WalletService>(walletRepositoryMock.Object);
        var currencyRateServiceMock = new Mock<CurrencyRateService>(
          rateProviderMock.Object,
          cacheMock.Object,
          rateRepositoryMock.Object
      );

        var wallet = new Wallet(1, "EUR", 100);

        walletServiceMock.Setup(x => x.GetWalletAsync(wallet.Id)).ReturnsAsync(wallet);

        walletServiceMock.Setup(x => x.AdjustWalletBalanceAsync(
                It.IsAny<long>(),
                It.IsAny<decimal>(),
                It.IsAny<IWalletAdjustmentStrategy>()))
            .Returns(Task.CompletedTask);

        currencyRateServiceMock
            .Setup(x => x.ConvertAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(100);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
            { "AllowedCurrencies:0", "EUR" },
            { "AllowedCurrencies:1", "USD" }
            })
            .Build();

        var controller = new WalletsController(
            walletServiceMock.Object,
            currencyRateServiceMock.Object,
            config
        );

        // Act
        var result = await controller.AdjustBalance(
            wallet.Id,
            50,
            "USD",
            "AddFundsStrategy"
        );

        // Assert
        walletServiceMock.Verify(x => x.AdjustWalletBalanceAsync(
            wallet.Id,
            It.IsAny<decimal>(),
            It.Is<IWalletAdjustmentStrategy>(s => s.GetType().Name == "AddFundsStrategy")
        ), Times.Once);

        Assert.IsType<NoContentResult>(result);
    }
}
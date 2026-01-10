//using Api.Contracts;
//using Api.Controllers;
//using Core.Ports;
//using Core.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace WalletControllerTests
//{
//    internal class FileName
//    {
//        [Fact]
//        public async Task Successful_WalletCreation()
//        {
//            // Arrange
//            var rateProviderMock = new Mock<ICurrencyRateProvider>();
//            var cacheMock = new Mock<ICurrencyRateCache>();
//            var rateRepositoryMock = new Mock<ICurrencyRateRepository>();
//            var walletRepositoryMock = new Mock<IWalletRepository>();

//            var walletService = new WalletService(walletRepositoryMock.Object);

//            var currencyRateService = new CurrencyRateService(
//                rateProviderMock.Object,
//                cacheMock.Object,
//                rateRepositoryMock.Object
//            );
//            var inMemoryConfig = new ConfigurationBuilder()
//                .AddInMemoryCollection(new Dictionary<string, string?>
//                {
//                { "AllowedCurrencies:0", "EUR" },
//                { "AllowedCurrencies:1", "USD" }
//                })
//                .Build();

//            var controller = new WalletsController(
//                walletService,
//                currencyRateService,
//                inMemoryConfig
//            );

//            var request = new CreateWalletRequest
//            {
//                Id = 1,
//                Currency = "EUR",
//                InitialBalance = 100
//            };

//            // Act
//            var result = await controller.CreateWallet(request);

//            // Assert
//            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
//            var response = Assert.IsType<WalletResponse>(created.Value);

//            Assert.Equal(1, response.Id);
//            Assert.Equal("EUR", response.Currency);
//            Assert.Equal(100, response.Balance);
//        }
//    }
//}

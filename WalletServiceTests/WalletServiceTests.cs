using Core.Entities;
using Core.Ports;
using Core.Services;
using Core.Strategies;
using Moq;

namespace WalletServiceTests;

public class WalletServiceTests
{
    [Fact]
    public async Task CreateWallet_CreatesWallet()
    {
        var repoMock = new Mock<IWalletRepository>();
        var service = new WalletService(repoMock.Object);

        var wallet = await service.CreateWalletAsync(1, "EUR", 100);

        Assert.Equal(100, wallet.Balance);
        Assert.Equal("EUR", wallet.Currency);
        repoMock.Verify(r => r.AddAsync(It.IsAny<Wallet>()), Times.Once);
    }

    [Fact]
    public async Task GetWallet_GetsWallet()
    {
        var repoMock = new Mock<IWalletRepository>();
        var service = new WalletService(repoMock.Object);

        var wallet = await service.GetWalletAsync(1);

        repoMock.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task AdjustWalletBalance_ShouldUpdateBalance()
    {
        var wallet = new Wallet(1, "USD", 100);
        var repoMock = new Mock<IWalletRepository>();
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wallet);

        var service = new WalletService(repoMock.Object);
        var strategy = new AddFundsStrategy();

        await service.AdjustWalletBalanceAsync(1, 50, strategy);

        Assert.Equal(150, wallet.Balance);
        repoMock.Verify(r => r.UpdateAsync(wallet), Times.Once);
    }
}

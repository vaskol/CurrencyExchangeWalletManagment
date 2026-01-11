using Api.Contracts;
using Api.StrategyResolvers;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/wallets")]
[EnableRateLimiting("fixed")]
public class WalletsController(
    WalletService walletService,
    CurrencyRateService currencyRateService,
    IConfiguration configuration) : ControllerBase
{
    private readonly WalletService _walletService = walletService;
    private readonly CurrencyRateService _currencyRateService = currencyRateService;
    private readonly IConfiguration _configuration = configuration;


    // POST /api/wallets
    // ------------------------------------------------------------
    [HttpPost]
    public async Task<ActionResult<WalletResponse>> CreateWallet([FromBody] CreateWalletRequest request)
    {
        var allowedCurrencies = _configuration.GetSection("AllowedCurrencies").Get<string[]>();
        if (string.IsNullOrWhiteSpace(request.Currency) || !allowedCurrencies.Contains(request.Currency.ToUpper()))
        {
            return BadRequest($"Invalid currency. Allowed currencies: {string.Join(", ", allowedCurrencies)}");
        }
        if (request.InitialBalance <= 0)
            return BadRequest("Amount must be a positive number.");

        var wallet = await _walletService.CreateWalletAsync(
            request.Id,
            request.Currency.ToUpper(),
            request.InitialBalance);

        return CreatedAtAction(
            nameof(GetWallet),
            new { walletId = wallet.Id },

            new WalletResponse
            {
                Id = wallet.Id,
                Balance = wallet.Balance,
                Currency = wallet.Currency
            });
    }


    // GET /api/wallets/{walletId}?currency={currency}
    // ------------------------------------------------------------
    [HttpGet("{walletId:long}")]
    public async Task<ActionResult<WalletResponse>> GetWallet(long walletId, [FromQuery] string? currency)
    {
        var wallet = await _walletService.GetWalletAsync(walletId);
        if (wallet is null)
            return NotFound();

        var resultCurrency = wallet.Currency;
        var balance = wallet.Balance;

        if (!string.IsNullOrWhiteSpace(currency))
        {
            var allowedCurrencies =
                _configuration.GetSection("AllowedCurrencies").Get<string[]>() ?? [];

            var requestedCurrency = currency.ToUpperInvariant();

            if (!allowedCurrencies.Contains(requestedCurrency))
            {
                return BadRequest(
                    $"Invalid currency. Allowed currencies: {string.Join(", ", allowedCurrencies)}");
            }

            if (!requestedCurrency.Equals(wallet.Currency, StringComparison.OrdinalIgnoreCase))
            {
                balance = await _currencyRateService.ConvertAsync(
                    wallet.Balance,
                    wallet.Currency,
                    requestedCurrency,
                    DateTime.UtcNow.Date
                );

                resultCurrency = requestedCurrency;
            }
        }

        return new WalletResponse
        {
            Id = wallet.Id,
            Balance = balance,
            Currency = resultCurrency
        };
    }


    // POST /api/wallets/{walletId}/adjustbalance
    // ------------------------------------------------------------
    [HttpPost("{walletId:long}/adjustbalance")]
    public async Task<IActionResult> AdjustBalance(long walletId, [FromQuery] decimal amount, [FromQuery] string? currency, [FromQuery] string strategy)
    {
        var wallet = await _walletService.GetWalletAsync(walletId);
        if (wallet is null)
            return NotFound();

        // If currency is not provided, default to wallet's currency
        if (string.IsNullOrWhiteSpace(currency))
            currency = wallet.Currency;

        var allowedCurrencies = _configuration.GetSection("AllowedCurrencies").Get<string[]>();
        if (!allowedCurrencies.Contains(currency.ToUpper()))
        {
            return BadRequest($"Invalid currency. Allowed currencies: {string.Join(", ", allowedCurrencies)}");
        }

        if (amount <= 0)
            return BadRequest("Amount must be a positive number.");

        var adjustmentStrategy = WalletStrategyResolver.Resolve(strategy);
        decimal adjustedAmount = amount;

        if (!currency.Equals(wallet.Currency, StringComparison.OrdinalIgnoreCase))
        {
            adjustedAmount = await _currencyRateService.ConvertAsync(
                amount,
                currency,
                wallet.Currency);
        }

        await _walletService.AdjustWalletBalanceAsync(
            walletId,
            adjustedAmount,
            adjustmentStrategy);

        return NoContent();
    }

}

using Api.Contracts;
using Api.StrategyResolvers;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/wallets")]
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

        var allowedCurrencies = _configuration.GetSection("AllowedCurrencies").Get<string[]>();
        if (string.IsNullOrWhiteSpace(currency) || !allowedCurrencies.Contains(currency.ToUpper()))
        {
            return BadRequest($"Invalid currency. Allowed currencies: {string.Join(", ", allowedCurrencies)}");
        }

        decimal balance = wallet.Balance;
        string resultCurrency = wallet.Currency;

        if (!string.IsNullOrWhiteSpace(currency) && !currency.Equals(wallet.Currency, StringComparison.OrdinalIgnoreCase))
        {
            // Convert using today’s rates
            balance = await _currencyRateService.ConvertAsync(
                wallet.Balance,
                wallet.Currency,
                currency.ToUpper(),
                DateTime.UtcNow.Date
            );
            resultCurrency = currency.ToUpper();
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
    public async Task<IActionResult> AdjustBalance(long walletId, [FromQuery] decimal amount, [FromQuery] string currency, [FromQuery] string strategy)
    {
        var wallet = await _walletService.GetWalletAsync(walletId);
        if (wallet is null)
            return NotFound();

        var allowedCurrencies = _configuration.GetSection("AllowedCurrencies").Get<string[]>();
        if (string.IsNullOrWhiteSpace(currency) || !allowedCurrencies.Contains(currency.ToUpper()))
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

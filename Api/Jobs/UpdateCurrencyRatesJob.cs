using Adapter.ECB;
using Core.Services;
using Quartz;

namespace Api.Jobs;

public class UpdateCurrencyRatesJob(CurrencyRateService currencyRateService, ILoggerFactory loggerFactory) : IJob
{
    private readonly CurrencyRateService _currencyRateService = currencyRateService;
    private readonly ILogger _logger = loggerFactory.CreateLogger<UpdateCurrencyRatesJob>();
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await _currencyRateService.UpdateRatesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quartz job 'UpdateCurrencyRatesJob' failed while updating currency rates.");
        }
    }
}

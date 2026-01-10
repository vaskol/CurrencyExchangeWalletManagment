using Core.Services;
using Quartz;

namespace Api.Jobs;

public class UpdateCurrencyRatesJob(CurrencyRateService currencyRateService) : IJob
{
    private readonly CurrencyRateService _currencyRateService = currencyRateService;

    public async Task Execute(IJobExecutionContext context)
    {
        await _currencyRateService.UpdateRatesAsync();
    }
}

using Adapter.ECB;
using Core.Ports;
using Adapter.SQL.Repositories;
using Quartz;

namespace CronJobs
{
    public class EcbUpdateJob : IJob
    {
        private readonly ICurrencyRateProvider _currencyRateProvider;
        private readonly ICurrencyRateRepository _currencyRateRepository;

        public EcbUpdateJob(
            ICurrencyRateProvider currencyRateProvider,
            ICurrencyRateRepository currencyRateRepository)
        {
            _currencyRateProvider = currencyRateProvider;
            _currencyRateRepository = currencyRateRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var rates = await _currencyRateProvider.GetLatestRatesAsync();
            await _currencyRateRepository.UpsertRatesAsync(rates, DateTime.UtcNow.Date);
        }
    }
}

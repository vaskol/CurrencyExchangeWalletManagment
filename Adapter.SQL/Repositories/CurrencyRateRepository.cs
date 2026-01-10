using Adapter.SQL.Data;
using Core.Entities;
using Core.Ports;
using Core.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Adapter.SQL.Repositories;

public class CurrencyRateRepository(AppDbContext dbContext, ILoggerFactory loggerFactory) : ICurrencyRateRepository
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger _logger = loggerFactory.CreateLogger<CurrencyRateRepository>();
    public async Task<int> UpsertRatesAsync(List<CurrencyRate> rates)
    {
        int totalRecordsAffected = 0;
        if (rates is null || !rates.Any())
            return 0;
        try
        {
            var parameters = new List<SqlParameter>();
            var valueStrings = new List<string>();

            for (int i = 0; i < rates.Count; i++)
            {
                var r = rates[i];
                valueStrings.Add($"(@currency{i}, @rate{i}, @rateDate{i})");

                parameters.AddRange(new[]
                {
                new SqlParameter($"@currency{i}", r.Currency),
                new SqlParameter($"@rate{i}", r.Rate),
                new SqlParameter($"@rateDate{i}", r.RateDate),
                //new SqlParameter($"@ratePretty{i}", NumberFormattingHelper.ToPrettyAmount(r.Rate)) IF we want
            });
            }

            var valuesClause = string.Join(", ", valueStrings);

            var sql = $@"
            DECLARE @OutputTable TABLE (AffectedRows INT);
            MERGE INTO CurrencyRate AS target
            USING (VALUES {valuesClause}) AS source (Currency, Rate, RateDate)
            ON target.Currency = source.Currency
               AND target.RateDate = source.RateDate
            
            WHEN MATCHED
                 AND target.Rate <> source.Rate
            THEN
                UPDATE SET Rate = source.Rate
            
            WHEN NOT MATCHED THEN
                INSERT (Currency, Rate, RateDate)
                VALUES (source.Currency, source.Rate, source.RateDate)
            
            OUTPUT 1 INTO @OutputTable;
            
            SELECT COUNT(*) FROM @OutputTable;
            ";

            totalRecordsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
            return totalRecordsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert rates in the database");
            throw;
        }
    }

    public async Task<decimal?> GetLatestRateAsync(string currencyCode, DateTime targetDate)
    {
        return await _dbContext.CurrencyRates
            .Where(r => r.Currency == currencyCode && r.RateDate <= targetDate)
            .OrderByDescending(r => r.RateDate)
            .Select(r => (decimal?)r.Rate) 
            .FirstOrDefaultAsync();
    }

}

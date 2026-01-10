using Adapter.ECB;
using Adapter.Redis;
using Adapter.SQL.Data;
using Adapter.SQL.Repositories;
using Api.Jobs;
using Core.Ports;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "CurrencyRates:";
});
builder.Services.AddScoped<ICurrencyRateCache, CurrencyRateRedisCache>();

//Quarz
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("UpdateCurrencyRatesJob");

    q.AddJob<UpdateCurrencyRatesJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("UpdateCurrencyRatesJob-trigger")
        .WithSimpleSchedule(x =>
            x.WithInterval(TimeSpan.FromMinutes(1))
             .RepeatForever()));
});

builder.Services.AddQuartzHostedService(q => { q.WaitForJobsToComplete = true; });

// HTTP client for ECB
builder.Services.AddHttpClient<ICurrencyRateProvider, EcbCurrencyRateProvider>();

// Repositories
builder.Services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

// Services
builder.Services.AddScoped<CurrencyRateService>();
builder.Services.AddScoped<WalletService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockPriceCalculator.Domain.Abstractions;
using StockPriceCalculator.Domain.Stocks.Interfaces;
using StockPriceCalculator.Infrastructure.Persistence;
using StockPriceCalculator.Infrastructure.Repositories;
using StockPriceCalculator.Infrastructure.Services;
using StockPriceCalculator.Infrastructure.Time;

namespace StockPriceCalculator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        services.AddDbContext<StockDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        // Repo
        services.AddScoped<IStockPriceRepository, StockPriceRepository>();
        services.AddScoped<IStockValuationRecordRepository, StockValuationRecordRepository>();
        services.AddScoped<IStockInfoRepository, StockInfoRepository>();

        // Service
        services.AddScoped<StockListImportService>();


        services.AddSingleton<IClock, SystemClock>();

        // TWSE API
        services.AddHttpClient("TWSEAPI", client =>
        {
            var baseUrl = configuration["TwseApi:BaseUrl"] ?? throw new InvalidOperationException("TWSE BaseUrl not configured");
            client.BaseAddress = new Uri(baseUrl);
        });

        // TWSE internal
        services.AddHttpClient("TWSE_internal", client =>
        {
            var baseUrl = configuration["Twse:BaseUrl"] ?? throw new InvalidOperationException("TWSE BaseUrl not configured");
            client.BaseAddress = new Uri(baseUrl);
        });

        // TPEX API
        services.AddHttpClient("TPEXAPI", client =>
        {
            var baseUrl = configuration["TpexApi:BaseUrl"] ?? throw new InvalidOperationException("TPEX BaseUrl not configured");
            client.BaseAddress = new Uri(baseUrl);
        });

        // TPEX internal
        services.AddHttpClient("TPEX_internal", client =>
        {
            var baseUrl = configuration["Tpex:BaseUrl"] ?? throw new InvalidOperationException("TWSE BaseUrl not configured");
            client.BaseAddress = new Uri(baseUrl);
        });

        // TPEX internal
        services.AddHttpClient("Isin.twse", client =>
        {
            var baseUrl = configuration["Isin.twse:BaseUrl"] ?? throw new InvalidOperationException("Isin.twse BaseUrl not configured");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddScoped<IMarketDataProvider, MarketDataProvider>();

        return services;
    }
}

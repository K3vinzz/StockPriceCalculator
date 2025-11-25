using System.Text;
using StockPriceCalculator.Application.Stocks.Handlers;
using StockPriceCalculator.Infrastructure;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPages", policy =>
    {
        policy.WithOrigins(
                "https://stockpricecalculator.kevincheng401.workers.dev"    // Cloudflare Pages 自動給的 domain
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);

// Handler
builder.Services.AddScoped<CalculateSettlementHandler>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowPages");

app.UseAuthorization();

app.MapControllers();

app.Run();

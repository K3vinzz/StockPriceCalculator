using Microsoft.EntityFrameworkCore;
using StockPriceCalculator.Infrastructure.Persistence.Entities;

namespace StockPriceCalculator.Infrastructure.Persistence;

public partial class StockDbContext : DbContext
{
    public StockDbContext()
    {
    }

    public StockDbContext(DbContextOptions<StockDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DailyStockPriceEntity> DailyStockPrices { get; set; }

    public virtual DbSet<StockEntity> Stocks { get; set; }

    public virtual DbSet<StockValuationRecordEntity> StockValuationRecords { get; set; }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=stockdb;Username=admin;Password=admin123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyStockPriceEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("daily_stock_prices_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Market).HasDefaultValueSql("'TWSE'::character varying");

            entity.HasIndex(e => new { e.Market, e.Symbol, e.TradeDate })
              .IsUnique();
        });

        modelBuilder.Entity<StockEntity>(entity =>
        {
            entity.HasKey(e => e.Symbol).HasName("stocks_pkey");
        });

        modelBuilder.Entity<StockValuationRecordEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stock_valuation_records_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

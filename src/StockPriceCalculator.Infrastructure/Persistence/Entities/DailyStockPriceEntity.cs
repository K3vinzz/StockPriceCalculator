using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StockPriceCalculator.Infrastructure.Persistence.Entities;

[Table("daily_stock_prices")]
[Index("Market", Name = "ix_daily_stock_prices_market")]
[Index("Market", "Symbol", "TradeDate", Name = "ux_daily_stock_prices_market_symbol_date", IsUnique = true)]
[Index("Symbol", "TradeDate", Name = "ux_daily_stock_prices_symbol_date", IsUnique = true)]
public partial class DailyStockPriceEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("symbol")]
    [StringLength(16)]
    public string Symbol { get; set; } = null!;

    [Column("trade_date")]
    public DateOnly TradeDate { get; set; }

    [Column("open_price")]
    [Precision(18, 4)]
    public decimal? OpenPrice { get; set; }

    [Column("high_price")]
    [Precision(18, 4)]
    public decimal? HighPrice { get; set; }

    [Column("low_price")]
    [Precision(18, 4)]
    public decimal? LowPrice { get; set; }

    [Column("close_price")]
    [Precision(18, 4)]
    public decimal ClosePrice { get; set; }

    [Column("volume")]
    public long? Volume { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("market")]
    [StringLength(8)]
    public string Market { get; set; } = null!;

    [Column("name")]
    [StringLength(64)]
    public string Name { get; set; } = null!;
}

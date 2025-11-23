using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StockPriceCalculator.Infrastructure.Persistence.Entities;

[Table("stock_valuation_records")]
[Index("Symbol", "CreatedAt", Name = "ix_stock_valuation_records_symbol_created_at", IsDescending = new[] { false, true })]
public partial class StockValuationRecordEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("symbol")]
    [StringLength(16)]
    public string Symbol { get; set; } = null!;

    [Column("trade_date")]
    public DateOnly TradeDate { get; set; }

    [Column("shares")]
    public int Shares { get; set; }

    [Column("close_price")]
    [Precision(18, 4)]
    public decimal ClosePrice { get; set; }

    [Column("total_amount")]
    [Precision(18, 4)]
    public decimal TotalAmount { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [Column("user_id")]
    [StringLength(64)]
    public string? UserId { get; set; }
}

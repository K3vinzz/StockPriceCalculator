using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPriceCalculator.Infrastructure.Persistence.Entities;

[Table("stocks")]
public partial class StockEntity
{
    [Key]
    [Column("symbol")]
    [StringLength(16)]
    public string Symbol { get; set; } = null!;

    [Column("name")]
    [StringLength(64)]
    public string Name { get; set; } = null!;

    [Column("market")]
    [StringLength(8)]
    public string Market { get; set; } = null!;
}

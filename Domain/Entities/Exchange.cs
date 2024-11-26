namespace Domain.Entities;
public class Exchange
{
    public int ExchangeId { get; set; }
    public string ExchangeName { get; set; }
    public ICollection<LiquidityPool> LiquidityPools { get; set; }
}
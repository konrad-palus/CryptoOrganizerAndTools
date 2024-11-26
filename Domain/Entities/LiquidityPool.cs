namespace Domain.Entities;
public class LiquidityPool
{
    //public int LiquidityPoolId { get; set; }
    //public Token Token { get; set; }
    public string TokenTicker { get; set; }
    //public Exchange Exchange { get; set; }
    public string ExchangeName { get; set; }
    public decimal BuyPrice { get; set; }
    //public decimal SellPrice { get; set; }
    //public DateTime LastUpdated { get; set; }
}
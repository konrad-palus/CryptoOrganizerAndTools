namespace CryptoOrganizerWebAPI.Models.DTO;

public class CoinGeckoTickerDto
{
    public CoinGeckoMarketDto Market { get; set; }
    public string Target { get; set; }
    public decimal? Last { get; set; }
}

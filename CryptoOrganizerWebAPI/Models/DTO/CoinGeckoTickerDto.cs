using Newtonsoft.Json;

namespace CryptoOrganizerWebAPI.Models.DTO;

public class CoinGeckoTickerDto
{
    public CoinGeckoMarketDto Market { get; set; }
    public string Target { get; set; }
    public decimal? Last { get; set; }
    [JsonProperty("trade_url")]
    public string TradeUrl { get; set; }
    [JsonProperty("last_fetch_at")]
    public string LastFetchAt { get; set; }
}

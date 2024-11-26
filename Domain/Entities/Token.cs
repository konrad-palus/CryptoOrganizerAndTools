using Domain.Entities.User;

namespace Domain.Entities;
public class Token
{
    public int TokenId { get; set; }
    public string TokenName { get; set; }
    public string Ticker { get; set; }
    //public string IconUrl { get; set; }
    public string Slug { get; set; }
    public ICollection<CryptoOrganizerUser> Users { get; set; }
    //public ICollection<TokenProvider> Providers { get; set; }
    //public ICollection<LiquidityPool> LiquidityPools { get; set; }
}
using Domain.Entities;

namespace Domain;
public class TokenProvider
{
    public int Id { get; set; }
    public int TokenId { get; set; }
    public string ProviderName { get; set; }
    public string ProviderTokenId { get; set; }
    public Token Token { get; set; }
}

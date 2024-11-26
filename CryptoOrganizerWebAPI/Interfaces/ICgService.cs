using Domain.Entities;

namespace CryptoOrganizerWebAPI.Interfaces;

public interface ICgService
{
    Task FetchLiquidityPoolsFromCoinGecko();
    Task FetchTokensFromCoinGecko(int limit = 1, string slug = "bitcoin");
}
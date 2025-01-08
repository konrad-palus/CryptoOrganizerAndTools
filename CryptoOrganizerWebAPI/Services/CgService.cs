using Application.Interfaces;
using CryptoOrganizerWebAPI.Interfaces;
using CryptoOrganizerWebAPI.Models.DTO;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace CryptoOrganizerWebAPI.Services;

public class CgService(ICacheService cache, IConfiguration configuration, ILogger<CgService> logger, ICryptoOrganizerContext context) : ICgService
{
    private const string TokensCacheKey = "TokensCacheKey";
    private const string LiquidityPoolsCacheKey = "LiquidityPoolsCacheKey";

    public async Task FetchTokensFromCoinGecko(int limit, string slug)
    {
        try
        {
            var newTokens = new List<Token>();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var apiUrl = $"{configuration["CoinGecko:BaseUrl"]}/list";
            logger.LogInformation("Fetching tokens from CoinGecko with URL: {ApiUrl}", apiUrl);

            var response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                logger.LogWarning("Failed to fetch tokens. Status code: {StatusCode}. Response: {Response}",
                    response.StatusCode, responseContent);
                return;
            }

            var responseContentSuccess = await response.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<List<CoinGeckoTokenDto>>(responseContentSuccess);

            if (!string.IsNullOrEmpty(slug))
            {
                var token = tokenData.FirstOrDefault(t => t.Id.Equals(slug, StringComparison.OrdinalIgnoreCase));
                if (token != null)
                {
                    newTokens.Add(new Token
                    {
                        TokenName = token.Name,
                        Ticker = token.Symbol,
                        Slug = token.Id
                    });
                }
            }
            else
            {
                foreach (var token in tokenData.Take(limit))
                {
                    newTokens.Add(new Token
                    {
                        TokenName = token.Name,
                        Ticker = token.Symbol,
                        Slug = token.Id
                    });
                }
            }

            var existingTokens = await context.Tokens.ToListAsync();

            foreach (var token in newTokens)
            {
                var existingToken = existingTokens.FirstOrDefault(t => t.Slug == token.Slug);

                if (existingToken != null)
                {
                    existingToken.TokenName = token.TokenName;
                    existingToken.Ticker = token.Ticker;
                }
                else
                {
                    context.Tokens.Add(token);
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Tokens updated in the database.");

            var allTokensFromDb = await context.Tokens.ToListAsync();

            var tokenDtos = allTokensFromDb
                .GroupBy(t => t.Slug)
                .Select(group => group.First())
                .Select(t => new TokenCacheDto
                {
                    TokenId = t.TokenId,
                    TokenName = t.TokenName,
                    Ticker = t.Ticker,
                    Slug = t.Slug
                }).ToList();

            cache.Set(TokensCacheKey, tokenDtos, TimeSpan.FromDays(1));
            logger.LogInformation("Tokens cached successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching tokens from CoinGecko.");
        }
    }

    public async Task FetchLiquidityPoolsFromCoinGecko()
    {
        try
        {
            var tokens = cache.Get<List<TokenCacheDto>>(TokensCacheKey);

            if (tokens == null || tokens.Count == 0)
            {
                logger.LogWarning("No tokens found in cache.");
                return;
            }

            var liquidityPools = new List<LiquidityPool>();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            foreach (var token in tokens)
            {
                var apiUrl = $"{configuration["CoinGecko:BaseUrl"]}/{token.Slug}/tickers";
                logger.LogInformation("Fetching liquidity pools for {TokenTicker} with URL: {ApiUrl}", token.Ticker, apiUrl);

                var response = await httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("Failed to fetch liquidity pools for token {TokenTicker}. Status code: {StatusCode}. Response: {Response}",
                        token.Ticker, response.StatusCode, responseContent);
                    continue;
                }

                var responseContentSuccess = await response.Content.ReadAsStringAsync();
                var marketPairsResponse = JsonConvert.DeserializeObject<CoinGeckoMarketResponseDto>(responseContentSuccess);

                if (marketPairsResponse?.Tickers != null)
                {
                    foreach (var ticker in marketPairsResponse.Tickers.Where(t => t.Target == "USDT"))
                    {
                        liquidityPools.Add(new LiquidityPool
                        {
                            TokenTicker = token.Ticker,
                            ExchangeName = ticker.Market.Name,
                            BuyPrice = ticker.Last ?? 0
                        });
                    }
                }
            }

            cache.Set(LiquidityPoolsCacheKey, liquidityPools, TimeSpan.FromMinutes(30));
            logger.LogInformation("Liquidity pools cached successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching liquidity pools.");
        }
    }
}
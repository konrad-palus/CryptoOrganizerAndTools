using CryptoOrganizerWebAPI.Interfaces;
using CryptoOrganizerWebAPI.Models.DTO;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOrganizerWebAPI.Controllers
{
    [ApiController]
    [Route("api/cache")]
    public class CacheController(ICacheService cache, ILogger<CacheController> logger) : ControllerBase
    {
        private const string TokensCacheKey = "TokensCacheKey";
        private const string LiquidityPoolsCacheKey = "LiquidityPoolsCacheKey";

        [HttpGet("Tokens")]
        public IActionResult GetCachedTokens()
        {
            var tokens = cache.Get<List<TokenCacheDto>>(TokensCacheKey);
            if (tokens == null || tokens.Count == 0)
            {
                logger.LogWarning("No tokens found in cache.");
                return NotFound(new { Message = "No tokens found in cache." });
            }
            return Ok(tokens);
        }

        [HttpGet("LiquidityPools")]
        public IActionResult GetCachedLiquidityPools()
        {
            var liquidityPools = cache.Get<List<LiquidityPool>>(LiquidityPoolsCacheKey);
            if (liquidityPools == null || liquidityPools.Count == 0)
            {
                logger.LogWarning("No liquidity pools found in cache.");
                return NotFound(new { Message = "No liquidity pools found in cache." });
            }
            return Ok(liquidityPools);
        }
    }
}

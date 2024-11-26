using CryptoOrganizerWebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOrganizerWebAPI.Controllers
{
    [ApiController]
    [Route("api/coingecko")]
    public class CgController(ICgService coinGeckoService, ILogger<CgController> logger) : ControllerBase
    {
        [HttpPost("TokenList")]
        public async Task<IActionResult> PopulateTokens([FromQuery] int limit = 5, [FromQuery] string? slug = "bitcoin")
        {
            try
            {
                await coinGeckoService.FetchTokensFromCoinGecko(limit, slug);
                logger.LogInformation("Tokens fetched and stored successfully.");

                return Ok(new { Message = "Tokens fetched and stored successfully in cache and database." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching tokens.");
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("LiquidityPools")]
        public async Task<IActionResult> PopulateLiquidityPools()
        {
            try
            {
                await coinGeckoService.FetchLiquidityPoolsFromCoinGecko();
                logger.LogInformation("Liquidity pools fetched and stored successfully.");

                return Ok(new { Message = "Liquidity pools fetched and stored successfully in cache." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching liquidity pools from CoinGecko.");
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}

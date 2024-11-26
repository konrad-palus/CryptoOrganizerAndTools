using CryptoOrganizerWebAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CryptoOrganizerWebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user")]
    public class UserController(IUserService userService, ILogger<UserController> logger) : ControllerBase
    {
        [HttpPost("AddTokenToFavourites/{tokenId}")]
        public async Task<IActionResult> AddFavoriteToken(int tokenId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                logger.LogWarning("Unauthorized access attempt to AddFavoriteToken.");
                return Unauthorized();
            }

            logger.LogInformation("User {UserId} is attempting to add token {TokenId} to favorites.", userId, tokenId);

            var success = await userService.AddFavoriteTokenAsync(userId, tokenId);
            if (!success)
            {
                logger.LogWarning("Failed to add token {TokenId} to favorites for user {UserId}.", tokenId, userId);
                return BadRequest(new { Message = "Failed to add token to favorites." });
            }

            logger.LogInformation("Token {TokenId} successfully added to favorites for user {UserId}.", tokenId, userId);
            return Ok(new { Message = "Token added to favorites." });
        }

        [HttpDelete("DeleteTokenFromFavourites/{tokenId}")]
        public async Task<IActionResult> RemoveFavoriteToken(int tokenId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                logger.LogWarning("Unauthorized access attempt to RemoveFavoriteToken.");
                return Unauthorized();
            }

            logger.LogInformation("User {UserId} is attempting to remove token {TokenId} from favorites.", userId, tokenId);

            var success = await userService.RemoveFavoriteTokenAsync(userId, tokenId);
            if (!success)
            {
                logger.LogWarning("Failed to remove token {TokenId} from favorites for user {UserId}.", tokenId, userId);
                return BadRequest(new { Message = "Failed to remove token from favorites." });
            }

            logger.LogInformation("Token {TokenId} successfully removed from favorites for user {UserId}.", tokenId, userId);
            return Ok(new { Message = "Token removed from favorites." });
        }

        [HttpGet("GetTokenFavouritesList")]
        public async Task<IActionResult> GetFavoriteTokens()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                logger.LogWarning("Unauthorized access attempt to GetFavoriteTokens.");
                return Unauthorized();
            }

            logger.LogInformation("Fetching favorite tokens for user {UserId}.", userId);

            var tokens = await userService.GetFavoriteTokensAsync(userId);

            if (tokens == null || tokens.Count == 0)
            {
                logger.LogWarning("No favorite tokens found for user {UserId}.", userId);
                return NotFound(new { Message = "No favorite tokens found." });
            }

            logger.LogInformation("{TokenCount} favorite tokens found for user {UserId}.", tokens.Count, userId);
            return Ok(tokens);
        }

        [HttpPost("CalculateAndSendNotifications")]
        public async Task<IActionResult> CalculateAndSendNotifications()
        {
            try
            {
                logger.LogInformation("Invoking arbitrage notification service.");

                await userService.NotifyArbitrageOpportunitiesAsync();

                logger.LogInformation("Arbitrage notification service completed successfully.");
                return Ok(new { Message = "Arbitrage notifications calculated and sent successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while calculating and sending arbitrage notifications.");
                return StatusCode(500, new { Message = "An error occurred while calculating and sending arbitrage notifications." });
            }
        }

        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                logger.LogInformation("Fetching notifications for the current user.");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    logger.LogWarning("Unauthorized access attempt to GetNotifications.");
                    return Unauthorized(new { Message = "User is not authorized." });
                }

                var notifications = await userService.GetNotificationsAsync(userId);

                if (notifications == null || notifications.Count == 0)
                {
                    logger.LogInformation("No notifications found for user {UserId}.", userId);
                    return NotFound(new { Message = "No notifications available." });
                }

                logger.LogInformation("{NotificationCount} notifications fetched for user {UserId}.", notifications.Count, userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching notifications.");
                return StatusCode(500, new { Message = "An error occurred while fetching notifications." });
            }
        }

        [HttpPatch("MarkNotificationAsRead/{notificationId}")]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                logger.LogWarning("Unauthorized access attempt to MarkNotificationAsRead.");
                return Unauthorized();
            }

            logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}.", notificationId, userId);

            var success = await userService.MarkNotificationAsReadAsync(userId, notificationId);
            if (!success)
            {
                logger.LogWarning("Failed to mark notification {NotificationId} as read for user {UserId}.", notificationId, userId);
                return NotFound(new { Message = "Notification not found or not associated with this user." });
            }

            return Ok(new { Message = "Notification marked as read." });
        }
    }
}
using CryptoOrganizerWebAPI.Interfaces;
using CryptoOrganizerWebAPI.Models.DTO;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace CryptoOrganizerWebAPI.Services
{
    public class UserService(CryptoOrganizerContext context, ILogger<UserService> logger, ICacheService cache, IEmailService emailService) : IUserService
    {
        private const string LiquidityPoolsCacheKey = "LiquidityPoolsCacheKey";

        public async Task<bool> AddFavoriteTokenAsync(string userId, int tokenId)
        {
            logger.LogInformation("Attempting to add favorite token. UserId: {UserId}, TokenId: {TokenId}", userId, tokenId);

            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                logger.LogWarning("User not found. UserId: {UserId}", userId);
                return false;
            }

            var token = await context.Tokens.FirstOrDefaultAsync(t => t.TokenId == tokenId);
            if (token == null)
            {
                logger.LogWarning("Token not found. TokenId: {TokenId}", tokenId);
                return false;
            }

            if (!user.FavoriteTokens.Any(t => t.TokenId == tokenId))
            {
                user.FavoriteTokens.Add(token);
                await context.SaveChangesAsync();
                logger.LogInformation("Token added to favorites. UserId: {UserId}, TokenId: {TokenId}", userId, tokenId);
            }
            else
            {
                logger.LogInformation("Token already in favorites. UserId: {UserId}, TokenId: {TokenId}", userId, tokenId);
            }

            return true;
        }

        public async Task<bool> RemoveFavoriteTokenAsync(string userId, int tokenId)
        {
            logger.LogInformation("Attempting to remove favorite token. UserId: {UserId}, TokenId: {TokenId}", userId, tokenId);

            var user = await context.Users.Include(u => u.FavoriteTokens).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                logger.LogWarning("User not found. UserId: {UserId}", userId);
                return false;
            }

            var token = user.FavoriteTokens.FirstOrDefault(t => t.TokenId == tokenId);
            if (token == null)
            {
                logger.LogWarning("Token not found in user's favorites. UserId: {UserId}, TokenId: {TokenId}", userId, tokenId);
                return false;
            }

            user.FavoriteTokens.Remove(token);
            await context.SaveChangesAsync();

            logger.LogInformation("Token removed from favorites. UserId: {UserId}, TokenId: {TokenId}", userId, tokenId);

            return true;
        }

        public async Task<List<FavoriteTokenDto>> GetFavoriteTokensAsync(string userId)
        {
            try
            {
                logger.LogInformation("Fetching favorite tokens for user {UserId}.", userId);

                var user = await context.Users.Include(u => u.FavoriteTokens).FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    logger.LogWarning("User {UserId} not found.", userId);

                    return [];
                }

                if (user.FavoriteTokens == null || user.FavoriteTokens.Count == 0)
                {
                    logger.LogWarning("No favorite tokens found for user {UserId}.", userId);

                    return [];
                }

                var tokenDtos = user.FavoriteTokens.Select(token => new FavoriteTokenDto { TokenName = token.TokenName, Ticker = token.Ticker, Slug = token.Slug }).ToList();

                logger.LogInformation("Fetched {TokenCount} favorite tokens for user {UserId}.", tokenDtos.Count, userId);

                return tokenDtos;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching favorite tokens for user {UserId}.", userId);

                return [];
            }
        }

        public async Task NotifyArbitrageOpportunitiesAsync()
        {
            try
            {
                logger.LogInformation("Starting arbitrage calculation.");

                var liquidityPools = cache.Get<List<LiquidityPool>>(LiquidityPoolsCacheKey);

                if (liquidityPools == null || liquidityPools.Count == 0)
                {
                    logger.LogWarning("No liquidity pools available in cache.");
                    return;
                }

                var userNotifications = new Dictionary<string, List<string>>();

                foreach (var group in liquidityPools.GroupBy(lp => lp.TokenTicker))
                {
                    if (group.Count() < 2)
                    {
                        logger.LogInformation("Skipping token {TokenTicker} because it has less than 2 liquidity pools.", group.Key);
                        continue;
                    }

                    var tokenTicker = group.Key;
                    var bestBuy = group.OrderBy(lp => lp.BuyPrice).FirstOrDefault();
                    var bestSell = group.OrderByDescending(lp => lp.BuyPrice).FirstOrDefault();

                    if (bestBuy == null || bestSell == null || bestBuy.BuyPrice >= bestSell.BuyPrice)
                    {
                        logger.LogInformation("No arbitrage opportunities found for token {TokenTicker}.", tokenTicker);
                        continue;
                    }

                    var arbitrageProfit = (bestSell.BuyPrice - bestBuy.BuyPrice) / bestBuy.BuyPrice * 100;

                    var message = $"Arbitrage opportunity for <b>{tokenTicker}</b>: Buy on <b>{bestBuy.ExchangeName}</b> at <b>{bestBuy.BuyPrice:N2}$</b> " +
                                  $"and sell on <b>{bestSell.ExchangeName}</b> at <b>{bestSell.BuyPrice:N2}$</b>. Potential profit: <b>{arbitrageProfit:F2}%</b>.";

                    logger.LogInformation("Arbitrage opportunity: {Message}", message);

                    var users = await context.Users.Include(u => u.FavoriteTokens)
                                                   .Where(u => u.FavoriteTokens.Any(t => t.Ticker == tokenTicker))
                                                   .ToListAsync();

                    if (users.Count == 0)
                    {
                        logger.LogInformation("No users have {TokenTicker} in favorites.", tokenTicker);
                        continue;
                    }

                    foreach (var user in users)
                    {
                        if (!userNotifications.TryGetValue(user.Id, out List<string>? value))
                        {
                            value = [];
                            userNotifications[user.Id] = value;
                        }

                        value.Add(message);

                        var notification = new Notification
                        {
                            Ticker = bestBuy.TokenTicker,
                            Message = message,
                            CreatedAt = DateTime.UtcNow,
                            IsChecked = false,
                            Users = [user]
                        };
                        context.Notifications.Add(notification);
                        logger.LogInformation("Notification created for user {UserId} regarding {TokenTicker}.", user.Id, tokenTicker);
                    }
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Arbitrage notifications saved successfully.");

                foreach (var userNotification in userNotifications)
                {
                    var userId = userNotification.Key;
                    var messages = userNotification.Value;

                    var user = await context.Users.FindAsync(userId);
                    if (user == null)
                    {
                        logger.LogWarning("User {UserId} not found when sending emails.", userId);
                        continue;
                    }

                    try
                    {
                        const string emailSubject = "Arbitrage Opportunities Notification";
                        var emailBody = $"""
                    <h3>Dear {user.UserName},</h3>
                    <p>We found the following arbitrage opportunities for you:</p>
                    <ul>
                        {string.Join("\n", messages.Select(m => $"<li>{m}</li>"))}
                    </ul>
                    <p>Take advantage of these opportunities before they disappear!</p>
                    """;

                        await emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
                        logger.LogInformation("Email sent to user {UserId}.", userId);
                    }
                    catch (Exception emailEx)
                    {
                        logger.LogError(emailEx, "Failed to send email to user {UserId}.", userId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while calculating arbitrage opportunities.");
            }
        }

        public async Task<List<NotificationDTO>> GetNotificationsAsync(string userId)
        {
            logger.LogInformation("Fetching notifications for user {UserId}.", userId);

            var user = await context.Users
                .Include(u => u.Notifications)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                logger.LogWarning("User {UserId} not found.", userId);
                return [];
            }

            var notifications = user.Notifications
                .Where(n => !n.IsChecked) 
                .Select(n => new NotificationDTO
                {
                    Id = n.NotificationId,
                    Ticker = n.Ticker,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsChecked = n.IsChecked
                })
                .ToList();

            logger.LogInformation("{NotificationCount} notifications fetched for user {UserId}.", notifications.Count, userId);

            return notifications;
        }

        public async Task<bool> MarkNotificationAsReadAsync(string userId, int notificationId)
        {
            var notification = await context.Notifications
                .Include(n => n.Users)
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.Users.Any(u => u.Id == userId));

            if (notification == null)
            {
                logger.LogWarning("Notification {NotificationId} not found for user {UserId}.", notificationId, userId);
                return false;
            }

            notification.IsChecked = true;
            await context.SaveChangesAsync();
            logger.LogInformation("Notification {NotificationId} marked as read for user {UserId}.", notificationId, userId);
            return true;
        }
    }
}
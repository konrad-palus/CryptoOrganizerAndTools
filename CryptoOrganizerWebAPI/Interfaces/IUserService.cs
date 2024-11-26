using CryptoOrganizerWebAPI.Models.DTO;

namespace CryptoOrganizerWebAPI.Interfaces;

public interface IUserService
{
    Task<bool> AddFavoriteTokenAsync(string userId, int tokenId);
    Task<bool> RemoveFavoriteTokenAsync(string userId, int tokenId);
    Task<List<FavoriteTokenDto>> GetFavoriteTokensAsync(string userId);
    Task NotifyArbitrageOpportunitiesAsync();
    Task<List<NotificationDTO>> GetNotificationsAsync(string userId);
    Task<bool> MarkNotificationAsReadAsync(string userId, int notificationId);
}
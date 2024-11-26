using Microsoft.AspNetCore.Identity;
namespace Domain.Entities.User
{
    public class CryptoOrganizerUser : IdentityUser
    {
        public ICollection<Token> FavoriteTokens { get; set; } = [];
        public ICollection<Notification> Notifications { get; set; } = [];
    }
}
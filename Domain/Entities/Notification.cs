using Domain.Entities.User;

namespace Domain.Entities;
public class Notification
{
    public int NotificationId { get; set; }
    public string Ticker { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<CryptoOrganizerUser> Users { get; set; }
    public bool IsChecked { get; set; }
}
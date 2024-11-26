namespace CryptoOrganizerWebAPI.Models.DTO;

public class NotificationDTO
{
    public int Id { get; set; }
    public string Ticker { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsChecked { get; set; }
}
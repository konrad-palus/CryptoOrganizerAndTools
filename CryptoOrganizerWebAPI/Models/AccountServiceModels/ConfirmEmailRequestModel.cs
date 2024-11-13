using System.ComponentModel.DataAnnotations;
namespace CryptoOrganizerWebAPI.Models.AccountServiceModels;

public class ConfirmEmailRequestModel
{
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Token { get; set; }
}
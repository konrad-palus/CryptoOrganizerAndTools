using System.ComponentModel.DataAnnotations;
namespace CryptoOrganizerWebAPI.Models.AccountServiceModels;

public class ResetPasswordRequestModel
{
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Token { get; set; }
    [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; }
}
using System.ComponentModel.DataAnnotations;
namespace CryptoOrganizerWebAPI.Models.AccountServiceModels;

public class LoginRequestModel
{
    [Required]
    public string Login { get; set; }
    [Required]
    public string Password { get; set; }
}
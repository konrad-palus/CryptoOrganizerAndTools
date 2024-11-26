using System.ComponentModel.DataAnnotations;
namespace CryptoOrganizerWebAPI.Models.AccountServiceModels;

public class ResetPasswordRequestModel
{
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}
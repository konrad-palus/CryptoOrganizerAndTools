using System.ComponentModel.DataAnnotations;
namespace CryptoOrganizerWebAPI.Models.AccountServiceModels;

public class ForgotPasswordRequestModel
{
        [Required, EmailAddress]
        public string Email { get; set; }
}
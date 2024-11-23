using CryptoOrganizerWebAPI.Models.AccountServiceModels;
using CryptoOrganizerWebAPI.Models.Responses;
namespace CryptoOrganizerWebAPI.Interfaces;

public interface IAccountService
{
    Task<ServiceResult> RegisterUserAsync(RegistrationRequestModel registrationRequest);
    Task<ServiceResult> LoginUserAsync(LoginRequestModel loginRequest);
    Task<ServiceResult> ConfirmUserEmailAsync(ConfirmEmailRequestModel confirmEmailRequest);
    Task<ServiceResult> ResetPasswordAsync(string email, string token, ResetPasswordRequestModel resetPasswordRequest);
    Task<ServiceResult> ForgotPasswordAsync(ForgotPasswordRequestModel forgotPasswordRequest);
}
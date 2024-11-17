using CryptoOrganizerWebAPI.Models.AccountServiceModels;
using CryptoOrganizerWebAPI.Models.Responses;
namespace CryptoOrganizerWebAPI.Interfaces;

public interface IAccountService
{
    Task<ServiceResult> RegisterUserAsync(RegistrationRequestModel registrationRequest);
    Task<ServiceResult> LoginUserAsync(LoginRequestModel loginRequest);
    Task<ServiceResult> ConfirmUserEmailAsync(ConfirmEmailRequestModel confirmEmailRequest);
    Task<ServiceResult> ForgotPasswordAsync(ForgotPasswordRequestModel forgotPasswordRequest);
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequestModel resetPasswordRequest);
}
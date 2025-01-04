using CryptoOrganizerWebAPI.Interfaces;
using CryptoOrganizerWebAPI.Models.AccountServiceModels;
using CryptoOrganizerWebAPI.Models.Responses;
using Domain.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace CryptoOrganizerWebAPI.Services
{
    public class AccountService(UserManager<CryptoOrganizerUser> userManager, SignInManager<CryptoOrganizerUser> signInManager,
    IEmailService emailService, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, ILogger<AccountService> logger) : IAccountService
    {
        public async Task<ServiceResult> RegisterUserAsync(RegistrationRequestModel registrationRequest)
        {
            try
            {
                var user = new CryptoOrganizerUser { UserName = registrationRequest.Username, Email = registrationRequest.Email };
                var result = await userManager.CreateAsync(user, registrationRequest.Password);

                if (!result.Succeeded)
                {
                    logger.LogWarning("User registration failed for {Username}. Errors: {Errors}", registrationRequest.Username, string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
                    return new ServiceResult(false, "User registration failed", result.Errors.Select(e => $"{e.Code}: {e.Description}").ToList());
                }

                var confirmationLink = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext)
                                                       .Action("ConfirmEmail", "Account", new { email = user.Email, token = await userManager.GenerateEmailConfirmationTokenAsync(user) },
                                                                actionContextAccessor.ActionContext.HttpContext.Request.Scheme);

                await emailService.SendEmailAsync(user.Email, "Confirm your email", $"""<a href="{confirmationLink}">Click here to confirm your email</a>""");

                logger.LogInformation("User registered successfully for {Username}", registrationRequest.Username);

                return new ServiceResult(true, "Registration successful. Please check your email to confirm your account.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred during registration for {Username}", registrationRequest.Username);

                return new ServiceResult(false, "An unexpected error occurred during registration.");
            }
        }

        public async Task<ServiceResult> LoginUserAsync(LoginRequestModel loginRequest)
        {
            try
            {
                var user = await userManager.FindByNameAsync(loginRequest.Login) ?? await userManager.FindByEmailAsync(loginRequest.Login);

                if (user == null)
                {
                    logger.LogWarning("Invalid login attempt for {Login}. User not found.", loginRequest.Login);

                    return new ServiceResult(false, "Invalid login attempt. User not found.");
                }

                if (!await userManager.IsEmailConfirmedAsync(user))
                {
                    return new ServiceResult(false, "Email not confirmed. Please check your email for the confirmation link.");
                }

                var result = await signInManager.PasswordSignInAsync(user, loginRequest.Password, false, false);
                if (!result.Succeeded)
                {
                    logger.LogWarning("Invalid login attempt for {Login}. Incorrect password.", loginRequest.Login);

                    return new ServiceResult(false, "Invalid login attempt. Incorrect password.");
                }

                return new ServiceResult(true, "Login successful");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred during login for {Login}", loginRequest.Login);

                return new ServiceResult(false, "An unexpected error occurred during login.");
            }
        }

        public async Task<ServiceResult> ConfirmUserEmailAsync(ConfirmEmailRequestModel confirmEmailRequest)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(confirmEmailRequest.Email);
                if (user == null)
                {
                    logger.LogWarning("User not found for email confirmation {Email}", confirmEmailRequest.Email);

                    return new ServiceResult(false, "User not found");
                }

                var result = await userManager.ConfirmEmailAsync(user, confirmEmailRequest.Token);

                return result.Succeeded ? new ServiceResult(true, "Email confirmed successfully") : new ServiceResult(false, "Invalid email confirmation token");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred during email confirmation for {Email}", confirmEmailRequest.Email);

                return new ServiceResult(false, "An unexpected error occurred during email confirmation.");
            }
        }

        public async Task<ServiceResult> ForgotPasswordAsync(ForgotPasswordRequestModel forgotPasswordRequest)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(forgotPasswordRequest.Email);

                if (user != null)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                    var resetLink = $"http://localhost:4200/welcome/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

                    await emailService.SendEmailAsync(user.Email, "Reset Password", $"""<a href="{resetLink}">Click here to reset your password</a>""");

                    logger.LogInformation("Password reset link sent successfully to {Email}", forgotPasswordRequest.Email);
                }

                return new ServiceResult(true, "If an account with this email exists, a password reset link has been sent.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred during forgot password process for {Email}", forgotPasswordRequest.Email);

                return new ServiceResult(false, "An unexpected error occurred during forgot password process.");
            }
        }

        public async Task<ServiceResult> ResetPasswordAsync(string email, string token, ResetPasswordRequestModel resetPasswordRequest)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    logger.LogWarning("Reset password request failed. User not found for email: {Email}", email);
                    return new ServiceResult(false, "User not found.");
                }

                var result = await userManager.ResetPasswordAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)), resetPasswordRequest.Password);

                if (result.Succeeded)
                {
                    logger.LogInformation("Password reset successful for {Email}.", email);

                    return new ServiceResult(true, "Password reset successful.");
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("Password reset failed for {Email}: {Errors}", email, errors);

                return new ServiceResult(false, $"Password reset failed: {errors}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred during reset password process for {Email}.", email);

                return new ServiceResult(false, "An unexpected error occurred during reset password process.");
            }
        }
    }
}
using CryptoOrganizerWebAPI.Interfaces;
using CryptoOrganizerWebAPI.Models.AccountServiceModels;
using Microsoft.AspNetCore.Mvc;

namespace CryptoOrganizerWebAPI.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController(IAccountService accountService) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestModel registrationRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid registration request.");
            }

            try
            {
                var result = await accountService.RegisterUserAsync(registrationRequest);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid login request.");
            }

            try
            {
                var result = await accountService.LoginUserAsync(loginRequest);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return Unauthorized(result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                var confirmEmailRequest = new ConfirmEmailRequestModel
                {
                    Email = email,
                    Token = token
                };

                var result = await accountService.ConfirmUserEmailAsync(confirmEmailRequest);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel forgotPasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid forgot password request.");
            }

            try
            {
                var result = await accountService.ForgotPasswordAsync(forgotPasswordRequest);
                return Ok(new { Message = "If an account with this email exists, a password reset link has been sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsync([FromQuery] string email, [FromQuery] string token, [FromBody] ResetPasswordRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid reset password request." });
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(new { Message = "Has³o oraz potwierdzone has³o musz¹ byæ identyczne." });
            }

            try
            {
                var result = await accountService.ResetPasswordAsync(email, token, model);

                if (result.IsSuccess)
                {
                    return Ok(new { Message = "Password reset successful." });
                }

                return BadRequest(new { Message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}

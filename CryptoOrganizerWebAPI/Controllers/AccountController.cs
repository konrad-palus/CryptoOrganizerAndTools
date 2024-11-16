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

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return NotFound(result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel resetPasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid reset password request.");
            }

            try
            {
                var result = await accountService.ResetPasswordAsync(resetPasswordRequest);

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
    }
}

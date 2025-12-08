using Microsoft.AspNetCore.Mvc;
using UserService.Interface;
using UserService.Models.QueryDto;
using UserService.Models.ResponseDto;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthRepository _auth;

        public AccountController(IAuthRepository auth)
        {
            _auth = auth;
        }

        [HttpPost("user-login")]
        public async Task<IActionResult> UserLogin([FromBody] LoginRequest request)
        {
            if (request == null)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "UserId and Password are required"
                });
            }

            var response = await _auth.CheckLoginAsync(request);
            return Ok(new
            {
                response.Token,
                response.FullName,
                response.BankId,
                response.RoleId
            });
        }
    }
}

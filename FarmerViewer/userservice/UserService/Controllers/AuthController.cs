using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Cryptography;
using UserService.Helpers;
using UserService.Interface;
using UserService.Models.QueryDto;
using UserService.Models.ResponseDto;

namespace UserService.Controllers
{
    [Route("fv_user-service/api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _auth;
        public AuthController(IAuthRepository auth)
        {
            _auth = auth;
        }

        [HttpPost("UserLogin")]
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
            try
            {
                var dPassword = AesAlgorithm.DecryptString(request.Password);
                var password = dPassword.Substring(5, dPassword.Length - 10);
                var encPassword = AesAlgorithm.EncryptString(password);

                var response = await _auth.CheckLoginAsync(request.UserId, encPassword);
                return Ok(new
                {
                    response.Token,
                    response.FullName,
                    response.BankId,
                    response.RoleId
                });
            }
            catch
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "UserID and Password does not match"
                });
            }
        }
    }
}

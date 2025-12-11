using Microsoft.AspNetCore.Mvc;
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
        private readonly IUserSessionRepository _sessionRepo;

        public AuthController(IAuthRepository auth, IUserSessionRepository sessionRepo)
        {
            _auth = auth;
            _sessionRepo = sessionRepo;
        }

        // ---------------------------------------------------------------
        // LOGIN
        // ---------------------------------------------------------------
        [HttpPost("UserLogin")]
        public async Task<IActionResult> UserLogin([FromBody] LoginRequest request)
        {
            if (request == null)
                return BadRequest(new { success = false, message = "Invalid request" });

            if (request.DeviceFingerprint == null)
                return BadRequest(new { success = false, message = "DeviceFingerprint is required" });

            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { success = false, message = "UserId and Password are required" });

            try
            {
                // ----------------------------------------------------
                // 1) Decrypt password from frontend
                // ----------------------------------------------------
                var dPassword = AesAlgorithm.DecryptString(request.Password);
                var password = dPassword.Substring(5, dPassword.Length - 10);
                var encPassword = AesAlgorithm.EncryptString(password);

                // Hash the device fingerprint
                var dfpHash = HashHelper.ComputeSha256Hex(request.DeviceFingerprint);

                // ----------------------------------------------------
                // 1a) Check for existing active session for this user
                // ----------------------------------------------------
                var existingSession = await _sessionRepo.GetActiveSessionByUserAsync(Convert.ToInt64(request.UserId));

                if (existingSession != null && existingSession.DeviceFingerprint != dfpHash)
                {
                    // Option 1: Reject login from new device
                    return Unauthorized(new { success = false, message = "User already logged in from another device" });

                    // Option 2: Or, force logout old device and allow new login
                    // await _sessionRepo.LogoutByTokenAsync(existingSession.TokenHash);
                }

                // ----------------------------------------------------
                // 2) Validate login credentials
                // ----------------------------------------------------
                var response = await _auth.CheckLoginAsync(request.UserId, encPassword, dfpHash);

                if (response == null || string.IsNullOrEmpty(response.Token))
                    return BadRequest(new { success = false, message = "Invalid login" });

                // ----------------------------------------------------
                // 3) Session: create or update session for this device
                // ----------------------------------------------------
                await _sessionRepo.CreateOrUpdateSessionAsync(new UserSessionCommand
                {
                    UserId = response.UserId,
                    Token = response.Token,
                    DeviceFingerprint = request.DeviceFingerprint
                });

                // ----------------------------------------------------
                // 4) Return token only
                // ----------------------------------------------------
                return Ok(new { token = response.Token });
            }
            catch
            {
                return BadRequest(new { success = false, message = "UserID and Password do not match" });
            }
        }


        // ---------------------------------------------------------------
        // HEARTBEAT — called every 30/60 seconds from frontend
        // ---------------------------------------------------------------
        [HttpPost("Heartbeat")]
        public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return BadRequest(new { success = false, message = "Token missing" });

            var rows = await _sessionRepo.UpdateLastSeenByTokenAsync(request.Token);
            if (rows == 0)
                return Unauthorized(new { success = false, message = "Session expired or invalid" });

            return Ok(new { success = true });
        }

        // ---------------------------------------------------------------
        // LOGOUT
        // ---------------------------------------------------------------
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return BadRequest(new { success = false, message = "Token missing" });

            await _sessionRepo.LogoutByTokenAsync(request.Token);

            return Ok(new { success = true, message = "Logged out" });
        }
    }
}

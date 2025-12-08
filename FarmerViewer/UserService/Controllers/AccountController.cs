using Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Model;
using Model.Admin;
using Newtonsoft.Json.Linq;
using Repository.Entity;
using Repository.Interface;
using Service.Interface;
using System.Data;
using System.Net;
using System.Reflection;
using System.Text.Json;
using UserService.Helper;
using UserService.Model.Request;
using UserService.Model.Response;
using UserService.Service;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ExceptionLogging = Helper.ExceptionLogging;
using Utility = Helper.Utility;

namespace UserService.Controllers;
public class AccountController : APIBaseController
{
    private readonly APIUrl urloptions;
    private readonly ITokenHelper tokenHelper;
    private readonly IUserManagerService userManagerService;
    private readonly IHttpClientService clientService;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<ApplicationRole> roleManager;
    private readonly ICurrentUserService _currentUserService;

    private readonly ICacheService cacheService;
    private readonly IErrorLogger errorLogger;
    private readonly AppleSarkarCred appCredOptions;
    private readonly ISessionService sessionService;
    private readonly IBruteForceService bruteForceService;



    public AccountController(UserManager<ApplicationUser> userManager,
                              ITokenHelper tokenHelper,
                              IUserManagerService userManagerService,
                              IOptions<APIUrl> urloptions,
                              IHttpClientService clientService,
                              RoleManager<ApplicationRole> roleManager,
                              IOptions<AppleSarkarCred> AppCredOptions, ICacheService cacheService,
                              IErrorLogger errorLogger,
                              IHttpClientService httpClientService,
                              ISessionService sessionService,
                              ICurrentUserService _currentUserService,
                              IBruteForceService bruteForceService
                              )
    {
        this.urloptions = urloptions.Value;
        this.userManager = userManager;
        this.tokenHelper = tokenHelper;

        this.userManagerService = userManagerService;
        this.clientService = clientService;
        this.roleManager = roleManager;
        this.cacheService = cacheService;
        this.errorLogger = errorLogger;
        appCredOptions = AppCredOptions.Value;
        this.sessionService = sessionService;
        this.bruteForceService = bruteForceService;
        this._currentUserService = _currentUserService;
    }


    [HttpGet("check-user-session")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckUserSession(string userId, string sessionId)
    {
        // string response = await sessionService.GetSessionFromRedis(userId);
        bool response = await sessionService.GetSessionFromMemoryCache(userId, sessionId);
        if (response)
        {
            return Ok(new
            {
                HttpStatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Message = "Session Valid"

            });
        }
        else
        {
            return BadRequest(new
            {
                HttpStatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = false,
                Message = "Session Invalid"

            });
        }

    }
    [HttpPost("applicant-forgot-password-OTP-send")]
    [AllowAnonymous]
    public async Task<IActionResult> ApplicantForgotPasswordOTPSend([FromBody] ForgotPasswordModel model)
    {
        try
        {
            model.UserName = AesAlgorithm.DecryptString(model.UserName);
            model.IP = HttpContext.Connection.RemoteIpAddress?.ToString();


            var isBruteForce = await bruteForceService.IsBruteForce(model.UserName, "ApplicantForgotPassword");
            if (isBruteForce)
            {
                bruteForceService.RegisterFailedAttempt(model.UserName, "ApplicantForgotPassword");
                return Ok(new { Status = false, Message = "Too many requests. You can raise next otp request after 5 minutes." });
            }
            else
            {
                bruteForceService.RegisterFailedAttempt(model.UserName, "ApplicantForgotPassword");
            }

            var user = await userManager.Users.SingleOrDefaultAsync(x => x.UserName == model.UserName);

            if (user != null)
            {
                var logindetails = await userManagerService.GetLogindetailByUserName(model.UserName);
                if (!string.IsNullOrEmpty(logindetails.MobileNo))
                {
                    await clientService.RequestSend<object>(HttpMethod.Post, $"{urloptions.NotificationService}/OTP/SMS/Send", logindetails.MobileNo);
                    return Ok(new { Status = true, Message = "OTP will send to your registered mobile number." });
                }
                else
                    return Ok(new { Status = true, Message = "OTP will send to your registered mobile number." }); // Mobile number not registered. VAPT - Abbas
            }
            else
                return Ok(new { Status = true, Message = "OTP will send to your registered mobile number." }); //User Not found. VAPT - Abbas

        }
        catch (Exception ex)
        {
            // ExceptionLogging.LogException(Convert.ToString(ex));
            return Ok(new { Status = false, Message = "Something went wrong.", Error = ex.Message });
        }
    }
    [HttpPost("applicant-forgot-password-OTP-verify")]
    [AllowAnonymous]
    public async Task<IActionResult> ApplicantForgotPasswordOTPVerify([FromBody] ForgotPassword model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Ok(ModelState);
            }
            model.UserName = AesAlgorithm.DecryptString(model.UserName);
            model.OTP = AesAlgorithm.DecryptString(model.OTP);
            model.NewPassword = AesAlgorithm.DecryptString(model.NewPassword);
            model.IP = HttpContext.Connection.RemoteIpAddress?.ToString();


            var isBruteForce = await bruteForceService.IsBruteForce(model.IP, "ApplicantForgotPasswordOTPVerify");
            if (isBruteForce)
            {
                return Ok(new { Status = false, Message = "Too many requests. You can raise next verify request after 5 minutes." });
            }
            else
            {
                bruteForceService.RegisterFailedAttempt(model.IP, "ApplicantForgotPasswordOTPVerify");
            }

            var user = await userManager.Users.SingleOrDefaultAsync(x => x.UserName == model.UserName);
            if (user != null)
            {
                var logindetails = await userManagerService.GetLogindetailByUserName(model.UserName);
                if (!string.IsNullOrEmpty(logindetails.MobileNo))
                {
                    HttpClient httpClient = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{urloptions.NotificationService}/OTP/SMS/Verify");
                    string content = JsonSerializer.Serialize(new { Mobile = logindetails.MobileNo, OTP = model.OTP });
                    request.Content = new StringContent(content, null, "application/json");
                    var response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var data = JsonSerializer.Deserialize<OTPVerifyResponse?>(result);
                        if (data != null && data.Status)
                        {
                            string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                            IdentityResult passwordChangeResult = await userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
                            if (passwordChangeResult.Succeeded)
                            {
                                return Ok(new { Status = true, Message = "Password Change Successfully." });
                            }
                            else
                            {
                                return Ok(new { Status = false, Message = "OTP Verify failed." });
                            }
                            // user.Password = model.NewPassword;
                            // var result1 = await userManager.UpdateAsync(user);
                            //   return Ok(new { Status = true, Message = "Password Change Successfully." });
                        }
                        else
                        {
                            return Ok(new { Status = false, Message = data != null ? data.Message : "OTP Verify failed." });
                        }
                    }
                    else
                    {
                        return Ok(new { Status = false, Message = "OTP Verify failed." });
                    }
                }
                else
                    return Ok(new { Status = false, Message = "OTP Verify failed." }); //Mobile number not registered. VAPT - Abbas
            }
            else
                return Ok(new { Status = false, Message = "OTP Verify failed." }); // User not found VAPT - Abbas

        }
        catch (Exception ex)
        {
            // ExceptionLogging.LogException(Convert.ToString(ex));
            return Ok(new { Status = false, Message = "Something went wrong.", Error = ex.Message });
        }
    }

    [HttpPost("farmer-login")]
    [AllowAnonymous]
    public async Task<IActionResult> FarmerLogin([FromBody] LoginRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var isBruteForce = await bruteForceService.IsBruteForce(model.UserName, "Username");
        if (isBruteForce)
        {
            bruteForceService.RegisterFailedAttempt(model.UserName, "Username");
            return Ok(new { Status = false, Message = "Too many requests. You can raise next login request after 5 minutes." });
        }
        else
        {
            bruteForceService.RegisterFailedAttempt(model.UserName, "Username");
        }

        try
        {
            var IsKeyExist = await cacheService.IsKeyExists(model.UserName + model.Password);
            if (IsKeyExist)
            {
                return Ok(new { HttpStatusCode = HttpStatusCode.BadRequest, Status = false, Message = "Username or Password is incorrect" });
            }
            await cacheService.GetOrCreateAsync(model.UserName + model.Password, async () => { return await CreatePwdCacheKey(); }, TimeSpan.FromHours(12));
            model.Password = AesAlgorithm.DecryptString(model.Password);
            var password = model.Password.Substring(5, model.Password.Length - 10);
            var mds = CreateMD5(password);

            var user = await userManager.Users.SingleOrDefaultAsync(x => x.UserName == model.UserName);
            var result = userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            var roles = await userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() ?? "NoRole";


            if (result == PasswordVerificationResult.Success)
            {

                var logindetails = await userManagerService.GetUserNumericId(user.Id);
                if (logindetails is not null)
                {
                    var sessionid = Guid.NewGuid().ToString();
                    var userToken = tokenHelper.GenerateAccessToken(user.Id, Convert.ToString(logindetails.ID), sessionid, roleName, "", "", "0");
                    await userManagerService.UserLoginSessionStore(user.Id, sessionid);
                    //await sessionService.StoreSessionInRedis(user.Id, sessionid);
                    await sessionService.StoreSessionInMemoryCache(user.Id, sessionid);
                    await cacheService.Clear(model.UserName);
                    //var IsNewSchool = await clientService.RequestSend<bool?>(HttpMethod.Get, $"{urloptions.SchoolProfilService}/api/nm_schoolservice/NewSchool/get/{logindetails.ID}", null);

                    return Ok(new
                    {
                        statusCode = HttpStatusCode.OK,
                        status = true,
                        Token = userToken,
                        user.UserName,
                        user.Name,
                        roleName,
                        LoginAt = DateTime.Now,
                        // IsNew = IsNewSchool
                    });
                }
                else
                {
                    return Ok(new SuccessResult(HttpStatusCode.BadRequest, false, "Username or Password is incorrect."));
                }
            }
            else
            {
                return Ok(new SuccessResult(HttpStatusCode.BadRequest, false, "Username or Password is incorrect."));
            }
        }
        catch (Exception ex)
        {
            // ExceptionLogging.LogException(Convert.ToString(ex));
            return Ok(new SuccessResult(HttpStatusCode.BadRequest, false, "Some Error Occurred. Please try again."));
        }



    }
    [HttpPost("forgot-password-OTP-send")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPasswordOTPSend([FromBody] ForgotPasswordModel model)
    {
        try
        {

            var isBruteForce = await bruteForceService.IsBruteForce(model.IP, "ForgotPassword");
            if (isBruteForce)
            {
                bruteForceService.RegisterFailedAttempt(model.IP, "ForgotPassword");
                return Ok(new { Status = false, Message = "Too many requests. You can raise next otp request after 5 minutes." });
            }
            else
            {
                bruteForceService.RegisterFailedAttempt(model.IP, "ForgotPassword");
            }

            try
            {
                model.UserName = AesAlgorithm.DecryptString(model.UserName);
            }
            catch (Exception ex)
            {
                model.UserName = "Text";
            }


            var user = await userManager.Users.SingleOrDefaultAsync(x => x.UserName == model.UserName);
            if (user != null)
            {
                var roles = await userManager.GetRolesAsync(user);


                if (roles.Contains("Warden"))
                {
                    string? Mobile = string.Empty;
                    try
                    {
                        var UserRegister = await clientService.RequestSend<HostelServiceUserModel>(HttpMethod.Get, $"{urloptions.HostelService}/account/{user.Id}", null);
                        if (UserRegister != null)
                        {
                            Mobile = UserRegister?.Mobile;
                        }
                    }
                    catch (Exception ex)
                    {
                        // ExceptionLogging.LogException(Convert.ToString(ex));
                        Mobile = string.Empty;
                    }

                    if (!string.IsNullOrEmpty(Mobile))
                    {
                        await clientService.RequestSend<object>(HttpMethod.Post, $"{urloptions.NotificationService}/OTP/SMS/Send", Mobile);
                        return Ok(new { Status = true, Message = "OTP will send to your registered mobile number." });
                    }
                    else
                        return Ok(new { Status = true, Message = "OTP will send to your registered mobile number." }); //Mobile number not registered. VAPT - Abbas
                }
                else
                {
                    if (!string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        await clientService.RequestSend<object>(HttpMethod.Post, $"{urloptions.NotificationService}/OTP/SMS/Send", user.PhoneNumber);
                        return Ok(new { Status = true, Message = "OTP will send to your registered mobile number." });
                    }
                    else
                        return Ok(new { Status = true, Message = "OTP will send to your registered mobile number." }); //Mobile number not registered. VAPT - Abbas
                }
            }
            else
                return Ok(new { Status = true, Message = "OTP will send to your registered mobile number." }); // User not found VAPT - Abbas

        }
        catch (Exception ex)
        {
            // ExceptionLogging.LogException(Convert.ToString(ex));
            return Ok(new { Status = false, Message = "Something went wrong.", Error = ex.Message });
        }
    }
    //[HttpPost("forgot-password-OTP-verify")]
    //[AllowAnonymous]
    //public async Task<IActionResult> ForgotPasswordOTPVerify([FromBody] ForgotPassword model)
    //{
    //    try
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return Ok(ModelState);
    //        }


    //        var isBruteForce = await bruteForceService.IsBruteForce(model.IP, "ForgotPasswordOTPVerify");
    //        if (isBruteForce)
    //        {
    //            return Ok(new { Status = false, Message = "Too many requests. You can raise next verify request after 5 minutes." });
    //        }
    //        else
    //        {
    //            bruteForceService.RegisterFailedAttempt(model.IP, "ForgotPasswordOTPVerify");
    //        }


    //        model.UserName = AesAlgorithm.DecryptString(model.UserName);

    //        model.OTP = AesAlgorithm.DecryptString(model.OTP);

    //        model.NewPassword = AesAlgorithm.DecryptString(model.NewPassword);

    //       var user = await userManager.Users.SingleOrDefaultAsync(x => x.UserName == model.UserName);
    //        if (user != null)
    //        {
    //            var roles = await userManager.GetRolesAsync(user);
    //            string? mobile = string.Empty;
    //            if (roles.Contains("Warden"))
    //            {
    //                try
    //                {
    //                    var UserRegister = await clientService.RequestSend<HostelServiceUserModel>(HttpMethod.Get, $"{urloptions.HostelService}/account/{user.Id}", null);

    //                    if (UserRegister != null)
    //                    {
    //                        mobile = UserRegister?.Mobile;
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    // ExceptionLogging.LogException(Convert.ToString(ex));
    //                }
    //            }
    //            else
    //                mobile = user.PhoneNumber;

    //            if (!string.IsNullOrEmpty(mobile))
    //            {
    //                HttpClient httpClient = new HttpClient();
    //                var request = new HttpRequestMessage(HttpMethod.Post, $"{urloptions.NotificationService}/OTP/SMS/Verify");
    //                string content = JsonSerializer.Serialize(new { Mobile = mobile, OTP = model.OTP });
    //                request.Content = new StringContent(content, null, "application/json");
    //                var response = await httpClient.SendAsync(request);
    //                if (response.IsSuccessStatusCode)
    //                {
    //                    var result = await response.Content.ReadAsStringAsync();
    //                    var data = JsonSerializer.Deserialize<OTPVerifyResponse?>(result);
    //                    if (data != null && data.Status)
    //                    {
    //                        string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
    //                        IdentityResult passwordChangeResult = await userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
    //                        if (passwordChangeResult.Succeeded)
    //                        {
    //                            return Ok(new { Status = true, Message = "Password Change Successfully." });
    //                        }
    //                        else
    //                        {
    //                            return Ok(new { Status = false, Message = "OTP Verify failed." });
    //                        }
    //                    }
    //                    else
    //                    {
    //                        return Ok(new { Status = false, Message = data != null ? data.Message : "OTP Verify failed." });
    //                    }
    //                }
    //                else
    //                {
    //                    return Ok(new { Status = false, Message = "OTP Verify failed." });
    //                }
    //            }
    //            else
    //                return Ok(new { Status = false, Message = "OTP Verify failed." });
    //        }
    //        else
    //            return Ok(new { Status = false, Message = "OTP Verify failed." }); // User Not found.VAPT Abbas

    //    }
    //    catch (Exception ex)
    //    {
    //        // ExceptionLogging.LogException(Convert.ToString(ex));
    //        return Ok(new { Status = false, Message = "Something went wrong.", Error = ex.Message });
    //    }
    //}
    [HttpPost("registration")]
    [AllowAnonymous]
    public async Task<IActionResult> Registration([FromBody] RegistrationModel model)
    {
        try
        {
            var user = await userManager.Users.FirstOrDefaultAsync(x => x.UserName == model.UdiseNo.Trim());
            if (user != null)
            {
                return Ok(new { Status = false, Message = "UDISE Code Already Exists." });
            }

            var userEmail = await userManager.Users.FirstOrDefaultAsync(x => x.Email == model.Email.Trim());
            if (userEmail != null)
            {
                return Ok(new { Status = false, Message = "Email exists.." });
            }

            var userMobile = await userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == model.Mobile.Trim());
            if (userMobile != null)
            {
                return Ok(new { Status = false, Message = "Mobile exists." });
            }

            var IsEmailVerify = await CheckVerify($"{urloptions.NotificationService}/OTP/is-email-verified", model.Email);
            if (!IsEmailVerify)
            {
                return Ok(new { Status = false, Message = "Email not verified." });

            }

            var IsMobileVerify = await CheckVerify($"{urloptions.NotificationService}/OTP/is-mobile-verified", model.Mobile);
            if (!IsMobileVerify)
            {
                return Ok(new { Status = false, Message = "Mobile Number not verified." });
            }


            var applicantRole = new ApplicationRole("Principle");

            if (roleManager.Roles.All(r => r.Name != applicantRole.Name))
            {
                await roleManager.CreateAsync(applicantRole);
            }

            var applicant = new ApplicationUser
            {
                UserName = model.UdiseNo.Trim(),
                //Password = model.Password.Trim().ToUpper(),
                Email = model.Email.Trim(),
                PhoneNumber = model.Mobile.Trim(),
                Status = Repository.Enums.Status.Active
            };
            var randdomPass = UserService.Helper.Utility.GeneratePassword(10);

            var result = await userManager.CreateAsync(applicant, randdomPass);
            if (result.Succeeded)
            {

                await userManager.AddToRolesAsync(applicant, new[] { applicantRole.Name });
                var numericid = await userManagerService.CreateNumericId(applicant.Id);
                var logindetails = new logindetails
                {
                    UserIdentity = applicant.Id,
                    IsFirstLogin = true,
                    IsAadharVerified = false,
                    EmailId = model.Email,
                    MobileNo = model.Mobile,
                    UserName = model.UdiseNo,
                    Domain = "fv",
                    IsEmailVerified = true,
                    IsMobileVerified = true,
                    CreatedOn = DateTime.Now,
                    CreatedBy = "admin"
                };
                long numericID = await userManagerService.SaveloginDetails(logindetails);
                var payload = new NewUDISEEntryModel
                {
                    UdiseNo = Convert.ToInt64(model.UdiseNo),
                    SchoolName = model.SchoolName,
                    OrgName = model.OrgName,
                    IsNew = model.IsNew,
                };
                var emailpayload = new EmailSender
                {
                    to = model.Email,
                    key = "FirstLoginPassSend",
                    param = new List<string> { model.UdiseNo.Trim(), randdomPass },
                };

                var UserRegister = await clientService.RequestSend<RegistrationModel>(HttpMethod.Post, $"{urloptions.SchoolProfilService}/api/nm_schoolservice/NewSchool/post", payload);
                //var emailSender = await clientService.RequestSend<EmailResponse>(HttpMethod.Post, $"{urloptions.NotificationService}/Email/schedule-send", emailpayload);
                if (numericID > 0 && UserRegister != null)
                    return Ok(new { Status = true, Message = "Registration Success." });
                else
                    return Ok(new { Status = false, Message = "Registration Failed ,Please try again." });
            }
            else
            {
                return Ok(new { Status = false, Message = result.Errors.Select(x => x.Description).FirstOrDefault() });
            }
        }
        catch (Exception ex)
        {
            // ExceptionLogging.LogException(Convert.ToString(ex));
            return Ok(new { Status = false, Message = ex.Message });
        }
    }

    [HttpPost("New-login-registration")]
    [AllowAnonymous]

    public async Task<IActionResult> NewLoginRegistration([FromBody] NewStudentRegistrationModel model)
    {
        try
        {
            var user = await userManager.Users.FirstOrDefaultAsync(x => x.UserName == model.UserName.Trim());
            if (user != null)
            {
                return Ok(new { Status = false, Message = "Username exist." });
            }

            var userEmail = await userManager.Users.FirstOrDefaultAsync(x => x.Email == model.Email.Trim());
            if (userEmail != null)
            {
                return Ok(new { Status = false, Message = "Email exists.." });
            }

            var userMobile = await userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == model.Mobile.Trim());
            if (userMobile != null)
            {
                return Ok(new { Status = false, Message = "Mobile exists." });
            }

            var IsEmailVerify = await CheckVerify($"{urloptions.NotificationService}/OTP/is-email-verified", model.Email);
            if (!IsEmailVerify)
            {
                return Ok(new { Status = false, Message = "Email not verified." });

            }

            var IsMobileVerify = await CheckVerify($"{urloptions.NotificationService}/OTP/is-mobile-verified", model.Mobile);
            if (!IsMobileVerify)
            {
                return Ok(new { Status = false, Message = "Mobile Number not verified." });
            }

            var randdomPass = UserService.Helper.Utility.GeneratePassword(10);
            var applicantRole = new ApplicationRole("Farmer");

            if (roleManager.Roles.All(r => r.Name != applicantRole.Name))
            {
                await roleManager.CreateAsync(applicantRole);
            }

            var applicant = new ApplicationUser
            {
                UserName = model.UserName.Trim(),
                //Password = model.Password.Trim().ToUpper(),
                Email = model.Email.Trim(),
                PhoneNumber = model.Mobile.Trim(),
                Status = Repository.Enums.Status.Active
            };

            var result = await userManager.CreateAsync(applicant, randdomPass);
            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(applicant, new[] { applicantRole.Name });
                var numericid = await userManagerService.CreateNumericId(applicant.Id);
                var logindetails = new logindetails
                {
                    UserIdentity = applicant.Id,
                    //Password = model.Password,
                    EmailId = model.Email,
                    MobileNo = model.Mobile,
                    UserName = model.UserName,
                    IsFirstLogin = true,
                    Domain = "FV",
                    IsEmailVerified = true,
                    IsMobileVerified = true,
                    IsAadharVerified = true,
                    CreatedOn = DateTime.Now,
                    CreatedBy = "admin"
                };
                long numericID = await userManagerService.SaveloginDetails(logindetails);
                var payload = new EmailSender
                {
                    to = model.Email,
                    key = "FirstLoginPassSend",
                    param = new List<string> { model.UserName, randdomPass },
                };

                var UserRegister = await clientService.RequestSend<EmailResponse>(HttpMethod.Post, $"{urloptions.NotificationService}/Email/schedule-send", payload);
                if (numericID > 0 && UserRegister != null)
                    return Ok(new { Status = true, Message = "Registration Successful,Please check your email for user name and password." });
                else
                    return Ok(new { Status = false, Message = "Registration Failed ,Please try again." });
            }
            else
            {
                return Ok(new { Status = false, Message = result.Errors.Select(x => x.Description).FirstOrDefault() });
            }
        }
        catch (Exception ex)
        {
            // ExceptionLogging.LogException(Convert.ToString(ex));
            return Ok(new { Status = false, Message = ex.Message });
        }
    }
    [HttpPost("check-user-exists")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckUserExists([FromBody] string Username)
    {
        if (userManager.Users.Any(u => u.UserName == Username))
            return Ok(true);
        else
            return Ok(false);
    }
    //Sh-01/1025:Create API for check the user existence
    [HttpPost("check-existence")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckExistence([FromBody] CheckUserExistenceModel model)
    {
        try
        {
            string value = model.Value?.Trim();
            bool exists = false;
            string message = string.Empty;
            if (string.IsNullOrWhiteSpace(model?.Key) || string.IsNullOrWhiteSpace(model?.Value))
            {
                return Ok(new { Status = false, Message = "Invalid input." });
            }
            switch (model.Key)
            {
                case "UserName":
                    exists = await userManager.Users.AnyAsync(x => x.UserName == value);
                    message = "Username Already Exists.";
                    break;
                case "Email":
                    exists = await userManager.Users.AnyAsync(x => x.Email == value);
                    message = "Email already exists.";
                    break;
                case "PhoneNumber":
                    exists = await userManager.Users.AnyAsync(x => x.PhoneNumber == value);
                    message = "Mobile already exists..";
                    break;
                default:
                    return Ok(new { Status = false, Message = "Invalid key provided." });
            }
            if (exists)
                return Ok(new { Status = false, Message = message });

            return Ok(new { Status = true });
        }
        catch (Exception ex)
        {
            return Ok(new { Status = false, Message = ex.Message });
        }
    }
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {

            var user = await userManager.GetUserAsync(User);
            await userManagerService.UserLoginSessionStore(user.Id, "Expired");


            await sessionService.RemoveKeyMemoryCache(_currentUserService.UserId);

            return Ok(new { Status = true });
        }
        catch (Exception ex)
        {

            return Ok(new { Status = false });
        }
    }

    [HttpGet("Update-AadharStatus")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateAadharStatus(bool IsAadharVerified, long UserId)
    {
        try
        {
            var aadhardata = await userManagerService.UpdateAadharStatus(IsAadharVerified, UserId);
            if (aadhardata)
                return Ok(new { Status = true, Message = "Aadhar verified successfully" });
            else
                return Ok(new { Status = false, Message = "Something went wrong." });
        }
        catch (Exception ex)
        {

            return Ok(new { Status = false, Message = "Something went wrong." });
        }
    }
    [HttpGet("check-verified-status")]

    public async Task<IActionResult> VerifiedStatus()
    {
        try
        {


            var result = await userManagerService.Getlogindetails();

            return Ok(result);
        }
        catch (Exception ex)
        {
            // ExceptionLogging.LogException(Convert.ToString(ex));
            return Ok(new VerifiedStatusModel());
        }
    }
    [HttpGet("logindetails-ByUserId")]
    [AllowAnonymous]
    public async Task<IActionResult> GetlogindetailsByAadharRefNo(long userId)
    {
        try
        {
            var result = await userManagerService.GetlogindetailsByUserId(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // ExceptionLogging.LogException(Convert.ToString(ex));
            return Ok(new Applicantdetails());
        }
    }



    //public async Task<IActionResult> CreateScrutinyLogins([FromBody] ScrutinyLogins model)
    //{
    //    try
    //    {
    //        var userCreateresult = await userManagerService.CreateScrutinyLoginsAsync(model);
    //        if (userCreateresult)
    //            return Ok(new { Status = true, Message = "All unique user created" });
    //        else
    //            return Ok(new { Status = false, Message = "Something went wrong." });
    //    }
    //    catch (Exception ex)
    //    {

    //        return Ok(new { Status = false, Message = "Unable to Create Users ,Pls try again later" });
    //    }
    //}
    [HttpPost("temp-token-Creation")]
    [AllowAnonymous]
    public async Task<IActionResult> tempTokenCreation(string UserId, string roleName)
    {
        try
        {
            var userToken = tokenHelper.GenerateAccessToken("Id", UserId, "sessionid", roleName, "521", "1", "1");

            return Ok(new { Status = true, token = userToken });
        }
        catch (Exception ex)
        {

            return Ok(new { Status = false, Message = "Unable to Create Users ,Pls try again later" });
        }
    }


    private async Task<bool> CheckVerify(string URL, string param)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, URL);
            string content = JsonSerializer.Serialize(param);
            request.Content = new StringContent(content, null, "application/json");
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(result);
        }
        catch (Exception ex)
        {
            // ExceptionLogging.LogException(Convert.ToString(ex));
            return false;
        }
    }
    public static string CreateMD5(string input)
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
    private async Task<bool> CreatePwdCacheKey()
    {

        return true;
    }

}

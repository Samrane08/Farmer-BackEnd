using Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Admin;
using Repository.Entity;
using Service.Interface;
using UserService.Helper;
using UserService.Model.Request;
using UserService.Service;

namespace UserService.Controllers
{

    public class AdministratorController : APIBaseController
    {
        private readonly IUserManagerService userManagerService;
        private readonly IMenuManagementService menuManagementService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHttpClientService httpClientService;
        private readonly APIUrl urloptions;

        public AdministratorController(IUserManagerService userManagerService,IMenuManagementService menuManagementService, UserManager<ApplicationUser> userManager,
                                       IHttpClientService httpClientService,IOptions<APIUrl> urlOptions)
        {
            this.userManagerService = userManagerService;
            this.menuManagementService = menuManagementService;
            this.userManager = userManager;
            this.httpClientService = httpClientService;
            this.urloptions = urlOptions.Value;
        }

        [HttpGet("entity-type-list")]
        public async Task<IActionResult> EntityTypeList()
        {
            return Ok(await userManagerService.GetEntityTypeList());
        }

        [HttpPost("create-entity-type")]
        public async Task<IActionResult> CreateTypeList([FromBody] EntityTypeRequest request)
        {
            var result = await userManagerService.CreateEntityAsync(request.EntityName, request.Status);
            if (result)
                return RedirectToAction("EntityTypeList");
            else
                return BadRequest(new { Message = "Failed to create entity type." });

        }

        [HttpGet("role-list")]
        public async Task<IActionResult> RoleList()
        {
            return Ok(await userManagerService.GetRoleList());
        }

        [HttpPost("create-role")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateRole(CreateRoleModel model)
        {
            int deptId = await userManagerService.GetDeptIdByDeptName(model.deptName);
            if (deptId > 0)
            {
                var result = await userManagerService.CreateRoleAsync(model.role, deptId);
                if (result)
                    return RedirectToAction("RoleList");
                else
                    return BadRequest(new { Message = "Failed to create role" });
            }else
            {
                return BadRequest(new { Message = "Invalid department name" });
            }
        }

        [HttpPost("update-role")]
        public async Task<IActionResult> UpdateRole([FromBody] ApplicationRole role)
        {
            if (role.DepartmentId > 0)
            {
                var result = await userManagerService.UpdateRoleAsync(role);
                if (result)
                    return RedirectToAction("RoleList");
                else
                    return BadRequest(new { Message = "Failed to create role" });
            }
            else
            {
                return BadRequest(new { Message = "Invalid department Id" });
            }
        }

        [HttpGet("enitity-role-mapping-list")]
        public async Task<IActionResult> GetEntityRoleMappingList()
        {
            return Ok(await userManagerService.RoleMappingList());
        }

        [HttpPost("entity-role-mapping")]
     
        public async Task<IActionResult> EntityRoleMapping([FromBody] EntityRoleMappingRequest request)
        {
            var result = await userManagerService.EntityRoleMapping(request.EntityTypeId,request.RoleId,request.Status);
            if (result)
                return Ok(new { Message = $"Entity Id : {request.EntityTypeId} and Role Id : {request.RoleId} mapped success." });
            else
                return Ok(new { Message = $"Entity Id : {request.EntityTypeId} and Role Id : {request.RoleId} mapped failed." });
        }

        [HttpGet("active-menu-list")]
        public async Task<IActionResult> ActiveMenuList()
        {
            return Ok(await menuManagementService.GetActiveMenuList());
        }

        [HttpGet("inactive-menu-list")]
        public async Task<IActionResult> InActiveMenuList()
        {
            return Ok(await menuManagementService.GetInActiveMenuList());
        }

        [HttpPost("upsert-menu")]
        public async Task<IActionResult> UpsertMenu([FromBody] MenuModel request)
        {
            var result = await menuManagementService.UpsertAsync(request);
            return Ok(result);
        }

        [HttpPost("update-menu-status")]
        public async Task<IActionResult> UpdateMenuStatus([FromBody] MenuStatusUpdateRequest request)
        {
            var result = await menuManagementService.MenuStatusUpdate(request.MenuId,(int)request.Status);
            if (result)
            {
                if((int)request.Status == 2)
                {
                    return RedirectToAction("InActiveMenuList");
                }
                else
                    return RedirectToAction("ActiveMenuList");
            }
            else
            {
                return BadRequest(new { Message = $"Menu id : {request.MenuId} status change {request.Status} failed." });
            }
        }

        [HttpPost("entity-role-menu-map")]
    
        public async Task<IActionResult> EntityRoleMenuMap([FromBody] EntityRoleMenuMapRequest request)
        {
            var result = await menuManagementService.EntityRoleMenuMapping(request.EntityRoleId,request.MenuId,(int)request.Status);
            if (result)
            {
                return Ok(new { Message = $"Entity role Id : {request.EntityRoleId} and Menu Id : {request.MenuId} mapped success." });
            }
            else
            {
                return Ok(new { Message = $"Entity role Id : {request.EntityRoleId} and Menu Id : {request.MenuId} mapped failed." });
            }
        }

        [HttpGet("get-menulist-by-entitymappingid")]
        public async Task<IActionResult> GetMenuByEntityRole([FromBody]List<int> EntityMappingId)
        {
            return Ok(await menuManagementService.GetMenuByRoleEntity(EntityMappingId));
        }

        [HttpPost("reset-warden-dept-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] string userName)
        {
            var decUsername = AesAlgorithm.DecryptString(userName);
            var subUsername = decUsername.Substring(5, decUsername.Length - 10);
            var user = await userManager.Users.SingleOrDefaultAsync(x => x.UserName == subUsername);

            if (user != null)
            {
                var roles = await userManager.GetRolesAsync(user);

                if (roles.Contains("Applicant"))
                {
                    return Ok(new { Status = false, Message = "Not a valid warden / department Login" });
                }

                string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                IdentityResult passwordChangeResult = await userManager.ResetPasswordAsync(user, resetToken, "Password@123");
                if (passwordChangeResult.Succeeded)
                {
                   // var response = await httpClientService.RequestSend<bool>(HttpMethod.Post, $"{urloptions.HostelService}/account/update-first-login-reset-password", user.Id);
                    return Ok(new { Status = true, Message = "Password Change Successfully." });
                }
                else
                {
                    return Ok(new { Status = false, Message = "Password could not be changed" });
                }
            }
            else
            {
                return Ok(new { Status = false, Message = "User not found" });
            }


        }

        [HttpGet("dept-list")]
        [AllowAnonymous]
        public async Task<IActionResult> DeptList()
        {

            try
            {
                var data = await userManagerService.GetDeptList();
                return Ok(data);
            }catch(Exception ex)
            {
                return Ok(ex);
            }


        }

        [HttpGet("roles-by-department")]
        [AllowAnonymous]

        public async Task<IActionResult> RolesByDept(int deptId)
        {

            try
            {
                var data = await userManagerService.GetRolesBydept(deptId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return Ok(new RolesSelectModel());
            }


        }


        [HttpPost("menu-from-role")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMenuByRole([FromBody] int entityroleMappingId)
        {
            var result = await menuManagementService.GetMenuListFromRole(entityroleMappingId);
            return Ok(result);
        }

        [HttpPost("upsert-menu-mapping")]
        [AllowAnonymous]
        public async Task<IActionResult> UpsertMenuMapping([FromBody] UpsertMenuMapping model)
        {
            var result = await menuManagementService.UpsertMenuMapping(model);
            return Ok(result);
        }

        // endpoint for applicant reset password

        [HttpPost("reset-applicant-password")] 
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordApplicant([FromBody] string userName)
        {
            var decUsername =AesAlgorithm.DecryptString(userName);
            var subUsername = decUsername.Substring(5, decUsername.Length - 10);
            var user = await userManager.Users.SingleOrDefaultAsync(x => x.UserName == subUsername);

            if (user != null)
            {
                var roles = await userManager.GetRolesAsync(user);

                if (!roles.Contains("Applicant"))
                {
                    return Ok(new { Status = false, Message = "Not a valid Applicant Login" });
                }

               
                var result = await userManagerService.ResetApplicantPassword(subUsername, "F91E15DBEC69FC40F81F0876E7009648");
                //string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                //IdentityResult passwordChangeResult = await userManager.ResetPasswordAsync(user, resetToken, "123456@AliyaKaif");
                if (result)
                {
                    // var response = await httpClientService.RequestSend<bool>(HttpMethod.Post, $"{urloptions.HostelService}/account/update-first-login-reset-password", user.Id);
                    return Ok(new { Status = true, Message = "Password Change Successfully." });
                }
                else
                {
                    return Ok(new { Status = false, Message = "Password could not be changed" });
                }
            }
            else
            {
                return Ok(new { Status = false, Message = "User not found" });
            }


        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using Service.Interface;
using UserService.Service;

namespace UserService.Controllers;


public class MenuController : APIBaseController
{
    private readonly IUserManagerService userManagerService;
    private readonly IMenuManagementService menuManagementService;
    private readonly ICurrentUserService currentUserService;

    public MenuController(IUserManagerService userManagerService,IMenuManagementService menuManagementService,ICurrentUserService currentUserService)
    {
        this.userManagerService = userManagerService;
        this.menuManagementService = menuManagementService;
        this.currentUserService = currentUserService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int? deptId)
    {
        try
        {
            
             var roles = await userManagerService.GetEntityRoleMappingId(currentUserService.UserId, deptId);
            var reult = await menuManagementService.GetMenuByRoleEntity(roles);
            return Ok(new { Status = true, MenuList = reult });
        }catch (Exception ex)
        {
           
            // ExceptionLogging.LogException(Convert.ToString(ex));
            return Ok(new { Status = false });
        }
    }
}

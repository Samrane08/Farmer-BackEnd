using UserService.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Models.QueryDto;
using UserService.Interface;

namespace UserService.Controllers
{
    [Route("fv_user-service/api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuRepository _menuRepository;

        public MenuController(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetMenus([FromBody] MenuRequest menuRequest)
        {
            if (menuRequest == null || menuRequest.RoleId <= 0)
                return BadRequest("Invalid request. RoleId is required.");

            try
            {
                var menus = await _menuRepository.GetMenuByRoleIdAsync(menuRequest.RoleId);

                if (menus == null || !menus.Any())
                    return NotFound("No menus found for the given RoleId.");

                return Ok(menus);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while fetching menu details.");
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetSubMenus([FromBody] SubMenuRequest subMenuRequest)
        {
            if (menuRequest == null || menuRequest.RoleId <= 0)
                return BadRequest("Invalid request. RoleId is required.");

            try
            {
                var menus = await _menuRepository.GetMenuByRoleIdAsync(subMenuRequest.RoleId, subMenuRequest.MenuId);

                if (menus == null || !menus.Any())
                    return NotFound("No menus found for the given RoleId.");

                return Ok(menus);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while fetching menu details.");
            }
        }
    }
}

using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using UserService.Interface;
using UserService.Models.ResponseDto;

namespace UserService.Repository
{
    public class MenuRepository : IMenuRepository
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public MenuRepository(IConfiguration config, ITokenService tokenService)
        {
            _config = config;
            _connectionString = config.GetConnectionString("FvCon");
        }

        public async Task<List<MenuResponseDto>> GetMenuByRoleIdAsync(int roleId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var param = new DynamicParameters();
                param.Add("@RoleId", roleId);

                var result = await connection.QueryAsync<MenuResponseDto>(
                    "USP_GetMenuNamesByRoleId",
                    param,
                    commandType: CommandType.StoredProcedure);

                return [.. result];
            }
        }

        public async Task<List<SubMenuResponseDto>> GetSubMenusAsync(int roleId, int menuId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var param = new DynamicParameters();
                param.Add("@RoleId", roleId);
                param.Add("@MenuId", menuId);

                var result = await connection.QueryAsync<SubMenuResponseDto>(
                    "USP_GetSubMenuNamesByRoleMenu",
                    param,
                    commandType: CommandType.StoredProcedure);

                return [.. result];
            }
        }

    }
}

using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using UserService.Interface;
using UserService.Models.QueryDto;
using UserService.Models.ResponseDto;

namespace UserService.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;
        private readonly ITokenService _tokenService;

        public AuthRepository(IConfiguration config, ITokenService tokenService)
        {
            _connectionString = config.GetConnectionString("FvCon");
            _config = config;
            _tokenService = tokenService;
        }

        public async Task<LoginResponse> CheckLoginAsync(LoginRequest loginRequest)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", loginRequest.UserId);
                parameters.Add("@Password", loginRequest.Password);

                var user = await connection.QueryFirstOrDefaultAsync<LoginResponse>(
                    "USP_CheckLogin",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (user != null)
                {
                    user.Token = _tokenService.GenerateToken(
                        user.FullName,
                        user.RoleId,
                        user.BankId
                    );

                    user.Success = true;
                    user.Message = "Login successful";
                    return user;
                }
                else
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid UserId or Password"
                    };
                }
            }
        }


    }
}

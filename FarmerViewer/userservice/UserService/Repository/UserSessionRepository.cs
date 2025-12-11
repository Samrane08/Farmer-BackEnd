using Dapper;
using Microsoft.Data.SqlClient;
using UserService.Helpers;
using UserService.Interface;
using UserService.Models.QueryDto;
using UserService.Models.ResponseDto;

namespace UserService.Repository
{
    public class UserSessionRepository : IUserSessionRepository
    {
        private readonly string _connStr;
        public UserSessionRepository(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection");
        }

        private SqlConnection GetConnection() => new SqlConnection(_connStr);

        // -------------------------------------------------------------
        // 1. Create or Update Session (User + DeviceFingerprintHash)
        // -------------------------------------------------------------
        public async Task<int> CreateOrUpdateSessionAsync(UserSessionCommand cmd)
        {
            string tokenHash = HashHelper.ComputeSha256Hex(cmd.Token);
            string dfpHash = HashHelper.ComputeSha256Hex(cmd.DeviceFingerprint);

            var sqlFind = @"
                SELECT TOP 1 UserSessionId
                FROM UserSessions WITH(NOLOCK)
                WHERE UserId = @UserId AND DeviceFingerprintHash = @DeviceFingerprintHash;
            ";

            var sqlUpdate = @"
                UPDATE UserSessions
                SET TokenHash = @TokenHash,
                    IssuedOn = SYSUTCDATETIME(),
                    LastSeen = SYSUTCDATETIME(),
                    IsLoggedIn = 1
                WHERE UserSessionId = @UserSessionId;
            ";

            var sqlInsert = @"
                INSERT INTO UserSessions 
                (UserId, TokenHash, DeviceFingerprintHash, IssuedOn, LastSeen, IsLoggedIn)
                VALUES (@UserId, @TokenHash, @DeviceFingerprintHash, SYSUTCDATETIME(), SYSUTCDATETIME(), 1);
            ";

            using var conn = GetConnection();
            await conn.OpenAsync();

            var existingId = await conn.QueryFirstOrDefaultAsync<long?>(
                sqlFind,
                new { cmd.UserId, DeviceFingerprintHash = dfpHash }
            );

            if (existingId.HasValue)
            {
                return await conn.ExecuteAsync(sqlUpdate, new
                {
                    TokenHash = tokenHash,
                    UserSessionId = existingId.Value
                });
            }
            else
            {
                return await conn.ExecuteAsync(sqlInsert, new
                {
                    cmd.UserId,
                    TokenHash = tokenHash,
                    DeviceFingerprintHash = dfpHash
                });
            }
        }

        // -------------------------------------------------------------
        // 2. Get active session for a user
        // -------------------------------------------------------------
        public async Task<UserSessionQuery?> GetActiveSessionByUserAsync(long userId)
        {
            var sql = @"
                SELECT TOP 1 
                    UserSessionId, UserId, TokenHash, DeviceFingerprintHash,
                    IssuedOn, LastSeen, IsLoggedIn
                FROM UserSessions WITH(NOLOCK)
                WHERE UserId = @userId AND IsLoggedIn = 1
                ORDER BY IssuedOn DESC;
            ";

            using var conn = GetConnection();
            return await conn.QueryFirstOrDefaultAsync<UserSessionQuery>(sql, new { userId });
        }

        // -------------------------------------------------------------
        // 3. Get active session by raw token (internally hashed)
        // -------------------------------------------------------------
        public async Task<UserSessionQuery?> GetSessionByTokenAsync(string token)
        {
            string tokenHash = HashHelper.ComputeSha256Hex(token);

            var sql = @"
                SELECT TOP 1 
                    UserSessionId, UserId, TokenHash, DeviceFingerprintHash,
                    IssuedOn, LastSeen, IsLoggedIn
                FROM UserSessions WITH(NOLOCK)
                WHERE TokenHash = @tokenHash AND IsLoggedIn = 1;
            ";

            using var conn = GetConnection();
            return await conn.QueryFirstOrDefaultAsync<UserSessionQuery>(sql, new { tokenHash });
        }

        // -------------------------------------------------------------
        // 4. Update LastSeen (heartbeat)
        // -------------------------------------------------------------
        public async Task<int> UpdateLastSeenByTokenAsync(string token)
        {
            string tokenHash = HashHelper.ComputeSha256Hex(token);

            var sql = @"
                UPDATE UserSessions
                SET LastSeen = SYSUTCDATETIME()
                WHERE TokenHash = @tokenHash AND IsLoggedIn = 1;
            ";

            using var conn = GetConnection();
            return await conn.ExecuteAsync(sql, new { tokenHash });
        }

        // -------------------------------------------------------------
        // 5. Logout user (invalidate session)
        // -------------------------------------------------------------
        public async Task<int> LogoutByTokenAsync(string token)
        {
            string tokenHash = HashHelper.ComputeSha256Hex(token);

            var sql = @"
                UPDATE UserSessions
                SET IsLoggedIn = 0,
                    TokenHash = NULL
                WHERE TokenHash = @tokenHash;
            ";

            using var conn = GetConnection();
            return await conn.ExecuteAsync(sql, new { tokenHash });
        }

        // -------------------------------------------------------------
        // 6. Cleanup: Kill stale sessions
        // -------------------------------------------------------------
        public async Task<int> InvalidateSessionsOlderThanAsync(DateTime cutoff)
        {
            var sql = @"
                UPDATE UserSessions
                SET IsLoggedIn = 0,
                    TokenHash = NULL
                WHERE LastSeen < @cutoff AND IsLoggedIn = 1;
            ";

            using var conn = GetConnection();
            return await conn.ExecuteAsync(sql, new { cutoff });
        }
    }
}

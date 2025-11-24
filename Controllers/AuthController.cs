using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using Sustainable.Models;
using Sustainable.Services;   // ⬅️ IMPORTANT : pour LogService

namespace Sustainable.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            LogService.Log($"AUTH LOGIN ATTEMPT | Username: {request.Username}");

            var user = await connection.QueryFirstOrDefaultAsync<User>(
            @"SELECT  u.iduser   AS IdUser,
                    u.username AS UserName,
                    u.password AS Password,
                    u.mail     AS Mail,
                    u.idrole   AS IdRole,
                    r.rolename AS RoleName
            FROM users u
            JOIN role r ON u.idrole = r.idrole
            WHERE u.username = @UserName AND u.password = @Password",
            new { request.Username, request.Password });

            if (user == null)
            {
                LogService.Log($"AUTH LOGIN FAIL | Username: {request.Username} | Reason: Invalid credentials");
                return Unauthorized(new { message = "Invalid credentials" });
            }

            LogService.Log($"AUTH LOGIN SUCCESS | UserId: {user.IdUser} | Username: {user.UserName}");

            return Ok(new {
                user.IdUser,
                user.UserName,
                user.Mail,
                user.IdRole,
                user.RoleName
            });

        }
    }

}

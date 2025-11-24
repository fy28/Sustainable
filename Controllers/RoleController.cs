using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using Sustainable.Models;
using Sustainable.Helpers;
using Sustainable.Services;   // 🔥 LogService

namespace Sustainable.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IConfiguration _config;

        public RoleController(IConfiguration config)
        {
            _config = config;
        }

        // 📋 Liste des rôles existants
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var roles = await connection.QueryAsync<Role>(
                @"SELECT idrole AS IdRole, rolename AS RoleName 
                  FROM role 
                  ORDER BY rolename"
            );

            LogService.Log("RoleController → GET ALL roles");

            return Ok(roles);
        }

        // ➕ Ajouter un rôle
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var lastId = await connection.ExecuteScalarAsync<string>(
                "SELECT idrole FROM role ORDER BY idrole DESC LIMIT 1"
            );

            var newIdRole = IdGenerator.GenerateId("ROL_", lastId);

            await connection.ExecuteAsync(
                "INSERT INTO role (idrole, rolename) VALUES (@Id, @Name)",
                new { Id = newIdRole, Name = request.RoleName }
            );

            LogService.Log($"RoleController → CREATE Role ID={newIdRole}, Name={request.RoleName}");

            return Ok(new { IdRole = newIdRole, RoleName = request.RoleName });
        }
    }
}

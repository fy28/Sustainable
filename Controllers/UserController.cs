using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using Sustainable.Models;
using Sustainable.Helpers;
using Sustainable.Services;   // 🔥 Pour LogService

namespace Sustainable.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        public UserController(IConfiguration config)
        {
            _config = config;
        }

        // 📋 Liste de tous les utilisateurs
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            var sql = @"
                SELECT 
                    u.iduser AS IdUser,
                    u.username AS UserName,
                    u.prenom AS Prenom,
                    u.datenaissance AS DateNaissance,
                    u.dateentree AS DateEntree,
                    u.mail AS Mail,
                    u.idrole AS IdRole,
                    r.rolename AS RoleName
                FROM users u
                LEFT JOIN role r ON u.idrole = r.idrole
                ORDER BY u.iduser;
            ";

            var users = await connection.QueryAsync<User>(sql);

            LogService.Log("UserController → GET ALL users");

            return Ok(users);
        }

        // 🔍 Détails d’un utilisateur
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var sql = @"
                SELECT 
                    u.iduser AS IdUser,
                    u.username AS UserName,
                    u.prenom AS Prenom,
                    u.datenaissance AS DateNaissance,
                    u.dateentree AS DateEntree,
                    u.mail AS Mail,
                    r.idrole AS IdRole,
                    r.rolename AS RoleName
                FROM users u
                LEFT JOIN role r ON u.idrole = r.idrole
                WHERE u.iduser = @IdUser;
            ";

            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { IdUser = id });

            if (user == null)
            {
                LogService.Log($"UserController → GET user FAILED (id={id})");
                return NotFound(new { message = "Utilisateur introuvable" });
            }

            LogService.Log($"UserController → GET user ID={id}");

            return Ok(user);
        }

        // ➕ Créer un utilisateur
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var lastId = await connection.ExecuteScalarAsync<string>(
                "SELECT iduser FROM users ORDER BY iduser DESC LIMIT 1");

            var newIdUser = IdGenerator.GenerateId("USR_", lastId);

            await connection.ExecuteAsync(@"
                INSERT INTO users 
                (iduser, username, prenom, datenaissance, dateentree, mail, password, idrole)
                VALUES (@IdUser, @UserName, @Prenom, @DateNaissance, @DateEntree, @Mail, @Password, @IdRole)",
                new {
                    IdUser = newIdUser,
                    request.UserName,
                    request.Prenom,
                    request.DateNaissance,
                    request.DateEntree,
                    request.Mail,
                    request.Password,
                    request.IdRole
                });

            LogService.Log($"UserController → CREATE user ID={newIdUser}, username={request.UserName}");

            return Ok(new { message = "Utilisateur ajouté avec succès", IdUser = newIdUser });
        }

        // 🗑️ Supprimer un utilisateur
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            await connection.ExecuteAsync("DELETE FROM users WHERE iduser = @Id", new { Id = id });

            LogService.Log($"UserController → DELETE user ID={id}");

            return Ok(new { message = "Utilisateur supprimé avec succès" });
        }
    }
}

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
    public class PaysController : ControllerBase
    {
        private readonly IConfiguration _config;
        public PaysController(IConfiguration config) => _config = config;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            using var cn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            var rows = await cn.QueryAsync<Pays>(
                "SELECT idpays AS IdPays, nompays AS NomPays FROM pays ORDER BY idpays");

            LogService.Log("PaysController → GET ALL pays");

            return Ok(rows);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Pays req)
        {
            using var cn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var last = await cn.ExecuteScalarAsync<string>(
                "SELECT idpays FROM pays ORDER BY idpays DESC LIMIT 1");

            var newId = IdGenerator.GenerateId("PAY_", last);

            await cn.ExecuteAsync(
                "INSERT INTO pays (idpays, nompays) VALUES (@Id,@Nom)",
                new { Id = newId, Nom = req.NomPays });

            LogService.Log($"PaysController → CREATE Pays ID={newId}, Nom={req.NomPays}");

            return Ok(new { IdPays = newId, NomPays = req.NomPays });
        }
    }
}

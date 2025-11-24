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
    public class UniteController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UniteController(IConfiguration config)
        {
            _config = config;
        }

        // 📋 GET : liste de toutes les unités
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            var unites = await connection.QueryAsync<Unite>(
                @"SELECT idunite AS IdUnite, 
                         nomunite AS NomUnite, 
                         symbole AS Symbole
                  FROM unite
                  ORDER BY idunite;"
            );

            LogService.Log("UniteController → GET ALL unités");

            return Ok(unites);
        }

        // 🔍 GET : une unité par ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            var unite = await connection.QueryFirstOrDefaultAsync<Unite>(
                @"SELECT idunite AS IdUnite, 
                         nomunite AS NomUnite, 
                         symbole AS Symbole
                  FROM unite
                  WHERE idunite = @Id;",
                new { Id = id }
            );

            if (unite == null)
            {
                LogService.Log($"UniteController → GET by ID FAILED (id={id})");
                return NotFound(new { message = "Unité non trouvée" });
            }

            LogService.Log($"UniteController → GET unité ID={id}");

            return Ok(unite);
        }

        // ➕ POST : créer une nouvelle unité
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUniteRequest request)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var lastId = await connection.ExecuteScalarAsync<string>(
                "SELECT idunite FROM unite ORDER BY idunite DESC LIMIT 1"
            );

            var newId = IdGenerator.GenerateId("UNI_", lastId);

            await connection.ExecuteAsync(
                @"INSERT INTO unite (idunite, nomunite, symbole)
                  VALUES (@Id, @Nom, @Symbole)",
                new { Id = newId, Nom = request.NomUnite, request.Symbole }
            );

            LogService.Log($"UniteController → CREATE unité ID={newId}, Nom={request.NomUnite}");

            return Ok(new { IdUnite = newId, message = "Unité créée avec succès" });
        }

        // ✏️ PUT : modifier une unité existante
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateUniteRequest request)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var affected = await connection.ExecuteAsync(
                @"UPDATE unite
                  SET nomunite = @NomUnite, symbole = @Symbole
                  WHERE idunite = @Id",
                new { Id = id, request.NomUnite, request.Symbole }
            );

            if (affected == 0)
            {
                LogService.Log($"UniteController → UPDATE FAILED (id={id})");
                return NotFound(new { message = "Unité non trouvée" });
            }

            LogService.Log($"UniteController → UPDATE unité ID={id}");

            return Ok(new { message = "Unité mise à jour avec succès" });
        }

        // 🗑️ DELETE : supprimer une unité
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var affected = await connection.ExecuteAsync(
                "DELETE FROM unite WHERE idunite = @Id",
                new { Id = id }
            );

            if (affected == 0)
            {
                LogService.Log($"UniteController → DELETE FAILED (id={id})");
                return NotFound(new { message = "Unité non trouvée" });
            }

            LogService.Log($"UniteController → DELETE unité ID={id}");

            return Ok(new { message = "Unité supprimée avec succès" });
        }
    }
}

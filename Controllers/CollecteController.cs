using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using Sustainable.Models;
using Sustainable.Helpers;
using Sustainable.Services;

namespace Sustainable.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollecteController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CollecteController(IConfiguration config)
        {
            _config = config;
        }

        // --------------------------------------------------------
        // 📌 GET ALL COLLECTES
        // --------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            LogService.Log("CollecteController - GET ALL collectes");

            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    c.idcollecte AS IdCollecte,
                    c.idcollecteur AS IdCollecteur,
                    col.nomcollecteur AS NomCollecteur,
                    c.idproduit AS IdProduit,
                    p.nom AS NomProduit,
                    c.quantite AS Quantite,
                    c.prix_kg AS PrixKg, -- 🔥 AJOUT
                    c.datecollecte AS DateCollecte,
                    c.statut AS Statut,
                    c.datearrivee AS DateArrivee,
                    c.derniere_modification AS DerniereModification
                FROM collecte c
                LEFT JOIN collecteur col ON c.idcollecteur = col.idcollecteur
                LEFT JOIN produit p ON c.idproduit = p.idproduit
                ORDER BY c.datecollecte DESC;
            ";

            var result = await conn.QueryAsync<Collecte>(sql);

            LogService.Log($"CollecteController - RETURN {result.Count()} collectes");

            return Ok(result);
        }

        // --------------------------------------------------------
        // 📌 GET COLLECTE BY ID
        // --------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            LogService.Log($"CollecteController - GET collecte by ID={id}");

            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    c.idcollecte AS IdCollecte,
                    c.idcollecteur AS IdCollecteur,
                    col.nomcollecteur AS NomCollecteur,
                    c.idproduit AS IdProduit,
                    p.nom AS NomProduit,
                    c.quantite AS Quantite,
                    c.prix_kg AS PrixKg, -- 🔥 AJOUT
                    c.datecollecte AS DateCollecte,
                    c.statut AS Statut,
                    c.datearrivee AS DateArrivee,
                    c.derniere_modification AS DerniereModification
                FROM collecte c
                LEFT JOIN collecteur col ON c.idcollecteur = col.idcollecteur
                LEFT JOIN produit p ON c.idproduit = p.idproduit
                WHERE c.idcollecte = @Id;
            ";

            var collecte = await conn.QueryFirstOrDefaultAsync<Collecte>(sql, new { Id = id });

            if (collecte == null)
            {
                LogService.Log($"CollecteController - Collecte ID={id} introuvable");
                return NotFound(new { message = "Collecte introuvable" });
            }

            LogService.Log($"CollecteController - Collecte ID={id} trouvée");
            return Ok(collecte);
        }

        // --------------------------------------------------------
        // 📌 CREATE COLLECTE
        // --------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCollecteRequest request)
        {
            LogService.Log($"CollecteController - CREATE collecte | Collecteur={request.IdCollecteur} | Produit={request.IdProduit}");

            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            var lastId = await conn.ExecuteScalarAsync<string>(
                "SELECT idcollecte FROM collecte ORDER BY idcollecte DESC LIMIT 1");

            var newId = IdGenerator.GenerateId("COLL_", lastId);

            await conn.ExecuteAsync(@"
                INSERT INTO collecte (
                    idcollecte, idcollecteur, idproduit,
                    quantite, prix_kg,
                    datecollecte, statut, datearrivee, derniere_modification
                )
                VALUES (
                    @Id, @Collecteur, @Produit,
                    @Quantite, @PrixKg,
                    @DateCollecte, @Statut, @DateArrivee, NOW()
                )",
                new
                {
                    Id = newId,
                    Collecteur = request.IdCollecteur,
                    Produit = request.IdProduit,
                    request.Quantite,
                    request.PrixKg, // 🔥 AJOUT
                    request.DateCollecte,
                    request.Statut,
                    request.DateArrivee
                });

            LogService.Log($"CollecteController - Collecte ID={newId} créée avec succès");

            return Ok(new { IdCollecte = newId, message = "Collecte enregistrée avec succès" });
        }

        // --------------------------------------------------------
        // 📌 UPDATE COLLECTE
        // --------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateCollecteRequest request)
        {
            LogService.Log($"CollecteController - UPDATE collecte ID={id}");

            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            var affected = await conn.ExecuteAsync(@"
                UPDATE collecte
                SET idcollecteur = @Collecteur,
                    idproduit = @Produit,
                    quantite = @Quantite,
                    prix_kg = @PrixKg, -- 🔥 AJOUT
                    datecollecte = @DateCollecte,
                    statut = @Statut,
                    datearrivee = @DateArrivee,
                    derniere_modification = NOW()
                WHERE idcollecte = @Id",
                new
                {
                    Id = id,
                    Collecteur = request.IdCollecteur,
                    Produit = request.IdProduit,
                    request.Quantite,
                    request.PrixKg, // 🔥 AJOUT
                    request.DateCollecte,
                    request.Statut,
                    request.DateArrivee
                });

            if (affected == 0)
            {
                LogService.Log($"CollecteController - UPDATE FAILED (introuvable) ID={id}");
                return NotFound(new { message = "Collecte introuvable" });
            }

            LogService.Log($"CollecteController - UPDATE SUCCESS ID={id}");
            return Ok(new { message = "Collecte mise à jour" });
        }

        // --------------------------------------------------------
        // 📌 DELETE COLLECTE
        // --------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            LogService.Log($"CollecteController - DELETE collecte ID={id}");

            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            var affected = await conn.ExecuteAsync(
                "DELETE FROM collecte WHERE idcollecte = @Id", new { Id = id });

            if (affected == 0)
            {
                LogService.Log($"CollecteController - DELETE FAILED (introuvable) ID={id}");
                return NotFound(new { message = "Collecte introuvable" });
            }

            LogService.Log($"CollecteController - DELETE SUCCESS ID={id}");
            return Ok(new { message = "Collecte supprimée" });
        }
    }
}

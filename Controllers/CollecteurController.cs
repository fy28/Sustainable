using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using Sustainable.Models;
using Sustainable.Helpers;
using Sustainable.Services;   // ⬅️ Pour le service de log

namespace Sustainable.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollecteurController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CollecteurController(IConfiguration config)
        {
            _config = config;
        }

        // 📋 GET — liste des collecteurs
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            LogService.Log("COLLECTEUR GET ALL | Start fetching all collecteurs");

            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var sql = @"
                SELECT 
                    c.idcollecteur        AS IdCollecteur,
                    c.nomcollecteur       AS NomCollecteur,
                    c.contact             AS Contact,
                    c.zone                AS Zone,
                    c.derniere_modification AS Derniere_Modification,
                    cp.idproduit          AS IdProduit,
                    p.nom                 AS NomProduit
                FROM collecteur c
                LEFT JOIN collecteurproduit cp ON c.idcollecteur = cp.idcollecteur
                LEFT JOIN produit p ON cp.idproduit = p.idproduit
                ORDER BY c.idcollecteur;
            ";

            var rows = await conn.QueryAsync(sql);

            LogService.Log($"COLLECTEUR GET ALL | Raw rows fetched: {rows.Count()}");

            var dict = new Dictionary<string, Collecteur>();

            foreach (var r in rows)
            {
                string id = r.idcollecteur;

                if (!dict.TryGetValue(id, out var col))
                {
                    col = new Collecteur
                    {
                        IdCollecteur = r.idcollecteur,
                        NomCollecteur = r.nomcollecteur,
                        Contact = r.contact,
                        Zone = r.zone,
                        DerniereModification = r.derniere_modification,
                        Produits = new List<CollecteurProduitItem>()
                    };

                    dict[id] = col;
                }

                if (r.idproduit != null)
                {
                    col.Produits.Add(new CollecteurProduitItem
                    {
                        IdProduit = r.idproduit,
                        NomProduit = r.nomproduit
                    });
                }
            }

            LogService.Log($"COLLECTEUR GET ALL | Final count={dict.Values.Count}");

            return Ok(dict.Values);
        }

        // 🔍 GET — collecteur par ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            LogService.Log($"COLLECTEUR GET BY ID | Id={id}");

            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var sql = @"
                SELECT 
                    c.idcollecteur        AS IdCollecteur,
                    c.nomcollecteur       AS NomCollecteur,
                    c.contact             AS Contact,
                    c.zone                AS Zone,
                    c.derniere_modification AS Derniere_Modification,
                    cp.idproduit          AS IdProduit,
                    p.nom                 AS NomProduit
                FROM collecteur c
                LEFT JOIN collecteurproduit cp ON c.idcollecteur = cp.idcollecteur
                LEFT JOIN produit p ON cp.idproduit = p.idproduit
                WHERE c.idcollecteur = @Id;
            ";

            var rows = await conn.QueryAsync(sql, new { Id = id });

            if (!rows.Any())
            {
                LogService.Log($"COLLECTEUR GET BY ID | NOT FOUND | Id={id}");
                return NotFound(new { message = "Collecteur introuvable" });
            }

            Collecteur? col = null;

            foreach (var r in rows)
            {
                if (col == null)
                {
                    col = new Collecteur
                    {
                        IdCollecteur = r.idcollecteur,
                        NomCollecteur = r.nomcollecteur,
                        Contact = r.contact,
                        Zone = r.zone,
                        DerniereModification = r.derniere_modification,
                        Produits = new List<CollecteurProduitItem>()
                    };
                }

                if (r.idproduit != null)
                {
                    col.Produits.Add(new CollecteurProduitItem
                    {
                        IdProduit = r.idproduit,
                        NomProduit = r.nomproduit
                    });
                }
            }

            LogService.Log($"COLLECTEUR GET BY ID | SUCCESS | Id={id}");

            return Ok(col);
        }

        // ✏️ UPDATE — modification collecteur
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateCollecteurRequest request)
        {
            LogService.Log($"COLLECTEUR UPDATE ATTEMPT | Id={id} | Name='{request.NomCollecteur}' | Zone='{request.Zone}'");

            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            var affected = await conn.ExecuteAsync(@"
                UPDATE collecteur
                SET nomcollecteur = @Nom,
                    contact = @Contact,
                    zone = @Zone,
                    derniere_modification = NOW()
                WHERE idcollecteur = @Id",
                new { Id = id, Nom = request.NomCollecteur, request.Contact, request.Zone }, tx);

            if (affected == 0)
            {
                LogService.Log($"COLLECTEUR UPDATE FAILED | NOT FOUND | Id={id}");
                await tx.RollbackAsync();
                return NotFound(new { message = "Collecteur introuvable" });
            }

            LogService.Log($"COLLECTEUR UPDATE | Fields updated | Id={id}");

            await conn.ExecuteAsync(
                "DELETE FROM collecteurproduit WHERE idcollecteur = @Id",
                new { Id = id }, tx);

            LogService.Log($"COLLECTEUR UPDATE | Cleared old product links | Id={id}");

            foreach (var prodId in request.ProduitIds.Distinct())
            {
                var lastLinkId = await conn.ExecuteScalarAsync<string>(
                    "SELECT idcollecteurproduit FROM collecteurproduit ORDER BY idcollecteurproduit DESC LIMIT 1",
                    transaction: tx);

                var newLinkId = IdGenerator.GenerateId("CLP_", lastLinkId);

                await conn.ExecuteAsync(@"
                    INSERT INTO collecteurproduit (idcollecteurproduit, idcollecteur, idproduit)
                    VALUES (@Id, @CollecteurId, @ProduitId)",
                    new { Id = newLinkId, CollecteurId = id, ProduitId = prodId }, tx);

                LogService.Log($"COLLECTEUR UPDATE | Added product link | Collecteur={id} | Produit={prodId}");
            }

            await tx.CommitAsync();

            LogService.Log($"COLLECTEUR UPDATE SUCCESS | Id={id}");

            return Ok(new { message = "Collecteur mis à jour avec succès" });
        }
    }
}

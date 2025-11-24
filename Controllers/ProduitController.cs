using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using Sustainable.Models;
using Sustainable.Helpers;
using Sustainable.Services;   // 🔥 Pour le LogService

namespace Sustainable.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProduitController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProduitController(IConfiguration config)
        {
            _config = config;
        }

        // 📋 Récupérer la liste de tous les produits
        [HttpGet]
        public async Task<IActionResult> GetAllProduits()
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var produits = await connection.QueryAsync<Produit>(
                @"SELECT 
                    p.idproduit AS IdProduit,
                    p.nom AS Nom,
                    p.codehs AS CodeHS,
                    p.prix AS Prix,
                    p.idunite AS IdUnite,
                    u.nomunite AS NomUnite,
                    p.dateajout AS DateAjout,
                    p.derniere_modification AS DerniereModification
                FROM produit p
                LEFT JOIN unite u ON p.idunite = u.idunite
                ORDER BY p.idproduit;"
            );

            LogService.Log("ProduitController → GET ALL produits demandé");

            return Ok(produits);
        }

        // ➕ Créer un nouveau produit
        [HttpPost]
        public async Task<IActionResult> CreateProduit([FromBody] CreateProduitRequest request)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var lastId = await connection.ExecuteScalarAsync<string>(
                "SELECT idproduit FROM produit ORDER BY idproduit DESC LIMIT 1");

            var newId = IdGenerator.GenerateId("PRO_", lastId);

            await connection.ExecuteAsync(
                @"INSERT INTO produit (idproduit, nom, codehs, prix, idunite, dateajout, derniere_modification)
                VALUES (@Id, @Nom, @CodeHS, @Prix, @IdUnite, NOW(), NOW())",
                new { Id = newId, request.Nom, request.CodeHS, request.Prix, request.IdUnite }
            );

            LogService.Log($"ProduitController → CREATE produit ID={newId}, Nom={request.Nom}");

            return Ok(new { IdProduit = newId, request.Nom });
        }

        // ✏️ Modifier un produit
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduit(string id, [FromBody] CreateProduitRequest request)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var affected = await connection.ExecuteAsync(
                @"UPDATE produit
                  SET nom = @Nom, 
                      codehs = @CodeHS, 
                      prix = @Prix, 
                      idunite = @IdUnite, 
                      derniere_modification = NOW()
                  WHERE idproduit = @IdProduit",
                new { IdProduit = id, request.Nom, request.CodeHS, request.Prix, request.IdUnite }
            );

            if (affected == 0)
            {
                LogService.Log($"ProduitController → UPDATE FAILED (produit introuvable) ID={id}");
                return NotFound(new { message = "Produit introuvable" });
            }

            LogService.Log($"ProduitController → UPDATE produit ID={id}, NouveauNom={request.Nom}");

            return Ok(new { message = "Produit mis à jour" });
        }

        // 🗑️ Supprimer un produit
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduit(string id)
        {
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var affected = await connection.ExecuteAsync(
                "DELETE FROM produit WHERE idproduit = @Id", new { Id = id });

            if (affected == 0)
            {
                LogService.Log($"ProduitController → DELETE FAILED (produit introuvable) ID={id}");
                return NotFound(new { message = "Produit non trouvé" });
            }

            LogService.Log($"ProduitController → DELETE produit ID={id}");

            return Ok(new { message = "Produit supprimé avec succès" });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using Sustainable.Models;
using Sustainable.Helpers;

namespace Sustainable.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DocumentController(IConfiguration config)
        {
            _config = config;
        }

        // ➕ AJOUTER UN DOCUMENT
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDocumentRequest req)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                var lastId = await conn.ExecuteScalarAsync<string>(
                    "SELECT iddocument FROM document ORDER BY iddocument DESC LIMIT 1",
                    transaction: tx);

                var newDocId = IdGenerator.GenerateId("DOC_", lastId);

                await conn.ExecuteAsync(
                    "INSERT INTO document (iddocument, nomdocument) VALUES (@Id, @Nom)",
                    new { Id = newDocId, Nom = req.NomDocument }, tx);

                // 🔹 Documents-pays (documentpays)
                foreach (var idPays in req.PaysSelectionnes)
                {
                    var lastDP = await conn.ExecuteScalarAsync<string>(
                        "SELECT iddocumentpays FROM documentpays ORDER BY iddocumentpays DESC LIMIT 1",
                        transaction: tx);

                    var newDP = IdGenerator.GenerateId("DPA_", lastDP);

                    await conn.ExecuteAsync(
                        "INSERT INTO documentpays (iddocumentpays, iddocument, idpays) VALUES (@Id, @Doc, @Pays)",
                        new { Id = newDP, Doc = newDocId, Pays = idPays }, tx);
                }

                // 🔹 Documents-produits (produitdocument)
                foreach (var idProduit in req.ProduitsSelectionnes)
                {
                    var lastPD = await conn.ExecuteScalarAsync<string>(
                        "SELECT idproduitdocument FROM produitdocument ORDER BY idproduitdocument DESC LIMIT 1",
                        transaction: tx);

                    var newPD = IdGenerator.GenerateId("PDR_", lastPD);

                    await conn.ExecuteAsync(
                        "INSERT INTO produitdocument (idproduitdocument, idproduit, iddocument) VALUES (@Id, @Prod, @Doc)",
                        new { Id = newPD, Prod = idProduit, Doc = newDocId }, tx);
                }

                await tx.CommitAsync();
                return Ok(new { message = "Document créé", id = newDocId });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 📋 LISTE COMPLETE DES DOCUMENTS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var sql = @"
                SELECT d.iddocument, d.nomdocument,
                       p.idpays, p.nompays,
                       pr.idproduit, pr.nom AS nomproduit
                FROM document d
                LEFT JOIN documentpays dp ON dp.iddocument = d.iddocument
                LEFT JOIN pays p ON p.idpays = dp.idpays
                LEFT JOIN produitdocument pd ON pd.iddocument = d.iddocument
                LEFT JOIN produit pr ON pr.idproduit = pd.idproduit
                ORDER BY d.iddocument;
            ";

            var rows = await conn.QueryAsync(sql);

            var dict = new Dictionary<string, Document>();

            foreach (var r in rows)
            {
                if (!dict.TryGetValue((string)r.iddocument, out var doc))
                {
                    doc = new Document
                    {
                        IdDocument = r.iddocument,
                        NomDocument = r.nomdocument
                    };
                    dict[doc.IdDocument] = doc;
                }

                if (r.idpays != null)
                    doc.PaysAssocies.Add(new PaysAssocie { IdPays = r.idpays, NomPays = r.nompays });

                if (r.idproduit != null)
                    doc.ProduitsAssocies.Add(new ProduitAssocie { IdProduit = r.idproduit, NomProduit = r.nomproduit });
            }

            return Ok(dict.Values);
        }

        // 🔍 OBTENIR UN DOCUMENT PAR ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var sql = @"
                SELECT d.iddocument, d.nomdocument,
                       p.idpays, p.nompays,
                       pr.idproduit, pr.nom AS nomproduit
                FROM document d
                LEFT JOIN documentpays dp ON dp.iddocument = d.iddocument
                LEFT JOIN pays p ON p.idpays = dp.idpays
                LEFT JOIN produitdocument pd ON pd.iddocument = d.iddocument
                LEFT JOIN produit pr ON pr.idproduit = pd.idproduit
                WHERE d.iddocument = @Id;
            ";

            var rows = await conn.QueryAsync(sql, new { Id = id });

            if (!rows.Any())
                return NotFound(new { message = "Document introuvable" });

            var first = rows.First();

            var doc = new Document
            {
                IdDocument = first.iddocument,
                NomDocument = first.nomdocument
            };

            foreach (var r in rows)
            {
                if (r.idpays != null)
                    doc.PaysAssocies.Add(new PaysAssocie { IdPays = r.idpays, NomPays = r.nompays });

                if (r.idproduit != null)
                    doc.ProduitsAssocies.Add(new ProduitAssocie { IdProduit = r.idproduit, NomProduit = r.nomproduit });
            }

            return Ok(doc);
        }

        // 📝 MODIFIER UN DOCUMENT
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateDocumentRequest req)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                await conn.ExecuteAsync(
                    "UPDATE document SET nomdocument = @Nom WHERE iddocument = @Id",
                    new { Nom = req.NomDocument, Id = id }, tx);

                await conn.ExecuteAsync("DELETE FROM documentpays WHERE iddocument = @Id", new { Id = id }, tx);
                await conn.ExecuteAsync("DELETE FROM produitdocument WHERE iddocument = @Id", new { Id = id }, tx);

                foreach (var p in req.PaysSelectionnes)
                {
                    var last = await conn.ExecuteScalarAsync<string>(
                        "SELECT iddocumentpays FROM documentpays ORDER BY iddocumentpays DESC LIMIT 1", transaction: tx);

                    var newId = IdGenerator.GenerateId("DPA_", last);

                    await conn.ExecuteAsync(
                        "INSERT INTO documentpays (iddocumentpays, iddocument, idpays) VALUES (@NewId, @Doc, @Pays)",
                        new { NewId = newId, Doc = id, Pays = p }, tx);
                }

                foreach (var prod in req.ProduitsSelectionnes)
                {
                    var last = await conn.ExecuteScalarAsync<string>(
                        "SELECT idproduitdocument FROM produitdocument ORDER BY idproduitdocument DESC LIMIT 1", transaction: tx);

                    var newId = IdGenerator.GenerateId("PDR_", last);

                    await conn.ExecuteAsync(
                        "INSERT INTO produitdocument (idproduitdocument, idproduit, iddocument) VALUES (@NewId, @Prod, @Doc)",
                        new { NewId = newId, Prod = prod, Doc = id }, tx);
                }

                await tx.CommitAsync();
                return Ok(new { message = "Document mis à jour" });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ❌ SUPPRESSION
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            await conn.ExecuteAsync("DELETE FROM documentpays WHERE iddocument = @Id", new { Id = id });
            await conn.ExecuteAsync("DELETE FROM produitdocument WHERE iddocument = @Id", new { Id = id });
            await conn.ExecuteAsync("DELETE FROM document WHERE iddocument = @Id", new { Id = id });

            return Ok(new { message = "Document supprimé" });
        }

        // --------------------------------------------------------
        // 🔹 RÈGLES PRODUIT + PAYS (produitdocumentpays)
        // --------------------------------------------------------

        // 📋 Lister toutes les règles
        [HttpGet("rules")]
        public async Task<IActionResult> GetRules()
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var sql = @"
                SELECT 
                    pdp.idproduitdocumentpays AS IdProduitDocumentPays,
                    pr.idproduit              AS IdProduit,
                    pr.nom                    AS NomProduit,
                    d.iddocument              AS IdDocument,
                    d.nomdocument             AS NomDocument,
                    p.idpays                  AS IdPays,
                    p.nompays                 AS NomPays
                FROM produitdocumentpays pdp
                JOIN produitdocument pd ON pdp.idproduitdocument = pd.idproduitdocument
                JOIN produit pr          ON pr.idproduit = pd.idproduit
                JOIN document d          ON d.iddocument = pd.iddocument
                JOIN pays p              ON p.idpays = pdp.idpays
                ORDER BY pr.nom, p.nompays, d.nomdocument;
            ";

            var rules = await conn.QueryAsync<ProduitPaysRule>(sql);
            return Ok(rules);
        }

        // ➕ Ajouter une règle Produit + Pays
        [HttpPost("rules")]
        public async Task<IActionResult> AddRule([FromBody] CreateRuleRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.IdProduit) ||
                string.IsNullOrWhiteSpace(req.IdDocument) ||
                string.IsNullOrWhiteSpace(req.IdPays))
            {
                return BadRequest(new { message = "Produit, document et pays sont obligatoires" });
            }

            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1️⃣ On s'assure qu'il existe un produitdocument (produit + document)
                var idProdDoc = await conn.ExecuteScalarAsync<string>(
                    "SELECT idproduitdocument FROM produitdocument WHERE idproduit = @Prod AND iddocument = @Doc",
                    new { Prod = req.IdProduit, Doc = req.IdDocument }, tx);

                if (idProdDoc == null)
                {
                    var lastPD = await conn.ExecuteScalarAsync<string>(
                        "SELECT idproduitdocument FROM produitdocument ORDER BY idproduitdocument DESC LIMIT 1",
                        transaction: tx);

                    idProdDoc = IdGenerator.GenerateId("PDR_", lastPD);

                    await conn.ExecuteAsync(
                        "INSERT INTO produitdocument (idproduitdocument, idproduit, iddocument) VALUES (@Id, @Prod, @Doc)",
                        new { Id = idProdDoc, Prod = req.IdProduit, Doc = req.IdDocument }, tx);
                }

                // 2️⃣ Vérifier si la règle existe déjà
                var exists = await conn.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*) FROM produitdocumentpays 
                      WHERE idproduitdocument = @PD AND idpays = @Pays",
                    new { PD = idProdDoc, Pays = req.IdPays }, tx);

                if (exists > 0)
                {
                    await tx.RollbackAsync();
                    return Conflict(new { message = "Cette règle existe déjà" });
                }

                // 3️⃣ Créer la règle produitdocumentpays
                var lastPdp = await conn.ExecuteScalarAsync<string>(
                    "SELECT idproduitdocumentpays FROM produitdocumentpays ORDER BY idproduitdocumentpays DESC LIMIT 1",
                    transaction: tx);

                var newPdpId = IdGenerator.GenerateId("PDP_", lastPdp);

                await conn.ExecuteAsync(
                    "INSERT INTO produitdocumentpays (idproduitdocumentpays, idproduitdocument, idpays) VALUES (@Id, @PD, @Pays)",
                    new { Id = newPdpId, PD = idProdDoc, Pays = req.IdPays }, tx);

                await tx.CommitAsync();
                return Ok(new { message = "Règle ajoutée", id = newPdpId });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ❌ Supprimer une règle Produit + Pays
        [HttpDelete("rules/{id}")]
        public async Task<IActionResult> DeleteRule(string id)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var rows = await conn.ExecuteAsync(
                "DELETE FROM produitdocumentpays WHERE idproduitdocumentpays = @Id",
                new { Id = id });

            if (rows == 0)
                return NotFound(new { message = "Règle introuvable" });

            return Ok(new { message = "Règle supprimée" });
        }
    }
}
// using Microsoft.AspNetCore.Mvc;
// using Npgsql;
// using Dapper;
// using Sustainable.Models;
// using Sustainable.Helpers;

// namespace Sustainable.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class DocumentController : ControllerBase
//     {
//         private readonly IConfiguration _config;

//         public DocumentController(IConfiguration config)
//         {
//             _config = config;
//         }

//         // ➕ AJOUTER UN DOCUMENT
//         [HttpPost]
//         public async Task<IActionResult> Create([FromBody] CreateDocumentRequest req)
//         {
//             using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
//             await conn.OpenAsync();

//             using var tx = conn.BeginTransaction();

//             try
//             {
//                 // 1️⃣ ID pour document
//                 var lastId = await conn.ExecuteScalarAsync<string>(
//                     "SELECT iddocument FROM document ORDER BY iddocument DESC LIMIT 1",
//                     transaction: tx);

//                 var newDocId = IdGenerator.GenerateId("DOC_", lastId);

//                 await conn.ExecuteAsync(
//                     "INSERT INTO document (iddocument, nomdocument) VALUES (@Id, @Nom)",
//                     new { Id = newDocId, Nom = req.NomDocument }, tx);

//                 // 2️⃣ Si pays associés → documentpays
//                 foreach (var idPays in req.PaysSelectionnes)
//                 {
//                     var lastDP = await conn.ExecuteScalarAsync<string>(
//                         "SELECT iddocumentpays FROM documentpays ORDER BY iddocumentpays DESC LIMIT 1",
//                         transaction: tx);

//                     var newDP = IdGenerator.GenerateId("DPA_", lastDP);

//                     await conn.ExecuteAsync(
//                         "INSERT INTO documentpays (iddocumentpays, iddocument, idpays) VALUES (@Id, @Doc, @Pays)",
//                         new { Id = newDP, Doc = newDocId, Pays = idPays }, tx);
//                 }

//                 // 3️⃣ Si produits associés → produitdocument
//                 foreach (var idProduit in req.ProduitsSelectionnes)
//                 {
//                     var lastPD = await conn.ExecuteScalarAsync<string>(
//                         "SELECT idproduitdocument FROM produitdocument ORDER BY idproduitdocument DESC LIMIT 1",
//                         transaction: tx);

//                     var newPD = IdGenerator.GenerateId("PDR_", lastPD);

//                     await conn.ExecuteAsync(
//                         "INSERT INTO produitdocument (idproduitdocument, idproduit, iddocument) VALUES (@Id, @Prod, @Doc)",
//                         new { Id = newPD, Prod = idProduit, Doc = newDocId }, tx);
//                 }

//                 await tx.CommitAsync();
//                 return Ok(new { message = "Document créé", id = newDocId });
//             }
//             catch (Exception ex)
//             {
//                 await tx.RollbackAsync();
//                 return StatusCode(500, new { error = ex.Message });
//             }
//         }

//         // 📋 LISTE COMPLETE DES DOCUMENTS + leurs pays + produits
//         [HttpGet]
//         public async Task<IActionResult> GetAll()
//         {
//             using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

//             var sql = @"
//                 SELECT d.iddocument, d.nomdocument,
//                        p.idpays, p.nompays,
//                        pr.idproduit, pr.nom AS nomproduit
//                 FROM document d
//                 LEFT JOIN documentpays dp ON dp.iddocument = d.iddocument
//                 LEFT JOIN pays p ON p.idpays = dp.idpays
//                 LEFT JOIN produitdocument pd ON pd.iddocument = d.iddocument
//                 LEFT JOIN produit pr ON pr.idproduit = pd.idproduit
//                 ORDER BY d.iddocument;
//             ";

//             var rows = await conn.QueryAsync(sql);

//             var dict = new Dictionary<string, Document>();

//             foreach (var r in rows)
//             {
//                 if (!dict.TryGetValue((string)r.iddocument, out var doc))
//                 {
//                     doc = new Document
//                     {
//                         IdDocument = r.iddocument,
//                         NomDocument = r.nomdocument
//                     };
//                     dict[doc.IdDocument] = doc;
//                 }

//                 if (r.idpays != null)
//                 {
//                     doc.PaysAssocies.Add(new PaysAssocie
//                     {
//                         IdPays = r.idpays,
//                         NomPays = r.nompays
//                     });
//                 }

//                 if (r.idproduit != null)
//                 {
//                     doc.ProduitsAssocies.Add(new ProduitAssocie
//                     {
//                         IdProduit = r.idproduit,
//                         NomProduit = r.nomproduit
//                     });
//                 }
//             }

//             return Ok(dict.Values);
//         }

//         // 🔍 GET : UN DOCUMENT PAR ID (pour l'écran d'édition)
//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetById(string id)
//         {
//             using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

//             var sql = @"
//                 SELECT d.iddocument, d.nomdocument,
//                        p.idpays, p.nompays,
//                        pr.idproduit, pr.nom AS nomproduit
//                 FROM document d
//                 LEFT JOIN documentpays dp ON dp.iddocument = d.iddocument
//                 LEFT JOIN pays p ON p.idpays = dp.idpays
//                 LEFT JOIN produitdocument pd ON pd.iddocument = d.iddocument
//                 LEFT JOIN produit pr ON pr.idproduit = pd.idproduit
//                 WHERE d.iddocument = @Id;
//             ";

//             var rows = await conn.QueryAsync(sql, new { Id = id });

//             if (!rows.Any())
//                 return NotFound(new { message = "Document introuvable" });

//             Document? doc = null;

//             foreach (var r in rows)
//             {
//                 if (doc == null)
//                 {
//                     doc = new Document
//                     {
//                         IdDocument = r.iddocument,
//                         NomDocument = r.nomdocument,
//                         PaysAssocies = new List<PaysAssocie>(),
//                         ProduitsAssocies = new List<ProduitAssocie>()
//                     };
//                 }

//                 if (r.idpays != null)
//                 {
//                     doc.PaysAssocies.Add(new PaysAssocie
//                     {
//                         IdPays = r.idpays,
//                         NomPays = r.nompays
//                     });
//                 }

//                 if (r.idproduit != null)
//                 {
//                     doc.ProduitsAssocies.Add(new ProduitAssocie
//                     {
//                         IdProduit = r.idproduit,
//                         NomProduit = r.nomproduit
//                     });
//                 }
//             }

//             return Ok(doc);
//         }

//         // ❌ SUPPRIMER UN DOCUMENT
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> Delete(string id)
//         {
//             using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

//             await conn.ExecuteAsync("DELETE FROM documentpays WHERE iddocument = @Id", new { Id = id });
//             await conn.ExecuteAsync("DELETE FROM produitdocument WHERE iddocument = @Id", new { Id = id });
//             await conn.ExecuteAsync("DELETE FROM document WHERE iddocument = @Id", new { Id = id });

//             return Ok(new { message = "Document supprimé" });
//         }

//         // ✏️ METTRE A JOUR UN DOCUMENT (+ ses liens pays / produits)
//         [HttpPut("{id}")]
//         public async Task<IActionResult> Update(string id, [FromBody] CreateDocumentRequest req)
//         {
//             using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
//             await conn.OpenAsync();

//             using var tx = conn.BeginTransaction();

//             try
//             {
//                 await conn.ExecuteAsync(
//                     "UPDATE document SET nomdocument = @Nom WHERE iddocument = @Id",
//                     new { Nom = req.NomDocument, Id = id }, tx);

//                 await conn.ExecuteAsync("DELETE FROM documentpays WHERE iddocument = @Id", new { Id = id }, tx);
//                 await conn.ExecuteAsync("DELETE FROM produitdocument WHERE iddocument = @Id", new { Id = id }, tx);

//                 foreach (var p in req.PaysSelectionnes)
//                 {
//                     var last = await conn.ExecuteScalarAsync<string>(
//                         "SELECT iddocumentpays FROM documentpays ORDER BY iddocumentpays DESC LIMIT 1",
//                         transaction: tx);
//                     var newId = IdGenerator.GenerateId("DPA_", last);

//                     await conn.ExecuteAsync(
//                         "INSERT INTO documentpays (iddocumentpays, iddocument, idpays) VALUES (@NewId, @Doc, @Pays)",
//                         new { NewId = newId, Doc = id, Pays = p }, tx);
//                 }

//                 foreach (var prod in req.ProduitsSelectionnes)
//                 {
//                     var last = await conn.ExecuteScalarAsync<string>(
//                         "SELECT idproduitdocument FROM produitdocument ORDER BY idproduitdocument DESC LIMIT 1",
//                         transaction: tx);
//                     var newId = IdGenerator.GenerateId("PDR_", last);

//                     await conn.ExecuteAsync(
//                         "INSERT INTO produitdocument (idproduitdocument, idproduit, iddocument) VALUES (@NewId, @Prod, @Doc)",
//                         new { NewId = newId, Prod = prod, Doc = id }, tx);
//                 }

//                 await tx.CommitAsync();
//                 return Ok(new { message = "Document mis à jour" });
//             }
//             catch (Exception ex)
//             {
//                 await tx.RollbackAsync();
//                 return StatusCode(500, new { error = ex.Message });
//             }
//         }

//         // 🔍🔹 GET : DOCUMENTS configurés pour un couple (Produit, Pays)
//         // Ex : /api/document/produit-pays?idProduit=PRO_002&idPays=PAY_001
//         [HttpGet("produit-pays")]
//         public async Task<IActionResult> GetByProduitPays([FromQuery] string idProduit, [FromQuery] string idPays)
//         {
//             using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

//             var sql = @"
//                 SELECT DISTINCT d.iddocument AS IdDocument, d.nomdocument AS NomDocument
//                 FROM produitdocument pd
//                 JOIN document d ON d.iddocument = pd.iddocument
//                 JOIN produitdocumentpays pdp ON pdp.idproduitdocument = pd.idproduitdocument
//                 WHERE pd.idproduit = @Prod
//                   AND pdp.idpays = @Pays;
//             ";

//             var docs = await conn.QueryAsync<DocItem>(sql, new { Prod = idProduit, Pays = idPays });

//             return Ok(docs);
//         }

//         // 💾🔹 POST : enregistrer les documents pour un couple (Produit, Pays)
//         // Body : { idProduit, idPays, idDocuments: [ "DOC_001", "DOC_002", ... ] }
//         [HttpPost("produit-pays")]
//         public async Task<IActionResult> SaveProduitPaysDocs([FromBody] ProductCountryDocumentRequest req)
//         {
//             if (string.IsNullOrWhiteSpace(req.IdProduit) || string.IsNullOrWhiteSpace(req.IdPays))
//             {
//                 return BadRequest(new { message = "IdProduit et IdPays sont obligatoires." });
//             }

//             using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
//             await conn.OpenAsync();
//             using var tx = conn.BeginTransaction();

//             try
//             {
//                 // 1️⃣ On récupère tous les idproduitdocument de ce produit
//                 var existingPd = (await conn.QueryAsync<string>(
//                     "SELECT idproduitdocument FROM produitdocument WHERE idproduit = @Prod",
//                     new { Prod = req.IdProduit }, tx)).ToList();

//                 if (existingPd.Any())
//                 {
//                     // 2️⃣ On supprime les anciens liens produit-pays pour ce produit & ce pays
//                     await conn.ExecuteAsync(@"
//                         DELETE FROM produitdocumentpays 
//                         WHERE idpays = @Pays 
//                           AND idproduitdocument = ANY(@PdList);
//                     ", new { Pays = req.IdPays, PdList = existingPd.ToArray() }, tx);
//                 }

//                 var pdIds = new List<string>();

//                 // 3️⃣ Pour chaque document demandé, on s'assure qu'il existe un produitdocument
//                 foreach (var idDoc in req.IdDocuments.Distinct())
//                 {
//                     if (string.IsNullOrWhiteSpace(idDoc)) continue;

//                     var pdId = await conn.ExecuteScalarAsync<string>(@"
//                         SELECT idproduitdocument 
//                         FROM produitdocument 
//                         WHERE idproduit = @Prod AND iddocument = @Doc
//                         LIMIT 1;
//                     ", new { Prod = req.IdProduit, Doc = idDoc }, tx);

//                     if (pdId == null)
//                     {
//                         var lastPD = await conn.ExecuteScalarAsync<string>(
//                             "SELECT idproduitdocument FROM produitdocument ORDER BY idproduitdocument DESC LIMIT 1",
//                             transaction: tx);

//                         pdId = IdGenerator.GenerateId("PDR_", lastPD);

//                         await conn.ExecuteAsync(@"
//                             INSERT INTO produitdocument (idproduitdocument, idproduit, iddocument)
//                             VALUES (@Id, @Prod, @Doc);
//                         ", new { Id = pdId, Prod = req.IdProduit, Doc = idDoc }, tx);
//                     }

//                     pdIds.Add(pdId);
//                 }

//                 // 4️⃣ On insère les nouveaux liens dans produitdocumentpays
//                 foreach (var pdId in pdIds.Distinct())
//                 {
//                     var lastPdp = await conn.ExecuteScalarAsync<string>(
//                         "SELECT idproduitdocumentpays FROM produitdocumentpays ORDER BY idproduitdocumentpays DESC LIMIT 1",
//                         transaction: tx);

//                     var newPdpId = IdGenerator.GenerateId("PDP_", lastPdp);

//                     await conn.ExecuteAsync(@"
//                         INSERT INTO produitdocumentpays (idproduitdocumentpays, idproduitdocument, idpays)
//                         VALUES (@Id, @Pd, @Pays);
//                     ", new { Id = newPdpId, Pd = pdId, Pays = req.IdPays }, tx);
//                 }

//                 await tx.CommitAsync();
//                 return Ok(new { message = "Règles produit/pays mises à jour." });
//             }
//             catch (Exception ex)
//             {
//                 await tx.RollbackAsync();
//                 return StatusCode(500, new { error = ex.Message });
//             }
//         }
//     }
// }

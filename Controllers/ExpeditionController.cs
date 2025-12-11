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
    public class ExpeditionController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ExpeditionController(IConfiguration config)
        {
            _config = config;
        }

        // --------------------------------------------------------
        // 📌 GET ALL EXPEDITIONS
        // --------------------------------------------------------
       // --------------------------------------------------------
// 📌 GET ALL EXPEDITIONS
// --------------------------------------------------------
[HttpGet]
public async Task<IActionResult> GetAll()
{
    using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

    var sql = @"
        SELECT 
            e.idexpedition      AS IdExpedition,
            e.idclient          AS IdClient,
            c.nomclient         AS ClientName,
            e.idpaysdestination AS IdPaysDestination,
            p.nompays           AS NomPaysDestination,
            e.datelivraison     AS DateLivraison,
            e.datecreation      AS DateCreation,

            ep.idexpeditionproduit AS IdExpeditionProduit,
            pr.idproduit           AS IdProduit,
            pr.nom                 AS NomProduit,
            ep.quantite            AS Quantite,
            ep.unite               AS Unite,

            ed.idexpeditiondocument AS IdExpeditionDocument,
            d.iddocument            AS IdDocument,
            d.nomdocument           AS NomDocument
        FROM expedition e
        LEFT JOIN client c             ON e.idclient = c.idclient
        LEFT JOIN pays p               ON e.idpaysdestination = p.idpays
        LEFT JOIN expeditionproduit ep ON e.idexpedition = ep.idexpedition
        LEFT JOIN produit pr           ON pr.idproduit = ep.idproduit
        LEFT JOIN expeditiondocument ed ON e.idexpedition = ed.idexpedition
        LEFT JOIN document d           ON ed.iddocument = d.iddocument
        ORDER BY e.datecreation DESC, e.idexpedition;
    ";

    // ✅ Correction ici
    var rows = await conn.QueryAsync(sql);

    var dict = new Dictionary<string, Expedition>();

    foreach (var r in rows)
    {
        string idExp = r.idexpedition;

        if (!dict.TryGetValue(idExp, out var exp))
        {
            exp = new Expedition
            {
                IdExpedition = r.idexpedition,
                IdClient = r.idclient,
                ClientName = r.clientname,
                IdPaysDestination = r.idpaysdestination,
                NomPaysDestination = r.nompaysdestination,
                DateLivraison = r.datelivraison,
                DateCreation = r.datecreation,
                Produits = new List<ExpeditionProduit>(),
                Documents = new List<DocItem>()
            };
            dict[idExp] = exp;
        }

        if (r.idexpeditionproduit != null)
        {
            exp.Produits.Add(new ExpeditionProduit
            {
                IdExpeditionProduit = r.idexpeditionproduit,
                IdProduit = r.idproduit,
                NomProduit = r.nomproduit,
                Quantite = r.quantite,
                Unite = r.unite
            });
        }

        if (r.iddocument != null &&
            !exp.Documents.Any(d => d.IdDocument == (string)r.iddocument))
        {
            exp.Documents.Add(new DocItem
            {
                IdDocument = r.iddocument,
                NomDocument = r.nomdocument
            });
        }
    }

    return Ok(dict.Values);
}


        // --------------------------------------------------------
        // 📌 GET EXPEDITION BY ID
        // --------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var sql = @"
                SELECT 
                    e.idexpedition      AS IdExpedition,
                    e.idclient          AS IdClient,
                    c.nomclient         AS ClientName,
                    e.idpaysdestination AS IdPaysDestination,
                    p.nompays           AS NomPaysDestination,
                    e.datelivraison     AS DateLivraison,
                    e.datecreation      AS DateCreation,

                    ep.idexpeditionproduit AS IdExpeditionProduit,
                    pr.idproduit           AS IdProduit,
                    pr.nom                 AS NomProduit,
                    ep.quantite            AS Quantite,
                    ep.unite               AS Unite,

                    ed.idexpeditiondocument AS IdExpeditionDocument,
                    d.iddocument            AS IdDocument,
                    d.nomdocument           AS NomDocument
                FROM expedition e
                LEFT JOIN client c             ON e.idclient = c.idclient
                LEFT JOIN pays p               ON e.idpaysdestination = p.idpays
                LEFT JOIN expeditionproduit ep ON e.idexpedition = ep.idexpedition
                LEFT JOIN produit pr           ON pr.idproduit = ep.idproduit
                LEFT JOIN expeditiondocument ed ON e.idexpedition = ed.idexpedition
                LEFT JOIN document d           ON ed.iddocument = d.iddocument
                WHERE e.idexpedition = @Id;
            ";

            var rows = await conn.QueryAsync(sql, new { Id = id });

            if (!rows.Any())
                return NotFound(new { message = "Expédition introuvable" });

            Expedition? exp = null;

            foreach (var r in rows)
            {
                if (exp == null)
                {
                    exp = new Expedition
                    {
                        IdExpedition = r.idexpedition,
                        IdClient = r.idclient,
                        ClientName = r.clientname,
                        IdPaysDestination = r.idpaysdestination,
                        NomPaysDestination = r.nompaysdestination,
                        DateLivraison = r.datelivraison,
                        DateCreation = r.datecreation,
                        Produits = new List<ExpeditionProduit>(),
                        Documents = new List<DocItem>()
                    };
                }

                if (r.idexpeditionproduit != null)
                {
                    exp.Produits.Add(new ExpeditionProduit
                    {
                        IdExpeditionProduit = r.idexpeditionproduit,
                        IdProduit = r.idproduit,
                        NomProduit = r.nomproduit,
                        Quantite = r.quantite,
                        Unite = r.unite
                    });
                }

                if (r.iddocument != null &&
                    !exp.Documents.Any(d => d.IdDocument == (string)r.iddocument))
                {
                    exp.Documents.Add(new DocItem
                    {
                        IdDocument = r.iddocument,
                        NomDocument = r.nomdocument
                    });
                }
            }

            return Ok(exp);
        }

        // --------------------------------------------------------
        // 📌 GET DOCUMENTS REQUIS POUR PRODUIT + PAYS
        // --------------------------------------------------------
        [HttpGet("docs")]
        public async Task<IActionResult> GetDocuments([FromQuery] string produit, [FromQuery] string pays)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var all = new List<DocItem>();
            var seen = new HashSet<string>();

            // 1️⃣ Docs pays
            var sqlPaysGlobal = @"
                SELECT d.iddocument, d.nomdocument
                FROM documentpays dp
                JOIN document d ON d.iddocument = dp.iddocument
                WHERE dp.idpays = @Pays;
            ";

            var docsPays = await conn.QueryAsync<DocItem>(sqlPaysGlobal, new { Pays = pays });

            foreach (var d in docsPays)
                if (seen.Add(d.IdDocument!))
                    all.Add(d);

            // 2️⃣ Docs produit + pays
            var sqlProdPays = @"
                SELECT d.iddocument, d.nomdocument
                FROM produitdocument pd
                JOIN produitdocumentpays pdp ON pdp.idproduitdocument = pd.idproduitdocument
                JOIN document d ON d.iddocument = pd.iddocument
                WHERE pd.idproduit = @Prod
                AND pdp.idpays = @Pays;
            ";

            var docsProdPays = (await conn.QueryAsync<DocItem>(
                sqlProdPays, new { Prod = produit, Pays = pays })).ToList();

            if (docsProdPays.Any())
            {
                foreach (var d in docsProdPays)
                    if (seen.Add(d.IdDocument!))
                        all.Add(d);
            }
            else
            {
                // 3️⃣ Sinon docs par produit
                var sqlProdDefault = @"
                    SELECT d.iddocument, d.nomdocument
                    FROM produitdocument pd
                    JOIN document d ON d.iddocument = pd.iddocument
                    WHERE pd.idproduit = @Prod;
                ";

                var docsProd = await conn.QueryAsync<DocItem>(
                    sqlProdDefault, new { Prod = produit });

                foreach (var d in docsProd)
                    if (seen.Add(d.IdDocument!))
                        all.Add(d);
            }

            return Ok(all);
        }

        // --------------------------------------------------------
        // 📌 CREATION EXPEDITION
        // --------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExpeditionRequest request)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1️⃣ Générer ID expédition
                var lastId = await conn.ExecuteScalarAsync<string>(
                    "SELECT idexpedition FROM expedition ORDER BY idexpedition DESC LIMIT 1",
                    transaction: tx);

                var newIdExp = IdGenerator.GenerateId("EXP_", lastId);

                await conn.ExecuteAsync(@"
                    INSERT INTO expedition (idexpedition, idclient, idpaysdestination, datelivraison, datecreation)
                    VALUES (@Id, @Client, @Pays, @DateLivraison, NOW())",
                    new
                    {
                        Id = newIdExp,
                        Client = request.IdClient,
                        Pays = request.IdPaysDestination,
                        DateLivraison = request.DateLivraison
                    }, tx);

                var allDocIds = new HashSet<string>();

                // 🔹 Requêtes réutilisables
                var sqlPaysGlobal = @"
                    SELECT d.iddocument
                    FROM documentpays dp
                    JOIN document d ON d.iddocument = dp.iddocument
                    WHERE dp.idpays = @Pays;
                ";

                var sqlProdPays = @"
                    SELECT d.iddocument
                    FROM produitdocument pd
                    JOIN produitdocumentpays pdp ON pdp.idproduitdocument = pd.idproduitdocument
                    JOIN document d ON d.iddocument = pd.iddocument
                    WHERE pd.idproduit = @Prod
                    AND pdp.idpays = @Pays;
                ";

                var sqlProdDefault = @"
                    SELECT d.iddocument
                    FROM produitdocument pd
                    JOIN document d ON d.iddocument = pd.iddocument
                    WHERE pd.idproduit = @Prod;
                ";

                // 2️⃣ Ajouter docs PAYS global (toujours)
                var docsPays = await conn.QueryAsync<string>(
                    sqlPaysGlobal, new { Pays = request.IdPaysDestination }, tx);

                foreach (var doc in docsPays)
                    allDocIds.Add(doc);

                // 3️⃣ Ajouter lignes produits + docs
                foreach (var ligne in request.Lignes)
                {
                    // Insertion produit
                    var lastLineId = await conn.ExecuteScalarAsync<string>(
                        "SELECT idexpeditionproduit FROM expeditionproduit ORDER BY idexpeditionproduit DESC LIMIT 1",
                        transaction: tx);

                    var newLineId = IdGenerator.GenerateId("EXPL_", lastLineId);

                    await conn.ExecuteAsync(@"
                        INSERT INTO expeditionproduit (idexpeditionproduit, idexpedition, idproduit, quantite, unite)
                        VALUES (@Id, @Exp, @Prod, @Qte, @Unite)",
                        new
                        {
                            Id = newLineId,
                            Exp = newIdExp,
                            Prod = ligne.IdProduit,
                            Qte = ligne.Quantite,
                            Unite = ligne.Unite
                        }, tx);

                    // Docs produit + pays
                    var docsProdPays = (await conn.QueryAsync<string>(
                        sqlProdPays,
                        new { Prod = ligne.IdProduit, Pays = request.IdPaysDestination },
                        tx)).ToList();

                    if (docsProdPays.Any())
                    {
                        foreach (var doc in docsProdPays)
                            allDocIds.Add(doc);
                    }
                    else
                    {
                        // Sinon docs produit
                        var docsProd = await conn.QueryAsync<string>(
                            sqlProdDefault, new { Prod = ligne.IdProduit }, tx);

                        foreach (var doc in docsProd)
                            allDocIds.Add(doc);
                    }
                }

                // 4️⃣ Enregistrer les documents expédition
                foreach (var docId in allDocIds)
                {
                    var lastDocId = await conn.ExecuteScalarAsync<string>(
                        "SELECT idexpeditiondocument FROM expeditiondocument ORDER BY idexpeditiondocument DESC LIMIT 1",
                        transaction: tx);

                    var newDocId = IdGenerator.GenerateId("EXPD_", lastDocId);

                    await conn.ExecuteAsync(@"
                        INSERT INTO expeditiondocument (idexpeditiondocument, idexpedition, iddocument)
                        VALUES (@Id, @Exp, @Doc)",
                        new { Id = newDocId, Exp = newIdExp, Doc = docId }, tx);
                }

                await tx.CommitAsync();
                return Ok(new { IdExpedition = newIdExp, message = "Expédition créée avec succès" });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}

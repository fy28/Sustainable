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
        // 📌 GET DOCUMENTS REQUIS (PRODUIT + PAYS)
        // --------------------------------------------------------
        [HttpGet("docs")]
        public async Task<IActionResult> GetDocuments([FromQuery] string produit, [FromQuery] string pays)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            // 1️⃣ Documents spécifiques pays
            var sqlPays = @"
                SELECT d.iddocument AS IdDocument, d.nomdocument AS NomDocument
                FROM produitdocument pd
                JOIN produitdocumentpays pdp ON pdp.idproduitdocument = pd.idproduitdocument
                JOIN document d ON d.iddocument = pd.iddocument
                WHERE pd.idproduit = @Prod
                AND pdp.idpays = @Pays;
            ";

            var docsPays = (await conn.QueryAsync<DocItem>(sqlPays, new { Prod = produit, Pays = pays })).ToList();

            if (docsPays.Count > 0)
                return Ok(docsPays);

            // 2️⃣ Documents par défaut
            var sqlDefault = @"
                SELECT d.iddocument AS IdDocument, d.nomdocument AS NomDocument
                FROM produitdocument pd
                JOIN document d ON d.iddocument = pd.iddocument
                WHERE pd.idproduit = @Prod;
            ";

            var docsDefault = await conn.QueryAsync<DocItem>(sqlDefault, new { Prod = produit });

            return Ok(docsDefault);
        }

        // --------------------------------------------------------
        // 📌 POST CRÉATION EXPÉDITION
        // --------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExpeditionRequest request)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                // 1️⃣ Générer id expédition
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

                // SQL utilisé pour POST, le même que GET/doc
                var sqlPays = @"
                    SELECT d.iddocument AS IdDocument, d.nomdocument AS NomDocument
                    FROM produitdocument pd
                    JOIN produitdocumentpays pdp ON pdp.idproduitdocument = pd.idproduitdocument
                    JOIN document d ON d.iddocument = pd.iddocument
                    WHERE pd.idproduit = @Prod
                    AND pdp.idpays = @Pays;
                ";

                var sqlDefault = @"
                    SELECT d.iddocument AS IdDocument, d.nomdocument AS NomDocument
                    FROM produitdocument pd
                    JOIN document d ON d.iddocument = pd.iddocument
                    WHERE pd.idproduit = @Prod;
                ";

                // 2️⃣ Lignes produits
                foreach (var ligne in request.Lignes)
                {
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

                    // 3️⃣ Documents spécifiques prioritaire
                    var docsPays = (await conn.QueryAsync<DocItem>(
                        sqlPays,
                        new { Prod = ligne.IdProduit, Pays = request.IdPaysDestination },
                        tx)).ToList();

                    if (docsPays.Any())
                    {
                        foreach (var doc in docsPays)
                            allDocIds.Add(doc.IdDocument!);
                    }
                    else
                    {
                        var docsDefault = await conn.QueryAsync<DocItem>(
                            sqlDefault,
                            new { Prod = ligne.IdProduit },
                            tx);

                        foreach (var doc in docsDefault)
                            allDocIds.Add(doc.IdDocument!);
                    }
                }

                // 4️⃣ Insérer documents expédition
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

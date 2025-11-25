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
    public class ClientController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ClientController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchClients([FromQuery] string q)
        {
            LogService.Log($"CLIENT SEARCH | Query = '{q}'");

            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var clients = await connection.QueryAsync<Client>(
                @"SELECT idclient AS IdClient, nomclient AS NomClient, mail AS Mail, 
                         specificite AS Specificite, derniere_modification AS DerniereModification
                  FROM client
                  WHERE nomclient ILIKE @Search OR mail ILIKE @Search OR specificite ILIKE @Search",
                new { Search = "%" + q + "%" });

            LogService.Log($"CLIENT SEARCH RESULT | Query='{q}' | Count={clients.Count()}");

            return Ok(clients);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClients()
        {
            LogService.Log("CLIENT GET ALL | Fetching all clients with countries…");

            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var sql = @"
                SELECT 
                    c.idclient AS IdClient,
                    c.nomclient AS NomClient,
                    c.mail AS Mail,
                    c.specificite AS Specificite,
                    c.derniere_modification AS DerniereModification,
                    COALESCE(string_agg(p.nompays, ', '), '') AS PaysAssocies
                FROM client c
                LEFT JOIN clientpays cp ON c.idclient = cp.idclient
                LEFT JOIN pays p ON cp.idpays = p.idpays
                GROUP BY c.idclient, c.nomclient, c.mail, c.specificite, c.derniere_modification
                ORDER BY c.idclient;
            ";

            var result = await connection.QueryAsync<ClientWithPays>(sql);

            LogService.Log($"CLIENT GET ALL | Total={result.Count()}");

            return Ok(result);
        }

        [HttpGet("{id}/pays")]
        public async Task<IActionResult> GetClientPays(string id)
        {
            LogService.Log($"CLIENT GET-PAYS | ClientId={id}");

            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var pays = await connection.QueryAsync<string>(
                @"SELECT p.nompays
                  FROM clientPays cp
                  JOIN pays p ON cp.idpays = p.idpays
                  WHERE cp.idclient = @IdClient",
                new { IdClient = id });

            LogService.Log($"CLIENT GET-PAYS RESULT | ClientId={id} | Count={pays.Count()}");

            return Ok(pays);
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            LogService.Log("CLIENT PING");
            return Ok("ClientController is alive!");
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
        {
            LogService.Log($"CLIENT CREATE ATTEMPT | Name='{request.NomClient}' | Mail='{request.Mail}'");

            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            var lastId = await connection.ExecuteScalarAsync<string>(
                "SELECT idclient FROM client ORDER BY idclient DESC LIMIT 1");
            var newIdClient = IdGenerator.GenerateId("CLI_", lastId);

            await connection.ExecuteAsync(
                @"INSERT INTO client (idclient, nomclient, mail, specificite, derniere_modification)
                  VALUES (@IdClient, @NomClient, @Mail, @Specificite, NOW())",
                new
                {
                    IdClient = newIdClient,
                    NomClient = request.NomClient,
                    Mail = request.Mail,
                    Specificite = request.Specificite
                });

            LogService.Log($"CLIENT CREATED | Id={newIdClient} | Name='{request.NomClient}'");

            foreach (var paysId in request.PaysIds)
            {
                var lastClp = await connection.ExecuteScalarAsync<string>(
                    "SELECT idclientpays FROM clientPays ORDER BY idclientpays DESC LIMIT 1");
                var newClpId = IdGenerator.GenerateId("CLP_", lastClp);

                await connection.ExecuteAsync(
                    "INSERT INTO clientPays (idclientpays, idclient, idpays) VALUES (@Id, @ClientId, @PaysId)",
                    new { Id = newClpId, ClientId = newIdClient, PaysId = paysId });

                LogService.Log($"CLIENT ADD COUNTRY | ClientId={newIdClient} | CountryId={paysId}");
            }

            return Ok(new { IdClient = newIdClient });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(string id, [FromBody] CreateClientRequest request)
        {
            LogService.Log($"CLIENT UPDATE ATTEMPT | Id={id}");

            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            await connection.ExecuteAsync(@"
                UPDATE client
                SET nomclient = @NomClient, 
                    mail = @Mail, 
                    specificite = @Specificite,
                    derniere_modification = NOW()
                WHERE idclient = @IdClient",
                new { IdClient = id, NomClient = request.NomClient, Mail = request.Mail, Specificite = request.Specificite });

            await connection.ExecuteAsync("DELETE FROM clientpays WHERE idclient = @IdClient", new { IdClient = id });

            LogService.Log($"CLIENT UPDATE | Id={id} | Fields updated");

            foreach (var paysId in request.PaysIds)
            {
                var lastClp = await connection.ExecuteScalarAsync<string>(
                    "SELECT idclientpays FROM clientPays ORDER BY idclientpays DESC LIMIT 1");
                var newClpId = IdGenerator.GenerateId("CLP_", lastClp);

                await connection.ExecuteAsync(
                    "INSERT INTO clientPays (idclientpays, idclient, idpays) VALUES (@Id, @ClientId, @PaysId)",
                    new { Id = newClpId, ClientId = id, PaysId = paysId });

                LogService.Log($"CLIENT UPDATE ADD COUNTRY | ClientId={id} | CountryId={paysId}");
            }

            return Ok(new { message = "Client mis à jour avec succès", updatedAt = DateTime.Now });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(string id)
        {
            LogService.Log($"CLIENT DELETE ATTEMPT | Id={id}");

            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

            await connection.ExecuteAsync("DELETE FROM clientpays WHERE idclient = @IdClient", new { IdClient = id });
            await connection.ExecuteAsync("DELETE FROM client WHERE idclient = @IdClient", new { IdClient = id });

            LogService.Log($"CLIENT DELETED | Id={id}");

            return Ok(new { message = "Client supprimé avec succès" });
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
//     public class ClientController : ControllerBase
//     {
//         private readonly IConfiguration _config;

//         public ClientController(IConfiguration config)
//         {
//             _config = config;
//         }

//         // Rechercher des clients par nom
//         [HttpGet("search")]
//         public async Task<IActionResult> SearchClients([FromQuery] string q)
//         {
//             using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

//             var clients = await connection.QueryAsync<Client>(
//                 @"SELECT idclient AS IdClient, nomclient AS NomClient
//                 FROM client
//                 WHERE nomclient ILIKE @Search",
//                 new { Search = "%" + q + "%" });

//             return Ok(clients);
//         }


//         // Récupérer les pays associés à un client
//         [HttpGet("{id}/pays")]
//         public async Task<IActionResult> GetClientPays(string id)
//         {
//             using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

//             var pays = await connection.QueryAsync<string>(
//                 @"SELECT p.nompays
//                   FROM clientPays cp
//                   JOIN pays p ON cp.idpays = p.idpays
//                   WHERE cp.idclient = @IdClient",
//                 new { IdClient = id });

//             return Ok(pays);
//         }

//         [HttpGet("ping")]
//         public IActionResult Ping()
//         {
//             return Ok("ClientController is alive!");
//         }
         
//         [HttpPost]
//         public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
//         {
//             using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

//             // check dernier client pour verifier id
//             var lastId = await connection.ExecuteScalarAsync<string>(
//                 "SELECT idclient FROM client ORDER BY idclient DESC LIMIT 1"
//             );
//             var newIdClient = IdGenerator.GenerateId("CLI_", lastId);

//             // Insertion client
//             await connection.ExecuteAsync(
//                 "INSERT INTO client (idclient, nomclient) VALUES (@IdClient, @NomClient)",
//                 new { IdClient = newIdClient, NomClient = request.NomClient }
//             );

//             // Insertion liaisons clientPays
//             foreach (var paysId in request.PaysIds)
//             {
//                 var lastClp = await connection.ExecuteScalarAsync<string>(
//                     "SELECT idclientpays FROM clientPays ORDER BY idclientpays DESC LIMIT 1"
//                 );
//                 var newClpId = IdGenerator.GenerateId("CLP_", lastClp);

//                 await connection.ExecuteAsync(
//                     "INSERT INTO clientPays (idclientpays, idclient, idpays) VALUES (@Id, @ClientId, @PaysId)",
//                     new { Id = newClpId, ClientId = newIdClient, PaysId = paysId }
//                 );
//             }

//             return Ok(new { IdClient = newIdClient, NomClient = request.NomClient });
//         }

//     }
// }

namespace Sustainable.Models
{
    public class Client
    {
        public string? IdClient { get; set; }
        public string? NomClient { get; set; }
        public string? Mail { get; set; }
        public string? Specificite { get; set; }
        public DateTime? DerniereModification { get; set; } // ✅ Ajouté
    }

    public class ClientWithPays
    {
        public string? IdClient { get; set; }
        public string? NomClient { get; set; }
        public string? Mail { get; set; }
        public string? PaysAssocies { get; set; }
        public string? Specificite { get; set; }
        public DateTime? DerniereModification { get; set; } // ✅ Ajouté ici aussi
    }

    public class CreateClientRequest
    {
        public string NomClient { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public List<string> PaysIds { get; set; } = new();
        public string Specificite { get; set; } = string.Empty;
    }
}



// namespace Sustainable.Models
// {
//     public class Client
//     {
//         public string? IdClient { get; set; }
//         public string? NomClient { get; set; }
//     }

//     public class ClientWithPays
//     {
//         public string? IdClient { get; set; }
//         public string? NomClient { get; set; }
//         public List<string>? PaysAssocies { get; set; }

//     }

//     public class CreateClientRequest
//     {
//         public string NomClient { get; set; } = string.Empty;
//         public List<string> PaysIds { get; set; } = new();
//     }
// }

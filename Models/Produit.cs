namespace Sustainable.Models
{
    public class Produit
    {
        public string? IdProduit { get; set; }
        public string? Nom { get; set; }
        public string? CodeHS { get; set; }
        public decimal Prix { get; set; }
        public string? IdUnite { get; set; }
        public string? NomUnite { get; set; }  // 🔹 jointure
        public DateTime? DateAjout { get; set; }
        public DateTime? DerniereModification { get; set; }
    }

    public class CreateProduitRequest
    {
        public string Nom { get; set; } = string.Empty;
        public string CodeHS { get; set; } = string.Empty;
        public decimal Prix { get; set; }
        public string IdUnite { get; set; } = string.Empty;  // 🔹 au lieu de texte
    }
}
// namespace Sustainable.Models
// {
//     public class Produit
//     {
//         public string? IdProduit { get; set; }
//         public string? Nom { get; set; }
//         public string? CodeHS { get; set; }
//         public decimal Prix { get; set; }
//         public string? Unite { get; set; }
//         public DateTime? DateAjout { get; set; }
//         public DateTime? DerniereModification { get; set; }
//     }

//     public class CreateProduitRequest
//     {
//         public string Nom { get; set; } = string.Empty;
//         public string CodeHS { get; set; } = string.Empty;
//         public decimal Prix { get; set; }
//         public string Unite { get; set; } = "kg";
//     }
// }

namespace Sustainable.Models
{
    public class Collecteur
    {
        public string? IdCollecteur { get; set; }
        public string? NomCollecteur { get; set; }
        public string? Contact { get; set; }
        public string? Zone { get; set; }
        public DateTime? DerniereModification { get; set; }

        public List<CollecteurProduitItem> Produits { get; set; } = new();
    }

    public class CollecteurProduitItem
    {
        public string? IdProduit { get; set; }
        public string? NomProduit { get; set; }
    }

    public class CreateCollecteurRequest
    {
        public string NomCollecteur { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public List<string> ProduitIds { get; set; } = new();
    }
}

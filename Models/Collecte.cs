namespace Sustainable.Models
{
   public class Collecte
{
    public string? IdCollecte { get; set; }
    public string? IdCollecteur { get; set; }
    public string? NomCollecteur { get; set; }
    public string? IdProduit { get; set; }
    public string? NomProduit { get; set; }
    public decimal Quantite { get; set; }
    public decimal? PrixKg { get; set; } // 🔥 AJOUT
    public DateTime? DateCollecte { get; set; }
    public string? Statut { get; set; }
    public DateTime? DateArrivee { get; set; }
    public DateTime? DerniereModification { get; set; }
}

  public class CreateCollecteRequest
{
    public string IdCollecteur { get; set; } = string.Empty;
    public string IdProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixKg { get; set; }   // 🔥 AJOUT
    public DateTime? DateCollecte { get; set; }
    public string Statut { get; set; } = "En attente";
    public DateTime? DateArrivee { get; set; }
}

}

using System;
using System.Collections.Generic;

namespace Sustainable.Models
{
    public class Expedition
    {
        public string? IdExpedition { get; set; }
        public string? IdClient { get; set; }
        public string? ClientName { get; set; }

        public string? IdPaysDestination { get; set; }
        public string? NomPaysDestination { get; set; }

        public DateTime? DateLivraison { get; set; }
        public DateTime? DateCreation { get; set; }

        public List<ExpeditionProduit> Produits { get; set; } = new();
        public List<DocItem> Documents { get; set; } = new();
    }

    public class ExpeditionProduit
    {
        public string? IdExpeditionProduit { get; set; }
        public string? IdProduit { get; set; }
        public string? NomProduit { get; set; }
        public decimal Quantite { get; set; }
        public string? Unite { get; set; }

        public decimal PrixUnitaire { get; set; }
    }

    public class DocItem
    {
        public string? IdDocument { get; set; }
        public string? NomDocument { get; set; }
    }

    // ---------- DTOs pour le POST ----------
    public class CreateExpeditionRequest
    {
        public string IdClient { get; set; } = string.Empty;
        public string IdPaysDestination { get; set; } = string.Empty;
        public DateTime? DateLivraison { get; set; }
        public List<CreateExpeditionLigne> Lignes { get; set; } = new();
    }

    public class CreateExpeditionLigne
    {
        public string IdProduit { get; set; } = string.Empty;
        public decimal Quantite { get; set; }
        public string Unite { get; set; } = "kg";
    }
}

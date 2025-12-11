namespace Sustainable.Models
{
    public class Document
    {
        public string IdDocument { get; set; } = string.Empty;
        public string NomDocument { get; set; } = string.Empty;

        public List<PaysAssocie> PaysAssocies { get; set; } = new();
        public List<ProduitAssocie> ProduitsAssocies { get; set; } = new();
    }

    public class PaysAssocie
    {
        public string IdPays { get; set; } = string.Empty;
        public string NomPays { get; set; } = string.Empty;
    }

    public class ProduitAssocie
    {
        public string IdProduit { get; set; } = string.Empty;
        public string NomProduit { get; set; } = string.Empty;
    }

    public class CreateDocumentRequest
    {
        public string NomDocument { get; set; } = string.Empty;
        public List<string> PaysSelectionnes { get; set; } = new();
        public List<string> ProduitsSelectionnes { get; set; } = new();
    }
}

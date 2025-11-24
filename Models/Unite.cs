namespace Sustainable.Models
{
    public class Unite
    {
        public string? IdUnite { get; set; }
        public string? NomUnite { get; set; }
        public string? Symbole { get; set; }
    }

    public class CreateUniteRequest
    {
        public string NomUnite { get; set; } = string.Empty;
        public string? Symbole { get; set; }
    }
}

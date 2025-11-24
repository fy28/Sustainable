namespace Sustainable.Models
{
    public class User
    {
        public string? IdUser { get; set; }
        public string? UserName { get; set; }
        public string? Prenom { get; set; }              // ✅ nouveau
        public DateTime? DateNaissance { get; set; }     // ✅ nouveau
        public DateTime? DateEntree { get; set; }        // ✅ nouveau
        public string? Password { get; set; }
        public string? Mail { get; set; }
        public string? IdRole { get; set; }
        public string? RoleName { get; set; }
    }

    public class CreateUserRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public DateTime DateNaissance { get; set; }
        public DateTime DateEntree { get; set; }
        public string Mail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string IdRole { get; set; } = string.Empty;
    }
}


// namespace Sustainable.Models
// {
//     public class User
//     {
//         public string? IdUser { get; set; }
//         public string? UserName { get; set; }
//         public string? Password { get; set; }
//         public string? Mail { get; set; }
//         public string? IdRole { get; set; }
//         public string? RoleName { get; set; }
//     }
// }

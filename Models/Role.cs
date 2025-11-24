namespace Sustainable.Models
{
    public class Role
    {
        public string? IdRole { get; set; }
        public string? RoleName { get; set; }
    }

    // ✅ Si tu veux permettre la création de nouveaux rôles depuis le front :
    public class CreateRoleRequest
    {
        public string RoleName { get; set; } = string.Empty;
    }
}

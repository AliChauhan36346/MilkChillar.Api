namespace MilkChillar.Application.DTOs.UserPermissions
{
    public class UserEffectivePermissionsDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = default!;
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public List<int> RolePermissionIds { get; set; } = new();
        public List<string> RolePermissionNames { get; set; } = new();
        public List<int> DirectUserPermissionIds { get; set; } = new();
        public List<string> DirectUserPermissionNames { get; set; } = new();
        public List<string> EffectivePermissionNames { get; set; } = new();
    }
}

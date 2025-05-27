using System.ComponentModel.DataAnnotations.Schema;

namespace MilkChillar.Domain.Entities;

public class Permission
{
    public int PermissionId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}



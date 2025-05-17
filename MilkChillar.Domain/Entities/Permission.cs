using System.ComponentModel.DataAnnotations.Schema;

namespace MilkChillar.Domain.Entities;

public class Permission
{
    [Column("permission_id")]
    public int PermissionID { get; set; } // maps to permission_id
    public string Name { get; set; }
    public string Description { get; set; }

    public List<RolePermission> RolePermissions { get; set; }
    public List<UserPermission> UserPermissions { get; set; }
}

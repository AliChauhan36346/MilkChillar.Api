namespace MilkChillar.Domain.Entities;

public class Role
{
    public int RoleID { get; set; } // maps to role_id
    public string Name { get; set; }
    public string Description { get; set; }

    public List<User> Users { get; set; }
    public List<RolePermission> RolePermissions { get; set; } // ✅ Add this

}

namespace MilkChillar.Domain.Entities;

public class Role
{
    public int RoleId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}



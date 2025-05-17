namespace MilkChillar.Domain.Entities;

public class User
{
    public int Id { get; set; } // maps to user_id
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public int RoleId { get; set; }
    public int TenantId { get; set; }
    public int? BuyerId { get; set; }
    public int? SupplierId { get; set; }
    public int? EmployeeId { get; set; }

    public Role Role { get; set; } // navigation

    public List<UserPermission> UserPermissions { get; set; }
}

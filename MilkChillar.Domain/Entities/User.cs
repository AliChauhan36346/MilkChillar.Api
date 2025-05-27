namespace MilkChillar.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public int TenantId { get; set; }
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public int? SupplierId { get; set; }
    public int? EmployeeId { get; set; }
    public int? BuyerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsBlocked { get; set; }
    public int? RoleId { get; set; }
    public string UserType { get; set; } = "standard";

    // Navigation Properties
    public Tenant Tenant { get; set; } = default!;
    public Supplier? Supplier { get; set; }
    public Employee? Employee { get; set; }
    public Buyer? Buyer { get; set; }
    public Role? Role { get; set; }

    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}



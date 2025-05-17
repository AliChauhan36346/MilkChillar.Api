using Microsoft.EntityFrameworkCore;
using MilkChillar.Domain.Entities;

namespace MilkChillar.Application;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Buyer> Buyers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<MainAccount> MainAccounts { get; set; }
    public DbSet<SubAccount> SubAccounts { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Employee> Employees { get; set; }

    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Existing configs...

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("user_id");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(e => e.RoleId);
        });

        // Roles
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.RoleID);
            entity.Property(e => e.RoleID).HasColumnName("role_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        // Permissions
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.PermissionID);
            entity.Property(e => e.PermissionID).HasColumnName("permission_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");

            entity.HasKey(rp => new { rp.RoleId, rp.PermissionID });

            entity.HasOne(rp => rp.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(rp => rp.RoleId);

            entity.HasOne(rp => rp.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(rp => rp.PermissionID);
        });

        // UserPermissions
        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.ToTable("user_permissions"); // or your actual table name
            entity.HasKey(up => new { up.UserId, up.PermissionId });

            entity.HasOne(up => up.User)
                  .WithMany(u => u.UserPermissions)
                  .HasForeignKey(up => up.UserId);

            entity.HasOne(up => up.Permission)
                  .WithMany(p => p.UserPermissions)
                  .HasForeignKey(up => up.PermissionId);
        });


        // Buyers
        modelBuilder.Entity<Buyer>(entity =>
        {
            entity.ToTable("buyers");
            entity.HasKey(e => e.BuyerId);
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.Rate).HasColumnName("rate");
            entity.Property(e => e.KhataNumber).HasColumnName("khata_number");
            entity.Property(e => e.CreditLimit).HasColumnName("credit_limit");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
        });

        // Suppliers
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");
            entity.HasKey(e => e.SupplierId);
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.Rate).HasColumnName("rate");
            entity.Property(e => e.KhataNumber).HasColumnName("khata_number");
            entity.Property(e => e.CreditLimit).HasColumnName("credit_limit");
            entity.Property(e => e.DodhiId).HasColumnName("dodhi_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.GiveCreditOnParchi).HasColumnName("give_credit_on_parchi");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
        });

        // ✅ Tenant
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("tenant_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        // ✅ MainAccount
        modelBuilder.Entity<MainAccount>(entity =>
        {
            entity.ToTable("main_accounts");
            entity.HasKey(e => e.MainAccountId);
            entity.Property(e => e.MainAccountId).HasColumnName("main_account_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.MainAccountCode).HasColumnName("main_account_code");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.FinancialStatementComponent).HasColumnName("financial_component");
        });

        // ✅ SubAccount
        modelBuilder.Entity<SubAccount>(entity =>
        {
            entity.ToTable("sub_accounts");
            entity.HasKey(e => e.SubAccountId);
            entity.Property(e => e.SubAccountId).HasColumnName("sub_account_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.MainAccountId).HasColumnName("main_account_id");
            entity.Property(e => e.SubAccountCode).HasColumnName("sub_account_code");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        // ✅ Account
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(e => e.AccountId);
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.SubAccountId).HasColumnName("sub_account_id");
            entity.Property(e => e.SubAccountCode).HasColumnName("sub_account_code");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        // ✅ Employee
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees");
            entity.HasKey(e => e.EmployeeId);
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.Designation).HasColumnName("designation");
            entity.Property(e => e.ContactNumber).HasColumnName("contact_number");
            entity.Property(e => e.Salary).HasColumnName("salary");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        });
    }
}

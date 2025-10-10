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
    public DbSet<Chillar> Chillars { get; set; }
    public DbSet<ChillarReceive> ChillarReceives { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<Sales> Sales { get; set; }
    public DbSet<StockEntry> StockEntries { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<AccountOpeningBalance> AccountOpeningBalances { get; set; }
    public DbSet<CashPayment> CashPayments { get; set; }
    public DbSet<CashPaymentLine> CashPaymentLines { get; set; }
    public DbSet<VwAccountSearch> VwAccountSearch { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VwAccountSearch>(entity =>
        {
            entity.HasNoKey();  // views don’t have primary keys
            entity.ToView("vw_account_search"); // map to the view name in Postgres

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.AccountCode).HasColumnName("account_code");
            entity.Property(e => e.AccountName).HasColumnName("account_name");
            entity.Property(e => e.FinancialStatementComponent).HasColumnName("financial_statement_component");
            entity.Property(e => e.Balance).HasColumnName("balance");
        });


        // USERS
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Username).HasColumnName("username").IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsBlocked).HasColumnName("is_blocked");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UserType).HasColumnName("user_type").HasDefaultValue("standard");

            // Foreign key relationships
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Buyer)
                .WithMany()
                .HasForeignKey(e => e.BuyerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Navigation for UserPermissions (many-to-many / one-to-many depending on design)
            entity.HasMany(e => e.UserPermissions)
                .WithOne(up => up.User)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // ROLES
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        // PERMISSIONS
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.PermissionId);
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        // ROLE-PERMISSIONS
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            entity.Property(rp => rp.RoleId).HasColumnName("role_id");  // Add this
            entity.Property(rp => rp.TenantId).HasColumnName("tenant_id");  // Add this
            entity.Property(rp => rp.PermissionId).HasColumnName("permission_id");  // Add this

            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .HasConstraintName("fk_role_permissions_role_id");  // Add constraint name

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .HasConstraintName("fk_role_permissions_permission_id");  // Add constraint name
        });

        // USER-PERMISSIONS
        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.ToTable("user_permissions");
            entity.HasKey(up => new { up.UserId, up.PermissionId });

            entity.Property(up => up.UserId).HasColumnName("user_id");  // Add this
            entity.Property(up => up.PermissionId).HasColumnName("permission_id");  // Add this

            entity.Property(up => up.GrantedAt).HasColumnName("granted_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId)
                .HasConstraintName("fk_user_permissions_user_id");  // Add constraint name

            entity.HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId)
                .HasConstraintName("fk_user_permissions_permission_id");  // Add constraint name
        });

        // BUYERS
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
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.HasOne(e => e.Tenant)
                  .WithMany(t => t.Buyers)
                  .HasForeignKey(e => e.TenantId);

            entity.HasOne(e => e.Account)
                  .WithMany()
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // SUPPLIERS
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
            entity.Property(e => e.NameUrdu).HasColumnName("name_urdu");

            entity.HasOne(e => e.Tenant)
                  .WithMany(t => t.Suppliers)
                  .HasForeignKey(e => e.TenantId);

            entity.HasOne(e => e.Account)
                  .WithMany()
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Dodhi)
                  .WithMany()
                  .HasForeignKey(e => e.DodhiId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // TENANTS
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(e => e.TenantId);
            entity.Property(e => e.TenantId).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        // MAIN ACCOUNT
        modelBuilder.Entity<MainAccount>(entity =>
        {
            entity.ToTable("main_accounts");
            entity.HasKey(e => e.MainAccountId);
            entity.Property(e => e.MainAccountId).HasColumnName("main_account_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.MainAccountCode).HasColumnName("main_account_code");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.FinancialStatementComponent).HasColumnName("financial_statement_component");
        });

        // SUB ACCOUNT
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

        // ACCOUNT
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(e => e.AccountId);
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.SubAccountId).HasColumnName("sub_account_id");
            entity.Property(e => e.FullCode).HasColumnName("full_code");
            entity.Property(e => e.AccountCode).HasColumnName("account_code");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        // EMPLOYEES
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

            entity.Property(e => e.ChillarId).HasColumnName("chillar_id");

            entity.HasOne(e => e.Tenant)
                  .WithMany(t => t.Employees)
                  .HasForeignKey(e => e.TenantId);
            entity.HasOne(e => e.Chillar)
                  .WithMany()
                  .HasForeignKey(e => e.ChillarId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Chillar>(entity =>
        {
            entity.ToTable("chillars");

            entity.HasKey(c => c.ChillarId);

            entity.Property(c => c.ChillarId).HasColumnName("chillar_id");
            entity.Property(c => c.TenantId).HasColumnName("tenant_id");
            entity.Property(c => c.Name).HasColumnName("name").IsRequired();
            entity.Property(c => c.Location).HasColumnName("location");
            entity.Property(c => c.NumberOfChillars).HasColumnName("number_of_chillars");
            entity.Property(c => c.Capacity).HasColumnName("capacity");
            

            entity.HasOne(c => c.Tenant)
                .WithMany(t => t.Chillars)
                .HasForeignKey(c => c.TenantId);
        });

        modelBuilder.Entity<ChillarReceive>(entity =>
        {
            entity.ToTable("chillar_receive");
            entity.HasKey(e => e.ReceiveId);
            entity.Property(e => e.ReceiveId).HasColumnName("receive_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.TimeOfDay).HasColumnName("time_of_day");
            entity.Property(e => e.ChillarId).HasColumnName("chillar_id");
            entity.Property(e => e.ChillarInchargeId).HasColumnName("chillar_incharge_id");
            entity.Property(e => e.DodhiId).HasColumnName("dodhi_id");
            entity.Property(e => e.AddedBy).HasColumnName("added_by");
            entity.Property(e => e.GrossLiters).HasColumnName("gross_liters");
            entity.Property(e => e.LR).HasColumnName("lr");
            entity.Property(e => e.Fat).HasColumnName("fat");
            entity.Property(e => e.NetLiters).HasColumnName("net_liters");

            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId);

            entity.HasOne(e => e.Chillar)
                  .WithMany()
                  .HasForeignKey(e => e.ChillarId);

            entity.HasOne(e => e.ChillarIncharge)
                  .WithMany()
                  .HasForeignKey(e => e.ChillarInchargeId);

            entity.HasOne(e => e.Dodhi)
                  .WithMany()
                  .HasForeignKey(e => e.DodhiId);

            entity.HasOne(e => e.AddedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.AddedBy);
        });

        modelBuilder.Entity<Sales>(entity =>
        {
            entity.ToTable("sales");

            entity.HasKey(e => e.SaleId);
            entity.Property(e => e.SaleId).HasColumnName("sale_id");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Date).HasColumnName("date");

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.RevenueAccountId).HasColumnName("revenue_account_id");
            entity.Property(e => e.ChillarId).HasColumnName("chillar_id");
            entity.Property(e => e.AddedBy).HasColumnName("added_by");

            entity.Property(e => e.GrossLiters).HasColumnName("gross_liters");
            entity.Property(e => e.LR).HasColumnName("lr");
            entity.Property(e => e.Fat).HasColumnName("fat");
            entity.Property(e => e.NetLiters).HasColumnName("net_liters");
            entity.Property(e => e.Rate).HasColumnName("rate");

            entity.Property(e => e.AmountReceived).HasColumnName("amount_received");
            entity.Property(e => e.Balance).HasColumnName("balance");

            // TotalAmount is a computed property in C#; not mapped by default
            entity.Ignore(e => e.TotalAmount);

            // Relationships
            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId);

            entity.HasOne(e => e.Account)
                  .WithMany()
                  .HasForeignKey(e => e.AccountId);

            entity.HasOne(e => e.RevenueAccount)
                  .WithMany()
                  .HasForeignKey(e => e.RevenueAccountId);

            entity.HasOne(e => e.Chillar)
                  .WithMany()
                  .HasForeignKey(e => e.ChillarId);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.AddedBy);
        });

        //stockEntry
        modelBuilder.Entity<StockEntry>(entity =>
        {
            entity.ToTable("stock_entry");

            entity.HasKey(e => e.StockEntryId);
            entity.Property(e => e.StockEntryId).HasColumnName("stock_entry_id");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.ChillarId).HasColumnName("chillar_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.TimeOfDay).HasColumnName("time_of_day").HasMaxLength(10);
            entity.Property(e => e.TheoreticalLiters).HasColumnName("theoretical_liters").HasPrecision(10, 2);
            entity.Property(e => e.MeasuredLiters).HasColumnName("measured_liters").HasPrecision(10, 2);

            // Computed by DB
            entity.Property(e => e.Variance)
                .HasColumnName("variance")
                .HasPrecision(10, 2)
                .ValueGeneratedOnAddOrUpdate()
                .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            entity.Property(e => e.CreatedBy).HasColumnName("created_by");

            // Unique constraint: tenant_id + chillar_id + date
            entity.HasIndex(e => new { e.TenantId, e.ChillarId, e.Date })
                .IsUnique()
                .HasDatabaseName("uq_stock_entry_day");

            // Relationships (no reverse navs)
            entity.HasOne(e => e.Chillar)
                .WithMany() // No StockEntries collection in Chillar
                .HasForeignKey(e => e.ChillarId)
                .HasConstraintName("fk_se_chillar")
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany() // No reverse nav in User
                .HasForeignKey(e => e.CreatedBy)
                .HasConstraintName("fk_se_user")
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Tenant)
                .WithMany() // No reverse nav in Tenant
                .HasForeignKey(e => e.TenantId)
                .HasConstraintName("fk_se_tenant")
                .OnDelete(DeleteBehavior.Cascade);
        });



        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.ToTable("purchase");
            entity.HasKey(e => e.PurchaseId);

            entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.TimeOfDay).HasColumnName("time_of_day");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.ExpenseAccountId).HasColumnName("expense_account_id");
            entity.Property(e => e.DodhiId).HasColumnName("dodhi_id");
            entity.Property(e => e.GrossLiters).HasColumnName("gross_liters");
            entity.Property(e => e.Rate).HasColumnName("rate");
            entity.Property(e => e.Balance).HasColumnName("balance");

            // TotalAmount is a computed property in C#; not mapped by default (it's computed in DB)
            entity.Ignore(e => e.TotalAmount);

            // Relationships
            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId);

            entity.HasOne(e => e.Account)
                  .WithMany()
                  .HasForeignKey(e => e.AccountId);

            entity.HasOne(e => e.ExpenseAccount)
                  .WithMany()
                  .HasForeignKey(e => e.ExpenseAccountId);

            entity.HasOne(e => e.Dodhi)
                  .WithMany()
                  .HasForeignKey(e => e.DodhiId);
        });



        modelBuilder.Entity<AccountOpeningBalance>(entity =>
        {
            entity.ToTable("account_opening_balances");
            entity.HasKey(e => e.OpeningBalanceId);

            entity.Property(e => e.OpeningBalanceId).HasColumnName("opening_balance_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.OpeningDate).HasColumnName("opening_date");
            entity.Property(e => e.DebitOpening).HasColumnName("debit_opening").HasPrecision(18, 2);
            entity.Property(e => e.CreditOpening).HasColumnName("credit_opening").HasPrecision(18, 2);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.AddedBy).HasColumnName("added_by");
            entity.Property(e => e.JournalEntryId).HasColumnName("journal_entry_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            // Relationships
            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                  .WithMany()
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.AddedBy)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.JournalEntry)
                  .WithMany()
                  .HasForeignKey(e => e.JournalEntryId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => new { e.TenantId, e.AccountId, e.OpeningDate })
                  .IsUnique()
                  .HasDatabaseName("idx_unique_account_opening");
        });

        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("journal_entries");
            entity.HasKey(e => e.JournalEntryId);

            entity.Property(e => e.JournalEntryId).HasColumnName("journal_entry_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.EntryDate).HasColumnName("entry_date");
            entity.Property(e => e.SourceTable)
                  .HasColumnName("source_table")
                  .HasMaxLength(50);
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.ReferenceNo)
                  .HasColumnName("reference_no")
                  .HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.AddedBy).HasColumnName("added_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            // Set default values
            entity.Property(e => e.EntryDate).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .HasConstraintName("fk_je_tenant")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.AddedBy)
                  .HasConstraintName("fk_je_user")
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(e => e.JournalEntryLines)
                  .WithOne(jel => jel.JournalEntry)
                  .HasForeignKey(jel => jel.JournalEntryId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => new { e.TenantId, e.EntryDate })
                  .HasDatabaseName("idx_journal_entries_tenant_date");
        });

        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.ToTable("journal_entry_lines");
            entity.HasKey(e => e.JournalLineId);

            entity.Property(e => e.JournalLineId).HasColumnName("journal_line_id");
            entity.Property(e => e.JournalEntryId).HasColumnName("journal_entry_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Debit)
                  .HasColumnName("debit")
                  .HasPrecision(18, 2)
                  .HasDefaultValue(0);
            entity.Property(e => e.Credit)
                  .HasColumnName("credit")
                  .HasPrecision(18, 2)
                  .HasDefaultValue(0);
            entity.Property(e => e.Narration).HasColumnName("narration");

            // Relationships
            entity.HasOne(e => e.JournalEntry)
                  .WithMany(je => je.JournalEntryLines)
                  .HasForeignKey(e => e.JournalEntryId)
                  .HasConstraintName("fk_jel_journal")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                  .WithMany()
                  .HasForeignKey(e => e.AccountId)
                  .HasConstraintName("fk_jel_account")
                  .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            entity.HasIndex(e => e.AccountId)
                  .HasDatabaseName("idx_journal_entry_lines_account");

            entity.HasIndex(e => e.JournalEntryId)
                  .HasDatabaseName("idx_journal_entry_lines_journal");

            // Check constraints (these will be handled by database constraints, but documented here)
            // chk_jel_debit_credit_not_both_zero: debit > 0 OR credit > 0
            // journal_entry_lines_debit_check: debit >= 0
            // journal_entry_lines_credit_check: credit >= 0
        });


        modelBuilder.Entity<CashPayment>(entity =>
        {
            entity.ToTable("cash_payments");
            entity.HasKey(e => e.CashPaymentId);

            entity.Property(e => e.CashPaymentId).HasColumnName("cash_payment_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.VoucherNo).HasColumnName("voucher_no");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.JobDescription).HasColumnName("job_description");
            entity.Property(e => e.CashAccountId).HasColumnName("cash_account_id");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.AddedBy).HasColumnName("added_by");
            entity.Property(e => e.JournalEntryId).HasColumnName("journal_entry_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            // Relationships
            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CashAccount)
                  .WithMany()
                  .HasForeignKey(e => e.CashAccountId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.AddedBy)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.JournalEntry)
                  .WithMany()
                  .HasForeignKey(e => e.JournalEntryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.CashPaymentLines)
                  .WithOne(e => e.CashPayment)
                  .HasForeignKey(e => e.CashPaymentId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => new { e.TenantId, e.PaymentDate })
                  .HasDatabaseName("idx_cash_payments_tenant_date");
        });

        modelBuilder.Entity<CashPaymentLine>(entity =>
        {
            entity.ToTable("cash_payment_lines");
            entity.HasKey(e => e.CashPaymentLineId);

            entity.Property(e => e.CashPaymentLineId).HasColumnName("cash_payment_line_id");
            entity.Property(e => e.CashPaymentId).HasColumnName("cash_payment_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(18, 2);

            // Relationships
            entity.HasOne(e => e.CashPayment)
                  .WithMany(e => e.CashPaymentLines)
                  .HasForeignKey(e => e.CashPaymentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                  .WithMany()
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            entity.HasIndex(e => e.CashPaymentId)
                  .HasDatabaseName("idx_cash_payment_lines_payment");

            // Check constraint
            entity.ToTable(t => t.HasCheckConstraint("cash_payment_lines_amount_check", "amount >= 0"));
        });




    }
}

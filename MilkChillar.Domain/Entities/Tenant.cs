using System;
using System.Collections.Generic;

namespace MilkChillar.Domain.Entities
{
    public class Tenant
    {
        public int TenantId { get; set; } // Renamed from 'Id' to 'TenantId' for clarity and consistency

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<MainAccount> MainAccounts { get; set; } = new List<MainAccount>();
        public ICollection<SubAccount> SubAccounts { get; set; } = new List<SubAccount>();
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        // for users 
        public ICollection<User> Users { get; set; } = new List<User>();

        // Optional: Add Buyers, Suppliers, etc., if related
        public ICollection<Buyer> Buyers { get; set; } = new List<Buyer>();
        public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();

        public ICollection<Chillar> Chillars { get; set; } = new List<Chillar>();
    }
}

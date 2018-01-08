using System;
using System.Collections.Generic;
using SimplCommerce.Infrastructure.Models;

namespace SimplCommerce.Module.Core.Models
{
    public class Vendor : EntityBase
    {
        public Vendor()
        {
            CreatedOn = DateTimeOffset.Now;
        }

        public string Name { get; set; }

        public string SeoTitle { get; set; }

        public string Description { get; set; }

        public string Email { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public IList<User> Users { get; set; } = new List<User>();
        
        public string  VendorType { get; set; }
        public string  VendorClass { get; set; }
        public string  Phone1 { get; set; }
        public string  Phone2 { get; set; }
        public string  Phone3 { get; set; }
        public string  Phone4 { get; set; }
        public string  Address { get; set; }
        public string  City { get; set; }
        public string  Area { get; set; }
        public long?   CountryId { get; set; }
        public string  Website { get; set; }
        public bool? SendEmails { get; set; }
        public string  BankName { get; set; }
        public string  AccountNumber { get; set; }
        public string  Iban { get; set; }
        public string  Notes { get; set; }
    }
}

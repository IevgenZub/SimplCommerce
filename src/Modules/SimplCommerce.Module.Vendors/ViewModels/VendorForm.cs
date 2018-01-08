using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SimplCommerce.Module.Vendors.ViewModels
{
    public class VendorForm
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Slug { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public IList<VendorManager> Managers { get; set; } = new List<VendorManager>();

        public string VendorType { get; set; }
        public string VendorClass { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Phone3 { get; set; }
        public string Phone4 { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Area { get; set; }
        public long? CountryId { get; set; }
        public string Website { get; set; }
        public bool? SendEmails { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string Iban { get; set; }
        public string Notes { get; set; }
    }
}

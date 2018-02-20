using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SimplCommerce.Module.Orders.ViewModels
{
    public class AddressFormVm
    {
        public long Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string DocumentNumber { get; set; }

        [Required]
        public string DocumentExpiration { get; set; }

        [Required]
        public string BirthDate { get; set; }
        
        public string Sex { get; set; }
    }
}

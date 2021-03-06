﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SimplCommerce.Module.Core.ViewModels
{
    public class UserAddressFormViewModel
    {
        public long Id { get; set; }

        [Required]
        public string ContactName { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public string PostalCode { get; set; }

        public string City { get; set; }

        public long StateOrProvinceId { get; set; }

        public long? DistrictId { get; set; }

        public long CountryId { get; set; }

        public string RedirectUrl { get; set; }

        public IList<SelectListItem> StateOrProvinces { get; set; }

        public IList<SelectListItem> Districts { get; set; }

        public IList<SelectListItem> Countries { get; set; }

        public IList<SelectListItem> SexTypes
        {
            get
            {
                return new List<SelectListItem> {
                    new SelectListItem { Text = "Male", Value = "Male" },
                    new SelectListItem { Text = "Female", Value = "Female" }
                };
            }
        }
    }
}

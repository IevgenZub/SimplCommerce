using System;

namespace SimplCommerce.Module.Catalog.ViewModels
{
    public class ProductListItem
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string FlightNumber { get; set; }

        public bool IsRoundTrip { get; set; }

        public bool HasOptions { get; set; }

        public string CreatedOn { get; set; }

        public bool IsPublished { get; set; }
        
        public string Seats { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public DateTimeOffset? DepartureDate { get; set; }

        public DateTimeOffset? LandingDate { get; set; }

        public DateTimeOffset? ReturnDepartureDate { get; set; }
    
        public string Status { get; set; }

        public string Operator { get; set; }

        public string FlightClass { get; set; }

        public string Price { get; set; }
    }
}

using SimplCommerce.Module.Catalog.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SimplCommerce.Module.Catalog.ViewModels
{
    public class ProductVm
    {
        public ProductVm()
        {
            IsPublished = true;
            IsCallForPricing = false;
            IsAllowToOrder = true;
            IsOutOfStock = false;
            Price = 0;
        }

        public long Id { get; set; }

        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        public decimal? SpecialPrice { get; set; }

        public DateTimeOffset? SpecialPriceStart { get; set; }

        public DateTimeOffset? SpecialPriceEnd { get; set; }

        public bool IsCallForPricing { get; set; }

        public bool IsAllowToOrder { get; set; }

        public bool IsOutOfStock { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Slug { get; set; }

        public string ShortDescription { get; set; }

        public string Description { get; set; }

        public string Specification { get; set; }

        public bool IsPublished { get; set; }

        public bool IsFeatured { get; set; }

        public IList<long> CategoryIds { get; set; } = new List<long>();

        public IList<ProductAttributeVm> Attributes { get; set; } = new List<ProductAttributeVm>();

        public IList<ProductOptionVm> Options { get; set; } = new List<ProductOptionVm>();

        public IList<ProductVariationVm> Variations { get; set; } = new List<ProductVariationVm>();

        public string ThumbnailImageUrl { get; set; }

        public IList<ProductMediaVm> ProductImages { get; set; } = new List<ProductMediaVm>();

        public IList<ProductMediaVm> ProductDocuments { get; set; } = new List<ProductMediaVm>();

        public IList<long> DeletedMediaIds { get; set; } = new List<long>();

        public long? BrandId { get; set; }

        public long? TaxClassId { get; set; }

        public List<ProductLinkVm> RelatedProducts { get; set; } = new List<ProductLinkVm>();

        public List<ProductLinkVm> CrossSellProducts { get; set; } = new List<ProductLinkVm>();

        public int Baggage { get; set; }

        public int? Seats { get; set; }

        public string TerminalInfo { get; set; }

        public string Via { get; set; }

        public string Currency { get; set; }

        public string Provider { get; set; }

        public string ReturnFlightNumber { get; set; }

        public DateTimeOffset? ReturnDepartureDate { get; set; }

        public DateTimeOffset? ReturnLandingDate { get; set; }

        public long? ReturnCarrierId { get; set; }

        public Brand ReturnCarrier { get; set; }

        public long? ReturnAircraftId { get; set; }

        public bool? IsRoundTrip { get; set; }

        public string ReturnTerminal { get; set; }

        public string ReturnVia { get; set; }

        public string FlightNumber { get; set; }

        public int? SoldSeats { get; set; }

        public bool? SaleRtOnly { get; set; }

        public string Status { get; set; }

        // Admin Rules Departure

        public bool? AdminRoundTrip { get; set; }
        public long? AdminRoundTripOperatorId { get; set; }
        public bool? AdminPayLater { get; set; }
        public string AdminPayLaterRule { get; set; }
        public string AdminBlackList { get; set; }
        public int? AdminPasExpirityRule { get; set; }
        public bool? AdminIsSpecialOffer { get; set; }
        public bool? AdminNotifyAgencies { get; set; }
        public bool? AdminNotifyLastPassanger { get; set; }
        public bool? AdminIsLastMinute { get; set; }

        // Admin Rules Return

        public bool? AdminReturnPayLater { get; set; }
        public string AdminReturnPayLaterRule { get; set; }
        public string AdminReturnBlackList { get; set; }
        public int? AdminReturnPasExpirityRule { get; set; }
        public bool? AdminReturnIsSpecialOffer { get; set; }
        public bool? AdminReturnNotifyAgencies { get; set; }
        public bool? AdminReturnNotifyLastPassanger { get; set; }
        public bool? AdminReturnIsLastMinute { get; set; }
        public bool UserIsAdmin { get; set; }
    }
}
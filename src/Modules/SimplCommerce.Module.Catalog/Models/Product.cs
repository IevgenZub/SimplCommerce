using System;
using System.Collections.Generic;
using SimplCommerce.Module.Core.Models;
using SimplCommerce.Module.Tax.Models;

namespace SimplCommerce.Module.Catalog.Models
{
    public class Product : Content
    {
        public string ShortDescription { get; set; }

        public string Description { get; set; }

        public string Specification { get; set; }

        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        public decimal? SpecialPrice { get; set; }

        public DateTimeOffset? SpecialPriceStart { get; set; }

        public DateTimeOffset? SpecialPriceEnd { get; set; }

        public bool HasOptions { get; set; }

        public bool IsVisibleIndividually { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsCallForPricing { get; set; }

        public bool IsAllowToOrder { get; set; }

        public int StockQuantity { get; set; }

        public string Sku { get; set; }

        public string NormalizedName { get; set; }

        public int DisplayOrder { get; set; }

        public long? VendorId { get; set; }

        public Vendor Vendor { get; set; }

        public Media ThumbnailImage { get; set; }

        public IList<ProductMedia> Medias { get; protected set; } = new List<ProductMedia>();

        public IList<ProductLink> ProductLinks { get; protected set; } = new List<ProductLink>();

        public IList<ProductLink> LinkedProductLinks { get; protected set; } = new List<ProductLink>();

        public IList<ProductAttributeValue> AttributeValues { get; protected set; } = new List<ProductAttributeValue>();

        public IList<ProductOptionValue> OptionValues { get; protected set; } = new List<ProductOptionValue>();

        public IList<ProductCategory> Categories { get; protected set; } = new List<ProductCategory>();

        public int ReviewsCount { get; set; }

        public double? RatingAverage { get; set; }

        public long? BrandId { get; set; }
        public Brand Brand { get; set; }
        public long? TaxClassId { get; set; }
        public TaxClass TaxClass { get; set; }
        public string Via { get; set; }
        public string Currency { get; set; }
        public string Provider { get; set; }
        public bool IsRoundTrip { get; set; }
        public string FlightNumber { get; set; }
        public int? SoldSeats { get; set; }
        public bool? SaleRtOnly { get; set; }
        public DateTimeOffset? DepartureDate { get; set; }
        public DateTimeOffset? LandingDate { get; set; }

        // Return

        public string ReturnFlightNumber { get; set; }
        public DateTimeOffset? ReturnDepartureDate { get; set; }
        public DateTimeOffset? ReturnLandingDate { get; set; }
        public long? ReturnCarrierId { get; set; }
        public Brand ReturnCarrier { get; set; }
        public long? ReturnAircraftId { get; set; }
        public TaxClass ReturnAircraft { get; set; }
        public string ReturnTerminal { get; set; }
        public string ReturnVia { get; set; }
        public string ReservationNumber { get; set; }

        public string Status { get; set; }
        public string FlightClass { get; set; }
        
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

        public void AddCategory(ProductCategory category)
        {
            category.Product = this;
            Categories.Add(category);
        }

        public void AddMedia(ProductMedia media)
        {
            media.Product = this;
            Medias.Add(media);
        }

        public void RemoveMedia(ProductMedia media)
        {
            media.Product = null;
            Medias.Remove(media);
        }

        public void AddAttributeValue(ProductAttributeValue attributeValue)
        {
            attributeValue.Product = this;
            AttributeValues.Add(attributeValue);
        }

        public void AddOptionValue(ProductOptionValue optionValue)
        {
            optionValue.Product = this;
            OptionValues.Add(optionValue);
        }

        public void AddProductLinks(ProductLink productLink)
        {
            productLink.Product = this;
            ProductLinks.Add(productLink);
        }

        public virtual IList<ProductOptionCombination> OptionCombinations { get; protected set; } = new List<ProductOptionCombination>();

        public void AddOptionCombination(ProductOptionCombination combination)
        {
            combination.Product = this;
            OptionCombinations.Add(combination);
        }

        public Product Clone()
        {
            var product = new Product();
            product.Name = Name;
            product.MetaTitle = MetaTitle;
            product.MetaKeywords = MetaKeywords;
            product.MetaDescription = MetaDescription;
            product.ShortDescription = ShortDescription;
            product.Description = Description;
            product.Specification = Specification;
            product.IsPublished = true;
            product.PublishedOn = DateTimeOffset.Now;
            product.Price = Price;
            product.OldPrice = OldPrice;
            product.IsAllowToOrder = IsAllowToOrder;
            product.IsCallForPricing = IsCallForPricing;
            product.StockQuantity = StockQuantity;
            product.BrandId = BrandId;
            product.VendorId = VendorId;
            product.Via = Via;
            product.DisplayOrder = DisplayOrder;
            product.SpecialPriceEnd = SpecialPriceEnd;
            product.SpecialPriceStart = SpecialPriceStart;
            product.Sku = Sku;
            product.Currency = Currency;
            product.Provider = Provider;
            product.TaxClassId = TaxClassId;
            product.IsRoundTrip = IsRoundTrip;
            product.FlightNumber = FlightNumber;
            product.ReturnAircraftId = ReturnAircraftId;
            product.ReturnCarrierId = ReturnCarrierId;
            product.ReturnFlightNumber = ReturnFlightNumber;
            product.ReturnVia = ReturnVia;
            product.ReturnTerminal = ReturnTerminal;
            product.ReturnDepartureDate = ReturnDepartureDate;
            product.ReturnLandingDate = ReturnLandingDate;
            product.AdminRoundTrip = AdminRoundTrip;
            product.AdminRoundTripOperatorId = AdminRoundTripOperatorId;
            product.AdminPayLater = AdminPayLater;
            product.AdminPayLaterRule = AdminPayLaterRule;
            product.AdminBlackList = AdminBlackList;
            product.AdminPasExpirityRule = AdminPasExpirityRule;
            product.AdminIsSpecialOffer = AdminIsSpecialOffer;
            product.AdminNotifyAgencies = AdminNotifyAgencies;
            product.AdminNotifyLastPassanger = AdminNotifyLastPassanger;
            product.AdminIsLastMinute = AdminIsLastMinute;
            product.AdminReturnPayLater = AdminReturnPayLater;
            product.AdminReturnPayLaterRule = AdminReturnPayLaterRule;
            product.AdminReturnBlackList = AdminReturnBlackList;
            product.AdminReturnPasExpirityRule = AdminReturnPasExpirityRule;
            product.AdminReturnIsSpecialOffer = AdminReturnIsSpecialOffer;
            product.AdminReturnNotifyAgencies = AdminReturnNotifyAgencies;
            product.AdminReturnNotifyLastPassanger = AdminReturnNotifyLastPassanger;
            product.AdminReturnIsLastMinute = AdminReturnIsLastMinute;
            product.ReservationNumber = ReservationNumber;
            product.Status = Status;
            product.FlightClass = FlightClass;
            product.SoldSeats = 0;
            product.DepartureDate = DepartureDate;
            product.LandingDate = LandingDate;

            foreach (var attribute in AttributeValues)
            {
                product.AddAttributeValue(new ProductAttributeValue
                {
                    AttributeId = attribute.AttributeId,
                    Value = attribute.Value
                });
            }

            foreach (var category in Categories)
            {
                product.AddCategory(new ProductCategory
                {
                    CategoryId = category.CategoryId
                });
            }

            return product;
        }
    }
}

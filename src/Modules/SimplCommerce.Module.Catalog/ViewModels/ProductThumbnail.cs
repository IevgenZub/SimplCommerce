using System;
using SimplCommerce.Module.Catalog.Models;
using SimplCommerce.Module.Core.Models;
using System.Linq;

namespace SimplCommerce.Module.Catalog.ViewModels
{
    public class ProductThumbnail
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string SeoTitle { get; set; }

        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        public decimal? SpecialPrice { get; set; }

        public bool IsCallForPricing { get; set; }

        public bool IsAllowToOrder { get; set; }

        public int? StockQuantity { get; set; }

        public DateTimeOffset? SpecialPriceStart { get; set; }

        public DateTimeOffset? SpecialPriceEnd { get; set; }

        public Media ThumbnailImage { get; set; }

        public string ThumbnailUrl { get; set; }

        public int ReviewsCount { get; set; }

        public double? RatingAverage { get; set; }

        public CalculatedProductPrice CalculatedProductPrice { get; set; }

        public string Departure { get; set; }
        public string Landing { get; set; }
        public DateTimeOffset? DepartureDate { get; set; }
        public DateTimeOffset? LandingDate { get; set; }
        public string Carrier { get; set; }
        public string Vendor { get; set; }
        public string Via { get; set; }
        public string Baggage { get; set; }
        public string Provider { get; set; }
        public string Terminal { get; set; }
        public string FlightNumber { get; set; }
        public string ReturnAircraft { get; set; }
        public string Aircraft { get; set; }
        public string ReturnCarrier { get; set; }
        public string ReturnFlightNumber { get; set; }
        public string ReturnVia { get; set; }
        public string ReturnTerminal { get; set; }
        public DateTimeOffset? ReturnDepartureDate { get; set; }
        public DateTimeOffset? ReturnLandingDate { get; set; }
        public bool? IsRoundTrip { get; set; }

        /*
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
            */
        public static ProductThumbnail FromProduct(Product product)
        {
            var productThumbnail = new ProductThumbnail
            {
                Id = product.Id,
                Name = product.Name,
                SeoTitle = product.SeoTitle,
                Price = product.Price,
                OldPrice = product.OldPrice,
                SpecialPrice = product.SpecialPrice,
                SpecialPriceStart = product.SpecialPriceStart,
                SpecialPriceEnd = product.SpecialPriceEnd,
                StockQuantity = product.StockQuantity,
                IsAllowToOrder = product.IsAllowToOrder,
                IsCallForPricing = product.IsCallForPricing,
                ThumbnailImage = product.ThumbnailImage,
                ReviewsCount = product.ReviewsCount,
                RatingAverage = product.RatingAverage,
                Departure = product.ShortDescription,
                Landing = product.Description,
                DepartureDate = product.SpecialPriceStart,
                LandingDate = product.SpecialPriceEnd,
                Provider = product.Provider,
                ReturnDepartureDate = product.ReturnDepartureDate,
                ReturnLandingDate = product.ReturnLandingDate,
                ReturnFlightNumber = product.ReturnFlightNumber,
                Terminal = product.Sku,
                ReturnTerminal = product.ReturnTerminal,
                IsRoundTrip = product.IsRoundTrip
        };

            return productThumbnail;
        }
    }
}

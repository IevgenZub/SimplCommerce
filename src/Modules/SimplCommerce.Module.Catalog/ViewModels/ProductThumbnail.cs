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

        public decimal ChildPrice { get; set; }

        public decimal? OldPrice { get; set; }

        public decimal? SpecialPrice { get; set; }

        public bool IsCallForPricing { get; set; }

        public bool IsAllowToOrder { get; set; }

        public int? StockQuantity { get; set; }

        public DateTimeOffset? SpecialPriceStart { get; set; }

        public DateTimeOffset? SpecialPriceEnd { get; set; }

        public Media ThumbnailImage { get; set; }

        public Media ReturnThumbnailImage { get; set; }

        public string ThumbnailUrl { get; set; }

        public string ReturnThumbnailUrl { get; set; }

        public int ReviewsCount { get; set; }

        public double? RatingAverage { get; set; }

        public CalculatedProductPrice CalculatedProductPrice { get; set; }

        public string Departure { get; set; }
        public string Landing { get; set; }
        public DateTimeOffset? DepartureDate { get; set; }
        public int DurationHours { get; set; }
        public int DurationMinutes { get; set; }
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
        public int ReturnDurationHours { get; set; }
        public int ReturnDurationMinutes { get; set; }
        public bool IsRoundTrip { get; set; }
        public string Currency { get; set; }

        public ProductDetail Details { get; set; }

        public static ProductThumbnail FromProduct(Product product, bool isVendor, bool isRoundTrip = false)
        {
            var productThumbnail = new ProductThumbnail
            {
                Id = product.Id,
                Name = product.Name,
                SeoTitle = product.SeoTitle,
                Price = isVendor ? product.AgencyPrice : product.PassengerPrice,
                ChildPrice = isVendor ? product.AgencyChildPrice : product.PassengerChildPrice,
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
                Departure = product.Departure.Split(',')[0] + " (" + product.Departure.Split('(', ')')[1] + ")",
                Landing = product.Destination.Split(',')[0] + " (" + product.Destination.Split('(', ')')[1] + ")",
                DepartureDate = product.DepartureDate,
                DurationHours = product.DurationHours,
                DurationMinutes = product.DurationMinutes,
                Provider = product.Provider,
                ReturnDepartureDate = product.ReturnDepartureDate,
                ReturnDurationHours = product.ReturnDurationHours,
                ReturnDurationMinutes = product.ReturnDurationMinutes,
                ReturnFlightNumber = product.ReturnFlightNumber,
                Terminal = product.Terminal,
                ReturnTerminal = product.ReturnTerminal,
                IsRoundTrip = product.IsRoundTrip,
                FlightNumber = product.FlightNumber,
                Currency = product.Currency,
                Carrier = product.Brand == null ? "" : product.Brand.Name,
                ReturnCarrier = product.ReturnCarrier == null ? "" : product.ReturnCarrier.Name,
                Aircraft = product.TaxClass == null ? "" : product.TaxClass.Name,
                Via = product.Via,
                ReturnAircraft = product.ReturnAircraft == null ? "" : product.ReturnAircraft.Name,
                ReturnVia = product.ReturnVia
            };

            return productThumbnail;
        }
    }
}

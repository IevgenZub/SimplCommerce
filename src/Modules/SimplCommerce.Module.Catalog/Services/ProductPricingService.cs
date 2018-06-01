using System;
using SimplCommerce.Module.Catalog.Models;
using SimplCommerce.Module.Catalog.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SimplCommerce.Module.Catalog.Services
{
    public class ProductPricingService : IProductPricingService
    {
        private decimal _priceRegulation;

        public ProductPricingService(IConfiguration config)
        {
            _priceRegulation = config.GetValue<decimal>("Price.Regulation");
        }

        public CalculatedProductPrice CalculateProductChildPrice(Product product, bool isVendor)
        {
            return CalculateProductPrice(isVendor ? product.AgencyChildPrice : product.PassengerChildPrice, product.OldPrice, product.SpecialPrice, product.SpecialPriceStart, product.SpecialPriceEnd);
        }

        public CalculatedProductPrice CalculateProductPrice(ProductThumbnail productThumbnail)
        {
            return CalculateProductPrice(productThumbnail.Price, productThumbnail.OldPrice, productThumbnail.SpecialPrice, productThumbnail.SpecialPriceStart, productThumbnail.SpecialPriceEnd);
        }

        public CalculatedProductPrice CalculateProductPrice(Product product, bool isVendor)
        {
            return CalculateProductPrice(isVendor ? product.AgencyPrice : product.PassengerPrice, product.OldPrice, product.SpecialPrice, product.SpecialPriceStart, product.SpecialPriceEnd);
        }

        public CalculatedProductPrice CalculateProductPrice(decimal price, decimal? oldPrice, decimal? specialPrice, DateTimeOffset? specialPriceStart, DateTimeOffset? specialPriceEnd)
        {
            var percentOfSaving = 0;
            var calculatedPrice = price;

            if (specialPrice.HasValue && specialPriceStart < DateTimeOffset.Now && DateTimeOffset.Now < specialPriceEnd)
            {
                calculatedPrice = specialPrice.Value;

                if (!oldPrice.HasValue || oldPrice < price)
                {
                    oldPrice = price;
                }
            }

            if (oldPrice.HasValue && oldPrice.Value > 0 && oldPrice > calculatedPrice)
            {
                percentOfSaving = (int)(100 - Math.Ceiling((calculatedPrice / oldPrice.Value) * 100));
            }

            calculatedPrice += _priceRegulation * calculatedPrice / 100;

            return new CalculatedProductPrice
            {
                Price = calculatedPrice,
                OldPrice = oldPrice,
                PercentOfSaving = percentOfSaving
            };
        }
    }
}

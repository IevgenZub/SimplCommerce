﻿using System.Collections.Generic;
using System.Linq;
using SimplCommerce.Module.Catalog.Models;
using System;

namespace SimplCommerce.Module.ShoppingCart.ViewModels
{
    public class CartItemVm
    {
        public long Id { get; set; }

        public long ProductId { get; set; }

        public string ProductName { get; set; }

        public string Departure { get; set; }

        public string Landing { get; set; }

        public DateTimeOffset? DepartureDate { get; set; }
        
        public DateTimeOffset? ReturnDepartureDate { get; set; }

        public string ProductImage { get; set; }

        public decimal ProductPrice { get; set; }

        public decimal ChildPrice { get; set; }

        public string ProductPriceString => ProductPrice.ToString("C");

        public int Quantity { get; set; }

        public int QuantityChild { get; set; }

        public int QuantityBaby { get; set; }

        public decimal Total => Quantity * ProductPrice + QuantityChild * ProductPrice + QuantityBaby * ChildPrice;

        public string TotalString => Total.ToString("C");

        public IEnumerable<ProductVariationOption> VariationOptions { get; set; } = new List<ProductVariationOption>();

        public static IEnumerable<ProductVariationOption> GetVariationOption(Product variation)
        {
            if (variation == null)
            {
                return new List<ProductVariationOption>();
            }

            return variation.OptionCombinations.Select(x => new ProductVariationOption
            {
                OptionName = x.Option.Name,
                Value = x.Value
            });
        }
    }
}

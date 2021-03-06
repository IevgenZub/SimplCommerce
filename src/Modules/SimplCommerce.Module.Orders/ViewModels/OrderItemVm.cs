﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SimplCommerce.Module.Catalog.Models;
using SimplCommerce.Module.ShoppingCart.ViewModels;

namespace SimplCommerce.Module.Orders.ViewModels
{
    public class OrderItemVm
    {
        public long Id { get; set; }

        public long ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductImage { get; set; }

        public decimal ProductPrice { get; set; }

        public decimal ChildPrice { get; set; }

        public int Quantity { get; set; }

        public int QuantityChild { get; set; }

        public int QuantityBaby { get; set; }

        public int ShippedQuantity { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TaxPercent { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal Total => (Quantity * ProductPrice) + (QuantityChild * ProductPrice) + (QuantityBaby * ChildPrice);

        public decimal RowTotal => Total + TaxAmount - DiscountAmount;

        public string TaxAmountString => TaxAmount + "$";

        public string ProductPriceString => ProductPrice + "$";

        public string ProductChildPriceString => ChildPrice + "$";

        public string DiscountAmountString => DiscountAmount + "$";

        public string TotalString => Total + "$";

        public string RowTotalString => RowTotal + "$";

        public IEnumerable<ProductVariationOptionVm> VariationOptions { get; set; } =
            new List<ProductVariationOptionVm>();

        public static IEnumerable<ProductVariationOptionVm> GetVariationOption(Product variation)
        {
            if (variation == null)
            {
                return new List<ProductVariationOptionVm>();
            }

            return variation.OptionCombinations.Select(x => new ProductVariationOptionVm
            {
                OptionName = x.Option.Name,
                Value = x.Value
            });
        }
    }
}

namespace SimplCommerce.Module.ShoppingCart.ViewModels
{
    public class AddToCartResult
    {
        public string ProductName { get; set; }

        public string ProductImage { get; set; }

        public decimal ProductPrice { get; set; }

        public decimal ChildPrice { get; set; }

        public string VariationName { get; set; }

        public int Quantity { get; set; }

        public int QuantityChild { get; set; }

        public int QuantityBaby { get; set; }

        public int CartItemCount { get; set; }

        public decimal CartAmount { get; set; }
    }
}

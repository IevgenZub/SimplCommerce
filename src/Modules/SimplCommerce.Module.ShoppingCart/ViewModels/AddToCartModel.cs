namespace SimplCommerce.Module.ShoppingCart.ViewModels
{
    public class AddToCartModel
    {
        public long ProductId { get; set; }

        public long MergedProductId { get; set; }

        public string VariationName { get; set; }

        public int Quantity { get; set; }

        public int QuantityChild { get; set; }

        public int QuantityBaby { get; set; }

    }
}

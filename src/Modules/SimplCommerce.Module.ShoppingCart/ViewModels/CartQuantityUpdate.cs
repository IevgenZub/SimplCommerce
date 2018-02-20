namespace SimplCommerce.Module.ShoppingCart.ViewModels
{
    public class CartQuantityUpdate
    {
        public long CartItemId { get; set; }

        public int Quantity { get; set; }

        public int QuantityChild { get; set; }

        public int QuantityBaby { get; set; }
    }
}

using SimplCommerce.Infrastructure.Models;
using SimplCommerce.Module.Catalog.Models;

namespace SimplCommerce.Module.Orders.Models
{
    public class OrderItem : EntityBase
    {
        public Order Order { get; set; }

        public long ProductId { get; set; }

        public Product Product { get; set; }

        public decimal ProductPrice { get; set; }

        public decimal ChildPrice { get; set; }

        public int Quantity { get; set; }

        public int QuantityChild { get; set; }

        public int QuantityBaby { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TaxPercent { get; set; }
    }
}

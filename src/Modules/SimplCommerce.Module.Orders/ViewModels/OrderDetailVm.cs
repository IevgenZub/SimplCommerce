using System;
using System.Collections.Generic;

namespace SimplCommerce.Module.Orders.ViewModels
{
    public class OrderDetailVm
    {
        public long Id { get; set; }

        public string PnrNumber { get; set; }

        public string ConfirmationNumber { get; set; }

        public long CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public string OrderStatusString { get; set; }

        public int OrderStatus { get; set; }

        public decimal Subtotal { get; set; }

        public decimal Discount { get; set; }

        public decimal SubTotalWithDiscount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ShippingAmount { get; set; }

        public decimal OrderTotal { get; set; }

        public string ShippingMethod { get; set; }

        public string PaymentMethod { get; set; }

        public string SubtotalString { get { return Subtotal + "$"; } }

        public string DiscountString { get { return Discount + "$"; } }

        public string SubtotalWithDiscountString { get { return SubTotalWithDiscount + "$"; } }

        public string TaxAmountString { get { return TaxAmount + "$"; } }

        public string ShippingAmountString { get { return ShippingAmount + "$"; } }

        public string OrderTotalString { get { return OrderTotal + "$"; } }

        public ShippingAddressVm ShippingAddress { get; set; }

        public IEnumerable<dynamic> RegistrationAddress { get; set; }

        public IList<OrderItemVm> OrderItems { get; set; } = new List<OrderItemVm>();
    }
}

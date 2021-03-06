﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SimplCommerce.Infrastructure.Models;
using SimplCommerce.Module.Core.Models;

namespace SimplCommerce.Module.Orders.Models
{
    public class Order : EntityBase
    {
        public Order()
        {
            CreatedOn = DateTimeOffset.Now;
            UpdatedOn = DateTimeOffset.Now;
            OrderStatus = OrderStatus.New;
        }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public long CreatedById { get; set; }

        [JsonIgnore]
        public User CreatedBy { get; set; }

        public long? VendorId { get; set; }

        public string ExternalNumber { get; set; }

        public string PnrNumber { get; set; }

        public string AgencyReservationNumber { get; set; }

        public string CouponCode { get; set; }

        public string CouponRuleName { get; set; }

        public decimal Discount { get; set; }

        public decimal SubTotal { get; set; }

        public decimal SubTotalWithDiscount { get; set; }

        public long ShippingAddressId { get; set; }

        public OrderAddress ShippingAddress { get; set; }

        public long BillingAddressId { get; set; }

        public OrderAddress BillingAddress { get; set; }

        public IList<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public OrderStatus OrderStatus { get; set; }

        public long? ParentId { get; set; }

        public Order Parent { get; set; }

        public string ShippingMethod { get; set; }

        public decimal ShippingAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal OrderTotal { get; set; }

        public string PaymentMethod { get; set; }

        public IList<Order> Children { get; protected set; } = new List<Order>();

        public IList<OrderRegistrationAddress> RegistrationAddress { get; set; } = new List<OrderRegistrationAddress>();

        public void AddOrderItem(OrderItem item)
        {
            item.Order = this;
            OrderItems.Add(item);
        }
    }
}

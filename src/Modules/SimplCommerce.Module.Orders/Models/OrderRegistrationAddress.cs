using System;
using SimplCommerce.Infrastructure.Models;
using SimplCommerce.Module.Core.Models;

namespace SimplCommerce.Module.Orders.Models
{
    public class OrderRegistrationAddress : EntityBase
    {
        public long OrderId { get; set; }

        public Order Order { get; set; }

        public long AddressId { get; set; }

        public OrderAddress Address { get; set; }
    }
}

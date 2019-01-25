using System;

namespace SimplCommerce.Module.Shipments.Services
{
    public class ShipmentItemVm
    {
        public long ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSku { get; set; }

        public long OrderItemId { get; set; }

        public int OrderedQuantity { get; set; }

        public int ShippedQuantity { get; set; }

        public int AvailableQuantity { get; set; }

        public int QuantityToShip { get; set; }

        public long OrderId { get; set; }
        public string SaleDate { get; set; }
        public string Status { get; set; }
        public string Booking { get; set; }
        public string Confirmation { get; set; }
        public string Route { get; set; }
        public bool IsRoundTrip { get; set; }
        public string Passenger { get; set; }
        public string FlightNumber { get; set; }
        public DateTime FlightDate { get; set; }
        public string AgencyPassenger { get; set; }
        public decimal AgencyPrice { get; set; }
        public string Currency { get; set; }
        public decimal AgencyFee { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal Profit { get; set; }
    }
}

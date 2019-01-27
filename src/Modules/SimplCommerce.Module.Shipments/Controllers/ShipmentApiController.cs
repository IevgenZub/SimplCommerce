using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Infrastructure.Web.SmartTable;
using SimplCommerce.Module.Core.Extensions;
using SimplCommerce.Module.Orders.Models;
using SimplCommerce.Module.Orders.Services;
using SimplCommerce.Module.Shipments.Models;
using SimplCommerce.Module.Shipments.Services;
using SimplCommerce.Module.Shipments.ViewModels;

namespace SimplCommerce.Module.Shipments.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/shipments")]
    public class ShipmentApiController : Controller
    {
        private readonly IRepository<Shipment> _shipmentRepository;
        private readonly IRepository<OrderRegistrationAddress> _orderRepository;
        private readonly IShipmentService _shipmentService;
        private readonly IWorkContext _workContext;

        public ShipmentApiController(IRepository<Shipment> shipmentRepository, 
            IShipmentService shipmentService, IWorkContext workContext,
            IRepository<OrderRegistrationAddress> orderRepository)
        {
            _shipmentRepository = shipmentRepository;
            _orderRepository = orderRepository;
            _shipmentService = shipmentService;
            _workContext = workContext;
        }

        [HttpGet("/api/orders/{orderId}/items-to-ship")]
        public async Task<IActionResult> GetItemsToShip(long orderId, long warehouseId)
        {
            var model = new ShipmentForm
            {
                OrderId = orderId,
                WarehouseId = warehouseId
            };

            var itemsToShip = await _shipmentService.GetItemToShip(orderId, warehouseId);
            foreach (var item in itemsToShip)
            {
               // item.QuantityToShip = item.OrderedQuantity - item.ShippedQuantity;
            }

            model.Items = itemsToShip;
            return Ok(model);
        }

        [HttpGet("/api/orders/{orderId}/shipments")]
        public async Task<IActionResult> GetByOrder(long orderId)
        {
            var shipments = await _shipmentRepository.Query()
                .Where(x => x.OrderId == orderId)
                .Select(x => new
                {

                }).ToListAsync();

            return Ok(shipments);
        }

        [HttpPost("grid")]
        public IActionResult List([FromBody] SmartTableParam param)
        {
            var query = _orderRepository.Query().
                Include(x => x.Address).
                Include(x => x.Order).
                ThenInclude(x => x.OrderItems).
                ThenInclude(x => x.Product).
                ThenInclude(x => x.Vendor);

            if (param.Search.PredicateObject != null)
            {
                dynamic search = param.Search.PredicateObject;

    //            if (search.TrackingNumber != null)
    //            {
                    //string trackingNumber = search.TrackingNumber;
                    //query = query.Where(x => x.TrackingNumber.Contains(trackingNumber));
    //            }
            }

            var passengers = query.ToSmartTableResult(
              param,
              passenger => GetReportItem(passenger));

            return Json(passengers);
        }

        private ShipmentItemVm GetReportItem(OrderRegistrationAddress passenger)
        {
            var order = passenger.Order;
            var address = passenger.Address;
            var flight = passenger.Order.OrderItems[0].Product;
            var vendor = passenger.Order.OrderItems[0].Product.Vendor;

            return new ShipmentItemVm
            {
                OrderId = passenger.Order.Id,
                SaleDate = order.CreatedOn.Date,
                Status = order.OrderStatus.ToString(),
                Booking = order.AgencyReservationNumber,
                Confirmation = order.PnrNumber,
                Route = flight.Departure.Split('(', ')')[1] + "-" + flight.Destination.Split('(', ')')[1],
                IsRoundTrip = flight.IsRoundTrip,
                Passenger = $"{address.ContactName} {address.AddressLine1} {address.AddressLine2} {address.City} {address.PostalCode} ",
                FlightNumber = flight.FlightNumber,
                FlightDate = flight.DepartureDate.Value.Date,
                AgencyPassenger = vendor == null ? string.Empty : vendor.Name,
                AgencyFee = order.ShippingAmount,
                AgencyPrice = flight.AgencyPrice,
                SalesPrice = flight.PassengerPrice
            };
        }

        [HttpGet("id")]
        public async Task<IActionResult> Get(long id)
        {
            var shipment = await _shipmentRepository.Query()
                .Select(x => new {
                    x.Id,
                    x.OrderId,
                    x.Warehouse.Name
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            return Ok(shipment);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ShipmentForm model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _workContext.GetCurrentUser();
                var shipment = new Shipment
                {
                    OrderId = model.OrderId,
                    WarehouseId = model.WarehouseId,
                    CreatedById = currentUser.Id,
                    TrackingNumber = model.TrackingNumber,
                    CreatedOn = DateTimeOffset.Now,
                    UpdatedOn = DateTimeOffset.Now
                };

                foreach(var item in model.Items)
                {


                    var shipmentItem = new ShipmentItem
                    {

                    };

                    shipment.Items.Add(shipmentItem);
                }

                var result = await _shipmentService.CreateShipment(shipment);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(Get), new { id = shipment.Id }, null);
                }

                return BadRequest(result.Error);
            }

            return BadRequest(ModelState);
        }
    }
}

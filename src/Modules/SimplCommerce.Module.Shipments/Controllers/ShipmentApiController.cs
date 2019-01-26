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
                item.QuantityToShip = item.OrderedQuantity - item.ShippedQuantity;
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
              passenger =>
                new ShipmentItemVm
                {
                    OrderId = passenger.Order.Id,
                    SaleDate = passenger.Order.CreatedOn.Date,
                    Status = passenger.Order.OrderStatus.ToString(),
                    Booking = passenger.Order.AgencyReservationNumber,
                    Confirmation = passenger.Order.PnrNumber,
                    Route = passenger.Order.OrderItems[0].Product.Departure.Split('(', ')')[1] + "-" + passenger.Order.OrderItems[0].Product.Destination.Split('(', ')')[1],
                    IsRoundTrip = passenger.Order.OrderItems[0].Product.IsRoundTrip,
                    Passenger = passenger.Address.ContactName,
                    FlightNumber = passenger.Order.OrderItems[0].Product.FlightNumber,
                    FlightDate = passenger.Order.OrderItems[0].Product.DepartureDate.Value.Date,
                    AgencyPassenger = passenger.Order.OrderItems[0].Product.Vendor == null ? string.Empty : passenger.Order.OrderItems[0].Product.Vendor.Name,
                    AgencyPrice = passenger.Order.OrderItems[0].Product.AgencyPrice,
                    SalesPrice = passenger.Order.OrderItems[0].Product.PassengerPrice
                    });

            return Json(passengers);
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
                    if(item.QuantityToShip <= 0)
                    {
                        continue;
                    }

                    var shipmentItem = new ShipmentItem
                    {
                        Shipment = shipment,
                        Quantity = item.QuantityToShip,
                        ProductId = item.ProductId,
                        OrderItemId = item.OrderItemId
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

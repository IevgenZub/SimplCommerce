using System;
using System.Linq;
using System.Linq.Expressions;
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
            IQueryable<OrderRegistrationAddress> query = _orderRepository.Query().
                Include(x => x.Address).
                Include(x => x.Order).
                ThenInclude(x => x.OrderItems).
                ThenInclude(x => x.Product).
                ThenInclude(x => x.Vendor);

            if (param.Search.PredicateObject != null)
            {
                dynamic search = param.Search.PredicateObject;

                if (search.Id != null)
                {
                    string id = search.Id;
                    query = query.Where(x => x.Order.Id.ToString() == id);
                }

                if (search.SaleDate != null)
                {
                    if (search.SaleDate.before != null)
                    {
                        DateTimeOffset before = search.SaleDate.before;
                        query = query.Where(x => x.Order.CreatedOn <= before);
                    }

                    if (search.SaleDate.after != null)
                    {
                        DateTimeOffset after = search.SaleDate.after;
                        query = query.Where(x => x.Order.CreatedOn >= after);
                    }
                }

                if (search.Status != null)
                {
                    string status = search.Status;
                    query = query.Where(x => x.Order.OrderStatus.ToString() == status);
                }

                if (search.Booking != null)
                {
                    string booking = search.Booking;
                    query = query.Where(x => x.Order.AgencyReservationNumber == booking);
                }

                if (search.Confirm != null)
                {
                    string confirm = search.Confirm;
                    query = query.Where(x => x.Order.PnrNumber == confirm);
                }

                if (search.FlightNumber != null)
                {
                    string flightNumber = search.FlightNumber;
                    query = query.Where(x => x.Order.OrderItems.Any(oi => oi.Product != null && 
                        oi.Product.FlightNumber == flightNumber));
                }

                if (search.Passenger != null)
                {
                    string passenger = search.Passenger;
                    query = query.Where(x => x.Address.ContactName.Contains(passenger));
                }

                if (search.Operator != null)
                {
                    string agency = search.Operator;
                    query = query.Where(x => x.Order.OrderItems.Any(oi => oi.Product != null &&
                        oi.Product.Vendor != null && oi.Product.Vendor.Name.Contains(agency)));
                }

                if (search.DepartureDate != null)
                {
                    if (search.DepartureDate.before != null)
                    {
                        DateTimeOffset before = search.DepartureDate.before;
                        query = query.Where(x => x.Order.OrderItems.Any(oi => oi.Product.DepartureDate <= before));
                    }

                    if (search.DepartureDate.after != null)
                    {
                        DateTimeOffset after = search.DepartureDate.after;
                        query = query.Where(x => x.Order.OrderItems.Any(oi => oi.Product.DepartureDate >= after));
                    }
                }
            }

            var sortExpression = GetSortExpression(param.Sort.Predicate);

            var passengers = query.ToSmartTableResult(param, passenger => GetReportItem(passenger), sortExpression);

            return Json(passengers);
        }

        private Expression<Func<OrderRegistrationAddress, object>> GetSortExpression(string sort)
        {
            Expression<Func<OrderRegistrationAddress, object>> sortExpression = x => x.Id;

            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "Id":
                        sortExpression = x => x.Id;
                        break;
                    case "SaleDate":
                        sortExpression = x => x.Order.CreatedOn;
                        break;
                    case "Status":
                        sortExpression = x => x.Order.OrderStatus;
                        break;
                    case "Booking":
                        sortExpression = x => x.Order.AgencyReservationNumber;
                        break;
                    case "Confirm":
                        sortExpression = x => x.Order.PnrNumber;
                        break;
                    case "Route":
                        sortExpression = x => x.Order.OrderItems[0].Product.Departure;
                        break;
                    case "IsRoundTrip":
                        sortExpression = x => x.Order.OrderItems[0].Product.IsRoundTrip;
                        break;
                    case "Passenger":
                        sortExpression = x => x.Address.ContactName;
                        break;
                    case "FlightNumber":
                        sortExpression = x => x.Order.OrderItems[0].Product.FlightNumber;
                        break;
                    case "FlightDate":
                        sortExpression = x => x.Order.OrderItems[0].Product.DepartureDate.Value.Date;
                        break;
                    case "FlightTime":
                        sortExpression = x => x.Order.OrderItems[0].Product.DepartureDate.Value.TimeOfDay;
                        break;
                    case "Operator":
                        sortExpression = x => x.Order.OrderItems[0].Product.Vendor.Name;
                        break;
                    case "AgencyPrice":
                        sortExpression = x => x.Order.OrderItems[0].Product.AgencyPrice;
                        break;
                    case "AgencyFee":
                        sortExpression = x => x.Order.ShippingAmount;
                        break;
                    case "SalesPrice":
                        sortExpression = x => x.Order.OrderItems[0].Product.PassengerPrice;
                        break;
                    default:
                        sortExpression = x => x.Id;
                        break;
                }
            }

            return sortExpression;
        }

        private ShipmentItemVm GetReportItem(OrderRegistrationAddress passenger)
        {
            var order = passenger.Order;
            var address = passenger.Address;
            var flight = passenger.Order.OrderItems[0].Product;
            var vendor = passenger.Order.OrderItems[0].Product?.Vendor;

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
                FlightDate = flight.DepartureDate.HasValue ? flight.DepartureDate.Value.Date : DateTime.Now,
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

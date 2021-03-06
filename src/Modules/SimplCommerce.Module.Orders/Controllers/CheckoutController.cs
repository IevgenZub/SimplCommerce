﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Module.Core.Extensions;
using SimplCommerce.Module.Core.Models;
using SimplCommerce.Module.Orders.Services;
using SimplCommerce.Module.Orders.ViewModels;
using SimplCommerce.Module.ShippingPrices.Services;
using SimplCommerce.Module.ShoppingCart.Models;
using SimplCommerce.Module.ShoppingCart.Services;
using SimplCommerce.Module.Core.ViewModels;
using SimplCommerce.Module.Orders.Models;

namespace SimplCommerce.Module.Orders.Controllers
{
    //[Authorize]
    [Route("checkout")]
    public class CheckoutController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<StateOrProvince> _stateOrProvinceRepository;
        private readonly IRepository<UserAddress> _userAddressRepository;
        private readonly IShippingPriceService _shippingPriceService;
        private readonly ICartService _cartService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<Cart> _cartRepository;

        public CheckoutController(
            IRepository<StateOrProvince> stateOrProvinceRepository,
            IRepository<Country> countryRepository,
            IRepository<UserAddress> userAddressRepository,
            IShippingPriceService shippingPriceService,
            IOrderService orderService,
            ICartService cartService,
            IWorkContext workContext,
            IRepository<Cart> cartRepository)
        {
            _stateOrProvinceRepository = stateOrProvinceRepository;
            _countryRepository = countryRepository;
            _userAddressRepository = userAddressRepository;
            _shippingPriceService = shippingPriceService;
            _orderService = orderService;
            _cartService = cartService;
            _workContext = workContext;
            _cartRepository = cartRepository;
        }

        [HttpGet("shipping")]
        public async Task<IActionResult> Shipping()
        {
            var model = new DeliveryInformationVm();

            var currentUser = await _workContext.GetCurrentUser();

            var cart = await _cartRepository.Query().Include(c => c.Items).ThenInclude(i => i.Product).Where(x => x.UserId == currentUser.Id && x.IsActive).FirstOrDefaultAsync();

            if (cart == null)
            {
                throw new ApplicationException($"Cart of user {currentUser.Id} cannot be found");
            }

            model.NumberofPassengers = cart.Items[0].Quantity + cart.Items[0].QuantityChild + cart.Items[0].QuantityBaby;
            model.PassportExpRule = cart.Items[0].Product.AdminPasExpirityRule.HasValue ? cart.Items[0].Product.AdminPasExpirityRule.Value : 0;
            model.DepartureDate = cart.Items[0].Product.DepartureDate;

            PopulateShippingForm(model, currentUser);

            model.NewAddress = new UserAddressFormViewModel
            {
                Countries = _countryRepository.Query()
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList()
            };

            return View(model);
        }

        [Route("add-address")]
        [HttpPost]
        public async Task<IActionResult> AddAddress(AddressFormVm model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _workContext.GetCurrentUser();

                var existing = _userAddressRepository.Query().Where(ua => ua.UserId == currentUser.Id);
                if (existing.Any(ua => ua.Address.ContactName == model.FirstName && ua.Address.AddressLine1 == model.LastName))
                {
                    return Ok("Error. Name already exists");
                }

                if (existing.Any(ua => ua.Address.City == model.DocumentNumber))
                {
                    return Ok("Error. Document number already exists");
                }

                var address = new Address
                {
                    ContactName = model.FirstName.ToUpper(),
                    AddressLine1 = model.LastName.ToUpper(),
                    AddressLine2 = model.BirthDate,
                    CountryId = model.CountryId,
                    StateOrProvinceId = 1,
                    DistrictId = 1,
                    City = model.DocumentNumber.ToUpper(),
                    PostalCode = model.DocumentExpiration,
                    Phone = model.Sex
                };

                var userAddress = new UserAddress
                {
                    Address = address,
                    AddressType = AddressType.Shipping,
                    UserId = currentUser.Id
                };

                _userAddressRepository.Add(userAddress);
                _userAddressRepository.SaveChanges();

                model.Id = userAddress.Id;

                return Ok(model);
            }

            return BadRequest();
        }

        [HttpPost("shipping")]
        public async Task<IActionResult> Shipping(DeliveryInformationVm model)
        {
            var currentUser = await _workContext.GetCurrentUser();
            if (model.ExistingShippingAddresses.Any(a => a.Selected))
            {
                model.ShippingAddressId = model.ExistingShippingAddresses.FirstOrDefault(a => a.Selected).UserAddressId;
            }

            // TODO Handle error messages
            if (!ModelState.IsValid && (model.ShippingAddressId == 0))
            {
                PopulateShippingForm(model, currentUser);
                return View(model);
            }

            var cart = await _cartRepository
               .Query().Include(c => c.Items).ThenInclude(ci => ci.Product)
               .Where(x => x.UserId == currentUser.Id && x.IsActive).FirstOrDefaultAsync();

            if (cart == null)
            {
                throw new ApplicationException($"Cart of user {currentUser.Id} cannot be found");
            }

            if ((cart.Items[0].Product.StockQuantity - cart.Items[0].Quantity - cart.Items[0].QuantityChild) < 0)
            {
                throw new ApplicationException("Can't order more than available");
            }

            cart.ShippingData = JsonConvert.SerializeObject(model);
            await _cartRepository.SaveChangesAsync();
            return Redirect("~/checkout/payment");
        }

        [HttpPost("update-tax-and-shipping-prices")]
        public async Task<IActionResult> UpdateTaxAndShippingPrices([FromBody] TaxAndShippingPriceRequestVm model)
        {
            var currentUser = await _workContext.GetCurrentUser();
            Address address;
            if (model.ExistingShippingAddressId != 0)
            {
                address = await _userAddressRepository.Query().Where(x => x.Id == model.ExistingShippingAddressId).Select(x => x.Address).FirstOrDefaultAsync();
                if (address == null)
                {
                    return NotFound();
                }
            }
            else
            {
                address = new Address
                {
                    CountryId = model.NewShippingAddress.CountryId,
                    StateOrProvinceId = model.NewShippingAddress.StateOrProvinceId,
                    AddressLine1 = model.NewShippingAddress.AddressLine1
                };
            }

            var orderTaxAndShippingPrice = new OrderTaxAndShippingPriceVm();
            orderTaxAndShippingPrice.Cart = await _cartService.GetCart(currentUser.Id, HttpContext.User.IsInRole("vendor"));

            var cart = await _cartRepository.Query().Where(x => x.Id == orderTaxAndShippingPrice.Cart.Id).FirstOrDefaultAsync();
            cart.TaxAmount = orderTaxAndShippingPrice.Cart.TaxAmount = await _orderService.GetTax(currentUser.Id, address.CountryId, address.StateOrProvinceId);

            var request = new GetShippingPriceRequest
            {
                OrderAmount = orderTaxAndShippingPrice.Cart.OrderTotal,
                ShippingAddress = address
            };

            orderTaxAndShippingPrice.ShippingPrices = await _shippingPriceService.GetApplicableShippingPrices(request);
            var selectedShippingMethod = string.IsNullOrWhiteSpace(model.SelectedShippingMethodName) 
                ? orderTaxAndShippingPrice.ShippingPrices.FirstOrDefault() 
                : orderTaxAndShippingPrice.ShippingPrices.FirstOrDefault(x => x.Name == model.SelectedShippingMethodName);
            if(selectedShippingMethod != null)
            {
                cart.ShippingAmount = orderTaxAndShippingPrice.Cart.ShippingAmount = selectedShippingMethod.Price;
                cart.ShippingMethod = orderTaxAndShippingPrice.SelectedShippingMethodName = selectedShippingMethod.Name;
            }

            await _cartRepository.SaveChangesAsync();
            return Ok(orderTaxAndShippingPrice);
        }

        [HttpGet("congratulation")]
        public IActionResult OrderConfirmation()
        {
            var pnr = HttpContext.Request.Query["pnr"].ToString();
            var lastName = HttpContext.Request.Query["lastName"].ToString();

            ViewData["pnr"] = pnr;
            ViewData["lastName"] = lastName;

            return View();
        }

        [HttpGet("payment-failed")]
        public IActionResult PaymentFailed()
        {
            return View();
        }

        private void PopulateShippingForm(DeliveryInformationVm model, User currentUser)
        {
            model.ExistingShippingAddresses = _userAddressRepository
                .Query()
                .Where(x => (x.AddressType == AddressType.Shipping) && (x.UserId == currentUser.Id))
                .Select(x => new ShippingAddressVm
                {
                    UserAddressId = x.Id,
                    ContactName = x.Address.ContactName,
                    Phone = x.Address.Phone,
                    AddressLine1 = x.Address.AddressLine1,
                    AddressLine2 = x.Address.AddressLine2,
                    DistrictName = x.Address.District.Name,
                    StateOrProvinceName = x.Address.StateOrProvince.Name,
                    CountryName = x.Address.Country.Name,
                    CityName = x.Address.City,
                    PostalCode = x.Address.PostalCode,
                    Mobile = x.Address.Mobile,
                    Email = x.Address.Email
                }).ToList();

            model.ShippingAddressId = currentUser.DefaultShippingAddressId ?? 0; 
        }
    }
}
﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stripe;
using SimplCommerce.Infrastructure;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Module.Core.Extensions;
using SimplCommerce.Module.Orders.Models;
using SimplCommerce.Module.Orders.Services;
using SimplCommerce.Module.ShoppingCart.Models;
using SimplCommerce.Module.Payments.Models;
using SimplCommerce.Module.PaymentStripe.ViewModels;
using SimplCommerce.Module.PaymentStripe.Models;
using SimplCommerce.Module.Core.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace SimplCommerce.Module.PaymentStripe.Controllers
{
    [Route("portmone")]
    public class StripeController : Controller
    {
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<PaymentProvider> _paymentProviderRepository;
        private readonly IRepository<Payment> _paymentRepository;


        public StripeController(
            IRepository<Cart> cartRepository,
            IOrderService orderService,
            IWorkContext workContext,
            IRepository<PaymentProvider> paymentProviderRepository,
            IRepository<Payment> paymentRepository,
            IRepository<User> userRepository)
        {
            _cartRepository = cartRepository;
            _orderService = orderService;
            _workContext = workContext;
            _paymentProviderRepository = paymentProviderRepository;
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Charge(string stripeEmail, string stripeToken)
        {
            var stripeProvider = await _paymentProviderRepository.Query().FirstOrDefaultAsync(x => x.Id == PaymentProviderHelper.StripeProviderId);
            var stripeSetting = JsonConvert.DeserializeObject<StripeConfigForm>(stripeProvider.AdditionalSettings);

            var customers = new StripeCustomerService(stripeSetting.PrivateKey);
            var charges = new StripeChargeService(stripeSetting.PrivateKey);
            var currentUser = await _workContext.GetCurrentUser();
            var order = await _orderService.CreateOrder(currentUser, "Portmone", User.IsInRole("vendor"), User.IsInRole("vendor"), OrderStatus.PendingPayment);

            var customer = customers.Create(new StripeCustomerCreateOptions
            {
                Email = stripeEmail,
                SourceToken = stripeToken
            });

            var zeroDecimalOrderAmount = order.OrderTotal;
            if(!CurrencyHelper.IsZeroDecimalCurrencies())
            {
                zeroDecimalOrderAmount = zeroDecimalOrderAmount * 100;
            }

            var regionInfo = new RegionInfo(CultureInfo.CurrentCulture.LCID);

            // TODO handle exception
            var charge = charges.Create(new StripeChargeCreateOptions
            {
                Amount = (int)zeroDecimalOrderAmount,
                Description = "Sample Charge",
                Currency =  regionInfo.ISOCurrencySymbol,
                CustomerId = customer.Id
            });

            var payment = new Payment()
            {
                OrderId = order.Id,
                Amount = order.OrderTotal,
                PaymentMethod = "Stripe",
                CreatedOn = DateTimeOffset.UtcNow,
                GatewayTransactionId = charge.Id
            };

            order.OrderStatus = OrderStatus.PaymentReceived;
            _paymentRepository.Add(payment);
            await _paymentRepository.SaveChangesAsync();

            return Redirect("~/checkout/congratulation");
        }
        
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateViewModel model)
        {
            var currentUser = await _workContext.GetCurrentUser();

            var order = await _orderService.CreateOrder(
                currentUser,
                "Portmone",
                HttpContext.User.IsInRole("vendor"),
                HttpContext.User.IsInRole("guest"),
                OrderStatus.PendingPayment);

            order.ExternalNumber = model.OrderNumber;
            _paymentRepository.SaveChanges();

            return Json("success");
        }

        [HttpPost("success-callback")]
        public async Task<IActionResult> PortmoneSuccessPostCallback([FromForm] PortmoneCallback portmoneCallback)
        {
            var userId = portmoneCallback.SHOPORDERNUMBER.Split('-')[0];

            var currentUser = _userRepository.Query()
                .Include(u => u.Roles).ThenInclude(r => r.Role)
                .First(u => u.Id.ToString() == userId);

            var order = _orderService.GetOrderByNumber(portmoneCallback.SHOPORDERNUMBER);

            var payment = new Payment()
            {
                OrderId = order.Id,
                Amount = order.OrderTotal,
                PaymentMethod = "Portmone",
                CreatedOn = DateTimeOffset.UtcNow,
                GatewayTransactionId = portmoneCallback.SHOPORDERNUMBER
            };
            
            order.OrderStatus = OrderStatus.PaymentReceived;
            _paymentRepository.Add(payment);
            await _paymentRepository.SaveChangesAsync();

            return Redirect($"~/checkout/congratulation?pnr={order.PnrNumber}" + 
                $"&lastName={order.RegistrationAddress[0].Address.AddressLine1}");
        }
    }
}

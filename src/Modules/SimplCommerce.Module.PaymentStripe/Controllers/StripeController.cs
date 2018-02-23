using System;
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

namespace SimplCommerce.Module.PaymentStripe.Controllers
{
    [Route("portmone")]
    public class StripeController : Controller
    {
        private readonly IRepository<Cart> _cartRepository;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<PaymentProvider> _paymentProviderRepository;
        private readonly IRepository<Payment> _paymentRepository;

        public StripeController(
            IRepository<Cart> cartRepository,
            IOrderService orderService,
            IWorkContext workContext,
            IRepository<PaymentProvider> paymentProviderRepository,
            IRepository<Payment> paymentRepository)
        {
            _cartRepository = cartRepository;
            _orderService = orderService;
            _workContext = workContext;
            _paymentProviderRepository = paymentProviderRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<IActionResult> Charge(string stripeEmail, string stripeToken)
        {
            var stripeProvider = await _paymentProviderRepository.Query().FirstOrDefaultAsync(x => x.Id == PaymentProviderHelper.StripeProviderId);
            var stripeSetting = JsonConvert.DeserializeObject<StripeConfigForm>(stripeProvider.AdditionalSettings);

            var customers = new StripeCustomerService(stripeSetting.PrivateKey);
            var charges = new StripeChargeService(stripeSetting.PrivateKey);
            var currentUser = await _workContext.GetCurrentUser();
            var order = await _orderService.CreateOrder(currentUser, "Portmone", OrderStatus.PendingPayment);

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

        [HttpGet("success-callback")]
        public async Task<IActionResult> PortmoneSuccessGetCallback()
        {
            // TODO: Create Order and save payment

            return Redirect("~/checkout/congratulation");
        }

        [HttpPost("success-callback")]
        public async Task<IActionResult> PortmoneSuccessPostCallback([FromForm] PortmoneCallback portmoneCallback)
        {
            // TODO: Create Order and save payment

            return Redirect("~/checkout/congratulation");
        }


    }

    public class PortmoneCallback
    {
        public string BILL_AMOUNT { get; set; }
        public string SHOPORDERNUMBER { get; set; }
        public string APPROVALCODE { get; set; }
        public string RECEIPT_URL { get; set; }
        public string TOKEN { get; set; }
        public string CARD_PAYMENT_SYSTEM { get; set; }
        public string CARD_LAST_DIGITS { get; set; }
        public string CARD_MASK { get; set; }
        public string DESCRIPTION { get; set; }
        public string ATTRIBUTE1 { get; set; }
        public string ATTRIBUTE2 { get; set; }
        public string RESULT { get; set; }
    }
}

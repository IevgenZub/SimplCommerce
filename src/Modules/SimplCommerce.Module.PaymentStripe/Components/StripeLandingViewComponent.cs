using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SimplCommerce.Module.Core.Extensions;
using SimplCommerce.Module.ShoppingCart.Services;
using SimplCommerce.Module.PaymentStripe.ViewModels;
using SimplCommerce.Infrastructure;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Module.Payments.Models;
using SimplCommerce.Module.PaymentStripe.Models;
using SimplCommerce.Module.Orders.Models;
using SimplCommerce.Module.Orders.Services;
using System;

namespace SimplCommerce.Module.PaymentStripe.Components
{
    public class StripeLandingViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<PaymentProvider> _paymentProviderRepository;
        private readonly IOrderService _orderService;

        public StripeLandingViewComponent(IOrderService orderService, ICartService cartService, IWorkContext workContext, IRepository<PaymentProvider> paymentProviderRepository)
        {
            _cartService = cartService;
            _workContext = workContext;
            _paymentProviderRepository = paymentProviderRepository;
            _orderService = orderService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var stripeProvider = await _paymentProviderRepository.Query().FirstOrDefaultAsync(x => x.Id == PaymentProviderHelper.StripeProviderId);
            var stripeSetting = JsonConvert.DeserializeObject<StripeConfigForm>(stripeProvider.AdditionalSettings);
            var curentUser = await _workContext.GetCurrentUser();
            var cart = await _cartService.GetCart(curentUser.Id);

            var currentUser = await _workContext.GetCurrentUser();
            
            var zeroDecimalAmount = cart.OrderTotal;

            var regionInfo = new RegionInfo(CultureInfo.CurrentCulture.LCID);
            var model = new StripeCheckoutForm();
            model.PublicKey = stripeSetting.PublicKey;
            model.Amount = (int)zeroDecimalAmount;
            model.OrderNumber = Guid.NewGuid().ToString();

            return View("/Modules/SimplCommerce.Module.PaymentStripe/Views/Components/StripeLanding.cshtml", model);
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimplCommerce.Module.Core.Extensions;
using SimplCommerce.Module.ShoppingCart.Services;
using SimplCommerce.Module.Orders.Services;
using SimplCommerce.Module.Core.Services;

namespace SimplCommerce.Module.Orders.Components
{
    public class OrderConfirmationViewComponent : ViewComponent
    {
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IMediaService _mediaService;

        public OrderConfirmationViewComponent(IOrderService orderService, IWorkContext workContext, IMediaService mediaService)
        {
            _orderService = orderService;
            _workContext = workContext;
            _mediaService = mediaService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string pnr, string lastName)
        {
            var curentUser = await _workContext.GetCurrentUser();
            var order = _orderService.GetOrderByPnr(pnr, lastName);

            if (order != null)
            { 
                ViewBag.CarrierImageUrl = _mediaService.GetThumbnailUrl(order.OrderItems[0].Product.ThumbnailImage);
                if (order.OrderItems.Count > 1)
                {
                    ViewBag.ReturnCarrierImageUrl = _mediaService.GetThumbnailUrl(order.OrderItems[1].Product.ThumbnailImage);
                }
            }

            return View("/Modules/SimplCommerce.Module.Orders/Views/Components/OrderConfirmation.cshtml", order);
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimplCommerce.Module.Core.Extensions;
using SimplCommerce.Module.ShoppingCart.Services;
using SimplCommerce.Module.Orders.Services;

namespace SimplCommerce.Module.Orders.Components
{
    public class OrderConfirmationViewComponent : ViewComponent
    {
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;

        public OrderConfirmationViewComponent(IOrderService orderService, IWorkContext workContext)
        {
            _orderService = orderService;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(string pnr)
        {
            var curentUser = await _workContext.GetCurrentUser();
            var order = _orderService.GetOrderByPnr(pnr);

            return View("/Modules/SimplCommerce.Module.Orders/Views/Components/OrderConfirmation.cshtml", order);
        }
    }
}

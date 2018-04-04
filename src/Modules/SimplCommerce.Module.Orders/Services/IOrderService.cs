using System.Threading.Tasks;
using SimplCommerce.Module.Core.Models;
using SimplCommerce.Module.Orders.Models;
using SimplCommerce.Module.Orders.ViewModels;

namespace SimplCommerce.Module.Orders.Services
{
    public interface IOrderService
    {
        /// <summary>
        /// Create order for user from active cart
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<Order> CreateOrder(User user, string paymentMethod, bool isVendor, bool isGuest, OrderStatus orderStatus = OrderStatus.New);

        Task<Order> CreateOrder(User user, string paymentMethod, DeliveryInformationVm shippingData, Address billingAddress, Address shippingAddress, bool isVendor, bool isGuest, OrderStatus orderStatus = OrderStatus.New);

        Task<decimal> GetTax(long cartOwnerUserId, long countryId, long stateOrProvinceId);

        Order GetOrder(int id);
    }
}

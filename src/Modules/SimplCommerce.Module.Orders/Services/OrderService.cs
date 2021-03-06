﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Module.Core.Models;
using SimplCommerce.Module.Orders.Models;
using SimplCommerce.Module.Pricing.Services;
using SimplCommerce.Module.ShoppingCart.Models;
using SimplCommerce.Module.Orders.ViewModels;
using SimplCommerce.Module.ShippingPrices.Services;
using SimplCommerce.Module.Tax.Services;
using SimplCommerce.Module.Catalog.Services;

namespace SimplCommerce.Module.Orders.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly ICouponService _couponService;
        private readonly IRepository<CartItem> _cartItemRepository;
        private readonly ITaxService _taxService;
        private readonly IShippingPriceService _shippingPriceService;
        private readonly IRepository<UserAddress> _userAddressRepository;
        private readonly IOrderEmailService _orderEmailService;
        private readonly IProductPricingService _productPricingService;


        public OrderService(IRepository<Order> orderRepository,
            IRepository<Cart> cartRepository,
            ICouponService couponService,
            IRepository<CartItem> cartItemRepository,
            ITaxService taxService,
            IShippingPriceService shippingPriceService,
            IRepository<UserAddress> userAddressRepository,
            IOrderEmailService orderEmailService,
            IProductPricingService productPricingService)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _couponService = couponService;
            _cartItemRepository = cartItemRepository;
            _taxService = taxService;
            _shippingPriceService = shippingPriceService;
            _userAddressRepository = userAddressRepository;
            _orderEmailService = orderEmailService;
            _productPricingService = productPricingService;
        }

        public async Task<Order> CreateOrder(User user, string paymentMethod, bool isVendor, bool isGuest, OrderStatus orderStatus = OrderStatus.New)
        {
            var cart = await _cartRepository
               .Query()
               .Where(x => x.UserId == user.Id && x.IsActive).FirstOrDefaultAsync();

            if (cart == null)
            {
                throw new ApplicationException($"Cart of user {user.Id} cannot be found");
            }

            var shippingData = JsonConvert.DeserializeObject<DeliveryInformationVm>(cart.ShippingData);
            Address billingAddress;
            Address shippingAddress;
            if (shippingData.ShippingAddressId == 0)
            {
                billingAddress = shippingAddress = null;
            }
            else
            {
                billingAddress = shippingAddress = _userAddressRepository.Query().Where(x => x.Id == shippingData.ShippingAddressId).Select(x => x.Address).First();
            }

            billingAddress.Email = shippingData.ContactEmail;
            billingAddress.Mobile = string.Format("{0} {1}", shippingData.ContactName, shippingData.ContactPhone);
            
            shippingAddress.Email = shippingData.ContactEmail;
            shippingAddress.Mobile = string.Format("{0} {1}", shippingData.ContactName, shippingData.ContactPhone);

            return await CreateOrder(user, paymentMethod, shippingData, billingAddress, shippingAddress, isVendor, isGuest, orderStatus);
        }

        public async Task<Order> CreateOrder(User user, string paymentMethod, DeliveryInformationVm shippingData, Address billingAddress, Address shippingAddress, bool isVendor, bool isGuest, OrderStatus orderStatus = OrderStatus.New)
        {
            var cart = _cartRepository
                .Query()
                .Include(c => c.Items).ThenInclude(x => x.Product)
                .Where(x => x.UserId == user.Id && x.IsActive).FirstOrDefault();

            if (cart == null)
            {
                throw new ApplicationException($"Cart of user {user.Id} cannot be found");
            }

            var discount = await ApplyDiscount(user, cart);
            var shippingMethod = await ValidateShippingMethod(shippingData.ShippingMethod, shippingAddress, cart);

            var orderBillingAddress = new OrderAddress()
            {
                AddressLine1 = billingAddress.AddressLine1,
                AddressLine2 = billingAddress.AddressLine2,
                ContactName = billingAddress.ContactName,
                CountryId = billingAddress.CountryId,
                StateOrProvinceId = billingAddress.StateOrProvinceId,
                DistrictId = billingAddress.DistrictId,
                City = billingAddress.City,
                PostalCode = billingAddress.PostalCode,
                Phone = billingAddress.Phone,
                Email = billingAddress.Email,
                Mobile = billingAddress.Mobile
            };

            var orderShippingAddress = new OrderAddress()
            {
                AddressLine1 = shippingAddress.AddressLine1,
                AddressLine2 = shippingAddress.AddressLine2,
                ContactName = shippingAddress.ContactName,
                CountryId = shippingAddress.CountryId,
                StateOrProvinceId = shippingAddress.StateOrProvinceId,
                DistrictId = shippingAddress.DistrictId,
                City = shippingAddress.City,
                PostalCode = shippingAddress.PostalCode,
                Phone = shippingAddress.Phone,
                Email = shippingAddress.Email,
                Mobile = shippingAddress.Mobile
            };

            var order = new Order
            {
                CreatedOn = DateTimeOffset.Now,
                CreatedById = user.Id,
                BillingAddress = orderBillingAddress,
                ShippingAddress = orderShippingAddress,
                PaymentMethod = paymentMethod,
                OrderStatus = orderStatus
            };

            foreach (var cartItem in cart.Items)
            {
                var taxPercent = await _taxService.GetTaxPercent(cartItem.Product.TaxClassId, shippingAddress.CountryId, shippingAddress.StateOrProvinceId);
                var orderItem = new OrderItem
                {
                    Product = cartItem.Product,
                    ProductPrice = _productPricingService.CalculateProductPrice(cartItem.Product, isVendor).Price,
                    ChildPrice = _productPricingService.CalculateProductChildPrice(cartItem.Product, isVendor).Price,
                    Quantity = cartItem.Quantity,
                    QuantityChild = cartItem.QuantityChild,
                    QuantityBaby = cartItem.QuantityBaby,
                    TaxPercent = taxPercent,
                    TaxAmount = cartItem.Quantity * (cartItem.Product.Price * taxPercent / 100)
                };

                order.AddOrderItem(orderItem);

                cartItem.Product.StockQuantity = cartItem.Product.StockQuantity - cartItem.Quantity - cartItem.QuantityChild;
                if (cartItem.Product.StockQuantity < 0)
                {
                    throw new ApplicationException("Can't order more seats that currently available");
                }

                cartItem.Product.SoldSeats = cartItem.Quantity + cartItem.QuantityChild;
            }

            order.VendorId = order.OrderItems[0].Product.VendorId;
            order.OrderStatus = orderStatus;
            order.CouponCode = cart.CouponCode;
            order.CouponRuleName = cart.CouponRuleName;
            order.Discount = discount;
            order.ShippingAmount = cart.ShippingAmount.HasValue ? cart.ShippingAmount.Value : 0;
            order.ShippingMethod = shippingMethod.Name;
            order.TaxAmount = order.OrderItems.Sum(x => x.TaxAmount);

            order.SubTotal = order.OrderItems.Sum(
                x => (x.ProductPrice * x.Quantity) + (x.ProductPrice * x.QuantityChild) + (x.QuantityBaby * x.ChildPrice));

            order.SubTotalWithDiscount = order.SubTotal - discount;
            order.OrderTotal = order.SubTotal + order.TaxAmount + order.ShippingAmount - order.Discount;
            _orderRepository.Add(order);

            cart.IsActive = false;
          
            foreach (var registrationAddress in shippingData.ExistingShippingAddresses.Where(esa => esa.Selected).ToList())
            {
                var address = _userAddressRepository.Query().Where(x => x.Id == registrationAddress.UserAddressId).Select(x => x.Address).First();
                var orderRegistrationAddress = new OrderRegistrationAddress
                {
                    Order = order,
                    Address = new OrderAddress {
                        ContactName = address.ContactName,
                        AddressLine1 = address.AddressLine1,
                        AddressLine2 = address.AddressLine2,
                        City = address.City,
                        CountryId = address.CountryId,
                        Phone = address.Phone,
                        PostalCode = address.PostalCode,
                        DistrictId = address.DistrictId,
                        StateOrProvinceId = address.StateOrProvinceId,
                        Email = address.Email,
                        Mobile = address.Mobile
                    }
                };

                order.RegistrationAddress.Add(orderRegistrationAddress);
            }
            
            _orderRepository.SaveChanges();

            var prefix = "E"; // Default
            var flightClass = cart.Items[0].Product.FlightClass;
            if (string.IsNullOrEmpty(flightClass))
            {
                prefix = flightClass[0].ToString();
            }

            order.PnrNumber = prefix + order.Id.ToString("000000.##");
            _orderRepository.SaveChanges();

            user.Email = order.ShippingAddress.Email;

            await _orderEmailService.SendEmailToUser(user, order, "OrderEmailToCustomer");
            await _orderEmailService.SendEmailToUser(user, order, "TicketEmail");

            return order;
        }

        public Order GetOrder(int id)
        {
            var order = _orderRepository.Query().Include(x => x.ShippingAddress).ThenInclude(x => x.District)
                .Include(x => x.ShippingAddress).ThenInclude(x => x.StateOrProvince)
                .Include(x => x.ShippingAddress).ThenInclude(x => x.Country)
                .Include(x => x.OrderItems).ThenInclude(x => x.Product).ThenInclude(x => x.ThumbnailImage)
                .Include(x => x.OrderItems).ThenInclude(x => x.Product).ThenInclude(x => x.OptionCombinations).ThenInclude(x => x.Option)
                .Include(x => x.CreatedBy)
                .FirstOrDefault(x => x.Id == id);

            if (order == null)
            {
                throw new ApplicationException($"Order not found by Id = {id}");
            }

            return order;
        }

        public Order GetOrderByNumber(string orderNumber)
        {
            var order = _orderRepository.Query()
                                .Include(x => x.RegistrationAddress).ThenInclude(ra => ra.Address).ThenInclude(a => a.Country)
                                .FirstOrDefault(x => x.ExternalNumber == orderNumber);

            if (order == null)
            {
                throw new ApplicationException($"Order not found by orderNumber = {orderNumber}");
            }

            return order;
        }


        public Order GetOrderByPnr(string pnr, string lastName)
        {
            var order = _orderRepository.Query().Include(x => x.ShippingAddress).ThenInclude(x => x.District)
                .Include(x => x.ShippingAddress).ThenInclude(x => x.StateOrProvince)
                .Include(x => x.ShippingAddress).ThenInclude(x => x.Country)
                .Include(x => x.OrderItems).ThenInclude(x => x.Product).ThenInclude(x => x.ThumbnailImage)
                .Include(x => x.OrderItems).ThenInclude(x => x.Product).ThenInclude(x => x.TaxClass)
                .Include(x => x.OrderItems).ThenInclude(x => x.Product).ThenInclude(x => x.Brand)
                .Include(x => x.OrderItems).ThenInclude(x => x.Product).ThenInclude(x => x.OptionCombinations).ThenInclude(x => x.Option)
                .Include(x => x.CreatedBy)
                .Include(x => x.RegistrationAddress).ThenInclude(ra => ra.Address).ThenInclude(a => a.Country)
                .FirstOrDefault(x => x.PnrNumber == pnr);

            return order.RegistrationAddress[0].Address.AddressLine1.ToLower() == lastName.ToLower() ? 
                order : null;
        }

        public async Task<decimal> GetTax(long cartOwnerUserId, long countryId, long stateOrProvinceId)
        {
            decimal taxAmount = 0;
            var cart = await _cartRepository.Query().FirstOrDefaultAsync(x => x.UserId == cartOwnerUserId && x.IsActive);
            if (cart == null)
            {
                throw new ApplicationException($"No active cart of user {cartOwnerUserId}");
            }

            var cartItems = _cartItemRepository.Query()
                .Where(x => x.CartId == cart.Id)
                .Select(x => new CartItemVm
                {
                    Quantity = x.Quantity,
                    Price = x.Product.Price,
                    TaxClassId = x.Product.TaxClass.Id
                }).ToList();

            foreach (var cartItem in cartItems)
            {
                if (cartItem.TaxClassId.HasValue)
                {
                    var taxRate = await _taxService.GetTaxPercent(cartItem.TaxClassId, countryId, stateOrProvinceId);
                    taxAmount = taxAmount + cartItem.Quantity * cartItem.Price * taxRate / 100;
                }
            }

            return taxAmount;
        }

        private async Task<decimal> ApplyDiscount(User user, Cart cart)
        {
            decimal discount = 0;
            if (!string.IsNullOrWhiteSpace(cart.CouponCode))
            {
                var cartInfoForCoupon = new CartInfoForCoupon
                {
                    Items = cart.Items.Select(x => new CartItemForCoupon { ProductId = x.ProductId, Quantity = x.Quantity }).ToList()
                };
                var couponValidationResult = await _couponService.Validate(cart.CouponCode, cartInfoForCoupon);
                if (couponValidationResult.Succeeded)
                {
                    discount = couponValidationResult.DiscountAmount;
                    _couponService.AddCouponUsage(user.Id, couponValidationResult.CouponId);
                }
                else
                {
                    throw new ApplicationException($"Unable to apply coupon {cart.CouponCode}. {couponValidationResult.ErrorMessage}");
                }
            }

            return discount;
        }

        private async Task<ShippingPrice> ValidateShippingMethod(string shippingMethodName, Address shippingAddress, Cart cart)
        {
            return new ShippingPrice() { Name = "Default", Price = 0, Description = "Default" };
        }
    }
}

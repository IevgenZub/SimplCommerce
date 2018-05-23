using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Module.ShoppingCart.Models;
using SimplCommerce.Module.ShoppingCart.ViewModels;
using SimplCommerce.Module.Core.Services;
using SimplCommerce.Module.Pricing.Services;
using SimplCommerce.Module.Catalog.Services;

namespace SimplCommerce.Module.ShoppingCart.Services
{
    public class CartService : ICartService
    {
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<CartItem> _cartItemRepository;
        private readonly IMediaService _mediaService;
        private readonly ICouponService _couponService;
        private readonly IProductPricingService _pricingService;

        public CartService(IProductPricingService pricingService, IRepository<Cart> cartRepository, IRepository<CartItem> cartItemRepository, ICouponService couponService, IMediaService mediaService)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _couponService = couponService;
            _mediaService = mediaService;
            _pricingService = pricingService;
        }

        public async Task AddToCart(long userId, long productId, int quantity, int quantityChild, int quaintyBaby)
        {
            var cart = _cartRepository.Query().Include(x => x.Items).FirstOrDefault(x => x.UserId == userId && x.IsActive);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                _cartRepository.Add(cart);
            }
            var cartItem = cart.Items.FirstOrDefault(x => x.ProductId == productId);
            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    Cart = cart,
                    ProductId = productId,
                    Quantity = quantity,
                    QuantityChild = quantityChild,
                    QuantityBaby = quaintyBaby,
                    CreatedOn = DateTimeOffset.Now
                };

                cart.Items.Add(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

           await  _cartRepository.SaveChangesAsync();
        }

        public async Task<CartVm> GetCart(long userId, bool isVendor)
        {
            var cart = _cartRepository.Query().FirstOrDefault(x => x.UserId == userId && x.IsActive);
            if (cart == null)
            {
                return new CartVm();
            }

            var cartVm = new CartVm()
            {
                Id = cart.Id,
                CouponCode = cart.CouponCode,
                TaxAmount = cart.TaxAmount,
                ShippingAmount = cart.ShippingAmount.HasValue ? cart.ShippingAmount : 0
            };

            cartVm.Items = _cartItemRepository
                .Query()
                .Include(x => x.Product).ThenInclude(p => p.ThumbnailImage)
                .Include(x => x.Product).ThenInclude(p => p.Brand)
                .Include(x => x.Product).ThenInclude(p => p.OptionCombinations).ThenInclude(o => o.Option)
                .Where(x => x.CartId == cart.Id)
                .Select(x => new CartItemVm
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    Carrier = x.Product.Brand.Name,
                    FlightNumber = x.Product.FlightNumber,
                    FlightClass = x.Product.FlightClass,
                    ReturnFlightNumber = x.Product.ReturnFlightNumber,
                    ProductPrice = _pricingService.CalculateProductPrice(x.Product, isVendor).Price,
                    ChildPrice = _pricingService.CalculateProductChildPrice(x.Product, isVendor).Price,
                    ProductImage = _mediaService.GetThumbnailUrl(x.Product.ThumbnailImage),
                    Quantity = x.Quantity,
                    QuantityChild = x.QuantityChild,
                    QuantityBaby = x.QuantityBaby,
                    Departure = x.Product.Departure,
                    Landing = x.Product.Destination,
                    IsRoundTrip = x.Product.IsRoundTrip,
                    DepartureDate = x.Product.DepartureDate,
                    LandingDate = x.Product.DepartureDate.Value.AddHours(x.Product.DurationHours).AddMinutes(x.Product.DurationMinutes),
                    ReturnDepartureDate = x.Product.ReturnDepartureDate,
                    ReturnLandingDate = x.Product.ReturnDepartureDate.Value.AddHours(x.Product.ReturnDurationHours).AddMinutes(x.Product.ReturnDurationMinutes),
                    VariationOptions = CartItemVm.GetVariationOption(x.Product)
                }).ToList();

            cartVm.SubTotal = cartVm.Items.Sum(x => x.Quantity * x.ProductPrice + x.QuantityChild * x.ProductPrice + x.QuantityBaby * x.ChildPrice);
            if (!string.IsNullOrWhiteSpace(cartVm.CouponCode))
            {
                var cartInfoForCoupon = new CartInfoForCoupon
                {
                    Items = cartVm.Items.Select(x => new CartItemForCoupon { ProductId = x.ProductId, Quantity = x.Quantity }).ToList()
                };
                var couponValidationResult = await _couponService.Validate(cartVm.CouponCode, cartInfoForCoupon);
                if (couponValidationResult.Succeeded)
                {
                    cartVm.Discount = couponValidationResult.DiscountAmount;
                }
            }

            return cartVm;
        }
        
        public async Task<CouponValidationResult> ApplyCoupon(long userId, string couponCode)
        {
            var cart = _cartRepository.Query().Include(x => x.Items).FirstOrDefault(x => x.UserId == userId && x.IsActive);

            var cartInfoForCoupon = new CartInfoForCoupon
            {
                Items = cart.Items.Select(x => new CartItemForCoupon { ProductId = x.ProductId, Quantity = x.Quantity }).ToList()
            };
            var couponValidationResult = await _couponService.Validate(couponCode, cartInfoForCoupon);
            if (couponValidationResult.Succeeded)
            {
                cart.CouponCode = couponCode;
                cart.CouponRuleName = couponValidationResult.CouponRuleName;
                _cartItemRepository.SaveChanges();
            }

            return couponValidationResult;
        }

        public void ApplyFee(long userId, decimal feeAmount)
        {
            var cart = _cartRepository.Query().Include(x => x.Items).FirstOrDefault(x => x.UserId == userId && x.IsActive);

            cart.ShippingAmount = feeAmount;
            _cartItemRepository.SaveChanges();
        }

        public async Task MigrateCart(long fromUserId, long toUserId)
        {
            var cartFrom = _cartRepository.Query().Include(x => x.Items).FirstOrDefault(x => x.UserId == fromUserId && x.IsActive);
            if (cartFrom != null && cartFrom.Items.Any())
            {
                var cartTo = _cartRepository.Query().Include(x => x.Items).FirstOrDefault(x => x.UserId == toUserId && x.IsActive);
                if (cartTo == null)
                {
                    cartTo = new Cart
                    {
                        UserId = toUserId
                    };

                    _cartRepository.Add(cartTo);
                }
                else
                {
                    if (cartFrom.Items.Any() && cartTo.Items.Any())
                    {

                        _cartItemRepository.Remove(cartTo.Items[0]);
                    }
                }

                foreach (var fromItem in cartFrom.Items)
                {
                    //var toItem = cartTo.Items.FirstOrDefault(x => x.ProductId == fromItem.ProductId);
                    //if(toItem == null)
                    //{
                        var toItem = new CartItem
                        {
                            Cart = cartTo,
                            ProductId = fromItem.ProductId,
                            Quantity = fromItem.Quantity,
                            QuantityChild = fromItem.QuantityChild,
                            QuantityBaby = fromItem.QuantityBaby,
                            CreatedOn = DateTimeOffset.Now
                        };

                        cartTo.Items.Add(toItem);
                    //}
                    //else
                    //{
                    ///    toItem.Quantity = toItem.Quantity + fromItem.Quantity;
                    ///}
                }

               await _cartRepository.SaveChangesAsync();
            }
        }
    }
}

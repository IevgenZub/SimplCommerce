﻿using System.Threading.Tasks;
using SimplCommerce.Module.Pricing.Services;
using SimplCommerce.Module.ShoppingCart.ViewModels;

namespace SimplCommerce.Module.ShoppingCart.Services
{
    public interface ICartService
    {
        Task AddToCart(long userId, long productId, int quantity, int quantityChild, int quantityBaby);

        Task<CartVm> GetCart(long userId, bool isVendor);

        Task<CouponValidationResult> ApplyCoupon(long userId, string couponCode);

        void ApplyFee(long userId, decimal feeAmount);

        Task MigrateCart(long fromUserId, long toUserId);
    }
}

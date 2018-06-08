using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Module.Core.Extensions;
using SimplCommerce.Module.Core.Models;
using SimplCommerce.Module.Core.Services;
using SimplCommerce.Module.ShoppingCart.Models;
using SimplCommerce.Module.ShoppingCart.Services;
using SimplCommerce.Module.ShoppingCart.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace SimplCommerce.Module.ShoppingCart.Controllers
{
    public class CartController : Controller
    {
        private readonly IRepository<CartItem> _cartItemRepository;
        private readonly ICartService _cartService;
        private readonly IMediaService _mediaService;
        private readonly IWorkContext _workContext;

        public CartController(
            UserManager<User> userManager,
            IRepository<CartItem> cartItemRepository,
            ICartService cartService,
            IMediaService mediaService,
            IWorkContext workContext)
        {
            _cartItemRepository = cartItemRepository;
            _cartService = cartService;
            _mediaService = mediaService;
            _workContext = workContext;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartModel model)
        {
            var currentUser = await _workContext.GetCurrentUser();
            var cart = await _cartService.GetCart(currentUser.Id, HttpContext.User.IsInRole("vendor"));
            var cartItems = _cartItemRepository.Query().Where(x => x.CartId == cart.Id);

            cartItems.ToList().ForEach(ci => _cartItemRepository.Remove(ci));
            _cartItemRepository.SaveChanges();

            await _cartService.AddToCart(currentUser.Id, model.ProductId, model.Quantity, model.QuantityChild, model.QuantityBaby);

            return RedirectToAction("AddToCartResult", new { productId = model.ProductId });
        }

        [HttpGet]
        public async Task<IActionResult> AddToCartResult(long productId)
        {
            var currentUser = await _workContext.GetCurrentUser();
            var cart = await _cartService.GetCart(currentUser.Id, User.IsInRole("vendor"));

            var model = new AddToCartResult
            {
                CartItemCount = cart.Items.Count,
                CartAmount = cart.SubTotal
            };

            var addedProduct = cart.Items.First(x => x.ProductId == productId);
            model.ProductName = addedProduct.FlightNumber;
            model.ProductImage = addedProduct.ProductImage;
            model.ProductPrice = addedProduct.ProductPrice;
            model.Quantity = addedProduct.Quantity;
            model.QuantityChild = addedProduct.QuantityChild;
            model.QuantityBaby = addedProduct.QuantityBaby;
            model.ChildPrice = addedProduct.ChildPrice;

            return PartialView(model);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var currentUser = await _workContext.GetCurrentUser();
            var cart = await _cartService.GetCart(currentUser.Id, HttpContext.User.IsInRole("vendor"));

            return Json(cart);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] CartQuantityUpdate model)
        {
            var query = _cartItemRepository.Query();
            var cartItem =  query.Include(c => c.Product).FirstOrDefault(x => x.Id == model.CartItemId);
            if (cartItem == null)
            {
                return new NotFoundResult();
            }

            if ((cartItem.Product.StockQuantity - model.Quantity - model.QuantityChild) < 0)
            {
                return BadRequest();
            }
            
            cartItem.Quantity = model.Quantity;
            cartItem.QuantityChild = model.QuantityChild;
            cartItem.QuantityBaby = model.QuantityBaby;

            _cartItemRepository.SaveChanges();

            return await List();
        }

        [HttpPost]
        public async Task<ActionResult> ApplyCoupon([FromBody] ApplyCouponForm model)
        {
            var currentUser = await _workContext.GetCurrentUser();
            var validationResult =  await _cartService.ApplyCoupon(currentUser.Id, model.CouponCode);
            if (validationResult.Succeeded)
            {
                var cart = await _cartService.GetCart(currentUser.Id, HttpContext.User.IsInRole("vendor"));
                return Json(cart);
            }

            return Json(validationResult);
        }

        [Authorize(Roles ="admin, vendor")]
        [HttpPost]
        public async Task<ActionResult> ApplyFee([FromBody] ApplyFeeForm model)
        {
            var currentUser = await _workContext.GetCurrentUser();

            _cartService.ApplyFee(currentUser.Id, model.FeeAmount);

            var cart = await _cartService.GetCart(currentUser.Id, HttpContext.User.IsInRole("vendor"));

            return Json(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Remove([FromBody] long itemId)
        {
            var cartItem = _cartItemRepository.Query().FirstOrDefault(x => x.Id == itemId);
            if (cartItem == null)
            {
                return new NotFoundResult();
            }

            _cartItemRepository.Remove(cartItem);
            _cartItemRepository.SaveChanges();

            return await List();
        }
    }
}

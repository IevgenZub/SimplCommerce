using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Module.Catalog.Models;
using SimplCommerce.Module.Catalog.Services;
using SimplCommerce.Module.Catalog.ViewModels;
using SimplCommerce.Module.Core.Services;
using SimplCommerce.Module.Core.ViewModels;
using System.Collections.Generic;
using System;

namespace SimplCommerce.Module.Catalog.Components
{
    public class ProductWidgetViewComponent : ViewComponent
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IMediaService _mediaService;
        private readonly IProductPricingService _productPricingService;

        public ProductWidgetViewComponent(IRepository<Product> productRepository, IMediaService mediaService, IProductPricingService productPricingService)
        {
            _productRepository = productRepository;
            _mediaService = mediaService;
            _productPricingService = productPricingService;
        }

        public IViewComponentResult Invoke(WidgetInstanceViewModel widgetInstance)
        {
            var model = new ProductWidgetComponentVm
            {
                Id = widgetInstance.Id,
                WidgetName = widgetInstance.Name,
                Setting = JsonConvert.DeserializeObject<ProductWidgetSetting>(widgetInstance.Data)
            };

            var query = _productRepository.Query()
              .Where(x =>(x.PassengerPrice > 0)  && x.IsPublished && x.IsVisibleIndividually && x.StockQuantity > 0 && x.DepartureDate >= DateTime.Now);
            
            if (model.Setting.FeaturedOnly)
            {
                query = query.Where(x => x.IsFeatured);
            }

            model.Products = query
              .Include(x => x.ThumbnailImage)
              .Include(x => x.ReturnAircraft)
              .Include(x => x.ReturnCarrier)
              .Include(x => x.Brand)
              .Include(x => x.TaxClass)
              .OrderByDescending(x => x.CreatedOn)
              .Take(model.Setting.NumberOfProducts)
              .Select(x => ProductThumbnail.FromProduct(x, User.IsInRole("vendor"), false)).ToList();

            foreach (var product in model.Products)
            {
                product.Details = GetProductDetails(product.Id);
                product.ThumbnailUrl = _mediaService.GetThumbnailUrl(product.ThumbnailImage);
                product.CalculatedProductPrice = _productPricingService.CalculateProductPrice(product);
            }

            return View("/Modules/SimplCommerce.Module.Catalog/Views/Components/ProductWidget.cshtml", model);
        }

        private ProductDetail GetProductDetails(long id)
        {
            var product = _productRepository.Query()
                .Include(x => x.OptionValues)
                .Include(x => x.Categories).ThenInclude(c => c.Category)
                .Include(x => x.ProductLinks).ThenInclude(p => p.LinkedProduct).ThenInclude(m => m.ThumbnailImage)
                .Include(x => x.ThumbnailImage)
                .Include(x => x.Medias).ThenInclude(m => m.Media)
                .Include(x => x.ReturnAircraft)
                .Include(x => x.ReturnCarrier)
                .Include(x => x.Brand)
                .Include(x => x.TaxClass)
                .FirstOrDefault(x => x.Id == id && x.IsPublished && x.PassengerPrice > 0);

            if (product == null)
            {
                return null;
            }

            var model = new ProductDetail
            {
                Id = product.Id,
                Name = product.Name,
                CalculatedProductPrice = _productPricingService.CalculateProductPrice(product, HttpContext.User.IsInRole("vendor")),
                IsCallForPricing = product.IsCallForPricing,
                IsAllowToOrder = product.IsAllowToOrder,
                StockQuantity = product.StockQuantity,
                Terminal = product.Terminal,
                ReturnTerminal = product.ReturnTerminal,
                IsRoundTrip = product.IsRoundTrip,
                FlightNumber = product.FlightNumber,
                FlightClass = product.FlightClass,
                ReturnFlightNumber = product.ReturnFlightNumber,
                Carrier = product.Brand == null ? "" : product.Brand.Name,
                ReturnCarrier = product.ReturnCarrier == null ? "" : product.ReturnCarrier.Name,
                Aircraft = product.TaxClass == null ? "" : product.TaxClass.Name,
                Via = product.Via,
                ReturnAircraft = product.ReturnAircraft == null ? "" : product.ReturnAircraft.Name,
                ReturnVia = product.ReturnVia,
                SoldSeats = product.SoldSeats,
                Baggage = product.Baggage,
                Departure = product.Departure,
                Destination = product.Destination,
                Specification = product.Specification,
                ReviewsCount = product.ReviewsCount,
                RatingAverage = product.RatingAverage,
                Attributes = product.AttributeValues.Select(x => new ProductDetailAttribute { Name = x.Attribute.Name, Value = x.Value }).ToList(),
                Categories = product.Categories.Select(x => new ProductDetailCategory { Id = x.CategoryId, Name = x.Category.Name, SeoTitle = x.Category.SeoTitle }).ToList()
            };

            // Special requirement from Koray: show fake available quantity 
            // to increase flight attractiveness and sense of urgency
            if (model.StockQuantity > 10)
            {
                model.StockQuantity = new Random().Next(7, 12);
            }

            MapProductVariantToProductVm(product, model);
            MapRelatedProductToProductVm(product, model);

            foreach (var item in product.OptionValues)
            {
                var optionValues = JsonConvert.DeserializeObject<IList<ProductOptionValueVm>>(item.Value);
                foreach (var value in optionValues)
                {
                    if (!model.OptionDisplayValues.ContainsKey(value.Key))
                    {
                        model.OptionDisplayValues.Add(value.Key, new ProductOptionDisplay
                        {
                            DisplayType = item.DisplayType,
                            Value = (string.IsNullOrEmpty(value.Display) || value.Display.ToLower() == "null") ? value.Key : value.Display
                        });
                    }
                }
            }

            model.Images = product.Medias.Where(x => x.Media.MediaType == Core.Models.MediaType.Image).Select(productMedia => new MediaViewModel
            {
                Url = _mediaService.GetMediaUrl(productMedia.Media),
                ThumbnailUrl = _mediaService.GetThumbnailUrl(productMedia.Media)
            }).ToList();

            return model;
        }

        private void MapProductVariantToProductVm(Product product, ProductDetail model)
        {
            var variations = _productRepository
                .Query()
                .Include(x => x.OptionCombinations).ThenInclude(o => o.Option)
                .Where(x => x.LinkedProductLinks.Any(link => link.ProductId == product.Id && link.LinkType == ProductLinkType.Super))
                .Where(x => x.IsPublished && x.StockQuantity > 0 && x.Status == "ACCEPTED");

            var departureDateMin = DateTime.Now;
            var departureDateMax = DateTime.Now.AddDays(30);
            variations = variations.Where(x =>
                    x.DepartureDate.Value.Date >= departureDateMin &&
                    x.DepartureDate.Value.Date < departureDateMax);

            foreach (var variation in variations)
            {
                var variationVm = new ProductDetailVariation
                {
                    Id = variation.Id,
                    Name = variation.Name,
                    NormalizedName = variation.NormalizedName,
                    IsAllowToOrder = variation.IsAllowToOrder,
                    IsCallForPricing = variation.IsCallForPricing,
                    StockQuantity = variation.StockQuantity,
                    SoldSeats = variation.SoldSeats,
                    DepartureDate = variation.DepartureDate,
                    ReturnDate = variation.ReturnDepartureDate,
                    InfantPrice = variation.OldPrice,
                    FlightClass = variation.FlightClass,
                    CalculatedProductPrice = _productPricingService.CalculateProductPrice(variation, HttpContext.User.IsInRole("vendor"))
                };


                if (variationVm.StockQuantity > 10)
                {
                    variationVm.StockQuantity = new Random().Next(7, 12);
                }

                var optionCombinations = variation.OptionCombinations.OrderBy(x => x.SortIndex);
                foreach (var combination in optionCombinations)
                {
                    variationVm.Options.Add(new ProductDetailVariationOption
                    {
                        OptionId = combination.OptionId,
                        OptionName = combination.Option.Name,
                        Value = combination.Value
                    });
                }

                model.Variations.Add(variationVm);
            }
        }

        private void MapRelatedProductToProductVm(Product product, ProductDetail model)
        {
            var publishedProductLinks = product.ProductLinks.Where(x => x.LinkedProduct.IsPublished && (x.LinkType == ProductLinkType.Related || x.LinkType == ProductLinkType.CrossSell));
            foreach (var productLink in publishedProductLinks)
            {
                var linkedProduct = productLink.LinkedProduct;
                var productThumbnail = ProductThumbnail.FromProduct(linkedProduct, HttpContext.User.IsInRole("vendor"));

                productThumbnail.ThumbnailUrl = _mediaService.GetThumbnailUrl(linkedProduct.ThumbnailImage);
                productThumbnail.CalculatedProductPrice = _productPricingService.CalculateProductPrice(linkedProduct, HttpContext.User.IsInRole("vendor"));

                if (productLink.LinkType == ProductLinkType.Related)
                {
                    model.RelatedProducts.Add(productThumbnail);
                }

                if (productLink.LinkType == ProductLinkType.CrossSell)
                {
                    model.CrossSellProducts.Add(productThumbnail);
                }
            }
        }
    }
}

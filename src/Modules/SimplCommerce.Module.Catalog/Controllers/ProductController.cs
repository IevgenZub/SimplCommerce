﻿using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Module.Catalog.Models;
using SimplCommerce.Module.Catalog.Services;
using SimplCommerce.Module.Core.Services;
using SimplCommerce.Module.Catalog.ViewModels;
using SimplCommerce.Module.Core.Events;
using SimplCommerce.Module.Core.ViewModels;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SimplCommerce.Module.Catalog.Controllers
{
    public class ProductController : Controller
    {
        private readonly IMediaService _mediaService;
        private readonly IRepository<Product> _productRepository;
        private readonly IMediator _mediator;
        private readonly IProductPricingService _productPricingService;

        public ProductController(IRepository<Product> productRepository, IMediaService mediaService, IMediator mediator, IProductPricingService productPricingService)
        {
            _productRepository = productRepository;
            _mediaService = mediaService;
            _mediator = mediator;
            _productPricingService = productPricingService;
        }

        public async Task<IActionResult> ProductDetail(long id)
        {
            var product = _productRepository.Query()
                .Include(x => x.OptionValues)
                .Include(x => x.Categories).ThenInclude(c => c.Category)
                .Include(x => x.AttributeValues).ThenInclude(a => a.Attribute)
                .Include(x => x.ProductLinks).ThenInclude(p => p.LinkedProduct).ThenInclude(m => m.ThumbnailImage)
                .Include(x => x.ThumbnailImage)
                .Include(x => x.Medias).ThenInclude(m => m.Media)
                .FirstOrDefault(x => x.Id == id && x.IsPublished);
            if (product == null)
            {
                return Redirect("~/Error/FindNotFound");
            }

            var model = new ProductDetail
            {
                Id = product.Id,
                Name = product.Name,
                CalculatedProductPrice = _productPricingService.CalculateProductPrice(product, HttpContext.User.IsInRole("vendor")),
                IsCallForPricing = product.IsCallForPricing,
                IsAllowToOrder = product.IsAllowToOrder,
                StockQuantity = product.StockQuantity,
                Departure = product.Departure,
                Destination= product.Destination,
                Specification = product.Specification,
                ReviewsCount = product.ReviewsCount,
                RatingAverage = product.RatingAverage,
                Attributes = product.AttributeValues.Select(x => new ProductDetailAttribute { Name = x.Attribute.Name, Value = x.Value }).ToList(),
                Categories = product.Categories.Select(x => new ProductDetailCategory { Id = x.CategoryId, Name = x.Category.Name, SeoTitle = x.Category.SeoTitle }).ToList()
            };

            MapProductVariantToProductVm(product, model);
            MapRelatedProductToProductVm(product, model);

            foreach(var item in product.OptionValues)
            {
                var optionValues = JsonConvert.DeserializeObject<IList<ProductOptionValueVm>>(item.Value);
                foreach(var value in optionValues)
                {
                    if (!model.OptionDisplayValues.ContainsKey(value.Key))
                    {
                        model.OptionDisplayValues.Add(value.Key, new ProductOptionDisplay { DisplayType = item.DisplayType, Value = value.Display});
                    }
                }
            }

            model.Images = product.Medias.Where(x => x.Media.MediaType == Core.Models.MediaType.Image).Select(productMedia => new MediaViewModel
            {
                Url = _mediaService.GetMediaUrl(productMedia.Media),
                ThumbnailUrl = _mediaService.GetThumbnailUrl(productMedia.Media)
            }).ToList();

            await _mediator.Publish(new EntityViewed {EntityId = product.Id, EntityTypeId = 3});
            _productRepository.SaveChanges();

            return View(model);
        }

        private void MapProductVariantToProductVm(Product product, ProductDetail model)
        {
            var variations = _productRepository
                .Query()
                .Include(x => x.OptionCombinations).ThenInclude(o => o.Option)
                .Where(x => x.LinkedProductLinks.Any(link => link.ProductId == product.Id && link.LinkType == ProductLinkType.Super))
                .Where(x => x.IsPublished)
                .ToList();

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
                    CalculatedProductPrice = _productPricingService.CalculateProductPrice(variation, HttpContext.User.IsInRole("vendor"))
                };

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
            foreach(var productLink in publishedProductLinks)
            {
                var linkedProduct = productLink.LinkedProduct;
                var productThumbnail = ProductThumbnail.FromProduct(linkedProduct, User.IsInRole("vendor"));

                productThumbnail.ThumbnailUrl = _mediaService.GetThumbnailUrl(linkedProduct.ThumbnailImage);
                productThumbnail.CalculatedProductPrice = _productPricingService.CalculateProductPrice(linkedProduct, HttpContext.User.IsInRole("vendor"));

                if(productLink.LinkType == ProductLinkType.Related)
                {
                    model.RelatedProducts.Add(productThumbnail);
                }

                if(productLink.LinkType == ProductLinkType.CrossSell)
                {
                    model.CrossSellProducts.Add(productThumbnail);
                }
            }
        }
    }
}

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Module.Catalog.Models;
using SimplCommerce.Module.Catalog.ViewModels;
using SimplCommerce.Module.Search.ViewModels;
using Microsoft.EntityFrameworkCore;
using SimplCommerce.Module.Core.Services;
using SimplCommerce.Module.Search.Models;
using Microsoft.Extensions.Configuration;
using SimplCommerce.Module.Catalog.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using SimplCommerce.Module.Core.ViewModels;

namespace SimplCommerce.Module.Search.Controllers
{
    public class SearchController : Controller
    {
        private int _pageSize;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Brand> _brandRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMediaService _mediaService;
        private readonly IRepository<Query> _queryRepository;
        private readonly IProductPricingService _productPricingService;

        public SearchController(IRepository<Product> productRepository,
            IRepository<Brand> brandRepository,
            IRepository<Category> categoryRepository,
            IMediaService mediaService,
            IRepository<Query> queryRepository,
            IProductPricingService productPricingService,
            IConfiguration config)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _mediaService = mediaService;
            _queryRepository = queryRepository;
            _productPricingService = productPricingService;
            _pageSize = config.GetValue<int>("Catalog.ProductPageSize");
        }



        [HttpGet("search")]
        public IActionResult Index(SearchOption searchOption)
        {
            var query = _productRepository.Query();

            if (!string.IsNullOrEmpty(searchOption.Reservation))
            {
                query = query.Where(x => 
                    x.ReservationNumber.ToUpper() == searchOption.Reservation.ToUpper() && 
                    x.Status == "ACCEPTED");
            }
            else
            {
                query = query.Where(x =>
                    x.ShortDescription.Contains(searchOption.Departure) &&
                    x.Description.Contains(searchOption.Landing) &&
                    x.Status == "ACCEPTED" &&
                    x.IsVisibleIndividually &&
                    x.DepartureDate >= DateTime.Now);

                
                if (!string.IsNullOrEmpty(searchOption.DepartureDate))
                {
                    var departureDate = Convert.ToDateTime(searchOption.DepartureDate);
                    var departureDateMin = departureDate.AddDays(-7);
                    var departureDateMax = departureDate.AddDays(7);
                    query = query.Where(x =>
                        (x.DepartureDate.Value.Date >= departureDateMin && 
                        x.DepartureDate.Value.Date < departureDateMax) || x.HasOptions);
                }

                if (searchOption.TripType == "round-trip")
                {
                    if (!string.IsNullOrEmpty(searchOption.ReturnDate))
                    {
                        var returnDate = Convert.ToDateTime(searchOption.ReturnDate);
                        var returnDateMin = returnDate.AddDays(-7);
                        var returnDateMax = returnDate.AddDays(7);
                        query = query.Where(x => 
                            x.ReturnDepartureDate.Value.Date >= returnDateMin && 
                            x.ReturnDepartureDate.Value.Date < returnDateMax);
                    }
                }
                else
                {
                    query = query.Where(x => !x.IsRoundTrip);
                }

                if (!string.IsNullOrEmpty(searchOption.NumberOfPeople))
                {
                    var numberOfPeople = Convert.ToInt32(searchOption.NumberOfPeople.Split("-")[0].Trim());
                    var flightClass = searchOption.NumberOfPeople.Split("-")[1].Trim();

                    query = query.Where(x => x.StockQuantity >= numberOfPeople);
                }
            }

            var model = new SearchResult
            {
                CurrentSearchOption = searchOption,
                FilterOption = new FilterOption()
            };


            model.FilterOption.Price.MaxPrice = query.Select(x => x.Price).DefaultIfEmpty(0).Max();
            model.FilterOption.Price.MinPrice = query.Select(x => x.Price).DefaultIfEmpty(0).Min();

            if (searchOption.MinPrice.HasValue)
            {
                query = query.Where(x => x.Price >= searchOption.MinPrice.Value);
            }

            if (searchOption.MaxPrice.HasValue)
            {
                query = query.Where(x => x.Price <= searchOption.MaxPrice.Value);
            }

            AppendFilterOptionsToModel(model, query);
            if (string.Compare(model.CurrentSearchOption.Category, "all", StringComparison.OrdinalIgnoreCase) != 0)
            {
                var categories = searchOption.GetCategories();
                if (categories.Any())
                {
                    var categoryIds = _categoryRepository.Query().Where(x => categories.Contains(x.SeoTitle)).Select(x => x.Id).ToList();
                    query = query.Where(x => x.Categories.Any(c => categoryIds.Contains(c.CategoryId)));
                }
            }

            var brands = searchOption.GetBrands();
            if (brands.Any())
            {
                var brandIs = _brandRepository.Query().Where(x => brands.Contains(x.SeoTitle)).Select(x => x.Id).ToList();
                query = query.Where(x => x.BrandId.HasValue && brandIs.Contains(x.BrandId.Value));
            }

            model.TotalProduct = query.Count();
            var currentPageNum = searchOption.Page <= 0 ? 1 : searchOption.Page;
            var offset = (_pageSize * currentPageNum) - _pageSize;
            while (currentPageNum > 1 && offset >= model.TotalProduct)
            {
                currentPageNum--;
                offset = (_pageSize * currentPageNum) - _pageSize;
            }
            
            SaveSearchQuery(searchOption, model);

            query = query
                .Include(x => x.ThumbnailImage)
                .Include(x => x.ReturnAircraft)
                .Include(x => x.ReturnCarrier)
                .Include(x => x.Brand)
                .Include(x => x.TaxClass)
                .Include(x => x.OptionValues);

            query = AppySort(searchOption, query);

            var products = query
                .Select(x => ProductThumbnail.FromProduct(x, User.IsInRole("vendor")))
                .Skip(offset)
                .Take(_pageSize)
                .ToList();

            foreach (var product in products)
            {
                product.ThumbnailUrl = _mediaService.GetThumbnailUrl(product.ThumbnailImage);
                product.CalculatedProductPrice = _productPricingService.CalculateProductPrice(product);
                product.Details = GetProductDetails(product.Id, searchOption);

                // Special requirement from Koray: show fake available quantity 
                // to increase flight attractiveness and sense of urgency
                if (product.StockQuantity > 10)
                {
                    product.StockQuantity = new Random().Next(7, 12);
                }
            }

            model.Products = products.Where(p => !p.Details.HasVariation || (p.Details.HasVariation && p.Details.Variations.Count > 0)).ToList();
            model.CurrentSearchOption.PageSize = _pageSize;
            model.CurrentSearchOption.Page = currentPageNum;

            return View(model);
        }

        private ProductDetail GetProductDetails(long id, SearchOption searchOption)
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
                .FirstOrDefault(x => x.Id == id && x.IsPublished);

            if (product == null)
            {
                return null;
            }

            var model = new ProductDetail
            {
                Id = product.Id,
                Name = product.Name,
                CalculatedProductPrice = _productPricingService.CalculateProductPrice(product, User.IsInRole("vendor")),
                IsCallForPricing = product.IsCallForPricing,
                IsAllowToOrder = product.IsAllowToOrder,
                StockQuantity = product.StockQuantity,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                Specification = product.Specification,
                ReviewsCount = product.ReviewsCount,
                RatingAverage = product.RatingAverage,
                Terminal = product.Sku,
                ReturnTerminal = product.ReturnTerminal,
                IsRoundTrip = product.IsRoundTrip,
                FlightNumber = product.FlightNumber,
                Carrier = product.Brand == null ? "" : product.Brand.Name,
                ReturnCarrier = product.ReturnCarrier == null ? "" : product.ReturnCarrier.Name,
                Aircraft = product.TaxClass == null ? "" : product.TaxClass.Name,
                Via = product.Via,
                ReturnAircraft = product.ReturnAircraft == null ? "" : product.ReturnAircraft.Name,
                ReturnVia = product.ReturnVia,
                Attributes = product.AttributeValues.Select(x => new ProductDetailAttribute { Name = x.Attribute.Name, Value = x.Value }).ToList(),
                Categories = product.Categories.Select(x => new ProductDetailCategory { Id = x.CategoryId, Name = x.Category.Name, SeoTitle = x.Category.SeoTitle }).ToList()
            };

            MapProductVariantToProductVm(product, model, searchOption);
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

        private static IQueryable<Product> AppySort(SearchOption searchOption, IQueryable<Product> query)
        {
            var sortBy = searchOption.Sort ?? string.Empty;
            switch (sortBy.ToLower())
            {
                case "price-desc":
                    query = query.OrderByDescending(x => x.Price);
                    break;
                default:
                    query = query.OrderBy(x => x.Price);
                    break;
            }

            return query;
        }

        private void MapProductVariantToProductVm(Product product, ProductDetail model, SearchOption searchOption)
        {
            var variations = _productRepository
                .Query()
                .Include(x => x.OptionCombinations).ThenInclude(o => o.Option)
                .Where(x => x.LinkedProductLinks.Any(link => link.ProductId == product.Id && link.LinkType == ProductLinkType.Super))
                .Where(x => x.IsPublished && x.StockQuantity > 0 && x.Status == "ACCEPTED");

            if (!string.IsNullOrEmpty(searchOption.DepartureDate))
            {
                var departureDate = Convert.ToDateTime(searchOption.DepartureDate);
                var departureDateMin = departureDate.AddDays(-3);
                var departureDateMax = departureDate.AddDays(3);
                variations = variations.Where(x =>
                    x.DepartureDate.Value.Date >= departureDateMin &&
                    x.DepartureDate.Value.Date <= departureDateMax);
            }

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
                    InfantPrice = variation.OldPrice,
                    ReturnLandingDate = variation.ReturnLandingDate,
                    FlightClass = variation.FlightClass,
                    CalculatedProductPrice = _productPricingService.CalculateProductPrice(variation, HttpContext.User.IsInRole("vendor"))
                };

                if (variation.StockQuantity > 10)
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
                var productThumbnail = ProductThumbnail.FromProduct(linkedProduct, User.IsInRole("vendor"));

                productThumbnail.ThumbnailUrl = _mediaService.GetThumbnailUrl(linkedProduct.ThumbnailImage);
                productThumbnail.CalculatedProductPrice = _productPricingService.CalculateProductPrice(linkedProduct, User.IsInRole("vendor"));

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


        private static void AppendFilterOptionsToModel(SearchResult model, IQueryable<Product> query)
        {
            model.FilterOption.Categories = query
                .SelectMany(x => x.Categories).Where(x => x.Category.Parent == null)
                .GroupBy(x => new {
                    x.Category.Id,
                    x.Category.Name,
                    x.Category.SeoTitle
                })
                .Select(g => new FilterCategory
                {
                    Id = (int)g.Key.Id,
                    Name = g.Key.Name,
                    SeoTitle = g.Key.SeoTitle,
                    Count = g.Count()
                }).ToList();

            model.FilterOption.Brands = query
               .Where(x => x.BrandId != null)
               .GroupBy(x => x.Brand)
               .Select(g => new FilterBrand
               {
                   Id = (int)g.Key.Id,
                   Name = g.Key.Name,
                   SeoTitle = g.Key.SeoTitle,
                   Count = g.Count()
               }).ToList();
        }

        private void SaveSearchQuery(SearchOption searchOption, SearchResult model)
        {
            var query = new Query
            {
                CreatedOn = DateTimeOffset.Now,
                QueryText = searchOption.Query,
                ResultsCount = model.TotalProduct
            };

            _queryRepository.Add(query);
            _queryRepository.SaveChanges();
        }
    }
}

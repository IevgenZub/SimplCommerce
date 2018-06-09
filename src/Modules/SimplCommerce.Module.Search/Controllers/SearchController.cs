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
using System.Globalization;

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

            var numberOfPeople = 1;
            var departureDate = Convert.ToDateTime(searchOption.DepartureDate);
            var isRoundTrip = searchOption.TripType == "round-trip";

            
            var departure = searchOption.Departure.Contains("*") ? searchOption.Departure.Split(',')[0] : searchOption.Departure;
            var landing = searchOption.Landing.Contains("*") ? searchOption.Landing.Split(',')[0] : searchOption.Landing;

            if (CultureInfo.CurrentCulture.Name.ToLower() == "ru-ru")
            {
                query = query.Where(x =>
                        x.DepartureRus.Contains(departure) &&
                        x.DestinationRus.Contains(landing));
            }
            else
            {
                query = query.Where(x =>
                        x.Departure.Contains(departure) &&
                        x.Destination.Contains(landing));
            }

            query = query.Where(x =>
                    x.Status == "ACCEPTED" &&
                    !x.IsDeleted  &&
                    x.IsPublished &&
                    !x.HasOptions &&
                    x.DepartureDate >= DateTime.Now);
                
            var departureDateMin = departureDate.AddDays(-7);
            var departureDateMax = departureDate.AddDays(7);
            query = query.Where(x =>
                x.DepartureDate.Value.Date >= departureDateMin && 
                x.DepartureDate.Value.Date < departureDateMax);

            query = query.Where(x => x.IsRoundTrip == isRoundTrip);
            
            if (isRoundTrip && !string.IsNullOrEmpty(searchOption.ReturnDate))
            {
                var returnDate = Convert.ToDateTime(searchOption.ReturnDate);
                var returnDateMin = returnDate.AddDays(-7);
                var returnDateMax = returnDate.AddDays(7);
                query = query.Where(x => (x.IsRoundTrip &&  
                        x.ReturnDepartureDate.Value.Date >= returnDateMin && 
                        x.ReturnDepartureDate.Value.Date <= returnDateMax));
            }

            if (!string.IsNullOrEmpty(searchOption.NumberOfPeople))
            {
                numberOfPeople = Convert.ToInt32(searchOption.NumberOfPeople.Split("-")[0].Trim());
                var flightClass = searchOption.NumberOfPeople.Split("-")[1].Trim();

                query = query.Where(x => x.StockQuantity >= numberOfPeople);
            }
            
            var model = new SearchResult
            {
                CurrentSearchOption = searchOption,
                FilterOption = new FilterOption()
            };


            model.FilterOption.Price.MaxPrice = query.Select(x => x.PassengerPrice).DefaultIfEmpty(0).Max();
            model.FilterOption.Price.MinPrice = query.Select(x => x.PassengerPrice).DefaultIfEmpty(0).Min();

            if (searchOption.MinPrice.HasValue)
            {
                query = query.Where(x => x.PassengerPrice >= searchOption.MinPrice.Value);
            }

            if (searchOption.MaxPrice.HasValue)
            {
                query = query.Where(x => x.PassengerPrice <= searchOption.MaxPrice.Value);
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
                .Select(x => ProductThumbnail.FromProduct(x, User.IsInRole("vendor"), isRoundTrip))
                .Skip(offset)
                .Take(_pageSize)
                .ToList();

            foreach (var product in products)
            {
                product.ThumbnailUrl = _mediaService.GetThumbnailUrl(product.ThumbnailImage);
                product.CalculatedProductPrice = _productPricingService.CalculateProductPrice(product);
                product.Details = GetProductDetails(product.Id, searchOption);
            }

            model.Products = products;
            model.CurrentSearchOption.PageSize = _pageSize;
            model.CurrentSearchOption.Page = currentPageNum;

           
            // Two OW as RT logic

            var directOneWays = SearchOneWayFlights(searchOption);

            var tempDirection = searchOption.Departure;
            searchOption.Departure = searchOption.Landing;
            searchOption.Landing = tempDirection;
            

            searchOption.DepartureDate = string.IsNullOrEmpty(searchOption.ReturnDate) ? string.Empty : searchOption.ReturnDate;
            
            var returnOneWays = SearchOneWayFlights(searchOption);

            for (int i = 0; i < directOneWays.Count(); i++)
            {
               
                if (i < returnOneWays.Count())
                {
                    var rt = directOneWays[i];
                    var ow = returnOneWays[i];
                    rt.IsRoundTrip = true;
                    rt.ReturnDepartureDate = ow.DepartureDate;
                    rt.ReturnTerminal = ow.Terminal;
                    rt.ReturnCarrier = ow.Carrier;
                    rt.ReturnAircraft = ow.Aircraft;
                    rt.ReturnDurationHours = ow.DurationHours;
                    rt.ReturnDurationMinutes = ow.DurationMinutes;
                    rt.ReturnFlightNumber = ow.FlightNumber;
                    rt.ReturnIsNextDayLanding= ow.IsNextDayLanding;
                    rt.ReturnLandingTime = ow.LandingTime;
                    rt.CalculatedProductPrice.Price += ow.CalculatedProductPrice.Price;

                    rt.Details.IsRoundTrip = true;
                    rt.Details.ReturnTerminal = ow.Terminal;
                    rt.Details.ReturnCarrier = ow.Carrier;
                    rt.Details.ReturnAircraft = ow.Aircraft;
                    rt.Details.CalculatedProductPrice.Price += ow.CalculatedProductPrice.Price;

                    if (rt.StockQuantity != ow.StockQuantity)
                    {
                        rt.StockQuantity = rt.StockQuantity > ow.StockQuantity ? ow.StockQuantity : rt.StockQuantity;
                    }

                    if (rt.Details.StockQuantity != ow.StockQuantity)
                    {
                        rt.Details.StockQuantity = rt.Details.StockQuantity > ow.StockQuantity ? ow.StockQuantity : rt.Details.StockQuantity;
                    }
                }
            }

            model.MergedProducts = directOneWays;

            return View(model);
        }

        private IList<ProductThumbnail> SearchOneWayFlights(SearchOption searchOption)
        {
            const bool isRoundTrip = false;
            var numberOfPeople = 1;

            var query = _productRepository.Query();
            
            var departure = searchOption.Departure.Contains("*") ? searchOption.Departure.Split(',')[0] : searchOption.Departure;
            var landing = searchOption.Landing.Contains("*") ? searchOption.Landing.Split(',')[0] : searchOption.Landing;

            if (CultureInfo.CurrentCulture.Name.ToLower() == "ru-ru")
            {
                query = query.Where(x =>
                        x.DepartureRus.Contains(departure) &&
                        x.DestinationRus.Contains(landing));
            }
            else
            {
                query = query.Where(x =>
                        x.Departure.Contains(departure) &&
                        x.Destination.Contains(landing));
            }

            query = query.Where(x =>
                    x.Status == "ACCEPTED" &&
                    !x.IsDeleted &&
                    x.IsPublished &&
                    !x.HasOptions &&
                    x.DepartureDate >= DateTime.Now);

            if (!string.IsNullOrEmpty(searchOption.DepartureDate))
            {
                var departureDate = Convert.ToDateTime(searchOption.DepartureDate);
                var departureDateMin = departureDate.AddDays(-7);
                var departureDateMax = departureDate.AddDays(7);
                query = query.Where(x =>
                    x.DepartureDate.Value.Date >= departureDateMin &&
                    x.DepartureDate.Value.Date < departureDateMax);
            }

            query = query.Where(x => x.IsRoundTrip == isRoundTrip);

            if (!string.IsNullOrEmpty(searchOption.NumberOfPeople))
            {
                numberOfPeople = Convert.ToInt32(searchOption.NumberOfPeople.Split("-")[0].Trim());
                var flightClass = searchOption.NumberOfPeople.Split("-")[1].Trim();

                query = query.Where(x => x.StockQuantity >= numberOfPeople);
            }

            query = query
                .Include(x => x.ThumbnailImage)
                .Include(x => x.ReturnAircraft)
                .Include(x => x.ReturnCarrier)
                .Include(x => x.Brand)
                .Include(x => x.TaxClass)
                .Include(x => x.OptionValues);

            query = AppySort(searchOption, query);

            var products = query
                .Select(x => ProductThumbnail.FromProduct(x, User.IsInRole("vendor"), isRoundTrip))
                .ToList();

            foreach (var product in products)
            {
                product.ThumbnailUrl = _mediaService.GetThumbnailUrl(product.ThumbnailImage);
                product.CalculatedProductPrice = _productPricingService.CalculateProductPrice(product);
                product.Details = GetProductDetails(product.Id, searchOption);
            }

            return products;
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
                Departure = product.Departure,
                Destination = product.Destination,
                Specification = product.Specification,
                ReviewsCount = product.ReviewsCount,
                RatingAverage = product.RatingAverage,
                Terminal = product.Terminal,
                ReturnTerminal = product.ReturnTerminal,
                IsRoundTrip = product.IsRoundTrip,
                Baggage = product.Baggage,
                FlightNumber = product.FlightNumber,
                FlightClass = product.FlightClass,
                Carrier = product.Brand == null ? "" : product.Brand.Name,
                ReturnCarrier = product.ReturnCarrier == null ? "" : product.ReturnCarrier.Name,
                Aircraft = product.TaxClass == null ? "" : product.TaxClass.Name,
                Via = product.Via,
                ReturnAircraft = product.ReturnAircraft == null ? "" : product.ReturnAircraft.Name,
                ReturnVia = product.ReturnVia,
                Attributes = product.AttributeValues.Select(x => new ProductDetailAttribute { Name = x.Attribute.Name, Value = x.Value }).ToList(),
                Categories = product.Categories.Select(x => new ProductDetailCategory { Id = x.CategoryId, Name = x.Category.Name, SeoTitle = x.Category.SeoTitle }).ToList()
            };

            // Special requirement from Koray: show fake available quantity 
            // to increase flight attractiveness and sense of urgency
            if (model.StockQuantity > 10)
            {
                model.StockQuantity = new Random().Next(7, 12);
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
                    query = query.OrderByDescending(x => x.PassengerPrice);
                    break;
                default:
                    query = query.OrderBy(x => x.PassengerPrice);
                    break;
            }

            return query;
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

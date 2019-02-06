using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using SimplCommerce.Infrastructure;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Infrastructure.Web.SmartTable;
using SimplCommerce.Module.Catalog.Models;
using SimplCommerce.Module.Catalog.Services;
using SimplCommerce.Module.Catalog.ViewModels;
using SimplCommerce.Module.Core.Models;
using SimplCommerce.Module.Core.Services;
using SimplCommerce.Module.Core.Extensions;
using System.Globalization;

namespace SimplCommerce.Module.Catalog.Controllers
{
    [Authorize(Roles = "admin, vendor")]
    [Route("api/products")]
    public class ProductApiController : Controller
    {
        private readonly IMediaService _mediaService;
        private readonly IRepository<ProductAttributeValue> _productAttributeValueRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductLink> _productLinkRepository;
        private readonly IRepository<ProductOptionValue> _productOptionValueRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;

        public ProductApiController(
            IRepository<Product> productRepository,
            IMediaService mediaService,
            IProductService productService,
            IRepository<ProductLink> productLinkRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductOptionValue> productOptionValueRepository,
            IRepository<ProductAttributeValue> productAttributeValueRepository,
            IWorkContext workContext)
        {
            _productRepository = productRepository;
            _mediaService = mediaService;
            _productService = productService;
            _productLinkRepository = productLinkRepository;
            _productCategoryRepository = productCategoryRepository;
            _productOptionValueRepository = productOptionValueRepository;
            _productAttributeValueRepository = productAttributeValueRepository;
            _workContext = workContext;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var product = _productRepository.Query()
                .Include(x => x.ThumbnailImage)
                .Include(x => x.Medias).ThenInclude(m => m.Media)
                .Include(x => x.ProductLinks).ThenInclude(p => p.LinkedProduct)
                .Include(x => x.OptionValues).ThenInclude(o => o.Option)
                .Include(x => x.AttributeValues).ThenInclude(a => a.Attribute).ThenInclude(g => g.Group)
                .Include(x => x.Categories)
                .FirstOrDefault(x => x.Id == id);

            var currentUser = await _workContext.GetCurrentUser();
            if (!User.IsInRole("admin") && product.VendorId != currentUser.VendorId)
            {
                return new BadRequestObjectResult(new { error = "You don't have permission to manage this product" });
            }

            var productVm = new ProductVm
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.SeoTitle,
                Departure = product.Departure,
                Destination = product.Destination,
                DepartureRus = product.DepartureRus,
                DestinationRus = product.DestinationRus,
                Specification = product.Specification,
                OldPrice = product.OldPrice,
                Price = product.Price,
                SpecialPrice = product.SpecialPrice,
                SpecialPriceStart = product.SpecialPriceStart,
                SpecialPriceEnd = product.SpecialPriceEnd,
                IsFeatured = product.IsFeatured,
                IsPublished = product.IsPublished,
                IsCallForPricing = product.IsCallForPricing,
                IsAllowToOrder = product.IsAllowToOrder,
                CategoryIds = product.Categories.Select(x => x.CategoryId).ToList(),
                ThumbnailImageUrl = _mediaService.GetThumbnailUrl(product.ThumbnailImage),
                BrandId = product.BrandId,
                TaxClassId = product.TaxClassId,
                Baggage = product.Baggage,
                Seats = product.StockQuantity,
                TerminalInfo = product.Terminal,
                Via = product.Via,
                Currency = product.Currency,
                ReturnFlightNumber = product.ReturnFlightNumber,
                ReturnCarrierId = product.ReturnCarrierId,
                ReturnDepartureDate = product.ReturnDepartureDate,
                IsRoundTrip = product.IsRoundTrip,
                ReturnAircraftId = product.ReturnAircraftId,
                ReturnTerminal = product.ReturnTerminal,
                ReturnVia = product.ReturnVia,
                FlightNumber = product.FlightNumber,
                SoldSeats = product.SoldSeats,
                SaleRtOnly = product.SaleRtOnly,
                Status = product.Status,
                ReservationNumber = product.ReservationNumber,
                VendorId = product.VendorId,
                FlightClass = product.FlightClass,
                DepartureDate = product.DepartureDate,
                DurationHours = product.DurationHours,
                DurationMinutes = product.DurationMinutes,
                ReturnDurationHours = product.ReturnDurationHours,
                ReturnDurationMinutes = product.ReturnDurationMinutes,
                ChildPrice = product.ChildPrice,
                LandingTime = product.LandingTime,
                IsNextDayLanding = product.IsNextDayLanding,
                ReturnLandingTime = product.ReturnLandingTime,
                ReturnIsNextDayLanding = product.ReturnIsNextDayLanding
            };

            if (User.IsInRole("admin"))
            {
                productVm.PassengerChildPrice = product.PassengerChildPrice;
                productVm.PassengerPrice = product.PassengerPrice;
                productVm.AgencyChildPrice = product.AgencyChildPrice;
                productVm.AgencyPrice = product.AgencyPrice;

                productVm.AdminRoundTrip = product.AdminRoundTrip;
                productVm.AdminRoundTripOperatorId = product.AdminRoundTripOperatorId;
                productVm.AdminPayLater = product.AdminPayLater;
                productVm.AdminPayLaterRule = product.AdminPayLaterRule;
                productVm.AdminBlackList = product.AdminBlackList;
                productVm.AdminPasExpirityRule = product.AdminPasExpirityRule;
                productVm.AdminIsSpecialOffer = product.AdminIsSpecialOffer;
                productVm.AdminNotifyAgencies = product.AdminNotifyAgencies;
                productVm.AdminNotifyLastPassanger = product.AdminNotifyLastPassanger;
                productVm.AdminIsLastMinute = product.AdminIsLastMinute;
                productVm.AdminReturnPayLater = product.AdminReturnPayLater;
                productVm.AdminReturnPayLaterRule = product.AdminReturnPayLaterRule;
                productVm.AdminReturnBlackList = product.AdminReturnBlackList;
                productVm.AdminReturnPasExpirityRule = product.AdminReturnPasExpirityRule;
                productVm.AdminReturnIsSpecialOffer = product.AdminReturnIsSpecialOffer;
                productVm.AdminReturnNotifyAgencies = product.AdminReturnNotifyAgencies;
                productVm.AdminReturnNotifyLastPassanger = product.AdminReturnNotifyLastPassanger;
                productVm.AdminReturnIsLastMinute = product.AdminReturnIsLastMinute;
            }

            foreach (var productMedia in product.Medias.Where(x => x.Media.MediaType == MediaType.Image))
            {
                productVm.ProductImages.Add(new ProductMediaVm
                {
                    Id = productMedia.Id,
                    MediaUrl = _mediaService.GetThumbnailUrl(productMedia.Media)
                });
            }

            foreach (var productMedia in product.Medias.Where(x => x.Media.MediaType == MediaType.File))
            {
                productVm.ProductDocuments.Add(new ProductMediaVm
                {
                    Id = productMedia.Id,
                    Caption = productMedia.Media.Caption,
                    MediaUrl = _mediaService.GetMediaUrl(productMedia.Media)
                });
            }


            productVm.Options = product.OptionValues.OrderBy(x => x.SortIndex).Select(x =>
                new ProductOptionVm
                {
                    Id = x.OptionId,
                    Name = x.Option.Name,
                    DisplayType = x.DisplayType,
                    Values = JsonConvert.DeserializeObject<IList<ProductOptionValueVm>>(x.Value)
                }).ToList();

            foreach (var variation in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Super).Select(x => x.LinkedProduct).Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                productVm.Variations.Add(new ProductVariationVm
                {
                    Id = variation.Id,
                    Name = variation.Name,
                    Price = variation.Price,
                    OldPrice = variation.OldPrice,
                    NormalizedName = variation.NormalizedName,
                    OptionCombinations = variation.OptionCombinations.Select(x => new ProductOptionCombinationVm
                    {
                        OptionId = x.OptionId,
                        OptionName = x.Option.Name,
                        Value = x.Value,
                        SortIndex = x.SortIndex
                    }).OrderBy(x => x.SortIndex).ToList()
                });
            }

            foreach (var relatedProduct in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Related).Select(x => x.LinkedProduct).Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                productVm.RelatedProducts.Add(new ProductLinkVm
                {
                    Id = relatedProduct.Id,
                    Name = relatedProduct.Name,
                    IsPublished = relatedProduct.IsPublished
                });
            }

            foreach (var crossSellProduct in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.CrossSell).Select(x => x.LinkedProduct).Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                productVm.CrossSellProducts.Add(new ProductLinkVm
                {
                    Id = crossSellProduct.Id,
                    Name = crossSellProduct.Name,
                    IsPublished = crossSellProduct.IsPublished
                });
            }

            productVm.Attributes = product.AttributeValues.Select(x => new ProductAttributeVm
            {
                AttributeValueId = x.Id,
                Id = x.AttributeId,
                Name = x.Attribute.Name,
                GroupName = x.Attribute.Group.Name,
                Value = x.Value
            }).ToList();

            return Json(productVm);
        }

        [HttpPost("grid")]
        public async Task<IActionResult> List([FromBody] SmartTableParam param)
        {
            var query = _productRepository.Query().Include(x => x.Vendor).Where(x => !x.IsDeleted);
            var currentUser = await _workContext.GetCurrentUser();
            if (!User.IsInRole("admin"))
            {
                query = query.Where(x => x.VendorId == currentUser.VendorId);
            }

            if (param.Search.PredicateObject != null)
            {
                dynamic search = param.Search.PredicateObject;
                if (search.FlightNumber != null)
                {
                    string name = search.FlightNumber;
                    query = query.Where(x => x.Name.Contains(name));
                }

                if (search.FlightClass != null)
                {
                    string flightClass = search.FlightClass;
                    query = query.Where(x => x.FlightClass.Contains(flightClass));
                }

                if (search.Operator != null)
                {
                    string oper = search.Operator;
                    query = query.Where(x => x.Vendor != null && x.Vendor.Name.Contains(oper));
                }

                if (search.From != null)
                {
                    string from = search.From;
                    query = query.Where(x => x.Departure.Contains(from));
                }

                if (search.To != null)
                {
                    string to = search.To;
                    query = query.Where(x => x.Destination.Contains(to));
                }

                if (search.HasOptions != null)
                {
                    bool hasOptions = search.HasOptions;
                    query = query.Where(x => x.HasOptions == hasOptions);
                }

                if (search.IsRoundTrip != null)
                {
                    bool isRoundTrip = search.IsRoundTrip;
                    query = query.Where(x => x.IsRoundTrip == isRoundTrip);
                }

                if (search.IsVisibleIndividually != null)
                {
                    bool isVisibleIndividually = search.IsVisibleIndividually;
                    query = query.Where(x => x.IsVisibleIndividually == isVisibleIndividually);
                }

                if (search.Status != null)
                {
                    string status = search.Status;
                    query = query.Where(x => x.Status.Contains(status));
                }

                if (search.CreatedOn != null)
                {
                    if (search.CreatedOn.before != null)
                    {
                        DateTimeOffset before = search.CreatedOn.before;
                        query = query.Where(x => x.CreatedOn <= before);
                    }

                    if (search.CreatedOn.after != null)
                    {
                        DateTimeOffset after = search.CreatedOn.after;
                        query = query.Where(x => x.CreatedOn >= after);
                    }
                }
            }

            var gridData = query.ToSmartTableResult(
                param,
                x => new ProductListItem
                {
                    Id = x.Id,
                    FlightNumber = x.FlightNumber,
                    IsRoundTrip = x.IsRoundTrip,
                    Name = x.Name,
                    HasOptions = x.HasOptions,
                    Seats = x.SoldSeats + "/" + x.StockQuantity,
                    CreatedOn = x.CreatedOn,
                    IsPublished = x.IsPublished,
                    From = x.Departure.Split('(', ')')[1],
                    To = x.Destination.Split('(', ')')[1],
                    DepartureDate = x.DepartureDate,
                    ReturnDepartureDate = x.ReturnDepartureDate,
                    Status = x.Status,
                    Operator = x.Vendor == null ? string.Empty : x.Vendor.Name,
                    FlightClass = x.FlightClass,
                    Price = x.Price.ToString("0.0") + "$"
                });

            return Json(gridData);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ProductForm model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            var currentUser = await _workContext.GetCurrentUser();

            var product = new Product
            {
                Name = model.Product.Name,
                SeoTitle = model.Product.Slug,
                Departure = model.Product.Departure,
                Destination = model.Product.Destination,
                DepartureRus = model.Product.DepartureRus,
                DestinationRus = model.Product.DestinationRus,
                Specification = model.Product.Specification,
                Price = model.Product.Price,
                OldPrice = model.Product.OldPrice,
                SpecialPrice = model.Product.SpecialPrice,
                SpecialPriceStart = model.Product.SpecialPriceStart,
                SpecialPriceEnd = model.Product.SpecialPriceEnd,
                IsPublished = model.Product.IsPublished,
                IsFeatured = model.Product.IsFeatured,
                IsCallForPricing = model.Product.IsCallForPricing,
                IsAllowToOrder = model.Product.IsAllowToOrder,
                BrandId = model.Product.BrandId,
                TaxClassId = model.Product.TaxClassId,
                HasOptions = model.Product.Variations.Any() ? true : false,
                IsVisibleIndividually = true,
                CreatedBy = currentUser,
                Baggage = model.Product.Baggage,
                StockQuantity = model.Product.Seats,
                Terminal = model.Product.TerminalInfo,
                Via = model.Product.Via,
                Currency = model.Product.Currency,
                ReturnFlightNumber = model.Product.ReturnFlightNumber,
                ReturnCarrierId = model.Product.ReturnCarrierId,
                ReturnDepartureDate = model.Product.ReturnDepartureDate,
                IsRoundTrip = model.Product.IsRoundTrip,
                ReturnAircraftId = model.Product.ReturnAircraftId,
                ReturnTerminal = model.Product.ReturnTerminal,
                ReturnVia = model.Product.ReturnVia,
                FlightNumber = model.Product.FlightNumber,
                SoldSeats = 0,
                SaleRtOnly = model.Product.SaleRtOnly,
                Status = "INSERTED",
                ReservationNumber = model.Product.ReservationNumber,
                FlightClass = model.Product.FlightClass,
                DepartureDate = DateTime.SpecifyKind(model.Product.DepartureDate.Value.DateTime, DateTimeKind.Utc),
                ChildPrice = model.Product.ChildPrice,
                DurationHours = model.Product.DurationHours,
                DurationMinutes = model.Product.DurationMinutes,
                ReturnDurationHours = model.Product.ReturnDurationHours,
                ReturnDurationMinutes = model.Product.ReturnDurationMinutes,
                LandingTime = model.Product.LandingTime.HasValue ? DateTime.SpecifyKind(model.Product.LandingTime.Value.DateTime, DateTimeKind.Utc) : default(DateTimeOffset?),
                IsNextDayLanding = model.Product.IsNextDayLanding,
                ReturnLandingTime = model.Product.ReturnLandingTime.HasValue ? DateTime.SpecifyKind(model.Product.ReturnLandingTime.Value.DateTime, DateTimeKind.Utc) : default(DateTimeOffset?),
                ReturnIsNextDayLanding = model.Product.ReturnIsNextDayLanding
            };

            if (!User.IsInRole("admin"))
            {
                product.VendorId = currentUser.VendorId;
            }
            else
            {
                product.PassengerPrice = model.Product.PassengerPrice;
                product.PassengerChildPrice = model.Product.PassengerChildPrice;
                product.AgencyPrice = model.Product.AgencyPrice;
                product.AgencyChildPrice = model.Product.AgencyChildPrice;
                product.VendorId = model.Product.VendorId;
                product.AdminRoundTrip = model.Product.AdminRoundTrip;
                product.AdminRoundTripOperatorId = model.Product.AdminRoundTripOperatorId;
                product.AdminPayLater = model.Product.AdminPayLater;
                product.AdminPayLaterRule = model.Product.AdminPayLaterRule;
                product.AdminBlackList = model.Product.AdminBlackList;
                product.AdminPasExpirityRule = model.Product.AdminPasExpirityRule;
                product.AdminIsSpecialOffer = model.Product.AdminIsSpecialOffer;
                product.AdminNotifyAgencies = model.Product.AdminNotifyAgencies;
                product.AdminNotifyLastPassanger = model.Product.AdminNotifyLastPassanger;
                product.AdminIsLastMinute = model.Product.AdminIsLastMinute;
                product.AdminReturnPayLater = model.Product.AdminReturnPayLater;
                product.AdminReturnPayLaterRule = model.Product.AdminReturnPayLaterRule;
                product.AdminReturnBlackList = model.Product.AdminReturnBlackList;
                product.AdminReturnPasExpirityRule = model.Product.AdminReturnPasExpirityRule;
                product.AdminReturnIsSpecialOffer = model.Product.AdminReturnIsSpecialOffer;
                product.AdminReturnNotifyAgencies = model.Product.AdminReturnNotifyAgencies;
                product.AdminReturnNotifyLastPassanger = model.Product.AdminReturnNotifyLastPassanger;
                product.AdminReturnIsLastMinute = model.Product.AdminReturnIsLastMinute;
            }

            var optionIndex = 0;
            foreach (var option in model.Product.Options)
            {
                product.AddOptionValue(new ProductOptionValue
                {
                    OptionId = option.Id,
                    DisplayType = option.DisplayType,
                    Value = JsonConvert.SerializeObject(option.Values),
                    SortIndex = optionIndex
                });

                optionIndex++;
            }

            foreach (var attribute in model.Product.Attributes)
            {
                var attributeValue = new ProductAttributeValue
                {
                    AttributeId = attribute.Id,
                    Value = attribute.Value
                };

                product.AddAttributeValue(attributeValue);
            }

            foreach (var categoryId in model.Product.CategoryIds)
            {
                var productCategory = new ProductCategory
                {
                    CategoryId = categoryId
                };
                product.AddCategory(productCategory);
            }

            await SaveProductMedias(model, product);

            MapProductVariationVmToProduct(model, product);
            MapProductLinkVmToProduct(model, product);

            _productService.Create(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, ProductForm model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = _productRepository.Query()
                .Include(x => x.ThumbnailImage)
                .Include(x => x.Medias).ThenInclude(m => m.Media)
                .Include(x => x.ProductLinks).ThenInclude(x => x.LinkedProduct)
                .Include(x => x.OptionValues).ThenInclude(o => o.Option)
                .Include(x => x.AttributeValues).ThenInclude(a => a.Attribute).ThenInclude(g => g.Group)
                .Include(x => x.Categories)
                .FirstOrDefault(x => x.Id == id);

            if(product == null)
            {
                return NotFound();
            }

            var currentUser = await _workContext.GetCurrentUser();
            if (!User.IsInRole("admin") && product.VendorId != currentUser.VendorId)
            {
                return new BadRequestObjectResult(new { error = "You don't have permission to manage this product" });
            }

            product.Name = model.Product.Name;
            product.SeoTitle = model.Product.Slug;
            product.Departure = model.Product.Departure;
            product.Destination = model.Product.Destination;
            product.DepartureRus = model.Product.DepartureRus;
            product.DestinationRus = model.Product.DestinationRus;
            product.Specification = model.Product.Specification;
            product.Price = model.Product.Price;
            product.OldPrice = model.Product.OldPrice;
            product.SpecialPrice = model.Product.SpecialPrice;
            product.SpecialPriceStart = model.Product.SpecialPriceStart;
            product.SpecialPriceEnd = model.Product.SpecialPriceEnd;
            product.BrandId = model.Product.BrandId;
            product.TaxClassId = model.Product.TaxClassId;
            product.IsFeatured = model.Product.IsFeatured;
            product.IsPublished = model.Product.IsPublished;
            product.IsCallForPricing = model.Product.IsCallForPricing;
            product.IsAllowToOrder = model.Product.IsAllowToOrder;
            product.UpdatedBy = currentUser;
            product.Baggage = model.Product.Baggage;
            product.StockQuantity = model.Product.Seats;
            product.Terminal = model.Product.TerminalInfo;
            product.Via = model.Product.Via;
            product.Currency = model.Product.Currency;
            product.ReturnFlightNumber = model.Product.ReturnFlightNumber;
            product.ReturnCarrierId = model.Product.ReturnCarrierId;
            product.ReturnDepartureDate = model.Product.ReturnDepartureDate;
            product.IsRoundTrip = model.Product.IsRoundTrip;
            product.ReturnAircraftId = model.Product.ReturnAircraftId;
            product.ReturnTerminal = model.Product.ReturnTerminal;
            product.ReturnVia = model.Product.ReturnVia;
            product.FlightNumber = model.Product.FlightNumber;
            product.SaleRtOnly = model.Product.SaleRtOnly;
            product.SoldSeats = model.Product.SoldSeats;
            product.ReservationNumber = model.Product.ReservationNumber;
            product.VendorId = model.Product.VendorId;
            product.FlightClass = model.Product.FlightClass;
            product.DepartureDate = DateTime.SpecifyKind(model.Product.DepartureDate.Value.DateTime, DateTimeKind.Utc);
            product.ChildPrice = model.Product.ChildPrice;
            product.DurationHours = model.Product.DurationHours;
            product.DurationMinutes = model.Product.DurationMinutes;
            product.ReturnDurationHours = model.Product.ReturnDurationHours;
            product.ReturnDurationMinutes = model.Product.ReturnDurationMinutes;
            product.LandingTime = model.Product.LandingTime.HasValue ? DateTime.SpecifyKind(model.Product.LandingTime.Value.DateTime, DateTimeKind.Utc) : default(DateTimeOffset?);
            product.IsNextDayLanding = model.Product.IsNextDayLanding;
            product.ReturnLandingTime = model.Product.ReturnLandingTime.HasValue ? DateTime.SpecifyKind(model.Product.ReturnLandingTime.Value.DateTime, DateTimeKind.Utc) : default(DateTimeOffset?);
            product.ReturnIsNextDayLanding = model.Product.ReturnIsNextDayLanding;

            if (User.IsInRole("admin"))
            {
                product.AgencyChildPrice = model.Product.AgencyChildPrice;
                product.AgencyPrice = model.Product.AgencyPrice;
                product.PassengerChildPrice = model.Product.PassengerChildPrice;
                product.PassengerPrice = model.Product.PassengerPrice;
                product.VendorId = model.Product.VendorId;
                product.Status = model.Product.Status;
                product.AdminRoundTrip = model.Product.AdminRoundTrip;
                product.AdminRoundTripOperatorId = model.Product.AdminRoundTripOperatorId;
                product.AdminPayLater = model.Product.AdminPayLater;
                product.AdminPayLaterRule = model.Product.AdminPayLaterRule;
                product.AdminBlackList = model.Product.AdminBlackList;
                product.AdminPasExpirityRule = model.Product.AdminPasExpirityRule;
                product.AdminIsSpecialOffer = model.Product.AdminIsSpecialOffer;
                product.AdminNotifyAgencies = model.Product.AdminNotifyAgencies;
                product.AdminNotifyLastPassanger = model.Product.AdminNotifyLastPassanger;
                product.AdminIsLastMinute = model.Product.AdminIsLastMinute;
                product.AdminReturnPayLater = model.Product.AdminReturnPayLater;
                product.AdminReturnPayLaterRule = model.Product.AdminReturnPayLaterRule;
                product.AdminReturnBlackList = model.Product.AdminReturnBlackList;
                product.AdminReturnPasExpirityRule = model.Product.AdminReturnPasExpirityRule;
                product.AdminReturnIsSpecialOffer = model.Product.AdminReturnIsSpecialOffer;
                product.AdminReturnNotifyAgencies = model.Product.AdminReturnNotifyAgencies;
                product.AdminReturnNotifyLastPassanger = model.Product.AdminReturnNotifyLastPassanger;
                product.AdminReturnIsLastMinute = model.Product.AdminReturnIsLastMinute;
            }

            await SaveProductMedias(model, product);

            foreach (var productMediaId in model.Product.DeletedMediaIds)
            {
                var productMedia = product.Medias.First(x => x.Id == productMediaId);
                await _mediaService.DeleteMediaAsync(productMedia.Media);
                product.RemoveMedia(productMedia);
            }

            AddOrDeleteProductOption(model, product);
            AddOrDeleteProductAttribute(model, product);
            AddOrDeleteCategories(model, product);
            AddOrDeleteProductVariation(model, product);
            AddOrDeleteProductLinks(model, product);

            _productService.Update(product);

            return Accepted();
        }

        [HttpPost("change-status/{id}")]
        public async Task<IActionResult> ChangeStatus(long id)
        {
            var product = _productRepository.Query().Include(p => p.Vendor).FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var currentUser = await _workContext.GetCurrentUser();
            if (!User.IsInRole("admin") && product.VendorId != currentUser.VendorId)
            {
                return BadRequest(new { error = "You don't have permission to manage this product" });
            }
            
            product.Status = product.Status == "ACCEPTED" ? "PAUSE" : "ACCEPTED";

            if (product.Status == "ACCEPTED" && string.IsNullOrEmpty(product.ReservationNumber))
            {
                product.ReservationNumber = product.Id.ToReservation(product.Vendor.Name);
            }

            await _productRepository.SaveChangesAsync();

            return Accepted();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var product = _productRepository.Query().FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var currentUser = await _workContext.GetCurrentUser();
            if (!User.IsInRole("admin") && product.VendorId != currentUser.VendorId)
            {
                return new BadRequestObjectResult(new { error = "You don't have permission to manage this product" });
            }

            await _productService.Delete(product);

            return NoContent();
        }

        private static void MapProductVariationVmToProduct(ProductForm model, Product product)
        {
            foreach (var variationVm in model.Product.Variations)
            {
                var productLink = new ProductLink
                {
                    LinkType = ProductLinkType.Super,
                    Product = product,
                    LinkedProduct = product.Clone()
                };

                productLink.LinkedProduct.Name = variationVm.Name;
                productLink.LinkedProduct.SeoTitle = variationVm.Name.ToUrlFriendly();
                productLink.LinkedProduct.Price = variationVm.Price;
                productLink.LinkedProduct.OldPrice = variationVm.OldPrice;
                productLink.LinkedProduct.NormalizedName = variationVm.NormalizedName;
                productLink.LinkedProduct.HasOptions = false;
                productLink.LinkedProduct.IsVisibleIndividually = false;

                GenerateValues(productLink.LinkedProduct, product, variationVm);
                
                foreach (var combinationVm in variationVm.OptionCombinations)
                {
                    productLink.LinkedProduct.AddOptionCombination(new ProductOptionCombination
                    {
                        OptionId = combinationVm.OptionId,
                        Value = combinationVm.Value,
                        SortIndex = combinationVm.SortIndex
                    });
                }

                productLink.LinkedProduct.ThumbnailImage = product.ThumbnailImage;

                product.AddProductLinks(productLink);
            }
        }

        private static void GenerateValues(Product linkedProduct, Product product, ProductVariationVm variation)
        {
            var departureDateOption = variation.OptionCombinations.FirstOrDefault(o => o.OptionName == "Departure Date");
            if (departureDateOption != null)
            {
                DateTimeOffset departureDate = DateTime.SpecifyKind(Convert.ToDateTime(departureDateOption.Value, new CultureInfo("ru-RU")), DateTimeKind.Utc);

                linkedProduct.DepartureDate = departureDate
                        .AddHours(product.DepartureDate.Value.Hour)
                        .AddMinutes(product.DepartureDate.Value.Minute);

                var daysOption = variation.OptionCombinations.FirstOrDefault(o => o.OptionName == "Package Days");
                if (daysOption != null)
                {
                    var days = Convert.ToInt32(daysOption.Value);
                    var returnDepartureTime = linkedProduct.ReturnDepartureDate.Value.TimeOfDay;
                    linkedProduct.ReturnDepartureDate = DateTime.SpecifyKind(linkedProduct.DepartureDate.Value.Date
                        .AddDays(days)
                        .Add(returnDepartureTime), DateTimeKind.Utc);
                }
            }
        
            var flightClass = variation.OptionCombinations.FirstOrDefault(o => o.OptionName == "Class");
            if (flightClass != null)
            {
                linkedProduct.FlightClass = flightClass.Value;
            }
        }

        private static void MapProductLinkVmToProduct(ProductForm model, Product product)
        {
            foreach (var relatedProductVm in model.Product.RelatedProducts)
            {
                var productLink = new ProductLink
                {
                    LinkType = ProductLinkType.Related,
                    Product = product,
                    LinkedProductId = relatedProductVm.Id
                };

                product.AddProductLinks(productLink);
            }

            foreach (var crossSellProductVm in model.Product.CrossSellProducts)
            {
                var productLink = new ProductLink
                {
                    LinkType = ProductLinkType.CrossSell,
                    Product = product,
                    LinkedProductId = crossSellProductVm.Id
                };

                product.AddProductLinks(productLink);
            }
        }

        private void AddOrDeleteCategories(ProductForm model, Product product)
        {
            foreach (var categoryId in model.Product.CategoryIds)
            {
                if (product.Categories.Any(x => x.CategoryId == categoryId))
                {
                    continue;
                }

                var productCategory = new ProductCategory
                {
                    CategoryId = categoryId
                };
                product.AddCategory(productCategory);
            }

            var deletedProductCategories =
                product.Categories.Where(productCategory => !model.Product.CategoryIds.Contains(productCategory.CategoryId))
                    .ToList();

            foreach (var deletedProductCategory in deletedProductCategories)
            {
                deletedProductCategory.Product = null;
                product.Categories.Remove(deletedProductCategory);
                _productCategoryRepository.Remove(deletedProductCategory);
            }
        }

        private void AddOrDeleteProductOption(ProductForm model, Product product)
        {
            var optionIndex = 0;
            foreach (var optionVm in model.Product.Options)
            {
                var optionValue = product.OptionValues.FirstOrDefault(x => x.OptionId == optionVm.Id);
                if (optionValue == null)
                {
                    product.AddOptionValue(new ProductOptionValue
                    {
                        OptionId = optionVm.Id,
                        DisplayType = optionVm.DisplayType,
                        Value = JsonConvert.SerializeObject(optionVm.Values),
                        SortIndex = optionIndex
                    });
                }
                else
                {
                    optionValue.Value = JsonConvert.SerializeObject(optionVm.Values);
                    optionValue.DisplayType = optionVm.DisplayType;
                    optionValue.SortIndex = optionIndex;
                }

                optionIndex++;
            }

            var deletedProductOptionValues = product.OptionValues.Where(x => model.Product.Options.All(vm => vm.Id != x.OptionId)).ToList();

            foreach (var productOptionValue in deletedProductOptionValues)
            {
                product.OptionValues.Remove(productOptionValue);
                _productOptionValueRepository.Remove(productOptionValue);
            }
        }

        private void AddOrDeleteProductVariation(ProductForm model, Product product)
        {
            foreach (var productVariationVm in model.Product.Variations)
            {
                var productLink = product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Super).FirstOrDefault(x => x.LinkedProduct.Name == productVariationVm.Name);
                if (productLink == null)
                {
                    productLink = new ProductLink
                    {
                        LinkType = ProductLinkType.Super,
                        Product = product,
                        LinkedProduct = product.Clone()
                    };

                    productLink.LinkedProduct.Name = productVariationVm.Name;
                    productLink.LinkedProduct.SeoTitle = StringHelper.ToUrlFriendly(productVariationVm.Name);
                    productLink.LinkedProduct.Price = productVariationVm.Price;
                    productLink.LinkedProduct.OldPrice = productVariationVm.OldPrice;
                    productLink.LinkedProduct.NormalizedName = productVariationVm.NormalizedName;
                    productLink.LinkedProduct.HasOptions = false;
                    productLink.LinkedProduct.IsVisibleIndividually = false;
                    productLink.LinkedProduct.ThumbnailImage = product.ThumbnailImage;
                   
                    foreach (var combinationVm in productVariationVm.OptionCombinations)
                    {
                        productLink.LinkedProduct.AddOptionCombination(new ProductOptionCombination
                        {
                            OptionId = combinationVm.OptionId,
                            Value = combinationVm.Value,
                            SortIndex = combinationVm.SortIndex
                        });
                    }

                    product.AddProductLinks(productLink);
                }
                else
                {
                    productLink.LinkedProduct.Price = productVariationVm.Price;
                    productLink.LinkedProduct.OldPrice = productVariationVm.OldPrice;
                    productLink.LinkedProduct.IsDeleted = false;
                }

                GenerateValues(productLink.LinkedProduct, product, productVariationVm);
            }

            foreach (var productLink in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Super))
            {
                if (model.Product.Variations.All(x => x.Name != productLink.LinkedProduct.Name))
                {
                    _productLinkRepository.Remove(productLink);
                    productLink.LinkedProduct.IsDeleted = true;
                }
            }
        }

        // Due to some issue with EF Core, we have to use _productLinkRepository in this case.
        private void AddOrDeleteProductLinks(ProductForm model, Product product)
        {
            foreach (var relatedProductVm in model.Product.RelatedProducts)
            {
                var productLink = product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Related).FirstOrDefault(x => x.LinkedProductId == relatedProductVm.Id);
                if (productLink == null)
                {
                    productLink = new ProductLink
                    {
                        LinkType = ProductLinkType.Related,
                        Product = product,
                        LinkedProductId = relatedProductVm.Id,
                    };

                    _productLinkRepository.Add(productLink);
                }
            }

            foreach (var productLink in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Related))
            {
                if (model.Product.RelatedProducts.All(x => x.Id != productLink.LinkedProductId))
                {
                    _productLinkRepository.Remove(productLink);
                }
            }

            foreach (var crossSellProductVm in model.Product.CrossSellProducts)
            {
                var productLink = product.ProductLinks.Where(x => x.LinkType == ProductLinkType.CrossSell).FirstOrDefault(x => x.LinkedProductId == crossSellProductVm.Id);
                if (productLink == null)
                {
                    productLink = new ProductLink
                    {
                        LinkType = ProductLinkType.CrossSell,
                        Product = product,
                        LinkedProductId = crossSellProductVm.Id,
                    };

                    _productLinkRepository.Add(productLink);
                }
            }

            foreach (var productLink in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.CrossSell))
            {
                if (model.Product.CrossSellProducts.All(x => x.Id != productLink.LinkedProductId))
                {
                    _productLinkRepository.Remove(productLink);
                }
            }
        }

        private void AddOrDeleteProductAttribute(ProductForm model, Product product)
        {
            foreach (var productAttributeVm in model.Product.Attributes)
            {
                var productAttrValue =
                    product.AttributeValues.FirstOrDefault(x => x.AttributeId == productAttributeVm.Id);
                if (productAttrValue == null)
                {
                    productAttrValue = new ProductAttributeValue
                    {
                        AttributeId = productAttributeVm.Id,
                        Value = productAttributeVm.Value
                    };
                    product.AddAttributeValue(productAttrValue);
                }
                else
                {
                    productAttrValue.Value = productAttributeVm.Value;
                }
            }

            var deletedAttrValues =
                product.AttributeValues.Where(attrValue => model.Product.Attributes.All(x => x.Id != attrValue.AttributeId))
                    .ToList();

            foreach (var deletedAttrValue in deletedAttrValues)
            {
                deletedAttrValue.Product = null;
                product.AttributeValues.Remove(deletedAttrValue);
                _productAttributeValueRepository.Remove(deletedAttrValue);
            }
        }

        private async Task SaveProductMedias(ProductForm model, Product product)
        {
            if (model.ThumbnailImage != null)
            {
                var fileName = await SaveFile(model.ThumbnailImage);
                if (product.ThumbnailImage != null)
                {
                    product.ThumbnailImage.FileName = fileName;
                }
                else
                {
                    product.ThumbnailImage = new Media {FileName = fileName};
                }
            }

            // Currently model binder cannot map the collection of file productImages[0], productImages[1]
            foreach (var file in Request.Form.Files)
            {
                if (file.ContentDisposition.Contains("productImages"))
                {
                    model.ProductImages.Add(file);
                }
                else if (file.ContentDisposition.Contains("productDocuments"))
                {
                    model.ProductDocuments.Add(file);
                }
            }

            foreach (var file in model.ProductImages)
            {
                var fileName = await SaveFile(file);
                var productMedia = new ProductMedia
                {
                    Product = product,
                    Media = new Media {FileName = fileName, MediaType = MediaType.Image}
                };
                product.AddMedia(productMedia);
            }

            foreach (var file in model.ProductDocuments)
            {
                var fileName = await SaveFile(file);
                var productMedia = new ProductMedia
                {
                    Product = product,
                    Media = new Media { FileName = fileName, MediaType = MediaType.File, Caption = file.FileName }
                };
                product.AddMedia(productMedia);
            }
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Value.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _mediaService.SaveMediaAsync(file.OpenReadStream(), fileName, file.ContentType);
            return fileName;
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Infrastructure.Web.SmartTable;
using SimplCommerce.Module.Core.Models;
using SimplCommerce.Module.Vendors.Services;
using SimplCommerce.Module.Vendors.ViewModels;

namespace SimplCommerce.Module.Vendors.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/vendors")]
    public class VendorApiController : Controller
    {
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IVendorService _vendorService;

        public VendorApiController(IRepository<Vendor> vendorRepository, IVendorService vendorService)
        {
            _vendorRepository = vendorRepository;
            _vendorService = vendorService;
        }

        [HttpPost("grid")]
        public ActionResult List([FromBody] SmartTableParam param)
        {
            var query = _vendorRepository.Query()
                .Where(x => !x.IsDeleted);

            if (param.Search.PredicateObject != null)
            {
                dynamic search = param.Search.PredicateObject;

                if (search.Email != null)
                {
                    string email = search.Email;
                    query = query.Where(x => x.Email.Contains(email));
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

            var vendors = query.ToSmartTableResult(
                param,
                x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    IsActive = x.IsActive,
                    SeoTitle = x.SeoTitle,
                    CreatedOn = x.CreatedOn
                });

            return Json(vendors);
        }

        public IActionResult Get()
        {
            var vendors = _vendorRepository.Query().Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.SeoTitle
            });

            return Json(vendors);
        }

        [HttpGet("{id}")]
        public IActionResult Get(long id)
        {
            var vendor = _vendorRepository.Query().Include(x => x.Users).FirstOrDefault(x => x.Id == id);
            var model = new VendorForm
            {
                Id = vendor.Id,
                Name = vendor.Name,
                Slug = vendor.SeoTitle,
                Email = vendor.Email,
                Description = vendor.Description,
                IsActive = vendor.IsActive,
                Managers = vendor.Users.Select(x => new VendorManager { UserId = x.Id, Email = x.Email }).ToList(),
                VendorType = vendor.VendorType,
                VendorClass = vendor.VendorClass,
                Phone1 = vendor.Phone1,
                Phone2 = vendor.Phone2,
                Phone3 = vendor.Phone3,
                Phone4 = vendor.Phone4,
                Address = vendor.Address,
                City = vendor.City,
                CountryId = vendor.CountryId,
                Area = vendor.Area,
                Website = vendor.Website,
                SendEmails = vendor.SendEmails,
                BankName = vendor.BankName,
                Iban = vendor.Iban,
                Notes = vendor.Notes
            };
    
            return Json(model);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VendorForm model)
        {
            if (ModelState.IsValid)
            {
                var vendor = new Vendor
                {
                    Name = model.Name,
                    SeoTitle = model.Slug,
                    Email = model.Email,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    VendorType = model.VendorType,
                    VendorClass = model.VendorClass,
                    Phone1 = model.Phone1,
                    Phone2 = model.Phone2,
                    Phone3 = model.Phone3,
                    Phone4 = model.Phone4,
                    Address = model.Address,
                    City = model.City,
                    CountryId = model.CountryId,
                    Area = model.Area,
                    Website = model.Website,
                    SendEmails = model.SendEmails,
                    BankName = model.BankName,
                    Iban = model.Iban,
                    Notes = model.Notes
                };

                _vendorService.Create(vendor);

                return Ok();
            }
            return new BadRequestObjectResult(ModelState);
        }

        [HttpPut("{id}")]
        public IActionResult Put(long id, [FromBody] VendorForm model)
        {
            if (ModelState.IsValid)
            {
                var vendor = _vendorRepository.Query().FirstOrDefault(x => x.Id == id);
                vendor.Name = model.Name;
                vendor.SeoTitle = model.Slug;
                vendor.Email = model.Email;
                vendor.Description = model.Description;
                vendor.IsActive = model.IsActive;
                vendor.UpdatedOn = DateTimeOffset.Now;
                vendor.VendorType = model.VendorType;
                vendor.VendorClass = model.VendorClass;
                vendor.Phone1 = model.Phone1;
                vendor.Phone2 = model.Phone2;
                vendor.Phone3 = model.Phone3;
                vendor.Phone4 = model.Phone4;
                vendor.Address = model.Address;
                vendor.City = model.City;
                vendor.CountryId = model.CountryId;
                vendor.Area = model.Area;
                vendor.Website = model.Website;
                vendor.SendEmails = model.SendEmails;
                vendor.BankName = model.BankName;
                vendor.Iban = model.Iban;
                vendor.Notes = model.Notes;

                _vendorService.Update(vendor);

                return Ok();
            }

            return new BadRequestObjectResult(ModelState);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var vendor = _vendorRepository.Query().FirstOrDefault(x => x.Id == id);
            if (vendor == null)
            {
                return new NotFoundResult();
            }

            await _vendorService.Delete(vendor);
            return Json(true);
        }
    }
}

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

        public async Task<IActionResult> Get()
        {
            var vendors = await _vendorRepository.Query().Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.SeoTitle
            }).ToListAsync();

            return Json(vendors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var vendor = await _vendorRepository.Query().Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == id);
            if(vendor == null)
            {
                return NotFound();
            }

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
                AccountNumber = vendor.AccountNumber,
                Iban = vendor.Iban,
                Notes = vendor.Notes
            };
    
            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] VendorForm model)
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
                    AccountNumber = model.AccountNumber,
                    Iban = model.Iban,
                    Notes = model.Notes
                };

                await _vendorService.Create(vendor);
                return CreatedAtAction(nameof(Get), new { id = vendor.Id }, null);
            }
            return BadRequest(ModelState);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] VendorForm model)
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
                vendor.AccountNumber = model.AccountNumber;
                vendor.Iban = model.Iban;
                vendor.Notes = model.Notes;

                await _vendorService.Update(vendor);
                return Accepted();
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var vendor = await _vendorRepository.Query().FirstOrDefaultAsync(x => x.Id == id);
            if (vendor == null)
            {
                return NotFound();
            }

            await _vendorService.Delete(vendor);
            return NoContent();
        }
    }
}

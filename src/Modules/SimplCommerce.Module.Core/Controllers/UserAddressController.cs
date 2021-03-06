﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Module.Core.Extensions;
using SimplCommerce.Module.Core.Models;
using SimplCommerce.Module.Core.ViewModels;

namespace SimplCommerce.Module.Core.Controllers
{
    [Authorize]
    public class UserAddressController : Controller
    {
        private readonly IRepository<UserAddress> _userAddressRepository;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<StateOrProvince> _stateOrProvinceRepository;
        private readonly IRepository<District> _districtRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IWorkContext _workContext;

        public UserAddressController(IRepository<UserAddress> userAddressRepository, IRepository<Country> countryRepository, IRepository<StateOrProvince> stateOrProvinceRepository,
            IRepository<District> districtRepository, IRepository<User> userRepository, IWorkContext workContext)
        {
            _userAddressRepository = userAddressRepository;
            _countryRepository = countryRepository;
            _stateOrProvinceRepository = stateOrProvinceRepository;
            _districtRepository = districtRepository;
            _userRepository = userRepository;
            _workContext = workContext;
        }

        [Route("user/address")]
        public async Task<IActionResult> List()
        {
            var currentUser = await _workContext.GetCurrentUser();
            var model = _userAddressRepository
                .Query()
                .Where(x => x.AddressType == AddressType.Shipping && x.UserId == currentUser.Id)
                .Select(x => new UserAddressListItem
                {
                    AddressId = x.AddressId,
                    UserAddressId = x.Id,
                    ContactName = x.Address.ContactName,
                    Phone = x.Address.Phone,
                    AddressLine1 = x.Address.AddressLine1,
                    AddressLine2 = x.Address.AddressLine1,
                    CountryName = x.Address.Country.Name
                }).ToList();

            foreach (var item in model)
            {
                item.IsDefaultShippingAddress = item.UserAddressId == currentUser.DefaultShippingAddressId;
            }

            return View(model);
        }

        [Route("user/address/create")]
        public IActionResult Create()
        {
            var model = new UserAddressFormViewModel();

            PopulateAddressFormData(model);

            return View(model);
        }

        [Route("user/address/create")]
        [HttpPost]
        public async Task<IActionResult> Create(UserAddressFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _workContext.GetCurrentUser();

                var address = new Address
                {
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    ContactName = model.ContactName,
                    CountryId = 238,
                    StateOrProvinceId = 1,
                    DistrictId = 1,
                    City = model.City,
                    PostalCode = model.PostalCode,
                    Phone = model.Phone,
                    Email = model.Email,
                    Mobile = model.Mobile
                };

                var userAddress = new UserAddress
                {
                    Address = address,
                    AddressType = AddressType.Shipping,
                    UserId = currentUser.Id
                };

                _userAddressRepository.Add(userAddress);
                _userAddressRepository.SaveChanges();

                if (!string.IsNullOrEmpty(model.RedirectUrl))
                {
                    return RedirectToAction(model.RedirectUrl, "checkout");
                }

                return RedirectToAction("List");
            }

            PopulateAddressFormData(model);
            return View(model);
        }

        [Route("user/address/edit/{id}")]
        public async Task<IActionResult> Edit(long id)
        {
            var currentUser = await _workContext.GetCurrentUser();

            var userAddress = _userAddressRepository.Query()
                .Include(x => x.Address)
                .FirstOrDefault(x => x.Id == id && x.UserId == currentUser.Id);

            if (userAddress == null)
            {
                return NotFound();
            }

            var model = new UserAddressFormViewModel
            {
                Id = userAddress.Id,
                ContactName = userAddress.Address.ContactName,
                Phone = userAddress.Address.Phone,
                AddressLine1 = userAddress.Address.AddressLine1,
                AddressLine2 = userAddress.Address.AddressLine2,
                CountryId = userAddress.Address.CountryId,
                DistrictId = 1,
                StateOrProvinceId = 1,
                City = userAddress.Address.City,
                PostalCode = userAddress.Address.PostalCode,
                Mobile = userAddress.Address.Mobile,
                Email = userAddress.Address.Email
            };

            PopulateAddressFormData(model);

            return View(model);
        }

        [Route("user/address/edit/{id}")]
        [HttpPost]
        public async Task<IActionResult> Edit(long id, UserAddressFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _workContext.GetCurrentUser();

                var userAddress = _userAddressRepository.Query()
                    .Include(x => x.Address)
                    .FirstOrDefault(x => x.Id == id && x.UserId == currentUser.Id);

                if (userAddress == null)
                {
                    return NotFound();
                }

                userAddress.Address.AddressLine1 = model.AddressLine1;
                userAddress.Address.AddressLine2 = model.AddressLine2;
                userAddress.Address.ContactName = model.ContactName;
                userAddress.Address.CountryId = model.CountryId;
                userAddress.Address.StateOrProvinceId = 1;
                userAddress.Address.DistrictId = 1;
                userAddress.Address.City = model.City;
                userAddress.Address.PostalCode = model.PostalCode;
                userAddress.Address.Phone = model.Phone;
                userAddress.Address.Email = model.Email;
                userAddress.Address.Mobile = model.Mobile;

                _userAddressRepository.SaveChanges();
                return RedirectToAction("List");
            }

            PopulateAddressFormData(model);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetAsDefault(long id)
        {
            var currentUser = await _workContext.GetCurrentUser();

            var userAddress = _userAddressRepository.Query()
                .FirstOrDefault(x => x.Id == id && x.UserId == currentUser.Id);

            if (userAddress == null)
            {
                return NotFound();
            }

            currentUser.DefaultShippingAddressId = userAddress.Id;
            _userRepository.SaveChanges();

            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUser = await _workContext.GetCurrentUser();

            var userAddress = _userAddressRepository.Query()
                .FirstOrDefault(x => x.Id == id && x.UserId == currentUser.Id);

            if (userAddress == null)
            {
                return NotFound();
            }

            if(currentUser.DefaultShippingAddressId == userAddress.Id)
            {
                currentUser.DefaultShippingAddressId = null;
            }

            _userAddressRepository.Remove(userAddress);
            _userAddressRepository.SaveChanges();

            return RedirectToAction("List");
        }

        private void PopulateAddressFormData(UserAddressFormViewModel model)
        {
            model.Countries = _countryRepository.Query()
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
        }
    }
}

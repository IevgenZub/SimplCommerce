using SimplCommerce.Module.Core.ViewModels;
using System.Collections.Generic;

namespace SimplCommerce.Module.Orders.ViewModels
{
    public class DeliveryInformationVm
    {
        public DeliveryInformationVm()
        {
        }

        public int PassportExpRule { get; set; }

        public int NumberofPassengers { get; set; }

        public IList<ShippingAddressVm> ExistingShippingAddresses { get; set; } =
            new List<ShippingAddressVm>();

        public long ShippingAddressId { get; set; }

        public string ShippingMethod { get; set; }

        public UserAddressFormViewModel NewAddress { get; set; } = new UserAddressFormViewModel { RedirectUrl = "Shipping" };
    }
}

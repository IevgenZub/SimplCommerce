using System.Threading.Tasks;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Module.Catalog.Models;
using SimplCommerce.Module.Core.Services;
using SimplCommerce.Module.Core.Models;
using System.Linq;

namespace SimplCommerce.Module.Catalog.Services
{
    public class ProductService : IProductService
    {
        private const long ProductEntityTypeId = 3;

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Vendor> _vendorRepository;

        private readonly IEntityService _entityService;

        public ProductService(IRepository<Product> productRepository, IRepository<Vendor> vendorRepository, IEntityService entityService)
        {
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;

            _entityService = entityService;
        }

        public void Create(Product product)
        {
            using (var transaction = _productRepository.BeginTransaction())
            {
                product.SeoTitle = _entityService.ToSafeSlug(product.SeoTitle, product.Id, ProductEntityTypeId);
                _productRepository.Add(product);
                _productRepository.SaveChanges();

                var vendor = _vendorRepository.Query().FirstOrDefault(v => v.Id == product.VendorId);
                
                product.ReservationNumber = product.Id.ToReservation(vendor.Name);

                foreach (var link in product.ProductLinks.Where(pl => pl.LinkType == ProductLinkType.Super))
                {
                    link.Product.ReservationNumber = link.Product.Id.ToReservation(vendor.Name);
                }

                _entityService.Add(product.Name, product.SeoTitle, product.Id, ProductEntityTypeId);
                _productRepository.SaveChanges();

                transaction.Commit();
            }
        }

        public void Update(Product product)
        {
            var slug = _entityService.Get(product.Id, ProductEntityTypeId);
            if (product.IsVisibleIndividually)
            {
                product.SeoTitle = _entityService.ToSafeSlug(product.SeoTitle, product.Id, ProductEntityTypeId);
                if (slug != null)
                {
                    _entityService.Update(product.Name, product.SeoTitle, product.Id, ProductEntityTypeId);
                }
                else
                {
                    _entityService.Add(product.Name, product.SeoTitle, product.Id, ProductEntityTypeId);
                }
            }
            else
            {
                if (slug != null)
                {
                    _entityService.Remove(product.Id, ProductEntityTypeId);
                }
            }
            _productRepository.SaveChanges();
        }

        public async Task Delete(Product product)
        {
            product.IsDeleted = true;
            await _entityService.Remove(product.Id, ProductEntityTypeId);
            _productRepository.SaveChanges();
        }
    }

    public static class ReservationNumberHelper
    {
        const string ReservationNumberFormat = "000000.##";

        public static string ToReservation(this long id, string vendor)
        {
            return vendor[0].ToString().ToUpper() + vendor[1].ToString().ToUpper() + id.ToString(ReservationNumberFormat);
        }
    }
}

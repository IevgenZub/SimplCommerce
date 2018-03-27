using System.Threading.Tasks;
using SimplCommerce.Infrastructure.Web;
using SimplCommerce.Module.Core.Models;
using SimplCommerce.Module.Core.Services;
using SimplCommerce.Module.Orders.Models;

namespace SimplCommerce.Module.Orders.Services
{
    public class OrderEmailService : IOrderEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly IRazorViewRenderer _viewRender;
        public OrderEmailService(IEmailSender emailSender, IRazorViewRenderer viewRender)
        {
            _emailSender = emailSender;
            _viewRender = viewRender;
        }

        public async Task SendEmailToUser(User user, Order order, string template)
        {
            var email = user.Email;
            var emailBody = await _viewRender.RenderViewToStringAsync($"/Modules/SimplCommerce.Module.Orders/Views/EmailTemplates/{template}.cshtml", order);

            var emailSubject = $"{template} #{order.Id}";
            await _emailSender.SendEmailAsync(email, emailSubject, emailBody, true);
        }
    }
}

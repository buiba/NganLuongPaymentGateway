using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Security;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Business.Commerce.NganLuongPayment
{
    public class NganLuongPaymentController : PageController<NganLuongPage>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IContentLoader _contentLoader;
        private readonly UrlResolver _urlResolver;

        public NganLuongPaymentController() : this(ServiceLocator.Current.GetInstance<IOrderRepository>(),
            ServiceLocator.Current.GetInstance<IContentLoader>(),
            ServiceLocator.Current.GetInstance<UrlResolver>())
        { }

        public NganLuongPaymentController(IOrderRepository orderRepository, IContentLoader contentLoader, UrlResolver urlResolver)
        {
            _orderRepository = orderRepository;
            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
        }

        public ActionResult Index()
        {
            if (Request.QueryString.Count == 0)
            {
                // cancel order
                var cancelUrl = GetUrlFromStartPageReferenceProperty("CheckoutPage"); // get link to Checkout page
                return Redirect(cancelUrl);
            }

            var cart = _orderRepository.LoadCart<ICart>(PrincipalInfo.CurrentPrincipal.GetContactId(), Cart.DefaultName);
            var payment = cart.GetFirstForm().Payments.First();
            var purchaseOrder = MakePurchaseOrder(cart, Request.QueryString["order_code"]);

            // redirect to Order Confirmation page
            var confirmationUrl = GetUrlFromStartPageReferenceProperty("NganLuongConfirmationPage"); // get link to NganLuong confirmation page            

            confirmationUrl = UriUtil.AddQueryString(confirmationUrl, "contactId", purchaseOrder.CustomerId.ToString());
            confirmationUrl = UriUtil.AddQueryString(confirmationUrl, "orderNumber", purchaseOrder.OrderLink.OrderGroupId.ToString());            
            confirmationUrl = UriUtil.AddQueryString(confirmationUrl, "email", payment.BillingAddress.Email);

            return Redirect(confirmationUrl);
        }

        private IPurchaseOrder MakePurchaseOrder(ICart cart, string orderNumber)
        {
            var orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
            var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
            purchaseOrder.OrderNumber = orderNumber;
            purchaseOrder.OrderStatus = OrderStatus.InProgress;
            purchaseOrder.GetFirstForm().Payments.First().Status = PaymentStatus.Processed.ToString();
            _orderRepository.Save(purchaseOrder);

            // Remove old cart
            _orderRepository.Delete(cart.OrderLink);

            return purchaseOrder;
        }

        /// <summary>
        /// Gets url from start page's page reference property.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The friendly url.</returns>
        private string GetUrlFromStartPageReferenceProperty(string propertyName)
        {
            var startPageData = _contentLoader.Get<PageData>(ContentReference.StartPage);
            if (startPageData == null)
            {
                return _urlResolver.GetUrl(ContentReference.StartPage);
            }

            var contentLink = startPageData.Property[propertyName]?.Value as ContentReference;
            if (!ContentReference.IsNullOrEmpty(contentLink))
            {
                return _urlResolver.GetUrl(contentLink);
            }
            return _urlResolver.GetUrl(ContentReference.StartPage);
        }
    }
}

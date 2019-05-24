using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;
using System;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace EPiServer.Business.Commerce.NganLuongPayment
{
    public class NganLuongPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        private IOrderNumberGenerator _orderNumberGenerator;
        private IOrderRepository _orderRepository;

        public NganLuongPaymentGateway() : this(
            ServiceLocator.Current.GetInstance<IOrderRepository>(),
            ServiceLocator.Current.GetInstance<IOrderNumberGenerator>())
        { }

        public NganLuongPaymentGateway(IOrderRepository orderRepository,
            IOrderNumberGenerator orderNumberGenerator)
        {
            _orderNumberGenerator = orderNumberGenerator;
            _orderRepository = orderRepository;
        }

        public override bool ProcessPayment(Payment payment, ref string message)
        {
            var orderGroup = payment.Parent.Parent;
            var processingResult = ProcessPayment(orderGroup, payment);

            if (processingResult.RedirectUrl.Length > 0)
            // if (!string.IsNullOrEmpty(processingResult.RedirectUrl))
            {
                HttpContext.Current.Response.Redirect(processingResult.RedirectUrl);
            }

            return processingResult.IsSuccessful;
        }

        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {

            var message = "message";
            _orderRepository.Save(orderGroup);
            var redirectUrl = CreateRedirectUrl(orderGroup);
            return PaymentProcessingResult.CreateSuccessfulResult(message, redirectUrl);
        }

        public string CreateMD5Hash(string input)
        {
            // Use input string to calculate MD5 hash
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
                // To force the hex string to lower-case letters instead of
                // upper-case, use he following line instead:
                // sb.Append(hashBytes[i].ToString("x2")); 
            }
            return sb.ToString();
        }

        public String GetMD5Hash(String input)
        {

            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();

            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);

            bs = x.ComputeHash(bs);

            System.Text.StringBuilder s = new System.Text.StringBuilder();

            foreach (byte b in bs)
            {

                s.Append(b.ToString("x2").ToLower());

            }

            String md5String = s.ToString();

            return md5String;
        }

        private string CreateRedirectUrl(IOrderGroup orderGroup)
        {
            var orderCode = _orderNumberGenerator.GenerateOrderNumber(orderGroup);
            var redirectUrl = ConfigurationManager.AppSettings["NganLuong:RedirectUrl"].ToString();

            var securePass = ConfigurationManager.AppSettings["NganLuong:SecurePass"].ToString();
      
            var merchantSiteCode = ConfigurationManager.AppSettings["NganLuong:SiteCode"].ToString();

            var returnUrl = ConfigurationManager.AppSettings["NganLuong:ReturnUrl"].ToString();

            var receiver = ConfigurationManager.AppSettings["NganLuong:Receiver"].ToString();

            var transactionInfo = ConfigurationManager.AppSettings["NganLuong:TransactionInfor"].ToString() + orderCode;
          
            var payment = orderGroup.GetFirstForm().Payments.First();
            var price = payment.Amount.ToString();
            var currency = orderGroup.Currency.CurrencyCode;
            var quantity = "1";
            var tax = "0";
            var discount = "0";
            var feeCal = "0";
            var feeShipping = "0";
            var orderDescription = ConfigurationManager.AppSettings["NganLuong:OrderDescription"] + orderCode;

            var billingAddress = payment.BillingAddress;
            var buyerInfo = $"{billingAddress.FirstName} {billingAddress.LastName}*|*{billingAddress.Email}*|*{billingAddress.DaytimePhoneNumber}*|*{billingAddress.Line1}";
            var affiliateCode = "";
            var lang = "vi";
            var cancelUrl = ConfigurationManager.AppSettings["NganLuong:CancelUrl"].ToString();

            var security_code = merchantSiteCode;
            security_code += " " + returnUrl;
            security_code += " " + receiver;
            security_code += " " + transactionInfo;
            security_code += " " + orderCode;
            security_code += " " + price;
            security_code += " " + currency;
            security_code += " " + quantity;
            security_code += " " + tax;
            security_code += " " + discount;
            security_code += " " + feeCal;
            security_code += " " + feeShipping;
            security_code += " " + orderDescription;
            security_code += " " + buyerInfo;
            security_code += " " + affiliateCode;
            security_code += " " + securePass;

            string secureCodeMd5 = CreateMD5Hash(security_code);

            redirectUrl = UriUtil.AddQueryString(redirectUrl, "merchant_site_code", merchantSiteCode);
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "return_url", HttpUtility.UrlEncode(returnUrl).ToLower());
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "receiver", HttpUtility.UrlEncode(receiver));
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "transaction_info", HttpUtility.UrlEncode(transactionInfo));
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "order_code", orderCode);
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "price", price);
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "currency", currency);
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "quantity", quantity);
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "tax", tax);
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "discount", discount);
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "fee_cal", feeCal);
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "fee_shipping", feeShipping);
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "order_description", HttpUtility.UrlEncode(orderDescription));
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "buyer_info", HttpUtility.UrlEncode(buyerInfo));
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "affiliate_code", affiliateCode);
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "lang", lang);
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "secure_code", secureCodeMd5);
            redirectUrl = UriUtil.AddQueryString(redirectUrl, "cancel_url", HttpUtility.UrlEncode(cancelUrl));

            return redirectUrl;
        }
    }
}

using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using System;
using System.Linq;

namespace EPiServer.Business.Commerce.NganLuongPayment
{
    [ServiceConfiguration(typeof(IPaymentMethod))]
    public class NganLuongPaymentMethod : IPaymentMethod
    {
        private readonly IOrderGroupFactory _orderGroupFactory;

        public NganLuongPaymentMethod() : this(ServiceLocator.Current.GetInstance<IOrderGroupFactory>())
        { }

        public NganLuongPaymentMethod(IOrderGroupFactory orderGroupFactory)
        {
            _orderGroupFactory = orderGroupFactory;

            var paymentMethodDto = PaymentManager.GetPaymentMethodBySystemName("NganLuong", SiteContext.Current.LanguageName)
                .PaymentMethod.FirstOrDefault();

            PaymentMethodId = paymentMethodDto.PaymentMethodId;
            SystemKeyword = paymentMethodDto.SystemKeyword;
            Name = paymentMethodDto.Name;
            Description = paymentMethodDto.Description;
        }

        public Guid PaymentMethodId { get; }

        public string SystemKeyword { get; }

        public string Name { get; }

        public string Description { get; }

        public IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreatePayment(_orderGroupFactory, typeof(OtherPayment));
            // var payment = _orderGroupFactory.CreatePayment(orderGroup);

            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = Name;
            payment.Amount = amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = TransactionType.Sale.ToString();

            return payment;
        }

        public bool ValidateData()
        {
            return true;
        }
    }
}

using Mollie.Api.Models;
using Mollie.Api.Models.Payment;
using Mollie.Api.Models.Payment.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.MolliePayments.Utilities
{
    public static class CreatePaymentRequest
    {
        public static PaymentRequest MolliePaymentRequest(decimal OrderTotal, string description)
        {
            return new PaymentRequest()
            {
                Amount = new Amount(Currency.EUR, OrderTotal),
                Description = description,
                RedirectUrl = "",
                Methods = new List<string>()
                {
                    PaymentMethod.Ideal,
                    PaymentMethod.CreditCard,
                    PaymentMethod.PayPal,
                    PaymentMethod.Bancontact
                }
            };
        }
    }
}

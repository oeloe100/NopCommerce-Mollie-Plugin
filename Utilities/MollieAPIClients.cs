using Mollie.Api.Client;
using Mollie.Api.Client.Abstract;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.MolliePayments.Utilities
{
    public static class MollieAPIClients
    {
        private static string _selectedKey;

        public static PaymentClient MolliePaymentClient(
            bool isSandbox, 
            IDictionary<string, string> apiKeys)
        {
            _selectedKey = apiKeys["live"];

            if (isSandbox)
            {
                _selectedKey = apiKeys["test"];
            }

            return new PaymentClient(_selectedKey);
        }

        public static OrderClient MollieOrderClient(
            bool isSandbox,
            IDictionary<string, string> apiKeys)
        {
            _selectedKey = apiKeys["live"];

            if (isSandbox)
            {
                _selectedKey = apiKeys["test"];
            }

            return new OrderClient(_selectedKey);
        }

        public static RefundClient MollieRefundClient(
            bool isSandbox,
            IDictionary<string, string> apiKeys)
        {
            _selectedKey = apiKeys["live"];

            if (isSandbox)
            {
                _selectedKey = apiKeys["test"];
            }

            return new RefundClient(_selectedKey);
        }
    }
}

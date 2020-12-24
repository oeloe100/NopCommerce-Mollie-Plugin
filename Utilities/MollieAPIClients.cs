using Mollie.Api.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.MolliePayments.Utilities
{
    public static class MollieAPIClients
    {
        private static string _selectedKey;

        public static PaymentClient MolliePaymentClient(bool isSandbox, IDictionary<string, string> apiKeys)
        {
            _selectedKey = apiKeys["live"];

            if (isSandbox)
                _selectedKey = apiKeys["test"];

            return new PaymentClient(_selectedKey);
        }
    }
}

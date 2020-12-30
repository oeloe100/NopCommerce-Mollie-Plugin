using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.MolliePayments
{
    public class MollieStandardPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use sandbox (testing environment)
        /// </summary>
        public bool UseSandbox { get; set; } = true;

        /// <summary>
        /// Gets or sets a value for the Mollie Live Key
        /// </summary>
        public string ApiTestKey { get; set; } = "ABC123";

        /// <summary>
        /// Gets or sets a value for the Mollie Test Key
        /// </summary>
        public string ApiLiveKey { get; set; } = "ABC123";
    }
}

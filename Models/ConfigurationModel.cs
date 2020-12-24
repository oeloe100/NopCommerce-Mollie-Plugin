using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.MolliePayments.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("ApiTestKey")]
        public string ApiTestKey { get; set; }
        public bool ApiTestKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("ApiLiveKey")]
        public string ApiLiveKey { get; set; }
        public bool ApiLiveKey_OverrideForStore { get; set; }
    }
}

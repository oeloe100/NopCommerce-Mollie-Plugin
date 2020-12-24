using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.MolliePayments.Components
{
    [ViewComponent(Name = "MolliePayments")]
    public class PaymentMolliePaymentStandardViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.MolliePayments/Views/PaymentInfo.cshtml");
        }
    }
}

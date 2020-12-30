using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mollie.Api.Client;
using Mollie.Api.Models.Order;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.MolliePayments.Models;
using Nop.Plugin.Payments.MolliePayments.Utilities;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.MolliePayments.Controllers
{
    public class PaymentMolliePaymentsController : BasePaymentController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private MollieStandardPaymentSettings _mollieStandardPaymentSettings;
        private OrderClient _mollieOrderClient;

        #endregion

        #region Ctor

        public PaymentMolliePaymentsController(ISettingService settingService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
        MollieStandardPaymentSettings mollieStandardPaymentSettings)
        {
            _settingService = settingService;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _mollieStandardPaymentSettings = mollieStandardPaymentSettings;

            _mollieOrderClient = MollieAPIClients.MollieOrderClient(
                _mollieStandardPaymentSettings.UseSandbox,
                GetKeysDictionary());
        }

        #endregion

        #region Utilities
        private Dictionary<string, string> GetKeysDictionary()
        {
            return new Dictionary<string, string>
            {
                { "live", _mollieStandardPaymentSettings.ApiLiveKey },
                { "test", _mollieStandardPaymentSettings.ApiTestKey }
            };
        }

        #endregion

        #region Methods

        [AuthorizeAdmin] //confirms access to the admin panel
        [Area(AreaNames.Admin)] //specifies the area containing a controller or action
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var MolliePaymentSettings = _settingService.LoadSetting<MollieStandardPaymentSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                UseSandbox = MolliePaymentSettings.UseSandbox,
                ApiLiveKey = MolliePaymentSettings.ApiLiveKey,
                ApiTestKey = MolliePaymentSettings.ApiTestKey
            };

            if (storeScope <= 0)
                return View("~/Plugins/Payments.MolliePayments/Views/Configure.cshtml", model);

            model.UseSandbox_OverrideForStore = _settingService.SettingExists(MolliePaymentSettings, x => x.UseSandbox, storeScope);
            model.UseSandbox_OverrideForStore = _settingService.SettingExists(MolliePaymentSettings, x => x.ApiLiveKey, storeScope);
            model.UseSandbox_OverrideForStore = _settingService.SettingExists(MolliePaymentSettings, x => x.ApiTestKey, storeScope);

            return View("~/Plugins/Payments.MolliePayments/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var MolliePaymentSettings = _settingService.LoadSetting<MollieStandardPaymentSettings>(storeScope);

            MolliePaymentSettings.UseSandbox = model.UseSandbox;
            MolliePaymentSettings.ApiLiveKey = model.ApiLiveKey;
            MolliePaymentSettings.ApiTestKey = model.ApiTestKey;

            /* We do not clear cache after each setting update.
            * This behavior can increase performance because cached settings will not be cleared 
            * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(MolliePaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(MolliePaymentSettings, x => x.ApiLiveKey, model.ApiLiveKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(MolliePaymentSettings, x => x.ApiTestKey, model.ApiTestKey_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpPost]
        public IActionResult MollieWebHook(string id)
        {
            OrderResponse retrieveOrder = _mollieOrderClient.GetOrderAsync(id).Result;

            Order order = _orderService.GetOrderById(Convert.ToInt32(retrieveOrder.OrderNumber));
            order.PaymentStatus = Core.Domain.Payments.PaymentStatus.Paid;

            return Ok(200);
        }

        #endregion
    }
}

﻿using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.MolliePayments.Utilities;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.MolliePayments
{
    /// <summary>
    /// Mollie Payments Processor
    /// </summary>
    class MolliePaymentsProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields
        
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        private readonly MollieStandardPaymentSettings _mollieStandardPaymentSettings;

        #endregion

        #region Ctor

        public MolliePaymentsProcessor(IWebHelper webHelper,
            ILocalizationService localizationService,
            ISettingService settingService,
            MollieStandardPaymentSettings mollieStandardPaymentSettings)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _settingService = settingService;
            _mollieStandardPaymentSettings = mollieStandardPaymentSettings;
        }

        #endregion

        #region Methods

        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new MollieStandardPaymentSettings
            {
                UseSandbox = true
            });

            //locales
            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
                ["Plugins.Payments.MolliePayments.PaymentMethodDescription"] = "You will be redirected to Mollie site to complete the payment",
                ["Plugins.Payments.MolliePayments.Fields.RedirectionTip"] = "You will be redirected to Mollie site to complete the order.",
            });

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<MollieStandardPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResources("Plugins.Payments.MolliePayments");

            base.Uninstall();
        }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// PostProcessPayment. 
        /// This method is invoked right after a customer places an order. 
        /// Usually this method is used when you need to redirect a customer to a third-party site for completing a payment (for example, PayPal Standard).
        /// </summary>
        /// <param name="postProcessPaymentRequest"></param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var keyDictionary = new Dictionary<string, string>
            {
                { "live", _mollieStandardPaymentSettings.ApiLiveKey },
                { "test", _mollieStandardPaymentSettings.ApiTestKey }
            };

            var mollieApiClient = MollieAPIClients.MolliePaymentClient(_mollieStandardPaymentSettings.UseSandbox, keyDictionary);

            CreatePaymentRequest.MolliePaymentRequest(
                postProcessPaymentRequest.Order.OrderTotal,
                postProcessPaymentRequest.Order.CheckoutAttributeDescription);
        }

        /// <summary>
        /// ValidatePaymentForm is used in the public store to validate customer input. 
        /// It returns a list of warnings (for example, a customer did not enter his credit card name). 
        /// If your payment method does not ask the customer to enter additional information, then the ValidatePaymentForm should return an empty list:
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        /// <summary>
        /// GetPaymentInfo method is used in the public store to parse customer input, such as credit card information. 
        /// This method returns a ProcessPaymentRequest object with parsed customer input (for example, credit card information). 
        /// If your payment method does not ask the customer to enter additional information, then GetPaymentInfo will return an empty ProcessPaymentRequest object:
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// HidePaymentMethod. You can put any logic here. For example, hide this payment method if all products in the cart are downloadable. 
        /// Or hide this payment method if current customer is from certain country
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        /// <summary>
        /// GetAdditionalHandlingFee. 
        /// You can return any additional handling fees which will be added to an order total.
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return 0;
        }

        /// <summary>
        /// Capture. Some payment gateways allow you to authorize payments before they're captured. 
        /// It allows store owners to review order details before the payment is actually done. 
        /// In this case you just authorize a payment in ProcessPayment or PostProcessPayment method (described above), and then just capture it. 
        /// In this case a Capture button will be visible on the order details page in admin area. 
        /// Note that an order should be already authorized and SupportCapture property should return true.
        /// </summary>
        /// <param name="capturePaymentRequest"></param>
        /// <returns></returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Refund. This method allows you make a refund. 
        /// In this case a Refund button will be visible on the order details page in admin area. 
        /// Note that an order should be paid, and SupportRefund or SupportPartiallyRefund property should return true.
        /// </summary>
        /// <param name="refundPaymentRequest"></param>
        /// <returns></returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            //Implement Mollie.Api refund logic here. 
            //This method enabled refund using store dashboard.

            throw new NotImplementedException();
        }

        /// <summary>
        /// Void. This method allows you void an authorized but not captured payment. 
        /// In this case a Void button will be visible on the order details page in admin area. 
        /// Note that an order should be authorized and SupportVoid property should return true.
        /// </summary>
        /// <param name="voidPaymentRequest"></param>
        /// <returns></returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ProcessRecurringPayment. Use this method to process recurring payments.
        /// </summary>
        /// <param name="processPaymentRequest"></param>
        /// <returns></returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// CancelRecurringPayment. Use this method to cancel recurring payments.
        /// </summary>
        /// <param name="cancelPaymentRequest"></param>
        /// <returns></returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// CanRePostProcessPayment. 
        /// Usually this method is used when it redirects a customer to a third-party site for completing a payment. 
        /// If the third party payment fails this option will allow customers to attempt the order again later without placing a new order.
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 1)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentMolliePayments/Configure";
        }

        /// <summary>
        /// This method should return the name of the view component which used to display public information for customers.
        /// </summary>
        /// <returns></returns>
        public string GetPublicViewComponentName()
        {
            return "MolliePayments";
        }


        #endregion

        #region Properties

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => true;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.MolliePayments.PaymentMethodDescription");

        #endregion
    }
}

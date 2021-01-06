using Mollie.Api.Models;
using Mollie.Api.Models.Order;
using Mollie.Api.Models.Payment;
using Mollie.Api.Models.Payment.Request;
using Nop.Core.Domain.Common;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nop.Plugin.Payments.MolliePayments.Utilities
{
    public static class CreatePaymentRequest
    {
        public static PaymentRequest MolliePaymentRequest(
            decimal OrderTotal, 
            string description,
            string redirectUrl)
        {
            return new PaymentRequest()
            {
                Amount = new Amount(Currency.EUR, OrderTotal),
                Description = description,
                RedirectUrl = redirectUrl,
                Methods = new List<string>()
                {
                    PaymentMethod.Ideal,
                    PaymentMethod.CreditCard,
                    PaymentMethod.PayPal,
                    PaymentMethod.Bancontact
                }
            };
        }

        public static OrderRequest MollieOrderRequest(
            PostProcessPaymentRequest postProcessPaymentRequest,
            List<OrderLineRequest> orderLines,
            string redirectUrl,
            Address orderAddress,
            Address billingAddress,
            Core.Domain.Directory.Currency selectCurrency,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            string url)
        {
            var orderRequest =  new OrderRequest()
            {
                Amount = new Amount(selectCurrency.CurrencyCode, postProcessPaymentRequest.Order.OrderTotal),
                OrderNumber = postProcessPaymentRequest.Order.CustomOrderNumber,
                Methods = new List<string>()
                {
                    PaymentMethod.Ideal,
                    PaymentMethod.CreditCard,
                    PaymentMethod.PayPal,
                    PaymentMethod.Bancontact
                },
                Lines = orderLines,
                BillingAddress = new OrderAddressDetails()
                {
                    GivenName = billingAddress?.FirstName,
                    FamilyName = billingAddress?.LastName,
                    Email = billingAddress?.Email,
                    City = billingAddress?.City,
                    Country = countryService.GetCountryByAddress(orderAddress)?.TwoLetterIsoCode,
                    PostalCode = billingAddress?.ZipPostalCode,
                    Region = stateProvinceService.GetStateProvinceByAddress(orderAddress)?.Abbreviation,
                    StreetAndNumber = billingAddress?.Address1
                },
                ShippingAddress = new OrderAddressDetails()
                {
                    GivenName = orderAddress?.FirstName,
                    FamilyName = orderAddress?.LastName,
                    Email = orderAddress?.Email,
                    City = orderAddress?.City,
                    Country = countryService.GetCountryByAddress(orderAddress)?.TwoLetterIsoCode,
                    PostalCode = orderAddress?.ZipPostalCode,
                    Region = stateProvinceService.GetStateProvinceByAddress(orderAddress)?.Abbreviation,
                    StreetAndNumber = orderAddress?.Address1
                },
                ShopperCountryMustMatchBillingCountry = true,
                RedirectUrl = redirectUrl,
                Locale = Locale.nl_NL,
                WebhookUrl = url + "/PaymentMolliePayments/MollieWebHook"
            };

            return orderRequest;
        }

        public static List<OrderLineRequest> BuildOrderLines(
            PostProcessPaymentRequest postProcessPaymentRequest,
            Core.Domain.Directory.Currency selectCurrency,
            IProductService productService, 
            IOrderService orderService)
        {
            var orderLine = new List<OrderLineRequest>();
            var taxRateToDecimal = Convert.ToDecimal(postProcessPaymentRequest.Order.TaxRates.Substring(0, 2));

            foreach (var item in orderService.GetOrderItems(postProcessPaymentRequest.Order.Id))
            {
                var product = productService.GetProductById(item.ProductId);
                var roundedItemPrice = Math.Round(item.UnitPriceInclTax, 2);
                var roundedTaxAmount = Math.Round((item.UnitPriceInclTax - item.UnitPriceExclTax), 2);

                orderLine.Add(new OrderLineRequest()
                {
                    Name = product.Name,
                    Quantity = item.Quantity,
                    Sku = product.Sku,
                    UnitPrice = new Amount(selectCurrency.CurrencyCode, roundedItemPrice),
                    TotalAmount = new Amount(selectCurrency.CurrencyCode, (item.Quantity * roundedItemPrice)),
                    VatRate = postProcessPaymentRequest.Order.TaxRates.Substring(0, 2),
                    VatAmount = new Amount(selectCurrency.CurrencyCode, roundedTaxAmount.ToString("0.00", CultureInfo.InvariantCulture))
                });
            }

            orderLine.Add(new OrderLineRequest()
            {
                Name = postProcessPaymentRequest.Order.ShippingMethod,
                Quantity = 1,
                UnitPrice = new Amount(selectCurrency.CurrencyCode, postProcessPaymentRequest.Order.OrderShippingInclTax),
                TotalAmount = new Amount(selectCurrency.CurrencyCode, postProcessPaymentRequest.Order.OrderShippingInclTax),
                VatRate = postProcessPaymentRequest.Order.TaxRates.Substring(0, 2),
                VatAmount = new Amount(selectCurrency.CurrencyCode, (postProcessPaymentRequest.Order.OrderShippingInclTax * taxRateToDecimal) / (taxRateToDecimal + 100)),
            });

            return orderLine;
        }
    }
}

﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Web.UI;

using Newtonsoft.Json;

using RestSharp;

using Rock.Attribute;
using Rock.Financial;
using Rock.Model;
using Rock.MyWell.Controls;
using Rock.Web.Cache;

// Use Newtonsoft RestRequest which is the same as RestSharp.RestRequest but uses the JSON.NET serializer.
using RestRequest = RestSharp.Newtonsoft.Json.RestRequest;

namespace Rock.MyWell
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Rock.Financial.GatewayComponent" />
    [Description( "The My Well Gateway is the primary gateway to use with My Well giving." )]
    [DisplayName( "My Well Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "My Well Gateway" )]

    #region Component Attributes

    [TextField(
        "Private API Key",
        Key = AttributeKey.PrivateApiKey,
        Description = "The private API Key used for internal operations",
        Order = 1 )]

    [TextField(
        "Public API Key",
        Key = AttributeKey.PublicApiKey,
        Description = "The public API Key used for web client operations",
        Order = 2 )]

    [TextField(
        "Card Sync Webhook Signature",
        Key = AttributeKey.CardSyncWebhookSignature,
        IsRequired = false,
        Description = "The Webhook Signature for CardSync. This field is required if using CardSync. See <a href='https://sandbox.gotnpgateway.com/docs/webhooks/#security'>webhook security documentation.</a>",
        Order = 3 )]

    [CustomRadioListField(
        "Mode",
        Key = AttributeKey.Mode,
        Description = "Set to Sandbox mode to use the sandbox test gateway instead of the production app gateway",
        ListSource = "Live,Sandbox",
        IsRequired = true,
        DefaultValue = "Live",
        Order = 4 )]

    [DecimalField(
        "Credit Card Fee Coverage Percentage",
        Key = AttributeKey.CreditCardFeeCoveragePercentage,
        Description = @"The credit card fee percentage that will be used to determine what to add to the person's donation, if they want to cover the fee.",
        IsRequired = false,
        DefaultValue = null,
        Order = 5 )]

    [CurrencyField(
        "ACH Transaction Fee Coverage Amount",
        Key = AttributeKey.ACHTransactionFeeCoverageAmount,
        Description = "The  dollar amount to add to an ACH transaction, if they want to cover the fee.",
        IsRequired = false,
        DefaultValue = null,
        Order = 6 )]

    #endregion Component Attributes
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MYWELL_FINANCIAL_GATEWAY )]
    public class MyWellGateway : GatewayComponent, IHostedGatewayComponent, IAutomatedGatewayComponent, IFeeCoverageGatewayComponent/*, IObsidianFinancialGateway*/
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Component Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The private API key
            /// </summary>
            public const string PrivateApiKey = "PrivateApiKey";

            /// <summary>
            /// The public API key
            /// </summary>
            public const string PublicApiKey = "PublicApiKey";

            /// <summary>
            /// The mode
            /// </summary>
            public const string Mode = "Mode";

            /// <summary>
            /// The credit card fee coverage percentage
            /// </summary>
            public const string CreditCardFeeCoveragePercentage = "CreditCardFeeCoveragePercentage";

            /// <summary>
            /// The ach transaction fee coverage amount
            /// </summary>
            public const string ACHTransactionFeeCoverageAmount = "ACHTransactionFeeCoverageAmount";

            /// <summary>
            /// The card sync webhook signature
            /// See https://sandbox.gotnpgateway.com/docs/webhooks/#security
            /// </summary>
            public const string CardSyncWebhookSignature = "CardSyncWebhookSignature";
        }

        #endregion Attribute Keys

        #region Obsidian

        ///// <summary>
        ///// Gets the Obsidian control file URL.
        ///// </summary>
        ///// <param name="financialGateway">The financial gateway.</param>
        ///// <returns></returns>
        ///// <value>
        ///// The control file URL.
        ///// </value>
        //public string GetObsidianControlFileUrl( FinancialGateway financialGateway )
        //{
        //    return "/Obsidian/Controls/MyWellGatewayControl.js";
        //}

        ///// <summary>
        ///// Gets the obsidian control settings.
        ///// </summary>
        ///// <param name="financialGateway">The financial gateway.</param>
        ///// <returns></returns>
        //public object GetObsidianControlSettings( FinancialGateway financialGateway )
        //{
        //    return new
        //    {
        //        PublicApiKey = GetPublicApiKey( financialGateway ),
        //        GatewayUrl = GetGatewayUrl( financialGateway )
        //    };
        //}

        #endregion Obsidian

        /// <summary>
        /// Gets the gateway URL.
        /// </summary>
        /// <value>
        /// The gateway URL.
        /// </value>
        [System.Diagnostics.DebuggerStepThrough]
        public string GetGatewayUrl( FinancialGateway financialGateway )
        {
            bool testMode = this.GetAttributeValue( financialGateway, AttributeKey.Mode ).Equals( "Sandbox" );
            if ( testMode )
            {
                return "https://sandbox.gotnpgateway.com";
            }
            else
            {
                return "https://app.gotnpgateway.com";
            }
        }

        /// <summary>
        /// Gets the public API key.
        /// </summary>
        /// <value>
        /// The public API key.
        /// </value>
        [System.Diagnostics.DebuggerStepThrough]
        public string GetPublicApiKey( FinancialGateway financialGateway )
        {
            return this.GetAttributeValue( financialGateway, AttributeKey.PublicApiKey );
        }

        /// <summary>
        /// Gets the private API key.
        /// </summary>
        /// <value>
        /// The private API key.
        /// </value>
        [System.Diagnostics.DebuggerStepThrough]
        private string GetPrivateApiKey( FinancialGateway financialGateway )
        {
            return this.GetAttributeValue( financialGateway, AttributeKey.PrivateApiKey );
        }

        /// <summary>
        /// Gets the CardSync signature.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns>System.String.</returns>
        private string GetCardSyncSignature( FinancialGateway financialGateway )
        {
            return this.GetAttributeValue( financialGateway, AttributeKey.CardSyncWebhookSignature );
        }

        /// <summary>
        /// Verifies the signature of a POST to a webhook,
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="postedSignature">The posted signature.</param>
        /// <param name="postedBody">The posted body.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool VerifySignature( FinancialGateway financialGateway, string postedSignature, string postedBody )
        {
            if ( postedSignature.IsNullOrWhiteSpace() )
            {
                return false;
            }

            // From C# example at https://sandbox.gotnpgateway.com/docs/webhooks/#security

            // ClientSecret is the 'Signature' from the WebHook at https://app.gotnpgateway.com/merchant/settings/webhooks/search
            string clientSecret = GetCardSyncSignature( financialGateway );
            if ( clientSecret.IsNullOrWhiteSpace() )
            {
                // no CardSyncSignature specified, so don't do signature validation
                return true;
            }

            byte[] clientSecretBytes = Encoding.ASCII.GetBytes( clientSecret );

            // From C# example at https://sandbox.gotnpgateway.com/docs/webhooks/#security
            // NOTE: The MyWell API may include a linefeed character ('\n' = 10 = 0x0A).
            //       If you do not include this linefeed character but we do, your
            //       signature will not match. To test with this linefeed character,
            //       add '\n' to the end of the string.
            //
            //       For example, the string below would change to:
            //       var body = "{\"data\":\"this is test data\"}\n";
            //
            //       If you're reading in the request body, in byte form, then you
            //       shouldn't have to change anything as it should include this
            //       linefeed if sent in the body.

            byte[] bodyBytes = Encoding.ASCII.GetBytes( postedBody );

            // Hash body to create signature.
            var hash = new System.Security.Cryptography.HMACSHA256( clientSecretBytes );
            byte[] signature = hash.ComputeHash( bodyBytes );

            // Base64 decode test signature.
            byte[] testSignature = Base64UrlDecode( postedSignature );

            // Compare signatures.
            if ( !signature.SequenceEqual( testSignature ) )
            {
                // Signatures do not match"
                return false;
            }

            // "Signatures match"
            return true;
        }

        /// <summary>
        /// Decodes an RFC4648 Base64 URL encoded string.
        /// </summary>
        private static byte[] Base64UrlDecode( string s )
        {
            // From C# example at https://sandbox.gotnpgateway.com/docs/webhooks/#security

            // 62nd char of encoding.
            s = s.Replace( '-', '+' );

            // 63rd char of encoding.
            s = s.Replace( '_', '/' );

            // Check for padding.
            switch ( s.Length % 4 )
            {
                case 0:
                    break;
                case 2:
                    s += "==";
                    break;
                case 3:
                    s += "=";
                    break;
                default:
                    throw new InvalidOperationException( "could not add padding" );
            }

            return Convert.FromBase64String( s );
        }

        #region IAutomatedGatewayComponent

        /// <summary>
        /// The most recent exception thrown by the gateway's remote API
        /// </summary>
        public Exception MostRecentException { get; private set; }

        /// <summary>
        /// Charges the specified payment info.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="metadata">Optional. Additional key value pairs to send to the gateway</param>
        /// <returns></returns>
        /// <exception cref="ReferencePaymentInfoRequired"></exception>
        public Payment AutomatedCharge( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage, Dictionary<string, string> metadata = null )
        {
            //// TODO - Fees? If MyWell provides fee info, we won't be able to use the Charge method as the transaction it returns
            //// doesn't have the capacity to relay fee info (also the reason this method returns a Payment rather than a
            //// transaction.

            //// payment.FeeAmount and payment.NetAmount

            MostRecentException = null;

            try
            {
                var transaction = Charge( financialGateway, paymentInfo, out errorMessage );

                if ( !string.IsNullOrEmpty( errorMessage ) )
                {
                    MostRecentException = new Exception( errorMessage );
                    return null;
                }

                var paymentDetail = transaction.FinancialPaymentDetail;

                var payment = new Payment
                {
                    AccountNumberMasked = paymentDetail.AccountNumberMasked,
                    Amount = paymentInfo.Amount,
                    ExpirationMonth = paymentDetail.ExpirationMonth,
                    ExpirationYear = paymentDetail.ExpirationYear,
                    IsSettled = transaction.IsSettled,
                    SettledDate = transaction.SettledDate,
                    NameOnCard = paymentDetail.NameOnCard,
                    Status = transaction.Status,
                    StatusMessage = transaction.StatusMessage,
                    TransactionCode = transaction.TransactionCode,
                    TransactionDateTime = transaction.TransactionDateTime ?? RockDateTime.Now
                };

                if ( paymentDetail.CreditCardTypeValueId.HasValue )
                {
                    payment.CreditCardTypeValue = DefinedValueCache.Get( paymentDetail.CreditCardTypeValueId.Value );
                }

                if ( paymentDetail.CurrencyTypeValueId.HasValue )
                {
                    payment.CurrencyTypeValue = DefinedValueCache.Get( paymentDetail.CurrencyTypeValueId.Value );
                }

                payment.GatewayPersonIdentifier = paymentDetail.GatewayPersonIdentifier;

                return payment;
            }
            catch ( Exception e )
            {
                MostRecentException = e;
                throw;
            }
        }

        #endregion IAutomatedGatewayComponent

        #region IFeeCoverageGatewayComponent

        /// <inheritdoc/>
        public decimal? GetCreditCardFeeCoveragePercentage( FinancialGateway financialGateway )
        {
            return this.GetAttributeValue( financialGateway, AttributeKey.CreditCardFeeCoveragePercentage )?.AsDecimalOrNull();
        }

        /// <inheritdoc/>
        public decimal? GetACHFeeCoverageAmount( FinancialGateway financialGateway )
        {
            return this.GetAttributeValue( financialGateway, AttributeKey.ACHTransactionFeeCoverageAmount )?.AsDecimalOrNull();
        }

        #endregion IFeeCoverageGatewayComponent

        #region IHostedGatewayComponent

        /// <summary>
        /// Gets the hosted payment information control which will be used to collect CreditCard, ACH fields
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="controlId">The control identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public Control GetHostedPaymentInfoControl( FinancialGateway financialGateway, string controlId, HostedPaymentInfoControlOptions options )
        {
            MyWellHostedPaymentControl myWellHostedPaymentControl = new MyWellHostedPaymentControl { ID = controlId };
            myWellHostedPaymentControl.MyWellGateway = this;
            myWellHostedPaymentControl.GatewayBaseUrl = this.GetGatewayUrl( financialGateway );
            List<MyWellPaymentType> enabledPaymentTypes = new List<MyWellPaymentType>();
            if ( options?.EnableACH ?? true )
            {
                enabledPaymentTypes.Add( MyWellPaymentType.ach );
            }

            if ( options?.EnableCreditCard ?? true )
            {
                enabledPaymentTypes.Add( MyWellPaymentType.card );
            }

            myWellHostedPaymentControl.EnabledPaymentTypes = enabledPaymentTypes.ToArray();

            myWellHostedPaymentControl.PublicApiKey = this.GetPublicApiKey( financialGateway );

            return myWellHostedPaymentControl;
        }

        /// <summary>
        /// Populates the properties of the referencePaymentInfo from this gateway's <seealso cref="M:Rock.Financial.IHostedGatewayComponent.GetHostedPaymentInfoControl(Rock.Model.FinancialGateway,System.String)" >hostedPaymentInfoControl</seealso>
        /// This includes the ReferenceNumber, plus any other fields that the gateway wants to set
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="hostedPaymentInfoControl">The hosted payment information control.</param>
        /// <param name="referencePaymentInfo">The reference payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        public void UpdatePaymentInfoFromPaymentControl( FinancialGateway financialGateway, Control hostedPaymentInfoControl, ReferencePaymentInfo referencePaymentInfo, out string errorMessage )
        {
            errorMessage = null;
            var myWellHostedPaymentControl = hostedPaymentInfoControl as MyWellHostedPaymentControl;
            var tokenResponse = myWellHostedPaymentControl.PaymentInfoTokenRaw.FromJsonOrNull<TokenizerResponse>();

            bool successful = tokenResponse?.IsSuccessStatus() ?? false;

            if ( !successful )
            {
                if ( tokenResponse?.HasValidationError() == true )
                {
                    errorMessage = tokenResponse.ValidationMessage;
                }

                errorMessage = tokenResponse?.Message ?? "null response from GetHostedPaymentInfoToken";
                referencePaymentInfo.ReferenceNumber = myWellHostedPaymentControl.PaymentInfoToken;
                referencePaymentInfo.InitialCurrencyTypeValue = myWellHostedPaymentControl.CurrencyTypeValue;
            }
            else
            {
                referencePaymentInfo.ReferenceNumber = myWellHostedPaymentControl.PaymentInfoToken;
                referencePaymentInfo.InitialCurrencyTypeValue = myWellHostedPaymentControl.CurrencyTypeValue;
            }
        }

        /// <summary>
        /// Gets the JavaScript needed to tell the hostedPaymentInfoControl to get send the paymentInfo and get a token
        /// Put this on your 'Next' or 'Submit' button so that the hostedPaymentInfoControl will fetch the token/response
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="hostedPaymentInfoControl">The hosted payment information control.</param>
        /// <returns></returns>
        public string GetHostPaymentInfoSubmitScript( FinancialGateway financialGateway, Control hostedPaymentInfoControl )
        {
            return $"submitTokenizer('{hostedPaymentInfoControl.ClientID}');";
        }

        /// <summary>
        /// Gets the URL that the Gateway Information UI will navigate to when they click the 'Learn More' link
        /// </summary>
        /// <value>
        /// The learn more URL.
        /// </value>
        public string LearnMoreURL => "https://www.mywell.org";

        /// <summary>
        /// Gets the URL that the Gateway Information UI will navigate to when they click the 'Configure' link
        /// </summary>
        /// <value>
        /// The configure URL.
        /// </value>
        public string ConfigureURL => "https://www.mywell.org/get-started/";

        /// <summary>
        /// Gets the hosted gateway modes that this gateway has configured/supports. Use this to determine which mode to use (in cases where both are supported, like Scheduled Payments lists ).
        /// If the Gateway supports both hosted and unhosted (and has Hosted mode configured), hosted mode should be preferred.
        /// </summary>
        /// <param name="financialGateway"></param>
        /// <returns></returns>
        /// <value>
        /// The hosted gateway modes that this gateway supports
        /// </value>
        public HostedGatewayMode[] GetSupportedHostedGatewayModes( FinancialGateway financialGateway )
        {
            // MyWellGateway only supports Hosted mode
            return new HostedGatewayMode[1] { HostedGatewayMode.Hosted };
        }

        /// <summary>
        /// Creates the customer account using a token received from the HostedPaymentInfoControl <seealso cref="M:Rock.Financial.IHostedGatewayComponent.GetHostedPaymentInfoControl(Rock.Model.FinancialGateway,System.Boolean,System.String)" />
        /// and returns a customer account token that can be used for future transactions.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public string CreateCustomerAccount( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage )
        {
            var createCustomerResponse = this.CreateCustomer( GetGatewayUrl( financialGateway ), GetPrivateApiKey( financialGateway ), paymentInfo );

            if ( createCustomerResponse?.IsSuccessStatus() != true )
            {
                errorMessage = createCustomerResponse?.Message ?? "null response from CreateCustomerAccount";
                return null;
            }
            else
            {
                errorMessage = string.Empty;
                return createCustomerResponse?.Data?.Id;
            }
        }

        /// <summary>
        /// Gets the earliest scheduled start date that the gateway will accept for the start date, based on the current local time.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        public DateTime GetEarliestScheduledStartDate( FinancialGateway financialGateway )
        {
            /* 2020-01-13 MDP
              The MyWell Gateway requires that a subscription has to have a start date at least 1 day after the current UTC Date. This sometimes will
              make the minimum date *2* days from now if it is already currently tomorrow in UTC. For example, Arizona Time is offset by -7 hours from UTC.
              So after 5pm AZ time, it is the next day in UTC. Here is a specific example:
              If a person is setting up a scheduled transaction on 2020-01-13 at 5:01PM, it will be 2020-01-14 12:01 AM in UTC. Since the first scheduled
              date need to be at least 1 date after the current date (UTC), the earliest that we can start the schedule is 2020-01-15!
            */

            // Get the current local datetime, and ensure that it is using the local timezone.
            var currentLocalDateTime = DateTime.SpecifyKind( RockDateTime.Now, DateTimeKind.Local );

            // Add a day since MyWell requires that the start date is at least 1 day in the future.
            var tomorrowLocalDateTime = currentLocalDateTime.AddDays( 1 );

            // Convert local "tomorrow date" to UTC date just in case it is a day after our local tomorrow date; (which would be true if after 5pm in AZ time).
            var tomorrowUTCDateTime = tomorrowLocalDateTime.ToUniversalTime();

            // We just want the Date portion (not time) since the Gateway determines the time of day that the actual transaction will occur.
            var tomorrowUTCDate = tomorrowUTCDateTime.Date;

            return tomorrowUTCDate;
        }

        #endregion IHostedGatewayComponent

        #region MyWellGateway Rock Wrappers

        #region Customers

        /// <summary>
        /// Creates the billing address.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private T CreateBillingAddress<T>( PaymentInfo paymentInfo ) where T : BillingAddress, new()
        {
            var result = new T
            {
                FirstName = paymentInfo.FirstName,
                LastName = paymentInfo.LastName,
                Company = paymentInfo.BusinessName,
                AddressLine1 = paymentInfo.Street1,
                AddressLine2 = paymentInfo.Street2,
                City = paymentInfo.City,
                State = paymentInfo.State,
                PostalCode = paymentInfo.PostalCode,
                Country = paymentInfo.Country,
                Email = string.Empty,
                Phone = paymentInfo.Phone,
            };

            // If the Gateway requires FirstName, just put '-' if no FirstName was provided.
            if ( result.FirstName.IsNullOrWhiteSpace() )
            {
                result.FirstName = "-";
            }

            return result;
        }

        /// <summary>
        /// Creates the customer.
        /// https://sandbox.gotnpgateway.com/docs/api/#create-a-new-customer
        /// NOTE: MyWell Gateway supports multiple payment tokens per customer, but Rock will implement it as one Payment Method per Customer, and 0 or more MyWell Customers (one for each payment entry) per Rock Person.
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private CustomerResponse CreateCustomer( string gatewayUrl, string apiKey, ReferencePaymentInfo paymentInfo )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( "api/customer", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            var createCustomer = new CreateCustomerRequest
            {
                Description = paymentInfo.FullName,
                PaymentMethod = new PaymentMethodRequest( paymentInfo.ReferenceNumber ),
                BillingAddress = CreateBillingAddress<BillingAddress>( paymentInfo )
            };

            restRequest.AddJsonBody( createCustomer );

            var response = restClient.Execute( restRequest );

            var createCustomerResponse = ParseResponse<CustomerResponse>( response );
            return createCustomerResponse;
        }

        /// <summary>
        /// Gets the customer.
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private CustomerResponse GetCustomer( string gatewayUrl, string apiKey, string customerId )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/customer/{customerId}", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<CustomerResponse>( response );
        }

        /// <summary>
        /// Updates the customer address.
        /// https://sandbox.gotnpgateway.com/docs/api/#update-address-token-deprecated
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="referencePaymentInfo">The payment information.</param>
        /// <returns></returns>
        private UpdateCustomerAddressResponse UpdateCustomerAddress( string gatewayUrl, string apiKey, ReferencePaymentInfo referencePaymentInfo )
        {
            string customerId = referencePaymentInfo.GatewayPersonIdentifier;
            var customer = GetCustomer( gatewayUrl, apiKey, customerId );
            var billingAddressId = customer?.Data?.BillingAddress?.Id;

            UpdateCustomerAddressRequest updateCustomerAddressRequest = CreateBillingAddress<UpdateCustomerAddressRequest>( referencePaymentInfo );

            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/customer/{customerId}/address/{billingAddressId}", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( updateCustomerAddressRequest );

            var response = restClient.Execute( restRequest );

            return ParseResponse<UpdateCustomerAddressResponse>( response );
        }

        #endregion Customers

        #region Transactions

        /// <summary>
        /// Posts a transaction.
        /// https://sandbox.gotnpgateway.com/docs/api/#processing-a-transaction
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="type">The type (sale, authorize, credit)</param>
        /// <param name="referencedPaymentInfo">The referenced payment information.</param>
        /// <returns></returns>
        private CreateTransactionResponse PostTransaction( string gatewayUrl, string apiKey, TransactionType type, ReferencePaymentInfo referencedPaymentInfo )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( "api/transaction", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            var customerId = referencedPaymentInfo.GatewayPersonIdentifier;
            var tokenizerToken = referencedPaymentInfo.ReferenceNumber;
            var amount = referencedPaymentInfo.Amount;

            var transaction = new Rock.MyWell.CreateTransaction
            {
                Type = type,
                Amount = amount
            };

            transaction.IPAddress = referencedPaymentInfo.IPAddress;

            if ( customerId.IsNotNullOrWhiteSpace() )
            {
                transaction.PaymentMethodRequest = new Rock.MyWell.PaymentMethodRequest( new Rock.MyWell.PaymentMethodCustomer( customerId ) );
            }
            else
            {
                transaction.PaymentMethodRequest = new Rock.MyWell.PaymentMethodRequest( tokenizerToken );
            }

            StringBuilder stringBuilderDescription = new StringBuilder();
            if ( referencedPaymentInfo.Description.IsNotNullOrWhiteSpace() )
            {
                stringBuilderDescription.AppendLine( referencedPaymentInfo.Description );
            }

            if ( referencedPaymentInfo.Comment1.IsNotNullOrWhiteSpace() )
            {
                stringBuilderDescription.AppendLine( referencedPaymentInfo.Comment1 );
            }

            if ( referencedPaymentInfo.Comment2.IsNotNullOrWhiteSpace() )
            {
                stringBuilderDescription.AppendLine( referencedPaymentInfo.Comment2 );
            }

            transaction.Description = stringBuilderDescription.ToString().Truncate( 255 );

            transaction.BillingAddress = CreateBillingAddress<BillingAddress>( referencedPaymentInfo );
            transaction.BillingAddress.CustomerId = customerId;

            restRequest.AddJsonBody( transaction );

            var response = restClient.Execute( restRequest );

            return ParseResponse<CreateTransactionResponse>( response );
        }

        /// <summary>
        /// Parses the response or throws an exception if the response could not be parsed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Unable to parse response: {response.Content}
        /// </exception>
        private static T ParseResponse<T>( IRestResponse response )
        {
            var result = JsonConvert.DeserializeObject<T>( response.Content );

            if ( result is BaseResponseData )
            {
                ( result as BaseResponseData ).StatusCode = response.StatusCode;
            }

            if ( result == null )
            {
                if ( response.ErrorException != null )
                {
                    throw response.ErrorException;
                }
                else if ( response.ErrorMessage.IsNotNullOrWhiteSpace() )
                {
                    throw new MyWellGatewayException( response.ErrorMessage );
                }
                else
                {
                    throw new MyWellGatewayException( $"Unable to parse response: {response.Content} " );
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the transaction status.
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns></returns>
        private TransactionStatusResponse GetTransactionStatus( string gatewayUrl, string apiKey, string transactionId )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/transaction/{transactionId}", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<TransactionStatusResponse>( response );
        }

        /// <summary>
        /// Posts the void.
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns></returns>
        private TransactionVoidRefundResponse PostVoid( string gatewayUrl, string apiKey, string transactionId )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/transaction/{transactionId}/void", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<TransactionVoidRefundResponse>( response );
        }

        /// <summary>
        /// Posts the refund.
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        private TransactionVoidRefundResponse PostRefund( string gatewayUrl, string apiKey, string transactionId, decimal amount )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/transaction/{transactionId}/refund", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            var refundRequest = new TransactionRefundRequest { Amount = amount };
            restRequest.AddJsonBody( refundRequest );

            var response = restClient.Execute( restRequest );

            return ParseResponse<TransactionVoidRefundResponse>( response );
        }

        #endregion Transactions

        #region Plans

        /// <summary>
        /// Updates the billing plan BillingFrequency, BillingCycleInterval, BillingDays and Duration
        /// </summary>
        /// <param name="subscriptionRequestParameters">The subscription request parameters.</param>
        /// <param name="scheduleTransactionFrequencyValueGuid">The schedule transaction frequency value unique identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private static bool SetSubscriptionBillingPlanParameters( SubscriptionRequestParameters subscriptionRequestParameters, Guid scheduleTransactionFrequencyValueGuid, DateTime startDate, out string errorMessage )
        {
            errorMessage = string.Empty;
            BillingPlanParameters billingPlanParameters = subscriptionRequestParameters as BillingPlanParameters;

            // NOTE: Don't convert startDate to UTC, let the gateway worry about that
            billingPlanParameters.NextBillDateUTC = startDate;
            BillingFrequency? billingFrequency = null;
            int billingDuration = 0;
            int billingCycleInterval = 1;
            string billingDays = null;
            int startDayOfMonth = subscriptionRequestParameters.NextBillDateUTC.Value.Day;

            // MyWell Gateway doesn't allow Day of Month over 28, but will automatically schedule for the last day of the month if you pass in 31
            if ( startDayOfMonth > 28 )
            {
                startDayOfMonth = 31;

                // since we have to use magic 31 to indicate the last day of the month, adjust the NextBillDate to be the last day of the specified month
                // (so it doesn't post on original startDate and again on the last day of the month)
                var nextBillYear = billingPlanParameters.NextBillDateUTC.Value.Year;
                var nextBillMonth = billingPlanParameters.NextBillDateUTC.Value.Month;
                DateTime endOfMonth = new DateTime( nextBillYear, nextBillMonth, DateTime.DaysInMonth( nextBillYear, nextBillMonth ) );

                billingPlanParameters.NextBillDateUTC = endOfMonth;
            }

            if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY.AsGuid() )
            {
                billingFrequency = BillingFrequency.monthly;
                billingDays = $"{startDayOfMonth}";
            }
            else if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_FIRST_AND_FIFTEENTH.AsGuid() )
            {
                // See https://sandbox.gotnpgateway.com/docs/api/#bill-once-month-on-the-1st-and-the-15th-until-canceled.
                var twiceMonthlyDays = new int[2] { 1, 15 };
                billingFrequency = BillingFrequency.twice_monthly;

                /* 2020-07-30 MDP
                  - When setting up a 1st/15th schedule, MyWell will report the NextBillDate as whatever we tell it,
                    but it really end up posting on whatever the next 1st or 15th lands on (which is what we want to happen).
                    For example, if we set up a 1st/15th schedule to start on July 23rd, it'll report that the NextBillDate is July 23rd,
                    but it won't really bill until Aug 1st. From then on, the NextBillDate will get reported as whatever the next 1st and 15th is.
                    Even though it bills on the correct days, it can be confusing when it says that the next bill date is July 23rd.
                    To avoid that confusion, lets round the start date to the next upcoming 1st or 15th.
                 */

                var nextBillDayOfMonth = billingPlanParameters.NextBillDateUTC.Value.Day;

                if ( nextBillDayOfMonth > 1 && nextBillDayOfMonth < 15 )
                {
                    // they specifed a start between the 1st and 15th, so round up the next 15th
                    var nextFifteenth = new DateTime( billingPlanParameters.NextBillDateUTC.Value.Year, billingPlanParameters.NextBillDateUTC.Value.Month, 15 );
                    billingPlanParameters.NextBillDateUTC = nextFifteenth;
                }
                else if ( nextBillDayOfMonth > 15 )
                {
                    // they specifed a start after 15th, so round up the next month's 1st
                    var nextFirst = new DateTime( billingPlanParameters.NextBillDateUTC.Value.Year, billingPlanParameters.NextBillDateUTC.Value.Month, 1 ).AddMonths( 1 );
                    billingPlanParameters.NextBillDateUTC = nextFirst;
                }

                // twiceMonthly Days have to be in numeric order
                billingDays = $"{twiceMonthlyDays.OrderBy( a => a ).ToList().AsDelimited( "," )}";
            }
            else if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY.AsGuid() )
            {
                // See https://sandbox.gotnpgateway.com/docs/api/#bill-once-every-7-days-until-canceled.
                billingCycleInterval = 1;
                billingFrequency = BillingFrequency.daily;
                billingDays = "7";
            }
            else if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY.AsGuid() )
            {
                // See https://sandbox.gotnpgateway.com/docs/api/#bill-once-other-week-until-canceled.
                billingCycleInterval = 2;
                billingFrequency = BillingFrequency.daily;
                billingDays = "7";
            }
            else if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() )
            {
                // If ONE-TIME create a monthly subscription, but with a duration of 1 so that it only does it once.
                billingCycleInterval = 1;
                billingFrequency = BillingFrequency.monthly;
                billingDays = $"{startDayOfMonth}";
                billingDuration = 1;
            }
            else
            {
                errorMessage = $"Unsupported Schedule Frequency {DefinedValueCache.Get( scheduleTransactionFrequencyValueGuid )?.Value}";
                return false;
            }

            billingPlanParameters.BillingFrequency = billingFrequency;
            billingPlanParameters.BillingCycleInterval = billingCycleInterval;
            billingPlanParameters.BillingDays = billingDays;
            billingPlanParameters.Duration = billingDuration;
            return true;
        }

        /// <summary>
        /// Creates the plan.
        /// https://sandbox.gotnpgateway.com/docs/api/#create-a-plan
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="planParameters">The plan parameters.</param>
        /// <returns></returns>
        private CreatePlanResponse CreatePlan( string gatewayUrl, string apiKey, CreatePlanParameters planParameters )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( "api/recurring/plan", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( planParameters );
            var response = restClient.Execute( restRequest );

            return ParseResponse<CreatePlanResponse>( response );
        }

        /// <summary>
        /// Deletes the plan.
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        private string DeletePlan( string gatewayUrl, string apiKey, string planId )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/plan/{planId}", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );
            var response = restClient.Execute( restRequest );

            return response.Content;
        }

        /// <summary>
        /// Gets the plans.
        /// https://sandbox.gotnpgateway.com/docs/api/#get-all-plans
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <returns></returns>
        private GetPlansResult GetPlans( string gatewayUrl, string apiKey )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( "api/recurring/plans", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<GetPlansResult>( response );
        }

        #endregion Plans

        #region Transaction Query

        /// <summary>
        /// Returns a list of Transactions that meet the queryTransactionStatusRequest parameters
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="queryTransactionStatusRequest">The query transaction status request.</param>
        /// <returns></returns>
        private TransactionSearchResult SearchTransactions( string gatewayUrl, string apiKey, QueryTransactionStatusRequest queryTransactionStatusRequest )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( "api/transaction/search", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( queryTransactionStatusRequest );

            var response = restClient.Execute( restRequest );

            return ParseResponse<TransactionSearchResult>( response );
        }

        #endregion

        #region Subscriptions

        /// <summary>
        /// Creates the subscription.
        /// https://sandbox.gotnpgateway.com/docs/api/#create-a-subscription
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionParameters">The subscription parameters.</param>
        /// <returns></returns>
        private SubscriptionResponse CreateSubscription( string gatewayUrl, string apiKey, SubscriptionRequestParameters subscriptionParameters )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( "api/recurring/subscription", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( subscriptionParameters );
            var response = restClient.Execute( restRequest );

            return ParseResponse<SubscriptionResponse>( response );
        }

        /// <summary>
        /// Sets the subscription status.
        /// Undocumented - Email from MyWell on 6/10/2021 told us about it
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="subscriptionStatus">The subscription status.</param>
        /// <returns></returns>
        private SubscriptionResponse SetSubscriptionStatus( string gatewayUrl, string apiKey, string subscriptionId, MyWellSubscriptionStatus subscriptionStatus )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"/api/recurring/subscription/{subscriptionId}/status/{subscriptionStatus.ConvertToString( false )}", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<SubscriptionResponse>( response );
        }

        /// <summary>
        /// Updates the subscription.
        /// https://sandbox.gotnpgateway.com/docs/api/#update-a-subscription
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="subscriptionParameters">The subscription parameters.</param>
        /// <returns></returns>
        private SubscriptionResponse UpdateSubscription( string gatewayUrl, string apiKey, string subscriptionId, SubscriptionRequestParameters subscriptionParameters )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/subscription/{subscriptionId}", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( subscriptionParameters );
            var response = restClient.Execute( restRequest );

            return ParseResponse<SubscriptionResponse>( response );
        }

        /// <summary>
        /// Deletes the subscription.
        /// https://sandbox.gotnpgateway.com/docs/api/#delete-a-subscription
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns></returns>
        private SubscriptionResponse DeleteSubscription( string gatewayUrl, string apiKey, string subscriptionId )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/subscription/{subscriptionId}", Method.DELETE );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<SubscriptionResponse>( response );
        }

        /// <summary>
        /// Gets the subscription.
        /// https://sandbox.gotnpgateway.com/docs/api/#get-a-subscription
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns></returns>
        private SubscriptionResponse GetSubscription( string gatewayUrl, string apiKey, string subscriptionId )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/subscription/{subscriptionId}", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<SubscriptionResponse>( response );
        }

        /// <summary>
        /// Searches the subscriptions. Leave customerId null to search for all.
        /// (undocumented as of 4/15/2019) /recurring/subscription/search
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public SubscriptionsSearchResult SearchCustomerSubscriptions( FinancialGateway financialGateway, string customerId )
        {
            string gatewayUrl = this.GetGatewayUrl( financialGateway );
            string apiKey = this.GetPrivateApiKey( financialGateway );

            var queryCustomerSubscriptionsRequest = new QueryCustomerSubscriptionsRequest( customerId );

            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/subscription/search", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( queryCustomerSubscriptionsRequest );

            var response = restClient.Execute( restRequest );

            return ParseResponse<SubscriptionsSearchResult>( response );
        }

        #endregion Subscriptions

        #endregion MyWellGateway Rock wrappers

        #region Exceptions

        /// <summary>
        ///
        /// </summary>
        /// <seealso cref="System.Exception" />
        public class ReferencePaymentInfoRequired : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReferencePaymentInfoRequired"/> class.
            /// </summary>
            public ReferencePaymentInfoRequired()
                : base( "MyWellGateway requires a token or customer reference" )
            {
            }
        }

        #endregion

        #region GatewayComponent implementation

        /// <summary>
        /// Gets the supported payment schedules.
        /// </summary>
        /// <value>
        /// The supported payment schedules.
        /// </value>
        public override List<DefinedValueCache> SupportedPaymentSchedules
        {
            get
            {
                var values = new List<DefinedValueCache>();
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_FIRST_AND_FIFTEENTH ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY ) );
                return values;
            }
        }

        /// <summary>
        /// Charges the specified payment info.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="ReferencePaymentInfoRequired"></exception>
        public override FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;
            var referencedPaymentInfo = paymentInfo as ReferencePaymentInfo;
            if ( referencedPaymentInfo == null )
            {
                throw new ReferencePaymentInfoRequired();
            }

            var response = this.PostTransaction( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), TransactionType.sale, referencedPaymentInfo );
            if ( !response.IsSuccessStatus() )
            {
                errorMessage = response.Message;

                // Write decline/error as an exception.
                var exception = new MyWellGatewayException( $"Error processing MyWell transaction. Message:  {response.Message}, " +
                    $"processorResponseCode: {response.ProcessorResponseCode}, " +
                    $"processorResponseText: {response.ProcessorResponseText} " );

                ExceptionLogService.LogException( exception );

                return null;
            }

            var financialTransaction = new FinancialTransaction();
            financialTransaction.TransactionCode = response.Data.Id;
            financialTransaction.FinancialPaymentDetail = PopulatePaymentInfo( paymentInfo, response.Data?.PaymentMethodResponse, response.Data?.BillingAddress );

            return financialTransaction;
        }

        /// <summary>
        /// Populates the FinancialPaymentDetail record for a FinancialTransaction or FinancialScheduledTransaction
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="paymentMethodResponse">The payment method response.</param>
        /// <param name="billingAddressResponse">The billing address response.</param>
        /// <returns></returns>
        private static FinancialPaymentDetail PopulatePaymentInfo( PaymentInfo paymentInfo, PaymentMethodResponse paymentMethodResponse, BillingAddress billingAddressResponse )
        {
            FinancialPaymentDetail financialPaymentDetail = new FinancialPaymentDetail();
            if ( billingAddressResponse != null )
            {
                // Since we are using a token for payment, it is possible that the Gateway has a different address associated with the payment method.
                financialPaymentDetail.NameOnCard = $"{billingAddressResponse.FirstName} {billingAddressResponse.LastName}";

                // If address wasn't collected when entering the transaction, set the address to the billing info returned from the gateway (if any).
                if ( paymentInfo.Street1.IsNullOrWhiteSpace() )
                {
                    if ( billingAddressResponse.AddressLine1.IsNotNullOrWhiteSpace() )
                    {
                        paymentInfo.Street1 = billingAddressResponse.AddressLine1;
                        paymentInfo.Street2 = billingAddressResponse.AddressLine2;
                        paymentInfo.City = billingAddressResponse.City;
                        paymentInfo.State = billingAddressResponse.State;
                        paymentInfo.PostalCode = billingAddressResponse.PostalCode;
                        paymentInfo.Country = billingAddressResponse.Country;
                    }
                }
            }

            var creditCardResponse = paymentMethodResponse?.Card;
            var achResponse = paymentMethodResponse?.ACH;
            financialPaymentDetail.GatewayPersonIdentifier = ( paymentInfo as ReferencePaymentInfo )?.GatewayPersonIdentifier;
            financialPaymentDetail.FinancialPersonSavedAccountId = ( paymentInfo as ReferencePaymentInfo )?.FinancialPersonSavedAccountId;

            if ( creditCardResponse != null )
            {
                financialPaymentDetail.CurrencyTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
                financialPaymentDetail.AccountNumberMasked = creditCardResponse.MaskedCard;

                if ( creditCardResponse.ExpirationDate?.Length == 5 )
                {
                    financialPaymentDetail.ExpirationMonth = creditCardResponse.ExpirationDate.Substring( 0, 2 ).AsIntegerOrNull();
                    financialPaymentDetail.ExpirationYear = creditCardResponse.ExpirationDate.Substring( 3, 2 ).AsIntegerOrNull();
                }

                //// The gateway tells us what the CreditCardType is since it was selected using their hosted payment entry frame.
                //// So, first see if we can determine CreditCardTypeValueId using the CardType response from the gateway

                // See if we can figure it out from the CC Type (Amex, Visa, etc)
                var creditCardTypeValue = CreditCardPaymentInfo.GetCreditCardTypeFromName( creditCardResponse.CardType );
                if ( creditCardTypeValue == null )
                {
                    // GetCreditCardTypeFromName should have worked, but just in case, see if we can figure it out from the MaskedCard using RegEx
                    creditCardTypeValue = CreditCardPaymentInfo.GetCreditCardTypeFromCreditCardNumber( creditCardResponse.MaskedCard );
                }

                financialPaymentDetail.CreditCardTypeValueId = creditCardTypeValue?.Id;
            }
            else if ( achResponse != null )
            {
                financialPaymentDetail.CurrencyTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );
                financialPaymentDetail.AccountNumberMasked = achResponse.MaskedAccountNumber;
            }

            return financialPaymentDetail;
        }

        /// <summary>
        /// Credits (Refunds) the specified transaction.
        /// </summary>
        /// <param name="origTransaction">The original transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Credit( FinancialTransaction origTransaction, decimal amount, string comment, out string errorMessage )
        {
            if ( origTransaction == null || origTransaction.TransactionCode.IsNullOrWhiteSpace() || origTransaction.FinancialGateway == null )
            {
                errorMessage = "Invalid original transaction, transaction code, or gateway.";
                return null;
            }

            var transactionId = origTransaction.TransactionCode;
            FinancialGateway financialGateway = origTransaction.FinancialGateway;

            var transactionStatus = this.GetTransactionStatus( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), transactionId );
            var transactionStatusTransaction = transactionStatus.Data.FirstOrDefault( a => a.Id == transactionId );

            // See https://sandbox.gotnpgateway.com/docs/api/#refund.
            // NOTE: If the transaction isn't settled yet, this will return an error. But that's OK.
            TransactionVoidRefundResponse response = this.PostRefund( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), transactionId, amount );

            if ( response.IsSuccessStatus() )
            {
                var transaction = new FinancialTransaction();
                transaction.TransactionCode = response.Data.Id;
                errorMessage = string.Empty;
                return transaction;
            }

            errorMessage = response.Message;
            return null;
        }

        /// <summary>
        /// Adds the scheduled payment.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="ReferencePaymentInfoRequired"></exception>
        public override FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            // Create a guid to include in the MyWell Subscription Description so that we can refer back to it to ensure an orphaned subscription doesn't exist when an exception occurs.
            var descriptionGuid = Guid.NewGuid();

            var referencedPaymentInfo = paymentInfo as ReferencePaymentInfo;
            if ( referencedPaymentInfo == null )
            {
                throw new ReferencePaymentInfoRequired();
            }

            var customerId = referencedPaymentInfo.GatewayPersonIdentifier;
            string subscriptionDescription = $"{referencedPaymentInfo.Description}|Subscription Ref: {descriptionGuid}";

            try
            {
                errorMessage = string.Empty;

                SubscriptionRequestParameters subscriptionParameters = new SubscriptionRequestParameters
                {
                    Customer = new SubscriptionCustomer { Id = customerId },
                    PlanId = null,
                    Description = subscriptionDescription,
                    Duration = 0,
                    Amount = paymentInfo.Amount
                };

                string subscriptionId;

                if ( SetSubscriptionBillingPlanParameters( subscriptionParameters, schedule.TransactionFrequencyValue.Guid, schedule.StartDate, out errorMessage ) )
                {
                    var subscriptionResult = this.CreateSubscription( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), subscriptionParameters );
                    subscriptionId = subscriptionResult.Data?.Id;

                    if ( subscriptionId.IsNullOrWhiteSpace() )
                    {
                        // Error from CreateSubscription.
                        errorMessage = subscriptionResult.Message;
                        return null;
                    }
                }
                else
                {
                    // Error from SetSubscriptionBillingPlanParameters.
                    return null;
                }

                // Set the paymentInfo.TransactionCode to the subscriptionId so that we know what CreateSubsciption created.
                // This might be handy in case we have an exception and need to know what the subscriptionId is.
                referencedPaymentInfo.TransactionCode = subscriptionId;

                var scheduledTransaction = new FinancialScheduledTransaction();
                scheduledTransaction.TransactionCode = customerId;
                scheduledTransaction.GatewayScheduleId = subscriptionId;
                scheduledTransaction.FinancialGatewayId = financialGateway.Id;

                CustomerResponse customerInfo;
                try
                {
                    customerInfo = this.GetCustomer( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), customerId );

                    if ( referencedPaymentInfo.IncludesAddressData() )
                    {
                        var updateCustomerAddressResponse = this.UpdateCustomerAddress( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), referencedPaymentInfo );
                    }
                }
                catch ( Exception ex )
                {
                    throw new MyWellGatewayException( $"Exception getting Customer Information for Scheduled Payment.", ex );
                }

                scheduledTransaction.FinancialPaymentDetail = PopulatePaymentInfo( paymentInfo, customerInfo?.Data?.PaymentMethod, customerInfo?.Data?.BillingAddress );
                try
                {
                    GetScheduledPaymentStatus( scheduledTransaction, out errorMessage );
                }
                catch ( Exception ex )
                {
                    throw new MyWellGatewayException( $"Exception getting Scheduled Payment Status. {errorMessage}", ex );
                }

                return scheduledTransaction;
            }
            catch ( Exception )
            {
                // If there is an exception, Rock won't save this as a scheduled transaction, so make sure the subscription didn't get created so mystery scheduled transactions don't happen.
                var subscriptionSearchResult = this.SearchCustomerSubscriptions( financialGateway, customerId );
                var orphanedSubscription = subscriptionSearchResult?.Data?.FirstOrDefault( a => a.Description == subscriptionDescription );

                if ( orphanedSubscription?.Id != null )
                {
                    var subscriptionId = orphanedSubscription.Id;
                    var subscriptionResult = this.DeleteSubscription( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), subscriptionId );
                }

                throw;
            }
        }

        internal static Rock.Model.FinancialScheduledTransactionStatus? GetFinancialScheduledTransactionStatus( MyWellSubscriptionStatus? subscriptionStatus )
        {
            if ( subscriptionStatus == null )
            {
                return null;
            }

            switch ( subscriptionStatus )
            {
                case MyWellSubscriptionStatus.active:
                    return FinancialScheduledTransactionStatus.Active;
                case MyWellSubscriptionStatus.canceled:
                    return FinancialScheduledTransactionStatus.Canceled;
                case MyWellSubscriptionStatus.completed:
                    return FinancialScheduledTransactionStatus.Completed;
                case MyWellSubscriptionStatus.failed:
                    return FinancialScheduledTransactionStatus.Failed;
                case MyWellSubscriptionStatus.past_due:
                    return FinancialScheduledTransactionStatus.PastDue;
                case MyWellSubscriptionStatus.paused:
                    return FinancialScheduledTransactionStatus.Paused;
                default:
                    return subscriptionStatus.ConvertToString( false ).ConvertToEnumOrNull<FinancialScheduledTransactionStatus>();
            }
        }

        /// <summary>
        /// Updates the scheduled payment.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool UpdateScheduledPayment( FinancialScheduledTransaction scheduledTransaction, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            var referencedPaymentInfo = paymentInfo as ReferencePaymentInfo;
            if ( referencedPaymentInfo == null )
            {
                throw new ReferencePaymentInfoRequired();
            }

            var subscriptionId = scheduledTransaction.GatewayScheduleId;

            SubscriptionRequestParameters subscriptionParameters = new SubscriptionRequestParameters
            {
                Duration = 0,
                Amount = referencedPaymentInfo.Amount
            };

            if ( referencedPaymentInfo.GatewayPersonIdentifier.IsNotNullOrWhiteSpace() )
            {
                subscriptionParameters.Customer = new SubscriptionCustomer { Id = referencedPaymentInfo.GatewayPersonIdentifier };
            }

            var transactionFrequencyGuid = DefinedValueCache.Get( scheduledTransaction.TransactionFrequencyValueId ).Guid;

            FinancialGateway financialGateway;
            string gatewayUrl;
            string apiKey;

            if ( SetSubscriptionBillingPlanParameters( subscriptionParameters, transactionFrequencyGuid, scheduledTransaction.StartDate, out errorMessage ) )
            {
                financialGateway = scheduledTransaction.FinancialGateway;

                gatewayUrl = this.GetGatewayUrl( financialGateway );
                apiKey = this.GetPrivateApiKey( financialGateway );

                if ( subscriptionParameters.Customer?.Id == null || referencedPaymentInfo.GatewayPersonIdentifier.IsNullOrWhiteSpace() )
                {
                    // If the GatewayPersonIdentifier wasn't known to Rock, get the CustomerId from MyWellGateway.
                    var subscription = GetSubscription( gatewayUrl, apiKey, subscriptionId );
                    referencedPaymentInfo.GatewayPersonIdentifier = subscription?.Data.Customer?.Id;
                    subscriptionParameters.Customer = new SubscriptionCustomer { Id = referencedPaymentInfo.GatewayPersonIdentifier };
                }

                SubscriptionResponse subscriptionResult;
                var subscriptionStatusResult = this.GetSubscription( gatewayUrl, apiKey, subscriptionId );
                if ( subscriptionStatusResult?.Data?.SubscriptionStatus != MyWellSubscriptionStatus.active )
                {
                    // If subscription isn't active (it might be cancelled due to expired card),
                    // change the status back to active
                    var setSubscriptionStatusResult = this.SetSubscriptionStatus( gatewayUrl, apiKey, subscriptionId, MyWellSubscriptionStatus.active );

                    if ( !setSubscriptionStatusResult.IsSuccessStatus() )
                    {
                        // Write decline/error as an exception.
                        // Note: MyWell doesn't include processor errors when creating subscriptions, probably because the processor doesn't do anything until the transactions are charged.
                        var exception = new MyWellGatewayException( $"Error re-activating MyWell subscription. Message:  {setSubscriptionStatusResult.Message} " );

                        ExceptionLogService.LogException( exception );

                        errorMessage = setSubscriptionStatusResult.Message;

                        return false;
                    }
                }

                subscriptionResult = this.UpdateSubscription( gatewayUrl, apiKey, subscriptionId, subscriptionParameters );

                if ( !subscriptionResult.IsSuccessStatus() )
                {
                    // Write decline/error as an exception.
                    // Note: MyWell doesn't include processor errors when creating subscriptions, probably because the processor doesn't do anything until the transactions are charged.
                    var exception = new MyWellGatewayException( $"Error processing MyWell subscription. Message:  {subscriptionResult.Message} " );

                    ExceptionLogService.LogException( exception );
                    errorMessage = subscriptionResult.Message;

                    return false;
                }

                subscriptionId = subscriptionResult?.Data?.Id;

                if ( subscriptionId != scheduledTransaction.GatewayScheduleId )
                {
                    // Shouldn't happen, but just in case...
                    if ( scheduledTransaction.PreviousGatewayScheduleIds == null )
                    {
                        scheduledTransaction.PreviousGatewayScheduleIds = new List<string>();
                    }

                    scheduledTransaction.PreviousGatewayScheduleIds.Add( scheduledTransaction.GatewayScheduleId );

                    referencedPaymentInfo.TransactionCode = subscriptionId;
                    scheduledTransaction.GatewayScheduleId = subscriptionId;
                }
            }
            else
            {
                // Error from SetSubscriptionBillingPlanParameters
                return false;
            }

            if ( referencedPaymentInfo.IncludesAddressData() )
            {
                var updateCustomerAddressResponse = this.UpdateCustomerAddress( gatewayUrl, apiKey, referencedPaymentInfo );
                if ( !updateCustomerAddressResponse.IsSuccessStatus() )
                {
                    errorMessage = updateCustomerAddressResponse.Message;
                    return false;
                }
            }

            var customerId = referencedPaymentInfo.GatewayPersonIdentifier;

            CustomerResponse customerInfo;
            try
            {
                customerInfo = this.GetCustomer( gatewayUrl, this.GetPrivateApiKey( financialGateway ), customerId );
            }
            catch ( Exception ex )
            {
                throw new MyWellGatewayException( $"Exception getting Customer Information for Scheduled Payment", ex );
            }

            scheduledTransaction.FinancialPaymentDetail = PopulatePaymentInfo( paymentInfo, customerInfo?.Data?.PaymentMethod, customerInfo?.Data?.BillingAddress );
            scheduledTransaction.TransactionCode = customerId;

            try
            {
                GetScheduledPaymentStatus( scheduledTransaction, out errorMessage );
            }
            catch ( Exception ex )
            {
                throw new MyWellGatewayException( $"Exception getting Scheduled Payment Status. {errorMessage}", ex );
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Cancels the scheduled payment.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool CancelScheduledPayment( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            var subscriptionId = scheduledTransaction.GatewayScheduleId;

            FinancialGateway financialGateway = scheduledTransaction.FinancialGateway;

            var subscriptionResult = this.DeleteSubscription( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), subscriptionId );
            if ( subscriptionResult.IsSuccessStatus() )
            {
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                if ( subscriptionResult.StatusCode == System.Net.HttpStatusCode.NotFound || subscriptionResult.StatusCode == System.Net.HttpStatusCode.Forbidden )
                {
                    // Assume that status code of Forbidden or NonFound indicates that the schedule doesn't exist, or was deleted.
                    errorMessage = string.Empty;
                    return true;
                }

                errorMessage = subscriptionResult.Message;
                return false;
            }
        }

        /// <summary>
        /// Flag indicating if gateway supports reactivating a scheduled payment.
        /// </summary>
        public override bool ReactivateScheduledPaymentSupported => false;

        /// <summary>
        /// Reactivates the scheduled payment.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool ReactivateScheduledPayment( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            errorMessage = "The payment gateway associated with this scheduled transaction (MyWell) does not support reactivating scheduled transactions. A new scheduled transaction should be created instead.";
            return false;
        }

        /// <summary>
        /// Gets the scheduled payment status.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool GetScheduledPaymentStatus( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            var subscriptionId = scheduledTransaction.GatewayScheduleId;

            FinancialGateway financialGateway = scheduledTransaction.FinancialGateway;
            if ( financialGateway == null && scheduledTransaction.FinancialGatewayId.HasValue )
            {
                financialGateway = new FinancialGatewayService( new Rock.Data.RockContext() ).GetNoTracking( scheduledTransaction.FinancialGatewayId.Value );
            }

            var subscriptionResult = this.GetSubscription( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), subscriptionId );
            if ( subscriptionResult.IsSuccessStatus() )
            {
                var subscriptionInfo = subscriptionResult.Data;
                if ( subscriptionInfo != null )
                {
                    var gatewayNextBillDate = subscriptionInfo.NextBillDateUTC?.Date;
                    if ( gatewayNextBillDate.HasValue )
                    {
                        // Rock DateTimes don't keep any TimeZone or offset, so make sure the date is DateTimeKind.Unspecified instead of UTC.
                        // Note that the DateTime stored to the database will get the DateTimeKind stripped off, so this is only issue for DateTime data
                        // that isn't saved to the database yet.
                        gatewayNextBillDate = DateTime.SpecifyKind( gatewayNextBillDate.Value, DateTimeKind.Unspecified );
                    }

                    scheduledTransaction.NextPaymentDate = gatewayNextBillDate;
                    scheduledTransaction.FinancialPaymentDetail.GatewayPersonIdentifier = subscriptionInfo.Customer?.Id;
                    scheduledTransaction.StatusMessage = subscriptionInfo.SubscriptionStatusRaw;
                    scheduledTransaction.Status = GetFinancialScheduledTransactionStatus( subscriptionInfo.SubscriptionStatus );
                }

                scheduledTransaction.LastStatusUpdateDateTime = RockDateTime.Now;

                errorMessage = string.Empty;
                return true;
            }
            else
            {
                if ( subscriptionResult.StatusCode == System.Net.HttpStatusCode.NotFound || subscriptionResult.StatusCode == System.Net.HttpStatusCode.Forbidden )
                {
                    // Assume that a status code of Forbidden or NonFound indicates that the schedule doesn't exist, or was deleted.
                    scheduledTransaction.IsActive = false;
                    errorMessage = string.Empty;
                    return true;
                }

                errorMessage = subscriptionResult.Message;
                return false;
            }
        }

        /// <summary>
        /// Gets the payments that have been processed for any scheduled transactions
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override List<Payment> GetPayments( FinancialGateway financialGateway, DateTime startDateTime, DateTime endDateTime, out string errorMessage )
        {
            QueryTransactionStatusRequest queryTransactionStatusRequest = new QueryTransactionStatusRequest
            {
                DateTimeRangeUTC = new QueryDateTimeRange( startDateTime, endDateTime ),

                /*
                 04/13/2022 MDP

                We only care about 'Sale' transaction. Here is why
                - Scheduled Transactions would normally be 'sale' transactions. Rock wouldn't have recorded these yet since the Gateway does the transaction according to the schedule. If the Gateway
                   ends up doing a 'sale' transaction due the scheduled transaction, then we want to know about it. If it was a scheduled transaction that is somehow a 'credit/refund/void', then we don't want it.

                - 'Sale' transactions could also be one time transactions (not scheduled). We already have those recorded, but we want to know if the settle status has changed. Or if somehow ended up rejected.

                - Any Refunds/Voids/Credits that were initialized by Rock would already be recorded as a FinancialTransaction in Rock. Since we know about those already, we don't need to get those from a Gateway. We also don't want
                them because Rock might not know what do with them and would record it as a new transactions. Resulting in duplicates.

                */

                // Only search for transactions that were a 'sale' (see above engineering note)
                TransactionTypeSearch = new QuerySearchTransactionType( TransactionType.sale )
            };

            List<TransactionQueryResultData> transactionQueryResultList = new List<TransactionQueryResultData>();
            bool getMoreTransactions = true;
            int offset = 0;
            errorMessage = string.Empty;

            // NOTE 2500 is the max that MyWell supports
            int fetchLimit = 2500;
            var paymentList = new List<Payment>();

            while ( getMoreTransactions )
            {
                queryTransactionStatusRequest.Limit = fetchLimit;
                queryTransactionStatusRequest.Offset = offset;
                TransactionSearchResult searchResult = this.SearchTransactions( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), queryTransactionStatusRequest );
                int expectedTotal = searchResult?.TotalCount ?? 0;

                if ( !searchResult.IsSuccessStatus() )
                {
                    errorMessage = searchResult.Message;
                    return null;
                }

                var downloadedTransactions = searchResult.Data ?? new TransactionQueryResultData[0];
                var actualDownloadCount = downloadedTransactions.Count();

                transactionQueryResultList.AddRange( downloadedTransactions );

                errorMessage = string.Empty;
                if ( !downloadedTransactions.Any() || actualDownloadCount < fetchLimit )
                {
                    getMoreTransactions = false;
                }

                offset += fetchLimit;
            }

            if ( !transactionQueryResultList.Any() )
            {
                // If no payments were found for the date range, searchResult.Data will be null,
                // so just return an empty paymentList.
                return paymentList;
            }
            

            foreach ( var transaction in transactionQueryResultList )
            {
                if ( !transaction.TransactionType.HasValue || ( transaction.TransactionType != TransactionType.sale ) )
                {
                    // We limited our search request to 'sale' transaction, but if we somehow got a transaction that wasn't a 'sale',
                    // skip it (see above engineering note)
                    continue;
                }

                var gatewayScheduleId = transaction.SubscriptionId;
                var payment = new Payment
                {
                    TransactionCode = transaction.Id,
                    Amount = transaction.Amount,

                    // We want datetimes that are stored in Rock to be in LocalTime, to convert from UTC to Local.
                    TransactionDateTime = transaction.CreatedDateTimeUTC.Value.ToLocalTime(),
                    GatewayScheduleId = gatewayScheduleId,
                    GatewayPersonIdentifier = transaction.CustomerId,

                    Status = transaction.Status,
                    IsFailure = transaction.IsFailure(),
                    IsSettled = transaction.IsSettled(),
                    SettledGroupId = transaction.SettlementBatchId,

                    // We want datetimes that are stored in Rock to be in LocalTime, to convert from UTC to Local.
                    SettledDate = transaction.SettledDateTimeUTC?.ToLocalTime(),
                    StatusMessage = transaction.Response,

                    //// NOTE on unpopulated fields:
                    //// CurrencyTypeValue and CreditCardTypeValue are determined by the FinanancialPaymentDetail of the ScheduledTransaction
                    //// ScheduleActive doesn't apply because MyWell subscriptions are either active or deleted (don't exist).
                    ////   - GetScheduledPaymentStatus will take care of setting ScheduledTransaction.IsActive to false
                    //// SettledGroupId isn't included in the response from MyWell (this is an open issue)
                    //// NameOnCard, ExpirationMonth, ExpirationYear are set when the FinancialScheduledTransaction record is created
                };

                if ( transaction.PaymentType == "ach" )
                {
                    payment.AccountNumberMasked = transaction?.PaymentMethodResponse?.ACH?.MaskedAccountNumber;
                }
                else
                {
                    payment.AccountNumberMasked = transaction?.PaymentMethodResponse?.Card?.MaskedCard;
                }

                paymentList.Add( payment );
            }

            return paymentList;
        }

        /// <summary>
        /// Gets an optional reference number needed to process future transaction from saved account.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override string GetReferenceNumber( FinancialTransaction transaction, out string errorMessage )
        {
            errorMessage = string.Empty;
            return transaction?.FinancialPaymentDetail?.GatewayPersonIdentifier;
        }

        /// <summary>
        /// Gets an optional reference number needed to process future transaction from saved account.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override string GetReferenceNumber( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            errorMessage = string.Empty;
            return scheduledTransaction?.FinancialPaymentDetail?.GatewayPersonIdentifier ?? scheduledTransaction.TransactionCode;
        }

        #endregion GatewayComponent implementation

        #region My Well Specific public methods

        /// <summary>
        /// Removes the emails.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="onProgress">The on progress.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Unexpected response from SearchCustomerSubscriptions</exception>
        public int RemoveEmails( FinancialGateway financialGateway, EventHandler<string> onProgress )
        {
            var emailRemoveCount = 0;
            var progressCount = 0;
            var subscriptionList = this.SearchCustomerSubscriptions( financialGateway, null )?.Data.ToList();
            var gatewayUrl = this.GetGatewayUrl( financialGateway );
            var apiKey = this.GetPrivateApiKey( financialGateway );
            if ( subscriptionList == null )
            {
                throw new MyWellGatewayException( "Unexpected response from SearchCustomerSubscriptions " );
            }

            var customerIds = subscriptionList.Select( a => a.Customer?.Id ).Where( a => a != null ).ToList();
            var progressTotal = customerIds.Count();
            foreach ( var customerId in customerIds )
            {
                var customer = this.GetCustomer( gatewayUrl, apiKey, customerId );
                if ( customer?.Data?.BillingAddress?.Email.IsNotNullOrWhiteSpace() == true )
                {
                    var billingAddressId = customer.Data.BillingAddress.Id;

                    var customerBillingAddress = customer.Data.BillingAddress;
                    customerBillingAddress.Email = string.Empty;

                    var restClient = new RestClient( gatewayUrl );
                    RestRequest restRequest = new RestRequest( $"api/customer/{customerId}/address/{billingAddressId}", Method.POST );
                    restRequest.AddHeader( "Authorization", apiKey );

                    restRequest.AddJsonBody( customerBillingAddress );

                    var restResponse = restClient.Execute( restRequest );

                    UpdateCustomerAddressResponse updateCustomerAddressResponse = ParseResponse<UpdateCustomerAddressResponse>( restResponse );
                    if ( updateCustomerAddressResponse.IsSuccessStatus() == true )
                    {
                        emailRemoveCount++;
                    }
                }

                progressCount++;

                if ( progressTotal > 0 )
                {
                    var progressPercent = Math.Round( decimal.Divide( progressCount * 100, progressTotal ), 2 );
                    onProgress?.Invoke( this, string.Format( $"Updated {emailRemoveCount} emails. {progressPercent}%" ) );
                }
            }

            return emailRemoveCount;
        }

        #endregion
    }
}

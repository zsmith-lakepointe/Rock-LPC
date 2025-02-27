//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// <copyright>
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
using System.Linq;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Entities
{
    /// <summary>
    /// FinancialPaymentDetail View Model
    /// </summary>
    public partial class FinancialPaymentDetailBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Masked Account Number (Last 4 of Account Number prefixed with 12 *'s)
        /// </summary>
        /// <value>
        /// The account number masked.
        /// </value>
        public string AccountNumberMasked { get; set; }

        /// <summary>
        /// Gets or sets the billing location identifier.
        /// </summary>
        /// <value>
        /// The billing location identifier.
        /// </value>
        public int? BillingLocationId { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the credit card type Rock.Model.DefinedValue indicating the credit card brand/type that was used
        /// to make this transaction. This value will be null for transactions that were not made by credit card.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the DefinedValueId of the credit card type Rock.Model.DefinedValue that was used to make this transaction.
        /// This value will be null for transactions that were not made by credit card.
        /// </value>
        public int? CreditCardTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the currency type Rock.Model.DefinedValue indicating the currency that the
        /// transaction was made in.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the DefinedValueId of the CurrencyType Rock.Model.DefinedValue for this transaction.
        /// </value>
        public int? CurrencyTypeValueId { get; set; }

        /// <summary>
        /// Gets the expiration month
        /// </summary>
        /// <value>
        /// The expiration month.
        /// </value>
        public int? ExpirationMonth { get; set; }

        /// <summary>
        /// Gets the 4 digit year
        /// </summary>
        /// <value>
        /// The expiration year.
        /// </value>
        public int? ExpirationYear { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.FinancialPersonSavedAccount id that was used for this transaction (if there was one)
        /// </summary>
        /// <value>
        /// The financial person saved account.
        /// </value>
        public int? FinancialPersonSavedAccountId { get; set; }

        /// <summary>
        /// Gets or sets the Gateway Person Identifier.
        /// This would indicate id the customer vault information on the gateway.
        /// </summary>
        /// <value>
        /// A System.String representing the Gateway Person Identifier of the account.
        /// </value>
        public string GatewayPersonIdentifier { get; set; }

        /// <summary>
        /// Gets the name on card.
        /// </summary>
        /// <value>
        /// The name on card.
        /// </value>
        public string NameOnCard { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created by person alias identifier.
        /// </summary>
        /// <value>
        /// The created by person alias identifier.
        /// </value>
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the modified by person alias identifier.
        /// </summary>
        /// <value>
        /// The modified by person alias identifier.
        /// </value>
        public int? ModifiedByPersonAliasId { get; set; }

    }
}

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
    /// FinancialPledge View Model
    /// </summary>
    public partial class FinancialPledgeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the AccountId of the Rock.Model.FinancialAccount that the pledge is directed toward.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the AccountId of the Rock.Model.FinancialAccount that the pledge is directed toward.
        /// </value>
        public int? AccountId { get; set; }

        /// <summary>
        /// Gets or sets the end date of the pledge period.
        /// </summary>
        /// <value>
        /// A System.DateTime representing the end date of the pledge period.
        /// </value>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// If a person belongs to one or more groups a particular type (i.e. Family), this field 
        /// is used to distinguish which group the pledge should be associated with.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the pledge frequency Rock.Model.DefinedValue representing how often the pledgor is promising to give a portion of the pledge amount.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the pledge frequency Rock.Model.DefinedValue.
        /// </value>
        public int? PledgeFrequencyValueId { get; set; }

        /// <summary>
        /// Gets or sets the start date of the pledge period.
        /// </summary>
        /// <value>
        /// A System.DateTime representing the start date of the pledge period.
        /// </value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the pledge amount that is promised to be given.
        /// </summary>
        /// <value>
        /// A System.Decimal representing the total amount to be pledged.
        /// </value>
        public decimal TotalAmount { get; set; }

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

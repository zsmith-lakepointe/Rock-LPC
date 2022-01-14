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

namespace Rock.SparkData.Api
{
    public sealed partial class SparkDataApi
    {
        /// <summary>
        /// Spark Data account status
        /// </summary>
        public enum AccountStatus
        {
            /// <summary>
            /// The account is enabled and there is a valid credit card associated with the account
            /// </summary>
            EnabledCard,
            /// <summary>
            /// The account is enabled and there is no credit card associated with the account
            /// </summary>
            EnabledNoCard,
            /// <summary>
            /// The account is enabled and the credit card associated with the account have expired
            /// </summary>
            EnabledCardExpired,
            /// <summary>
            /// The account is enabled and the credit card associated with the account have no expire date
            /// </summary>
            EnabledCardNoExpirationDate,
            /// <summary>
            /// The account is disabled
            /// </summary>
            Disabled,
            /// <summary>
            /// The account have no name
            /// </summary>
            AccountNoName,
            /// <summary>
            /// The account was not found
            /// </summary>
            AccountNotFound,
            /// <summary>
            /// Invalid spark data key
            /// </summary>
            InvalidSparkDataKey
        }
    }
}

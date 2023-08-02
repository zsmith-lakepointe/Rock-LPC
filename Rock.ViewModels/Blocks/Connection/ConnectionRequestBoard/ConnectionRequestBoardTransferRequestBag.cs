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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard
{
    /// <summary>
    /// A bag that contains information about a connection request that should be transferred.
    /// </summary>
    public class ConnectionRequestBoardTransferRequestBag
    {
        /// <summary>
        /// Gets or sets the selected connection request identifier.
        /// </summary>
        public int ConnectionRequestId { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity to which this connection request should be transferred.
        /// </summary>
        public ListItemBag ConnectionOpportunity { get; set; }

        /// <summary>
        /// Gets or sets the connection status to which this connection request should be transferred.
        /// </summary>
        public ListItemBag ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the "connector" person to whom this connection request should be transferred.
        /// </summary>
        public ListItemBag Connector { get; set; }
    }
}

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

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard
{
    /// <summary>
    /// The box that contains all the initialization information for the Connection Request Board block.
    /// </summary>
    public class ConnectionRequestBoardInitializationBox : BlockBox
    {
        /// <summary>
        /// The connection types that are available to be selected within the opportunities sidebar.
        /// </summary>
        public List<ConnectionRequestBoardConnectionTypeBag> ConnectionTypes { get; set; }

        /// <summary>
        /// Gets or sets the selected connection opportunity.
        /// </summary>
        public ConnectionRequestBoardConnectionOpportunityBag ConnectionOpportunity { get; set; }

        /// <summary>
        /// Gets or sets the selected connection request identifier, if a specific request should be opened.
        /// </summary>
        public int? ConnectionRequestId { get; set; }

        /// <summary>
        /// Gets or sets the filter options that can be used to filter connection requests.
        /// </summary>
        public ConnectionRequestBoardFilterOptionsBag FilterOptions { get; set; }

        /// <summary>
        /// Gets or sets the selected filters to be used to initialize the connection request board.
        /// </summary>
        public ConnectionRequestBoardFiltersBag Filters { get; set; }

        /// <summary>
        /// Gets or sets whether the board is in card view mode.
        /// </summary>
        public bool IsCardViewMode { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of cards that should be displayed per column in card view mode.
        /// </summary>
        public int MaxCardsPerColumn { get; set; }

        /// <summary>
        /// Gets or sets the status icons template that should be used at the top of each connection request card (in card view mode),
        /// the first column of each row (in grid view mode) + the top of the connection request modal.
        /// </summary>
        public string StatusIconsTemplate { get; set; }
    }
}

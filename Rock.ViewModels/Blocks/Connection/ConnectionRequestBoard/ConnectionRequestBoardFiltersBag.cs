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

using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard
{
    /// <summary>
    /// A bag that contains filters information for the connection request board.
    /// </summary>
    public class ConnectionRequestBoardFiltersBag
    {
        /// <summary>
        /// Gets or sets the person alias identifier of the "connector" to be used to filter connection requests.
        /// </summary>
        public int? ConnectorPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier of the "requester" to be used to filter connection requests.
        /// </summary>
        public int? RequesterPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier to be used to filter various aspects of the connection request board.
        /// </summary>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the date range to be used to filter connection requests (using the last activity date).
        /// </summary>
        public SlidingDateRangeBag DateRange { get; set; }

        /// <summary>
        /// Gets or sets whether to only include connection requests that are "due today or already past due".
        /// </summary>
        public bool? PastDueOnly { get; set; }

        /// <summary>
        /// Gets or sets the available connection statuses that can be used to filter connection requests.
        /// </summary>
        public List<ListItemBag> AvailableStatuses { get; set; }

        /// <summary>
        /// Gets or sets the selected connection statuses to be used to filter connection requests.
        /// </summary>
        public List<string> SelectedStatuses { get; set; }

        /// <summary>
        /// Gets or sets the available connection states that can be used to filter connection requests.
        /// </summary>
        public List<ListItemBag> AvailableConnectionStates { get; set; }

        /// <summary>
        /// Gets or sets the selected connection states to be used to filter connection requests.
        /// </summary>
        public List<string> SelectedConnectionStates { get; set; }

        /// <summary>
        /// Gets or sets the available "last activity" types that can be used to filter connection requests.
        /// </summary>
        public List<ListItemBag> AvailableLastActivityTypes { get; set; }

        /// <summary>
        /// Gets or sets the selected "last activity" types to be used to filter connection requests.
        /// </summary>
        public List<string> SelectedLastActivityTypes { get; set; }

        /// <summary>
        /// Gets or sets the model property to be used for sorting connection requests.
        /// </summary>
        public ConnectionRequestViewModelSortProperty SortProperty { get; set; }
    }
}

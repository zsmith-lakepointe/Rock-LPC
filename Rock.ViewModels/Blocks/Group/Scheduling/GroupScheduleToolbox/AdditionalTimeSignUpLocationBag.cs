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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about a location for an additional time sign-up for the group schedule toolbox.
    /// </summary>
    public class AdditionalTimeSignUpLocationBag
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the count of people already scheduled at this location.
        /// </summary>
        public int PeopleScheduledCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum capacity.
        /// </summary>
        public int? MaximumCapacity { get; set; }

        /// <summary>
        /// Gets or sets whether the maximum number of people have already been scheduled.
        /// </summary>
        public bool IsAtCapacity { get; set; }

        /// <summary>
        /// Gets or sets the count of people needed at this location.
        /// </summary>
        public int PeopleNeededCount { get; set; }
    }
}

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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about an additional time sign-up for the group schedule toolbox.
    /// </summary>
    public class AdditionalTimeSignUpBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the schedule to which this additional time sign-up belongs.
        /// </summary>
        public Guid ScheduleGuid { get; set; }

        /// <summary>
        /// Gets or sets the locations available for this additional time sign-up.
        /// </summary>
        public List<AdditionalTimeSignUpLocationBag> Locations { get; set; }
    }
}

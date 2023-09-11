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
    /// A bag that contains information about additional time sign-ups for the group schedule toolbox.
    /// </summary>
    public class AdditionalTimeSignUpsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group to which these additional time sign-ups belong.
        /// </summary>
        public Guid GroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the instructions HTML.
        /// </summary>
        public string InstructionsHtml { get; set; }

        /// <summary>
        /// Gets or sets the immediate additional time sign-ups, grouped by date.
        /// </summary>
        public Dictionary<string, List<AdditionalTimeSignUpBag>> ImmediateSignUpsByDate { get; set; }

        /// <summary>
        /// Gets or sets the non-immediate additional time sign-ups, grouped by date.
        /// </summary>
        public Dictionary<string, List<AdditionalTimeSignUpBag>> NonImmediateSignUpsByDate { get; set; }
    }
}

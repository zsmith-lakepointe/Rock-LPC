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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents A collection of <see cref="Rock.Model.Person"/> entities. This can be a family, small group, Bible study, security group,  etc. Groups can be hierarchical.
    /// </summary>
    /// <remarks>
    /// In Rock any collection or defined subset of people are considered a group.
    /// </remarks>
    [RockDomain( "Group" )]
    [Table( "GroupTptTest1" )]
    [DataContract]

    public partial class GroupTptTest1 : Group
    {
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [DataMember]
        [Previewable]
        public string Text1 { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool Bool1 { get; set; }
    }


    #region Entity Configuration

    /// <summary>
    /// Group Configuration class.
    /// </summary>
    public partial class GroupTptTest1Configuration : EntityTypeConfiguration<GroupTptTest1>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTptTest1Configuration"/> class.
        /// </summary>
        public GroupTptTest1Configuration()
        {
            this.ToTable( "GroupTptTest1" );
        }
    }

    #endregion
}

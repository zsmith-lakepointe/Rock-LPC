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
    /// Represents a test Group.
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupTptTest2" )]
    [DataContract]

    public partial class GroupTptTest2 : Group
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
        [DataMember]
        [Previewable]
        public string Text2 { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Required]
        [DataMember]
        [Previewable]
        public bool Bool1 { get; set; }
    }


    #region Entity Configuration

    /// <summary>
    /// Group Configuration class.
    /// </summary>
    public partial class GroupTptTest2Configuration : EntityTypeConfiguration<GroupTptTest2>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTptTest2Configuration"/> class.
        /// </summary>
        public GroupTptTest2Configuration()
        {
            this.ToTable( "GroupTptTest2" );
        }
    }

    #endregion
}

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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using Rock;
using Rock.Web.UI.Controls;
using Rock.Security;

namespace RockWeb.Plugins.org_lakepointe.Tutorials
{
    [DisplayName( "Person Group Picker" )]
    [Category( "lakepointe > Tutorials" )]
    [Description( "Pick Person and Group they should be associated with" )]


    public partial class PersonGroupPicker : Rock.Web.UI.RockBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ShowEmailAddress = "ShowEmailAddress";
            public const string Email = "Email";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string StarkId = "StarkId";
        }

        #endregion PageParameterKeys

        #region Fields

        // Used for private variables.

        #endregion

        #region Properties

        // Used for public / protected properties.

        #endregion

        #region Base Control Methods

        // Overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

     
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

         
        }

        #endregion

        #region Events
        protected void btnAddToGroup_Click( object sender, EventArgs e )
        {
            // Check if a person and group are selected
            if ( ppPerson.PersonId.HasValue && gpGroup.GroupId.HasValue )
            {
                var rockContext = new RockContext();
                var groupService = new GroupService( rockContext );
                var personService = new PersonService( rockContext );

                // Get the selected person and group
                var selectedPerson = personService.Get( ppPerson.PersonId.Value );
                var selectedGroup = groupService.Get( gpGroup.GroupId.Value );

                if ( selectedPerson != null && selectedGroup != null )
                {
                    // Add the person to the group
                    var groupMember = new GroupMember
                    {
                        GroupId = selectedGroup.Id,
                        PersonId = selectedPerson.Id,
                        GroupRoleId = selectedGroup.GroupType.DefaultGroupRoleId ?? 0 // Use a default role ID or handle it as needed
                    };

                    selectedGroup.Members.Add( groupMember );
                    rockContext.SaveChanges();

                    lblResult.Text = $"{selectedPerson.NickName} has been added to {selectedGroup.Name}.";
                }
                else
                {
                    lblResult.Text = "Error: Unable to retrieve selected person or group.";
                }
            }
            else
            {
                lblResult.Text = "Error: Both a person and a group must be selected.";
            }
        }
        #endregion
    }
}
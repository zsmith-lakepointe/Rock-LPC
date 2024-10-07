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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace org.lakepointe.Checkin.Workflow.Action.Checkin
{
    /// <summary>
    /// Removes the people that are restricted from checking in.
    /// </summary>
    [ActionCategory( "LPC Check-In" )]
    [Description( "Removes the people that are restricted from checking in." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter By Restricted" )]
    [AttributeField(
        Rock.SystemGuid.EntityType.PERSON,
        "Cannot Check In Until After Date Attribute",
        "The date attribute that determines the last date that the person is unable to check in. The person will be unable to check in on or before the date.",
        true,
        false,
        "",
        "",
        0,
        "Attribute" )]

    public class FilterByRestricted : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            var family = checkInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                // If attribute exists
                var attributeGuid = GetAttributeValue( action, "Attribute" )?.AsGuid() ?? Guid.Empty;
                if ( attributeGuid != Guid.Empty )
                {
                    List<CheckInPerson> peopleToRemove = new List<CheckInPerson>();

                    // For each person
                    foreach ( var person in family.People )
                    {
                        // If endDate is after or equal to today
                        person.Person.LoadAttributes();
                        var endDate = person.Person.GetAttributeValue( attributeGuid )?.AsDateTime() ?? DateTime.MinValue;
                        if ( endDate != DateTime.MinValue )
                        {
                            if ( endDate.Date >= DateTime.Today )
                            {
                                // The person is restricted from checking in
                                peopleToRemove.Add( person );
                            }
                        }
                    }

                    // For each person that should be removed
                    foreach ( var person in peopleToRemove )
                    {
                        // Remove person
                        family.People.Remove( person );
                    }
                }
            }

            return true;
        }
    }
}

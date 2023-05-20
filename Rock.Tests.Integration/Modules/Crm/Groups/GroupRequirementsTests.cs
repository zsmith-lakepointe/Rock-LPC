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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Tests.Integration.Core.Lava;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Crm.Groups
{
    /// <summary>
    /// Tests that verify the operation of Group Requirements.
    /// </summary>
    [TestClass]
    public class GroupRequirementsTests
    {
        /// <summary>
        /// Verifies that attempting to process an Interaction session having an empty Guid will ignore the invalid session and continue processing.
        /// </summary>
        [TestMethod]
        public void CalculateGroupRequirementsJob_CanProcess()
        {
            // Get a list of all people in the database.
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var allPeopleIdList = personService.Queryable()
                .Select( p => p.Id.ToString() )
                .ToList();

            // Add Test Group
            var personIdList = allPeopleIdList.GetRandomizedList( 1000 );

            //CreateTestGroupWithRequirement( $"Test Group 1", Guid.NewGuid(), personIdList );

            TestHelper.StartTimer( "Calculate Group Requirements (Old)" );
            var jobOld = new CalculateGroupRequirements();
            jobOld.Execute();
            var outputOld = jobOld.Result;
            Assert.That.Contains( outputOld, "group requirements re-calculated" );
            TestHelper.EndTimer( "Calculate Group Requirements (Old)" );

            TestHelper.StartTimer( "Calculate Group Requirements (New)" );
            var jobNew = new CalculateGroupRequirements();
            jobNew.Execute_New();
            var outputNew = jobNew.Result;
            Assert.That.Contains( outputNew, "group requirements re-calculated" );
            TestHelper.EndTimer( "Calculate Group Requirements (New)" );

        }

        private void CreateTestGroupWithRequirement( string name, Guid guid, List<string> personIdList )
        {
            // Create a new Group.
            var rockContext = new RockContext();
            var addGroupArgs = new TestDataHelper.Crm.AddGroupArgs
            {
                ReplaceIfExists = true,
                GroupGuid = guid.ToString(),
                GroupName = name,
                ForeignKey = "Test Data",
                GroupTypeIdentifier = SystemGuid.GroupType.GROUPTYPE_GENERAL
            };

            TestDataHelper.Crm.AddGroup( addGroupArgs );
            rockContext.SaveChanges();

            // Add Group Members
            var addPeopleArgs = new TestDataHelper.Crm.AddGroupMemberArgs
            {
                GroupIdentifier = guid.ToString(),
                GroupRoleIdentifier =  "Member",
                ForeignKey = "IntegrationTest",
                PersonIdentifiers = personIdList.AsDelimited(",")
            };

            var groupMembers = TestDataHelper.Crm.AddGroupMembers( rockContext, addPeopleArgs );
            rockContext.SaveChanges();

            // Add a Group Requirement.
            var addGroupRequirementArgs = new TestDataHelper.Crm.AddGroupRequirementArgs
            {
                ReplaceIfExists = true,
                GroupIdentifier = guid.ToString(),
                ForeignKey = "Test Data",
                AppliesToDataViewIdentifier = TestGuids.DataViews.AdultMembersAndAttendees,
                GroupRequirementTypeIdentifier = TestGuids.Groups.GroupRequirementBackgroundCheckGuid
            };

            TestDataHelper.Crm.AddGroupRequirement( rockContext, addGroupRequirementArgs );
            rockContext.SaveChanges();
        }
    }
}

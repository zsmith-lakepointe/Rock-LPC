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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Jobs;
using Rock.Logging;
using Rock.Model;
using Rock.Tests.Integration;
using Rock.Tests.Shared;

namespace Rock.Tests.Performance.Crm.Groups
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
        public void CalculateGroupRequirementsJob_MeasurePerformance()
        {
            // Remove all existing Group Requirement results.
            DbService.ExecuteCommand( "DELETE FROM [GroupMemberRequirement]" );

            string output = null;
            TestHelper.ExecuteWithTimer( "Calculate Group Requirements", () =>
            {
                var logger = new RockLoggerMemoryBuffer();
                logger.EventLogged += ( s, e ) =>
                {
                    TestHelper.Log( $"<{e.Event.Domain}> {e.Event.Message}" );
                };

                var job = new CalculateGroupRequirements();
                job.Logger = logger;

                job.Execute();

                output = job.Result;
                TestHelper.Log( output );
            } );
        }

        private string _RequirementsGroup1Guid = "23370B05-7A6A-4F37-959A-71120BF4E4A2";
        private string _RequirementsGroup2Guid = "179DC394-C546-4521-A7BD-F607F8B11870";
        private string _RequirementsGroup3Guid = "0D9F711F-467F-4649-812B-3428BF073B5C";
        private string _RequirementsGroup4Guid = "025CE685-0EC0-4E69-8CA5-E9FCBACD5417";
        private string _RequirementsGroup5Guid = "469B30DD-F4AF-4D7E-96AD-8AA0549200E8";


        /// <summary>
        /// Creates test data for Group Requirements.
        /// </summary>
        [TestMethod]
        public void CalculateGroupRequirementsJob_CreateTestData()
        {
            // Add the sample data.
            TestHelper.Log( $"Creating sample data for CalculateGroupRequirements Job..." );

            // Get a list of all people in the database.
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var allPeopleIdList = personService.Queryable().Select( p => p.Id.ToString() ).ToList();

            // Add Test Groups
            CreateTestGroup( $"Requirements Group 1", _RequirementsGroup1Guid, allPeopleIdList.GetRandomizedList( 1000 ) );
            CreateTestGroup( $"Requirements Group 2", _RequirementsGroup2Guid, allPeopleIdList.GetRandomizedList( 1000 ) );
            CreateTestGroup( $"Requirements Group 3", _RequirementsGroup3Guid, allPeopleIdList.GetRandomizedList( 1000 ) );
            CreateTestGroup( $"Requirements Group 4", _RequirementsGroup4Guid, allPeopleIdList.GetRandomizedList( 1000 ) );
            CreateTestGroup( $"Requirements Group 5", _RequirementsGroup5Guid, allPeopleIdList.GetRandomizedList( 1000 ) );

            CreateTestGroupRequirement( _RequirementsGroup1Guid, TestGuids.DataViews.AdultMembersAndAttendees, TestGuids.Groups.GroupRequirementBackgroundCheckGuid );
            CreateTestGroupRequirement( _RequirementsGroup2Guid, TestGuids.DataViews.AdultMembersAndAttendees, TestGuids.Groups.GroupRequirementBackgroundCheckGuid );
            CreateTestGroupRequirement( _RequirementsGroup3Guid, TestGuids.DataViews.AdultMembersAndAttendees, TestGuids.Groups.GroupRequirementBackgroundCheckGuid );
            CreateTestGroupRequirement( _RequirementsGroup4Guid, TestGuids.DataViews.AdultMembersAndAttendees, TestGuids.Groups.GroupRequirementBackgroundCheckGuid );
            CreateTestGroupRequirement( _RequirementsGroup5Guid, TestGuids.DataViews.AdultMembersAndAttendees, TestGuids.Groups.GroupRequirementBackgroundCheckGuid );
        }

        private void CreateTestGroup( string name, string guid, List<string> personIdList )
        {
            var rockContext = new RockContext();

            // NOTE: GroupType "Group With Requirements 1" must be defined in the current test database,
            // with [EnableSpecificGroupRequirements] = True.

            // Create a new Group.
            var addGroupArgs = new TestDataHelper.Crm.AddGroupArgs
            {
                ReplaceIfExists = false,
                GroupGuid = guid,
                GroupName = name,
                ForeignKey = "Test Data",
                GroupTypeIdentifier = "Group With Requirements 1"
            };

            var result = TestDataHelper.Crm.AddGroup( addGroupArgs );
            if ( result.AffectedItemCount == 0 )
            {
                return;
            }

            // Add Group Members
            var addPeopleArgs = new TestDataHelper.Crm.AddGroupMemberArgs
            {
                GroupIdentifier = guid,
                GroupRoleIdentifier = "Member",
                ForeignKey = "IntegrationTest",
                PersonIdentifiers = personIdList.AsDelimited( "," )
            };

            var groupMembers = TestDataHelper.Crm.AddGroupMembers( rockContext, addPeopleArgs );
            rockContext.SaveChanges( disablePrePostProcessing: true );

            TestHelper.Log( $"Added Group \"{name}\" ({groupMembers.Count} members)." );
        }

        private void CreateTestGroupRequirement( string groupIdentifier, string appliesToDataViewIdentifier, string groupRequirementTypeIdentifier )
        {
            var rockContext = new RockContext();

            // Add a Group Requirement.
            var addGroupRequirementArgs = new TestDataHelper.Crm.AddGroupRequirementArgs
            {
                ReplaceIfExists = true,
                GroupIdentifier = groupIdentifier,
                ForeignKey = "Test Data",
                AppliesToDataViewIdentifier = appliesToDataViewIdentifier,
                GroupRequirementTypeIdentifier = groupRequirementTypeIdentifier
            };

            var requirement = TestDataHelper.Crm.AddGroupRequirement( rockContext, addGroupRequirementArgs );
            rockContext.SaveChanges( disablePrePostProcessing: true );

            TestHelper.Log( $"Added Requirement #{requirement.Id} to Group \"{groupIdentifier}\"." );
        }
    }
}

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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Model
{
    /// <summary>
    /// Used for testing anything regarding GroupMember.
    /// </summary>
    [TestClass]
    public class GroupTptTests
    {
        #region Setup

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            RemoveTestData();
        }

        /// <summary>
        /// Runs after all tests in this class is executed.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            //RemoveTestData();
        }

        #endregion

        private const string groupAGuid = "71A50F86-8E08-4E39-9CBC-36B8EBFDF83C";
        private const string groupBGuid = "5FE4FEDC-D328-4BC0-9EEB-8865D72EEAFE";

        [TestMethod]
        public void GroupTpt_AddGroupTptWithAdditionalValues_CreatesNewRecord()
        {
            AddTestGroup1A();

            // Retrieve the sub-classed GroupTpt record.
            var rockContext = new RockContext();
            var tpt1Service = new GroupTptTest1Service( rockContext );
            var group1A = tpt1Service.Get( groupAGuid.AsGuid() );

            // Verify the value of the sub-classed properties.
            Assert.AreEqual( "Text1 for Group 1A", group1A.Text1 );
            Assert.AreEqual( true, group1A.Bool1 );
        }

        [TestMethod]
        public void GroupTpt_RetrieveExisting_ReturnsGroupTptEntity()
        {
            AddTestGroup1A();

            // Retrieve and verify the sub-classed GroupTpt record.
            var rockContext = new RockContext();
            var tpt1Service = new GroupTptTest1Service( rockContext );
            var group1A = tpt1Service.Get( groupAGuid.AsGuid() );

            Assert.AreEqual( "Text1 for Group 1A", group1A.Text1 );
        }

        [TestMethod]
        public void GroupTpt_RetrieveExistingAsBaseGroup_ReturnsGroupEntity()
        {
            AddTestGroup1A();

            var rockContext = new RockContext();

            // Retrieve the GroupTpt entity as a base Group entity.
            var groupService = new GroupService( rockContext );
            var group = groupService.Get( groupAGuid.AsGuid() );

            Assert.AreEqual( "Group 1A", group.Name );
        }

        #region Support methods

        private static bool RemoveGroupTpt1( Guid guid, RockContext rockContext )
        {
            //var rockContext = new RockContext();
            var tpt1Service = new GroupTptTest1Service( rockContext );
            var group1A = tpt1Service.Get( groupAGuid.AsGuid() );

            if ( group1A != null )
            {
                tpt1Service.Delete( group1A );

                return true;
            }

            return false;
        }

        private static void RemoveTestData()
        {
            var rockContext = new RockContext();

            RemoveGroupTpt1( groupAGuid.AsGuid(), rockContext );
            RemoveGroupTpt1( groupBGuid.AsGuid(), rockContext );

            rockContext.SaveChanges();
        }

        private static void AddTestGroup1A()
        {
            RemoveTestData();

            var rockContext = new RockContext();
            var tpt1Service = new GroupTptTest1Service( rockContext );

            // Add Group with sub-class values set.
            var group1A = new GroupTptTest1
            {
                Guid = groupAGuid.AsGuid(),
                GroupTypeId = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP ).Id,
                Name = $"Group 1A",
                IsActive = true,
                Text1 = "Text1 for Group 1A",
                Bool1 = true
            };

            tpt1Service.Add( group1A );

            rockContext.SaveChanges();
        }

        #endregion
    }
}

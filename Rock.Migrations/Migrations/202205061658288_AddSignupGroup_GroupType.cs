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
namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class AddSignupGroup_GroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddDefinedType();
            AddDefinedTypeValues();
            AddGroupType();
            AddGroupTypeRole();
            AddGroupTypeAssociation();
            AddGroupTypeAttribute();
            AddGroupTypeAttributeQualifier();
            AddGroup();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveGroup();
            RemoveGroupTypeAttribute();
            RemoveGroupTypeRole();
            RemoveGroupType();
            RemoveDefinedTypeValues();
            RemoveDefinedType();
        }

        #region Up Methods

        private void AddDefinedType()
        {
            RockMigrationHelper.AddDefinedType( "Group Type", "Project Type", "The project type.", SystemGuid.DefinedType.PROJECT_TYPE, "The project type." );
        }

        private void AddDefinedTypeValues()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.PROJECT_TYPE, "In-Person", "When the project occurs on the configured date/time.",
                SystemGuid.DefinedValue.GROUP_PROJECT_TYPE_IN_PERSON );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.PROJECT_TYPE, "Project Due", "When the project is due on the configured date/time.",
                SystemGuid.DefinedValue.GROUP_PROJECT_TYPE_PROJECT_DUE );
        }

        private void AddGroupType()
        {
            //LocationSelectionMode-  None = 0, Address = 1, Named = 2, Point = 4, Polygon = 8, GroupMember = 16, All = Address | Named | Point | Polygon | GroupMember

            RockMigrationHelper.AddGroupType( "Signup Group", "Root signup group type.", "Group", "Member", true, true, true,
                "fa fa-clipboard-check", 0,
                "", 2, "", SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP,false );

            Sql( $"UPDATE [GroupType] SET [EnableRSVP] = 1 WHERE [Guid] = '{SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP}'" );

            Sql( $"UPDATE [GroupType] SET [IsSchedulingEnabled] = 1 WHERE [Guid] = '{SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP}'" );

            // AllowedScheduleTypes - Weekly = 1, Custom = 2, Named = 4,
            Sql( $"UPDATE [GroupType] SET [AllowedScheduleTypes] = 2 WHERE [Guid] = '{SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP}'" );

            Sql( $"UPDATE [GroupType] SET [EnableLocationSchedules] = 1 WHERE [Guid] = '{SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP}'" );
        }

        private void AddGroupTypeRole()
        {
            RockMigrationHelper.AddGroupTypeRole( SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP, "Member", "Member of group.", 0, null, null, "15333F3E-8CE0-4A11-89AC-D3F8D8BBCA79", true );
        }

        private void AddGroupTypeAssociation()
        {
            RockMigrationHelper.AddGroupTypeAssociation( SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP, SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP );
        }

        private void AddGroup()
        {
            RockMigrationHelper.UpdateGroup( null, SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP, "Signup Groups", "The parent group for all signup groups.", null, 0,
                SystemGuid.Group.GROUP_SIGNUP_GROUPS,false);
        }

        private void AddGroupTypeAttribute()
        {
            RockMigrationHelper.AddGroupTypeGroupAttribute( SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP, SystemGuid.FieldType.DEFINED_VALUE, "Project Type", "The specified project type.", 0, null, "77A9B0CB-D81E-46E1-8721-4B166D68EF24" );
        }

        private void AddGroupTypeAttributeQualifier()
        {
            RockMigrationHelper.AddDefinedTypeAttributeQualifier( "77A9B0CB-D81E-46E1-8721-4B166D68EF24", SystemGuid.DefinedType.PROJECT_TYPE, "64333382-43DD-49D1-8696-8B59F8E64C76" );
        }

        #endregion Up Methods

        #region Down Methods

        private void RemoveGroupTypeAttribute()
        {
            RockMigrationHelper.DeleteAttribute( "77A9B0CB-D81E-46E1-8721-4B166D68EF24" );
        }

        private void RemoveDefinedType()
        {
            RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.PROJECT_TYPE );
        }

        private void RemoveDefinedTypeValues()
        {
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.GROUP_PROJECT_TYPE_IN_PERSON );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.GROUP_PROJECT_TYPE_PROJECT_DUE );
        }

        private void RemoveGroupTypeRole()
        {
            Sql( $"UPDATE [GroupType] SET [DefaultGroupRoleId] = NULL WHERE [Guid] = '{SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP}'" );
            RockMigrationHelper.DeleteGroupTypeRole( "15333F3E-8CE0-4A11-89AC-D3F8D8BBCA79" );
        }

        private void RemoveGroupType()
        {
            RockMigrationHelper.DeleteGroupType( SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP );
        }

        private void RemoveGroup()
        {
            RockMigrationHelper.DeleteGroup( SystemGuid.Group.GROUP_SIGNUP_GROUPS );
        }

        #endregion Down Methods
    }
}

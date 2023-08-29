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
    using Rock.Security;

    /// <summary>
    ///
    /// </summary>
    public partial class AddViewProtectionProfileToPersonBioSummaryBlock : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Removing the ViewProtectionProfile Auth which were added previously
            Sql( @"DECLARE @EntityTypeId INT = (
            		SELECT TOP 1 [Id]
            		FROM [EntityType]
            		WHERE [Name] = 'Rock.Model.Block'
            		)
            DECLARE @PersonBioBlockId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Block]
            		WHERE [Guid] = '1E6AF671-9C1A-4C6C-8156-36B6D7589F34'
            		)
            DECLARE @PersonEditBlockId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Block]
            		WHERE [Guid] = '59C7EA79-2073-4EA9-B439-7E74F06E8F5B'
            		)
            DECLARE @RockAdminGroupId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Group]
            		WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E'
            		)
            DECLARE @StaffWorkersGroupId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Group]
            		WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4'
            		)
            DECLARE @StaffLikeWorkersGroupId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Group]
            		WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745'
            		)

            UPDATE [Auth] SET [AllowOrDeny] = 'A' WHERE [Action] = 'ViewProtectionProfile' 
                AND [SpecialRole] = 1 AND [Groupid] IS NULL
                AND [EntityTypeId] = 9 AND [EntityId] IN (@PersonBioBlockId, @PersonEditBlockId);
            
            DELETE FROM [Auth]
                WHERE [Auth].[GroupId] IN (@RockAdminGroupId, @StaffWorkersGroupId, @StaffLikeWorkersGroupId)
                AND [Auth].[Action] = 'ViewProtectionProfile' AND [Auth].[EntityTypeId] = @EntityTypeId 
                AND [Auth].[EntityId] IN (@PersonBioBlockId, @PersonEditBlockId);" );

            // Add Auth as Allow for All Users for Person Summary Bio Block
            RockMigrationHelper.AddSecurityAuthForBlock( "C9523ABF-7FFA-4F43-ACEE-EE20D5D2C9E5", 1, Authorization.VIEW_PROTECTION_PROFILE, true, null, Model.SpecialRole.AllUsers, "746A89DD-D4BC-40F2-9802-7D00B78D87C1" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}

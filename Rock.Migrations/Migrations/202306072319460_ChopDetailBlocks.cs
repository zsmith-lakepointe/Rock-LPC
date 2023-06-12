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
    using System.Collections.Generic;
    using Rock.Jobs;

    /// <summary>
    ///
    /// </summary>
    public partial class ChopDetailBlocks : Rock.Migrations.RockMigration
    {
        private static readonly string JobClassName = "PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks";
        private static readonly string FullyQualifiedJobClassName = $"Rock.Jobs.{JobClassName}";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Configure run-once job by modifying these variables.
            var commandTimeout = 14000;
            var blockTypeReplacements = new Dictionary<string, string>
            {
                // Schedule Detail
                { "59C9C862-570C-4410-99B6-DD9064B5E594", "7C10240A-7EE5-4720-AAC9-5C162E9F5AAC" },
                // ShortLink Detail
                { "794C564C-6395-4303-812F-3BFBD1057443", "72EDDF3D-625E-40A9-A68B-76236E77A3F3" },
                // Asset Storage Provider
                { "C4CD9A9D-424A-4F4F-A470-C1B4AFD123BC", "4B50E08A-A805-4213-A5AF-BCA570FCB528" },
                // Event Detail
                { "762BC126-1A2E-4483-A63B-3AB4939D19F1", "78f27537-c05f-44e0-af84-2329c8b5d71d" },
                // Prayer Request Detail
                { "F791046A-333F-4B2A-9815-73B60326162D", "EBB91B46-292E-4784-9E37-38781C714008" },
                // Streak Detail
                { "EA9857FF-6703-4E4E-A6FF-65C23EBD2216", "1C98107F-DFBF-44BD-A860-0C9DF2E6C495" },
                // Streak Type Detail
                { "D9D4AF22-7743-478A-9D21-AEA4F1A0C5F6", "A83A1F49-10A6-4362-ACC3-8027224A2120" },
            };
            var shouldKeepOldBlocks = true;

            Sql( $@"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = '{FullyQualifiedJobClassName}' AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_160_REPLACE_8_BLOCKS_WITH_OBSIDIAN_BLOCKS}' )
    BEGIN
        INSERT INTO [ServiceJob] (
            [IsSystem]
            ,[IsActive]
            ,[Name]
            ,[Description]
            ,[Class]
            ,[CronExpression]
            ,[NotificationStatus]
            ,[Guid] )
        VALUES ( 
            0
            ,1
            ,'Rock Update Helper - Replace WebForms Blocks with Obsidian Blocks'
            ,'This job will replace WebForms blocks with their Obsidian blocks on all sites, pages, and layouts.'
            ,'{FullyQualifiedJobClassName}'
            ,'0 0 21 1/1 * ? *'
            ,1
            ,'{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_REPLACE_8_BLOCKS_WITH_OBSIDIAN_BLOCKS}'
            );
    END" );

            // Attribute: Rock.Jobs.PostDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Command Timeout
            var commandTimeoutAttributeGuid = "B7B057E0-D5C0-4DCC-9398-08615AF7516E";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.INTEGER, "Class", FullyQualifiedJobClassName, "Command Timeout", "Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.", 0, "14000", commandTimeoutAttributeGuid, "CommandTimeout" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_REPLACE_8_BLOCKS_WITH_OBSIDIAN_BLOCKS, commandTimeoutAttributeGuid, commandTimeout.ToString() );

            // Attribute: Rock.Jobs.PostDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Block Type Guid Replacement Pairs
            var blockTypeReplacementsAttributeGuid = "FDA45B81-6704-4F28-96EA-B390F64F9E3B";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.KEY_VALUE_LIST, "Class", FullyQualifiedJobClassName, "Block Type Guid Replacement Pairs", "Block Type Guid Replacement Pairs", "The key-value pairs of replacement BlockType.Guid values, where the key is the existing BlockType.Guid and the value is the new BlockType.Guid. Blocks of BlockType.Guid == key will be replaced by blocks of BlockType.Guid == value in all sites, pages, and layouts.", 1, "", blockTypeReplacementsAttributeGuid, "BlockTypeGuidReplacementPairs" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_REPLACE_8_BLOCKS_WITH_OBSIDIAN_BLOCKS, blockTypeReplacementsAttributeGuid, PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks.SerializeDictionary( blockTypeReplacements ) );

            // Attribute: Rock.Jobs.PostDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Should Keep Old Blocks
            var shouldKeepOldBlocksAttributeGuid = "2BA39678-C848-40D8-AD63-64CECE57D307";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.BOOLEAN, "Class", FullyQualifiedJobClassName, "Should Keep Old Blocks", "Should Keep Old Blocks", "Determines if old blocks should be kept instead of being deleted. By default, old blocks will be deleted.", 2, "False", shouldKeepOldBlocksAttributeGuid, "ShouldKeepOldBlocks" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_REPLACE_8_BLOCKS_WITH_OBSIDIAN_BLOCKS, shouldKeepOldBlocksAttributeGuid, shouldKeepOldBlocks.ToTrueFalse() );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}

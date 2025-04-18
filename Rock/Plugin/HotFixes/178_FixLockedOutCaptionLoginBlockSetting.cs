﻿// <copyright>
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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 178, "1.15.0" )]
    public class FixLockedOutCaptionLoginBlockSetting : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixLockedOutCaptionLoginBlockSettingsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // No Down
        }

        /// <summary>
        /// Fixes the "Locked Out Caption" Login Block setting.
        /// </summary>
        private void FixLockedOutCaptionLoginBlockSettingsUp()
        {
            FixLockedOutCaptionLoginBlockSettingsUp( "7B83D513-1178-429E-93FF-E76430E038E4" ); // WebForms Login Block Type
            FixLockedOutCaptionLoginBlockSettingsUp( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3" ); // Obsidian Login Block Type
        }

        /// <summary>
        /// Fixes the "Locked Out Caption" Login Block setting for a specific BlockType.
        /// </summary>
        private void FixLockedOutCaptionLoginBlockSettingsUp( string blockTypeGuid )
        {
            Sql( $@"
                DECLARE @BlockTypeId AS int
                SELECT @BlockTypeId=Id FROM [BlockType] WHERE [Guid] = '{blockTypeGuid}'
                DECLARE @AttributeId AS int
                SELECT @AttributeId=Id FROM [Attribute] WHERE [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = @BlockTypeId AND [Key] = 'LockedOutCaption'
                -- Update the default attribute value.
                UPDATE [Attribute]
                   SET [DefaultValue] = REPLACE([DefaultValue], 'assign phone = Global''', 'assign phone = ''Global''')
                     , [IsDefaultPersistedValueDirty] = 1
                 WHERE [Id] = @AttributeId
                -- Update attribute values.
                UPDATE [AttributeValue]
                   SET [Value] = REPLACE([Value], 'assign phone = Global''', 'assign phone = ''Global''')
                     , [IsPersistedValueDirty] = 1
                WHERE [AttributeId] = @AttributeId" );
        }
    }
}

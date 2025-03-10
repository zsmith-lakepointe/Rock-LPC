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
    [MigrationNumber( 174, "1.14.3" )]
    public class AddIndicesForPerformanceImprovements : Migration
    {
        
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
                IF NOT EXISTS (SELECT * 
                FROM sys.indexes 
                WHERE name = 'IX_IpAddress_LookupDateTime' 
                    AND object_id = OBJECT_ID('dbo.InteractionSessionLocation'))
                BEGIN
	                -- Create Index IX_IpAddress_LookupDateTime
	                CREATE NONCLUSTERED INDEX [IX_IpAddress_LookupDateTime] ON [dbo].[InteractionSessionLocation]
	                (
		                [IpAddress] ASC,
		                [LookupDateTime] ASC
	                ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);
                END

                IF NOT EXISTS (SELECT * 
                FROM sys.indexes 
                WHERE name = 'IX_AuthorizedPersonAliasId' 
                    AND object_id = OBJECT_ID('dbo.FinancialTransaction'))
                BEGIN
	                -- Create Index IX_AuthorizedPersonAliasId
	                CREATE NONCLUSTERED INDEX [IX_AuthorizedPersonAliasId] ON [dbo].[FinancialTransaction]
	                (
		                [AuthorizedPersonAliasId] ASC
	                ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);
                END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}

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
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class TestTpt : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.GroupTptTest1",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Text1 = c.String(),
                        Bool1 = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Group", t => t.Id)
                .Index(t => t.Id);

            CreateTable(
                "dbo.GroupTptTest2",
                c => new
                {
                    Id = c.Int( nullable: false ),
                    Text1 = c.String(),
                    Text2 = c.String(),
                    Bool1 = c.Boolean( nullable: false ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Group", t => t.Id )
                .Index( t => t.Id );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupTptTest1", "Id", "dbo.Group");
            DropForeignKey("dbo.GroupTptTest2", "Id", "dbo.Group");
            DropIndex("dbo.GroupTptTest1", new[] { "Id" });
            DropIndex("dbo.GroupTptTest2", new[] { "Id" });
            DropTable("dbo.GroupTptTest1");
            DropTable("dbo.GroupTptTest2");
        }
    }
}

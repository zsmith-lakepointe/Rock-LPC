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
    [MigrationNumber( 171, "1.14.3" )]
    public class UpdateColorsInTBD : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"UPDATE [AttributeValue] SET [Value]=N'{
    ""SeriesColors"": [
      ""#38BDF8"",
      ""#A3E635"",
      ""#34D399"",
      ""#FB7185"",
      ""#818CF8"",
      ""#FB923C"",
      ""#C084FC"",
      ""#FBBF24"",
      ""#A8A29E""
    ],
    ""GoalSeriesColor"": ""red"",
    ""Grid"": {
      ""ColorGradient"": null,
      ""Color"": null,
      ""BackgroundColorGradient"": null,
      ""BackgroundColor"": ""transparent"",
      ""BorderWidth"": {
        ""top"": 0,
        ""right"": 0,
        ""bottom"": 1,
        ""left"": 1
      },
      ""BorderColor"": null
    },
    ""XAxis"": {
      ""Color"": ""rgba(81, 81, 81, 0.2)"",
      ""Font"": {
        ""Size"": 10,
        ""Family"": null,
        ""Color"": ""#515151""
      },
      ""DateTimeFormat"": ""%b %e,<br />%Y""
    },
    ""YAxis"": {
      ""Color"": ""rgba(81, 81, 81, 0.2)"",
      ""Font"": {
        ""Size"": null,
        ""Family"": null,
        ""Color"": ""#515151""
      },
      ""DateTimeFormat"": null
    },
    ""FillOpacity"": 0.2,
    ""FillColor"": null,
    ""Legend"": {
      ""BackgroundColor"": ""transparent"",
      ""BackgroundOpacity"": null,
      ""LabelBoxBorderColor"": null
    },
    ""Title"": {
      ""Font"": {
        ""Size"": 16,
        ""Family"": null,
        ""Color"": null
      },
      ""Align"": ""left""
    },
    ""Subtitle"": {
      ""Font"": {
        ""Size"": 12,
        ""Family"": null,
        ""Color"": null
      },
      ""Align"": ""left""
    }
  }' WHERE ([Guid]='81c5ac0e-1065-4d57-8ebb-5bc2e60090b9')" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}

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
    [MigrationNumber( 170, "1.14.3" )]
    public class UpdateInsightsMetricsToExcludeAnonymousVisitor : Migration
    {
        private const string SelectPercentageActivePeopleSql = @"
DECLARE
	@ActiveRecordStatusValueId INT = (
	    SELECT TOP 1 Id
	    FROM DefinedValue
	    WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
	)
	,@PersonRecordTypeValueId INT = (
	    SELECT TOP 1 Id
	    FROM DefinedValue
	    WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
	)   

DECLARE 
    @ActivePeopleCount decimal = (
        SELECT COUNT(*) 
        FROM Person
        WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
        AND [RecordStatusValueId] = @ActiveRecordStatusValueId
		AND [Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
        AND [IsDeceased] = 0
    )

SELECT Round(CAST(COUNT(*) * 100 AS decimal) / @ActivePeopleCount, 1), P.[PrimaryCampusId]
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
	AND [Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateAgeRangeMetrics();
            UpdateGenderMetrics();
            UpdateInformationCompletenessMetrics();
            UpdateUnCategorizedMetrics();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        private void UpdateAgeRangeMetrics()
        {
            string sql = string.Empty;
            const string updateAgeRangeSql = @"
UPDATE Metric 
SET SourceSql = 'DECLARE @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
    )     
    ,@Today Date = GetDate()

SELECT COUNT(1), P.[PrimaryCampusId]
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
	AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
	AND P.[AgeBracket] = {0}

GROUP BY ALL P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'
WHERE Guid = '{1}'
";
            sql += string.Format( updateAgeRangeSql, "1", "EEDEE264-F49D-46B9-815D-C5DBB5DCC9CE" );

            sql += string.Format( updateAgeRangeSql, "2", "5FFD8F4C-5199-41D7-BF33-14497DFEDD3F" );

            sql += string.Format( updateAgeRangeSql, "3", "12B581AD-EFE3-4A77-B2E7-80A385EFDDEE" );

            sql += string.Format( updateAgeRangeSql, "4", "95141ECF-F29F-4AEB-A966-B63AE21B9520" );

            sql += string.Format( updateAgeRangeSql, "5", "6EEB5736-E17A-44E2-AC27-0CF933E4EC37" );

            sql += string.Format( updateAgeRangeSql, "6", "411A8AF5-FE70-43E1-8BCE-C1FA52051663" );

            sql += string.Format( updateAgeRangeSql, "7", "93D36C22-6D88-4A2D-BCE2-0CF7CF1FC8F0" );

            sql += string.Format( updateAgeRangeSql, "8", "9BF88708-D258-49DA-928E-CF8D894EF21B" );

            sql += string.Format( updateAgeRangeSql, "0", "54EEDC65-4D5F-4E28-8709-7F282FA9412A" );

            Sql( sql );
        }

        private void UpdateGenderMetrics()
        {
            string sql = string.Empty;
            const string format = @"
UPDATE Metric 
SET SourceSql = 'DECLARE @GenderValue INT = {0} --  0=Unknown, 1=Male, 2=Female,
    ,@ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
    )        

SELECT COUNT(1), P.[PrimaryCampusId] 
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
    AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
    AND P.[Gender] = @GenderValue
GROUP BY ALL P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'

WHERE Guid = '{1}'
";

            sql += string.Format( format, "1", "44A00879-D836-4BA1-8CD1-B74EC2C53D5F" );

            sql += string.Format( format, "2", "71CDC173-8B4E-4AFC-AD0E-925F69D299DA" );

            sql += string.Format( format, "0", "53883982-E730-4396-8B12-0D83304C5880" );

            Sql( sql );
        }

        private void UpdateInformationCompletenessMetrics()
        {
            Sql( $@"
                -- ACTIVE EMAIL
                UPDATE Metric 
                SET SourceSql = '{SelectPercentageActivePeopleSql}
                    AND P.[Email] != ''''
                    AND P.[Email] IS NOT NULL
                    AND P.[IsEmailActive] = 1
                GROUP BY ALL P.PrimaryCampusId'
                WHERE Guid = '0C1C1231-DB5D-4B44-9172-F39B56786960'

                -- AGE
                UPDATE Metric 
                SET SourceSql = '{SelectPercentageActivePeopleSql}
                    AND P.[Age] IS NOT NULL
                GROUP BY ALL P.PrimaryCampusId',
                Description = 'Percent of active people with known ages'
                WHERE Guid = '8046A160-941F-4CCD-9EB6-5BD7601DD536'

                -- DATE OF BIRTH
                UPDATE Metric 
                SET SourceSql = '{SelectPercentageActivePeopleSql}
                    AND P.[BirthDate] IS NOT NULL
                GROUP BY ALL P.PrimaryCampusId'
                WHERE Guid = 'D79DECDD-BA7B-4E4B-81F1-5B6392FD7BD8'

                -- GENDER
                UPDATE Metric 
                SET SourceSql = '{SelectPercentageActivePeopleSql}
                    AND P.[GENDER] != 0
                GROUP BY ALL P.PrimaryCampusId'
                WHERE Guid = 'C4F9A612-D487-4CE0-9D9B-691DC733857D'

                -- HOME ADDRESS
                UPDATE Metric 
                SET SourceSql = 'DECLARE
                    @ActiveRecordStatusValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
                    )
                    ,@PersonRecordTypeValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
                    )   

                DECLARE 
	                @ActivePeopleCount decimal = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [RecordStatusValueId] = @ActiveRecordStatusValueId
                        AND [Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
		                AND [IsDeceased] = 0
	                )

                SELECT Round(CAST(COUNT(*) * 100 AS decimal) / @ActivePeopleCount, 1) AS PercentWithHomeAddress, P.[PrimaryCampusId]
                FROM Person P
                JOIN GroupLocation G
                ON G.[GroupId] = P.[PrimaryFamilyId]
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
                    AND P.[IsDeceased] = 0
                    AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
	                AND G.[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = ''8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'')
                GROUP BY ALL P.PrimaryCampusId'
                WHERE Guid = '7964D01D-41B7-469F-8CE7-0C4A84968E62'

                -- MARITAL STATUS
                UPDATE Metric 
                SET SourceSql = '{SelectPercentageActivePeopleSql}
                    AND P.[MaritalStatusValueId] IS NOT NULL
                    AND P.[MaritalStatusValueId] != (SELECT Id FROM DefinedValue WHERE [Guid] = ''99844b92-3d63-4246-bb22-b0db7bda8d01'')
                GROUP BY ALL P.PrimaryCampusId'
                WHERE Guid = '17AC2A8A-B130-4900-B2BD-203D0F8FF971'

                -- MOBILE PHONE
                UPDATE Metric 
                SET SourceSql = 'DECLARE
                    @ActiveRecordStatusValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
                    )
                    ,@PersonRecordTypeValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
                    )     

                DECLARE 
	                @ActivePeopleCount decimal = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [RecordStatusValueId] = @ActiveRecordStatusValueId
                        AND [Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
		                AND [IsDeceased] = 0
	                )

                SELECT Round(CAST(COUNT(*) * 100 AS decimal) / @ActivePeopleCount, 1) AS PercentWithPhone, P.[PrimaryCampusId]
                FROM [Person] P
                JOIN [PhoneNumber] PH
                ON P.[Id] = PH.[PersonId]
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
                    AND P.[IsDeceased] = 0
                    AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
                    AND PH.[NumberTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = ''407E7E45-7B2E-4FCD-9605-ECB1339F2453'')
                GROUP BY ALL P.PrimaryCampusId'
                WHERE Guid = '75A8E234-AEC3-4C75-B902-C3F954616BBC'

                -- PHOTO
                UPDATE Metric 
                SET SourceSql = '{SelectPercentageActivePeopleSql}
                    AND P.[PhotoId] IS NOT NULL
                GROUP BY ALL P.PrimaryCampusId'
                WHERE Guid = '4DACA1E0-E768-417C-BB5B-DAB5DC0BDA79'

                -- ACTIVE RECORDS
                UPDATE Metric 
                SET SourceSql = 'DECLARE
                    @ActiveRecordStatusValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
                    )
                    ,@PersonRecordTypeValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
                    )  
	
                DECLARE 
	                @ActivePeopleCount decimal = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
                        AND [Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
		                AND [IsDeceased] = 0
	                )

                SELECT Round(CAST(COUNT(*) * 100 AS decimal) / @ActivePeopleCount, 1), P.[PrimaryCampusId], P.[RecordStatusValueId]
                FROM [Person] P
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND [Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
                    AND P.[IsDeceased] = 0
                GROUP BY ALL P.[RecordStatusValueId], P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'
                WHERE Guid = '7AE9475F-389E-496F-8DF0-508B66ADA6A0'
" );
        }

        private void UpdateUnCategorizedMetrics()
        {
            string sql = string.Empty;
            const string format = @"
UPDATE METRIC
SET SourceSql = 'DECLARE
    @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
    )        

SELECT COUNT(1), P.[PrimaryCampusId], {0}
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
	AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
    AND P.[IsDeceased] = 0
    {2}
GROUP BY ALL P.[PrimaryCampusId], {0} ORDER BY P.[PrimaryCampusId]'
WHERE Guid = '{1}'
";

            sql += string.Format( format, "P.[ConnectionStatusValueId]", "08A7360A-642E-4FA9-A5F8-288496D380EF", "AND P.[ConnectionStatusValueId] IS NOT NULL" );
            sql += string.Format( format, "P.[EthnicityValueId]", "B0420908-6AED-487C-BCA4-9B63EA4F87F5", "AND P.[EthnicityValueId] IS NOT NULL" );
            sql += string.Format( format, "P.[RaceValueId]", "3EB53204-CED6-4ACF-8045-288BD2EA8E82", "AND P.[RaceValueId] IS NOT NULL" );
            sql += string.Format( format, "P.[MaritalStatusValueId]", "D9B85AD9-2573-4EAE-8DCF-980FC13B81B5", "AND P.[MaritalStatusValueId] IS NOT NULL" );
            sql += string.Format( @"
UPDATE Metric 
SET SourceSql = 'DECLARE @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
    )     
    ,@Today Date = GetDate()

-- People, who are Active, who are not deceased and...
SELECT COUNT(1), P.[PrimaryCampusId]
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
	AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
    AND P.[Age] IS NULL
GROUP BY ALL P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'
WHERE Guid = '{0}'
", "54EEDC65-4D5F-4E28-8709-7F282FA9412A" );

            Sql( sql );
        }
    }
}

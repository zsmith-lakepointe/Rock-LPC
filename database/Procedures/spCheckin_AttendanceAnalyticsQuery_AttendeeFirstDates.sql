/*
<doc>
	<summary>
        This function return people who attended based on selected filter criteria and the first 5 dates they ever attended the selected group type
	</summary>

	<returns>
		* PersonId
		* TimeAttending
		* SundayDate
	</returns>
	<param name='GroupTypeIds' datatype='varchar(max)'>The Group Type Ids (only attendance for these group types will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>
	</remarks>
	<code>
        EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates] '18,19,20,21,22,23,25', '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, null
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates]
	  @GroupTypeIds varchar(max)
	, @GroupIds varchar(max)
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
	, @ScheduleIds varchar(max) = NULL
	WITH RECOMPILE

AS

BEGIN

    -- Calculate the SundayDates within the StartDate and EndDate so we can filter by AttendanceOccurrence.SundayDate
    DECLARE @startDateSundayDate DATE
    DECLARE @endDateSundayDate DATE

    SELECT
        @startDateSundayDate = sdr.StartSundayDate
        , @endDateSundayDate = sdr.EndSundayDate
    FROM
        [dbo].ufnUtility_GetSundayDateRange(@StartDate, @EndDate) sdr;

    -- Define the CTEs for the lookup data needed to calculate the attendance.
    WITH
    CampusIdList AS
    (
        SELECT CONVERT( int, [Item]) AS [Id] FROM [dbo].ufnUtility_CsvToTable( ISNULL(@CampusIds,'') )
    ),
    ScheduleIdList AS
    (
        SELECT CONVERT( int, [Item]) AS [Id] FROM [dbo].ufnUtility_CsvToTable( ISNULL(@ScheduleIds,'') )
    ),
    GroupIdList AS
    (
        SELECT CONVERT( int, [Item]) AS [Id] FROM [dbo].ufnUtility_CsvToTable( ISNULL(@GroupIds,'') )
    ),
    GroupTypeIdList AS
    (
        SELECT CONVERT( int, [Item]) AS [Id] FROM [dbo].ufnUtility_CsvToTable( ISNULL(@GroupTypeIds,'') )
    ),
    AttendeePersonIdList AS
    (
        -- Get the set of people who have attended any of the selected groups/schedules/campuses during the
		-- reporting period.
        SELECT DISTINCT
            pa.[PersonId]
        FROM
		    [dbo].[Attendance] a
            INNER JOIN [dbo].[AttendanceOccurrence] o ON o.[Id] = a.[OccurrenceId]
            INNER JOIN [dbo].[PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
            INNER JOIN GroupIdList g ON g.[Id] = o.[GroupId]
            LEFT OUTER JOIN CampusIdList c ON c.[id] = a.[CampusId]
            LEFT OUTER JOIN ScheduleIdList s ON s.[id] = o.[ScheduleId]
        WHERE
            o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
            AND [DidAttend] = 1
            AND (
                ( @CampusIds IS NULL OR c.[Id] IS NOT NULL ) OR
                ( @IncludeNullCampusIds = 1 AND a.[CampusId] IS NULL )
            )
            AND ( @ScheduleIds IS NULL OR s.[Id] IS NOT NULL )
    )
        -- For each person who has attended during the reporting period, get the first
		-- 5 occasions on which they attended one of the selected group types, for any group or campus.
        -- Multiple attendances on the same date are considered as a single occasion.
        SELECT DISTINCT
            [PersonId]
            , [TimeAttending]
            , [StartDate]
        FROM (
            SELECT
                p.[Id] AS [PersonId]
                , DENSE_RANK() OVER ( PARTITION BY p.[Id] ORDER BY ao.[OccurrenceDate] ) AS [TimeAttending]
                , ao.[OccurrenceDate] AS [StartDate]
            FROM
                [dbo].[Attendance] a
                INNER JOIN [dbo].[AttendanceOccurrence] ao ON ao.[Id] = a.[OccurrenceId]
                INNER JOIN [dbo].[Group] g ON g.[Id] = ao.[GroupId]
                INNER JOIN [dbo].[PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
                INNER JOIN [dbo].[Person] p ON p.[Id] = pa.[PersonId]
                INNER JOIN GroupTypeIdList gt ON gt.[id] = g.[GroupTypeId]
            WHERE
                p.[Id] IN ( SELECT [PersonId] FROM AttendeePersonIdList )
                AND [DidAttend] = 1
            ) x
        WHERE
		    x.[TimeAttending] <= 5

END
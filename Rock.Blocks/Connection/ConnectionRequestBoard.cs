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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace Rock.Blocks.Connection
{
    /// <summary>
    /// Display the Connection Requests for a selected Connection Opportunity as a list or board view.
    /// </summary>

    [DisplayName( "Connection Request Board" )]
    [Category( "Obsidian > Connection" )]
    [Description( "Display the Connection Requests for a selected Connection Opportunity as a list or board view." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField(
        "Max Cards per Column",
        Key = AttributeKey.MaxCards,
        Description = "The maximum number of cards to display per column. This is to prevent performance issues caused by rendering too many cards at a time.",
        DefaultIntegerValue = DefaultMaxCards,
        IsRequired = true,
        Order = 0 )]

    [LinkedPage(
        "Person Profile Page",
        Key = AttributeKey.PersonProfilePage,
        Description = "Page used for viewing a person's profile. If set a view profile button will show for each grid item.",
        DefaultValue = Rock.SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES,
        IsRequired = true,
        Order = 1 )]

    [LinkedPage(
        "Workflow Detail Page",
        Key = AttributeKey.WorkflowDetailPage,
        Description = "Page used to display details about a workflow.",
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_DETAIL,
        IsRequired = true,
        Order = 2 )]

    [LinkedPage(
        "Workflow Entry Page",
        Key = AttributeKey.WorkflowEntryPage,
        Description = "Page used to launch a new workflow of the selected type.",
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_ENTRY,
        IsRequired = true,
        Order = 3 )]

    [CodeEditorField(
        "Status Template",
        Key = AttributeKey.StatusTemplate,
        Description = "Lava Template that can be used to customize what is displayed in the status bar. Includes common merge fields plus ConnectionOpportunities, ConnectionTypes and the default IdleTooltip.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        DefaultValue = StatusTemplateDefaultValue,
        IsRequired = true,
        Order = 4 )]

    [CodeEditorField(
        "Connection Request Status Icons Template",
        Key = AttributeKey.ConnectionRequestStatusIconsTemplate,
        Description = "Lava Template that can be used to customize what is displayed for the status icons in the connection request grid.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        DefaultValue = ConnectionRequestStatusIconsTemplateDefaultValue,
        IsRequired = true,
        Order = 5 )]

    [LinkedPage(
        "Group Detail Page",
        Key = AttributeKey.GroupDetailPage,
        Description = "Page used to display group details.",
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        IsRequired = true,
        Order = 6 )]

    [LinkedPage(
        "SMS Link Page",
        Key = AttributeKey.SmsLinkPage,
        Description = "Page that will be linked for SMS enabled phones.",
        DefaultValue = Rock.SystemGuid.Page.NEW_COMMUNICATION,
        IsRequired = true,
        Order = 7 )]

    [BadgesField(
        "Badges",
        Key = AttributeKey.Badges,
        Description = "The badges to display in this block.",
        IsRequired = false,
        Order = 8 )]

    [CodeEditorField(
        "Lava Heading Template",
        Key = AttributeKey.LavaHeadingTemplate,
        Description = "The HTML Content to render above the person’s name. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        IsRequired = false,
        Order = 9 )]

    [CodeEditorField(
        "Lava Badge Bar",
        Key = AttributeKey.LavaBadgeBar,
        Description = "The HTML Content intended to be used as a kind of custom badge bar for the connection request. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        IsRequired = false,
        Order = 10 )]

    [ConnectionTypesField(
        "Connection Types",
        Key = AttributeKey.ConnectionTypes,
        Description = "Optional list of connection types to limit the display to (All will be displayed by default).",
        IsRequired = false,
        Order = 11 )]

    [BooleanField(
        "Limit to Assigned Connections",
        Key = AttributeKey.OnlyShowAssigned,
        Description = "When enabled, only requests assigned to the current person will be shown.",
        DefaultBooleanValue = false,
        IsRequired = true,
        Order = 12 )]

    [LinkedPage(
        "Connection Request History Page",
        Key = AttributeKey.ConnectionRequestHistoryPage,
        Description = "Page used to display history details.",
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        IsRequired = true,
        Order = 13 )]

    [LinkedPage(
        "Bulk Update Requests",
        Key = AttributeKey.BulkUpdateRequestsPage,
        Description = "Page used to update selected connection requests",
        DefaultValue = Rock.SystemGuid.Page.CONNECTION_REQUESTS_BULK_UPDATE,
        IsRequired = true,
        Order = 14 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "4C517EE6-B440-415B-9D0A-6573AC9EBACB" )]
    [Rock.SystemGuid.BlockTypeGuid( "AEC71B37-5498-47BC-939C-E2C102999D5C" )]
    public class ConnectionRequestBoard : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Keys for attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string MaxCards = "MaxCards";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string WorkflowDetailPage = "WorkflowDetailPage";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
            public const string StatusTemplate = "StatusTemplate";
            public const string ConnectionRequestStatusIconsTemplate = "ConnectionRequestStatusIconsTemplate";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string SmsLinkPage = "SmsLinkPage";
            public const string Badges = "Badges";
            public const string LavaHeadingTemplate = "LavaHeadingTemplate";
            public const string LavaBadgeBar = "LavaBadgeBar";
            public const string ConnectionTypes = "ConnectionTypes";
            public const string OnlyShowAssigned = "OnlyShowAssigned";
            public const string ConnectionRequestHistoryPage = "ConnectionRequestHistoryPage";
            public const string BulkUpdateRequestsPage = "BulkUpdateRequestsPage";
        }

        /// <summary>
        /// Keys for page parameters.
        /// </summary>
        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
            public const string ConnectionOpportunityId = "ConnectionOpportunityId";
            public const string ConnectionRequestGuid = "ConnectionRequestGuid";
            public const string ConnectionRequestId = "ConnectionRequestId";
            public const string ConnectionTypeId = "ConnectionTypeId";
            public const string EntitySetId = "EntitySetId";
            public const string WorkflowId = "WorkflowId";
        }

        /// <summary>
        /// Keys for person preferences.
        /// </summary>
        private static class PersonPreferenceKey
        {
            public const string CampusFilter = "campus-filter";
            public const string ConnectionOpportunityId = "connection-opportunity-id";
            public const string ConnectorPersonAliasId = "connector-person-alias-id";
            public const string DateRange = "date-range";
            public const string LastActivities = "last-activities";
            public const string PastDueOnly = "past-due-only";
            public const string Requester = "requester";
            public const string SortBy = "sort-by";
            public const string States = "states";
            public const string Statuses = "statuses";
            public const string ViewMode = "view-mode";
        }

        #endregion Keys

        #region Defaults

        /// <summary>
        /// The default maximum cards per status column.
        /// </summary>
        private const int DefaultMaxCards = 100;

        /// <summary>
        /// The initial count of activities to show in grid. There is a "show more" button for the user to
        /// click if the actual count exceeds this.
        /// </summary>
        private const int InitialActivitiesToShowInGrid = 10;

        /// <summary>
        /// The default status template.
        /// </summary>
        private const string StatusTemplateDefaultValue = @"
<div class='pull-left badge-legend padding-r-md'>
    <span class='pull-left badge badge-info badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>
    <span class='pull-left badge badge-warning badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned Item'><span class='sr-only'>Unassigned Item</span></span>
    <span class='pull-left badge badge-critical badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Critical Status'><span class='sr-only'>Critical Status</span></span>
    <span class='pull-left badge badge-danger badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>
</div>";

        /// <summary>
        /// The default connection request status icons template. Used at the top of each connection request card (in card view mode),
        /// the first column of each row (in grid view mode) + the top of the connection request modal.
        /// </summary>
        private const string ConnectionRequestStatusIconsTemplateDefaultValue = @"
<div class='board-card-pills'>
    {% if ConnectionRequestStatusIcons.IsAssignedToYou %}
    <span class='board-card-pill badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsUnassigned %}
    <span class='board-card-pill badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned'><span class='sr-only'>Unassigned</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsCritical %}
    <span class='board-card-pill badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical'><span class='sr-only'>Critical</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsIdle %}
    <span class='board-card-pill badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>
    {% endif %}
</div>
";

        /// <summary>
        /// The default delimiter.
        /// </summary>
        private const string DefaultDelimiter = "|";

        #endregion Defaults

        #region Fields

        private PersonPreferenceCollection _personPreferences;

        #endregion Fields

        #region Properties

        public override string ObsidianFileUrl => $"{base.ObsidianFileUrl}.obs";

        public PersonPreferenceCollection PersonPreferences
        {
            get
            {
                if ( _personPreferences == null )
                {
                    _personPreferences = this.GetBlockPersonPreferences();
                }

                return _personPreferences;
            }
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        public Person CurrentPerson => this.RequestContext.CurrentPerson;

        /// <summary>
        /// Gets the current person identifier, or zero if the person is not defined.
        /// </summary>
        public int CurrentPersonId => this.CurrentPerson?.Id ?? 0;

        /// <summary>
        /// Gets the current person's primary alias identifier, or zero if the person or primary alias identifier is not defined.
        /// </summary>
        public int CurrentPersonAliasId => this.CurrentPerson?.PrimaryAliasId ?? 0;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new ConnectionRequestBoardInitializationBox();

                SetBoxInitialState( box, rockContext );

                return box;
            }
        }

        /// <summary>
        /// Sets the initial state of the box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialState( ConnectionRequestBoardInitializationBox box, RockContext rockContext )
        {
            var boardData = GetConnectionRequestBoardData( rockContext );

            box.ConnectionTypes = boardData.AllowedConnectionTypes;
        }

        /// <summary>
        /// Gets the connection request board data needed for the board to operate.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The connection request board data needed for the board to operate.</returns>
        private ConnectionRequestBoardData GetConnectionRequestBoardData( RockContext rockContext )
        {
            var boardData = new ConnectionRequestBoardData();

            var block = new BlockService( rockContext ).Get( this.BlockId );
            block.LoadAttributes( rockContext );

            GetAllowedConnectionTypes( rockContext, boardData );
            GetConnectionAndCampusSelectionsFromURLOrPersonPreferences( rockContext, boardData );
            GetFilterOptions( rockContext, boardData );

            // Preferences will only be saved if updates were actually set above.
            this.PersonPreferences.Save();

            return boardData;
        }

        /// <summary>
        /// Gets the allowed connection types for the current user and loads them onto the supplied <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data onto which to load the allowed connection types.</param>
        private void GetAllowedConnectionTypes( RockContext rockContext, ConnectionRequestBoardData boardData )
        {
            var allowedConnectionTypes = new List<ConnectionRequestBoardConnectionTypeBag>();

            var opportunitiesQuery = new ConnectionOpportunityService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( o => o.ConnectionType )
                .Include( o => o.ConnectionOpportunityCampuses )
                .Where( o => o.IsActive && o.ConnectionType.IsActive );

            var typeFilter = GetAttributeValue( AttributeKey.ConnectionTypes ).SplitDelimitedValues().AsGuidList();
            if ( typeFilter.Any() )
            {
                opportunitiesQuery = opportunitiesQuery.Where( o => typeFilter.Contains( o.ConnectionType.Guid ) );
            }

            var selfAssignedOpportunityIds = new List<int>();
            var wasSelfAssignedOpportunityIdsQueried = false;

            // Get this person's favorite opportunity IDs so we can mark them as such below.
            var entityTypeId = EntityTypeCache.Get<ConnectionOpportunity>().Id;
            var favoriteOpportunityIds = new FollowingService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( f =>
                    f.EntityTypeId == entityTypeId
                    && string.IsNullOrEmpty( f.PurposeKey )
                    && f.PersonAliasId == CurrentPersonAliasId
                )
                .Select( f => f.EntityId )
                .ToList();

            foreach ( var opportunity in opportunitiesQuery.ToList() )
            {
                if ( opportunity.ConnectionType.EnableRequestSecurity && !wasSelfAssignedOpportunityIdsQueried )
                {
                    selfAssignedOpportunityIds = new ConnectionRequestService( rockContext )
                        .Queryable()
                        .Where( r => r.ConnectorPersonAlias.PersonId == CurrentPersonId )
                        .Select( r => r.ConnectionOpportunityId )
                        .Distinct()
                        .ToList();

                    wasSelfAssignedOpportunityIdsQueried = true;
                }

                var canView = opportunity.IsAuthorized( Authorization.VIEW, CurrentPerson )
                    || (
                        opportunity.ConnectionType.EnableRequestSecurity
                        && selfAssignedOpportunityIds.Contains( opportunity.Id )
                    );

                if ( !canView )
                {
                    continue;
                }

                // Add the opportunity's type if it hasn't already been added.
                var connectionType = allowedConnectionTypes.FirstOrDefault( t => t.Id == opportunity.ConnectionType.Id );
                if ( connectionType == null )
                {
                    connectionType = new ConnectionRequestBoardConnectionTypeBag
                    {
                        Id = opportunity.ConnectionType.Id,
                        Name = opportunity.ConnectionType.Name,
                        IconCssClass = opportunity.ConnectionType.IconCssClass,
                        Order = opportunity.ConnectionType.Order,
                        ConnectionOpportunities = new List<ConnectionRequestBoardConnectionOpportunityBag>()
                    };

                    allowedConnectionTypes.Add( connectionType );
                }

                // Add the opportunity.
                connectionType.ConnectionOpportunities.Add( new ConnectionRequestBoardConnectionOpportunityBag
                {
                    Id = opportunity.Id,
                    PublicName = opportunity.PublicName,
                    IconCssClass = opportunity.IconCssClass,
                    ConnectionTypeName = connectionType.Name,
                    PhotoUrl = ConnectionOpportunity.GetPhotoUrl( opportunity.PhotoId ),
                    Order = opportunity.Order,
                    IsFavorite = favoriteOpportunityIds.Contains( opportunity.Id )
                } );
            }

            // Sort each type's opportunities.
            foreach ( var connectionType in allowedConnectionTypes )
            {
                connectionType.ConnectionOpportunities = connectionType.ConnectionOpportunities
                    .OrderBy( co => co.Order )
                    .ThenBy( co => co.PublicName )
                    .ToList();
            }

            // Sort and add the allowed types.
            boardData.AllowedConnectionTypes = allowedConnectionTypes
                .Where( ct => ct.ConnectionOpportunities.Any() )
                .OrderBy( ct => ct.Order )
                .ThenBy( ct => ct.Name )
                .ToList();
        }

        /// <summary>
        /// Gets the selected connection request, connection opportunity and campus from page parameters or person preferences, loading them
        /// onto the supplied <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data onto which to load the selected connection request, connection opportunity and campus.</param>
        private void GetConnectionAndCampusSelectionsFromURLOrPersonPreferences( RockContext rockContext, ConnectionRequestBoardData boardData )
        {
            int? connectionOpportunityId = null;

            var availableOpportunityIds = boardData.AllowedConnectionTypes
                .SelectMany( ct => ct.ConnectionOpportunities.Select( co => co.Id ) )
                .ToList();

            // If a connection request page parameter was provided, it takes priority since it's more specific.
            var paramConnectionRequestId = GetEntityIdFromPageParameter<ConnectionRequest>( PageParameterKey.ConnectionRequestId, rockContext );
            if ( paramConnectionRequestId.HasValue )
            {
                var result = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( cr => cr.Id == paramConnectionRequestId.Value )
                    .Select( cr => new
                    {
                        cr.ConnectionOpportunityId
                    } )
                    .FirstOrDefault();

                if ( result != null && availableOpportunityIds.Contains( result.ConnectionOpportunityId ) )
                {
                    connectionOpportunityId = result.ConnectionOpportunityId;
                    // This means we'll be auto-opening a specific connection request modal immediately.
                    boardData.ConnectionRequestId = paramConnectionRequestId;
                }
            }

            // If not, was a connection opportunity page parameter provided?
            var paramConnectionOpportunityId = GetEntityIdFromPageParameter<ConnectionOpportunity>( PageParameterKey.ConnectionOpportunityId, rockContext );
            if ( !connectionOpportunityId.HasValue
                && paramConnectionOpportunityId.HasValue
                && availableOpportunityIds.Contains( paramConnectionOpportunityId.Value ) )
            {
                connectionOpportunityId = paramConnectionOpportunityId;
            }

            // If not, does this person have a connection opportunity preference?
            var personPrefKeyConnectionOpportunityId = boardData.GetPersonPreferenceKey( PersonPreferenceKey.ConnectionOpportunityId );
            int? personPrefConnectionOpportunityId = this.PersonPreferences.GetValue( personPrefKeyConnectionOpportunityId ).AsIntegerOrNull();
            if ( !connectionOpportunityId.HasValue
                && personPrefConnectionOpportunityId.HasValue
                && availableOpportunityIds.Contains( personPrefConnectionOpportunityId.Value ) )
            {
                connectionOpportunityId = personPrefConnectionOpportunityId;
            }

            // If set (and different than the current preference), update preferences with this connection opportunity.
            if ( connectionOpportunityId.HasValue && connectionOpportunityId != personPrefConnectionOpportunityId )
            {
                this.PersonPreferences.SetValue( personPrefKeyConnectionOpportunityId, connectionOpportunityId.ToString() );
            }

            // Get the connection opportunity instance from the database.
            var query = new ConnectionOpportunityService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( co => co.ConnectionType.ConnectionStatuses )
                .Include( co => co.ConnectionType.ConnectionWorkflows )
                .Include( co => co.ConnectionWorkflows )
                .Where( co =>
                    co.IsActive
                    && boardData.AllowedConnectionTypeIds.Contains( co.ConnectionTypeId )
                )
                .OrderBy( co => co.Order )
                .ThenBy( co => co.Name );

            // Fall back to the first record if one was not explicitly selected.
            boardData.ConnectionOpportunity = connectionOpportunityId.HasValue
                ? query.FirstOrDefault( co => co.Id == connectionOpportunityId.Value )
                : query.FirstOrDefault();

            // Does this person have a campus preference for this connection type?
            int? personPrefCampusId = null;
            var personPrefKeyCampusFilter = boardData.GetPersonPreferenceKey( PersonPreferenceKey.CampusFilter );
            if ( personPrefKeyCampusFilter != null )
            {
                personPrefCampusId = this.PersonPreferences.GetValue( personPrefKeyCampusFilter ).AsIntegerOrNull();
                boardData.Filters.CampusId = personPrefCampusId;
            }

            // If a campus page parameter was provided, it overrules any previous preference.
            var paramCampusId = GetEntityIdFromPageParameter<Campus>( PageParameterKey.CampusId, rockContext );
            if ( paramCampusId.HasValue )
            {
                boardData.Filters.CampusId = paramCampusId;

                // If different than the current preference, update preferences with this campus.
                if ( paramCampusId != personPrefCampusId && personPrefKeyCampusFilter != null )
                {
                    this.PersonPreferences.SetValue( personPrefKeyCampusFilter, paramCampusId.ToString() );
                }
            }
        }

        /// <summary>
        /// Gets the [available] filter options, some of which are based on the currently-selected connection opportunity,
        /// and loads them onto the supplied <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data onto which to load the filter options.</param>
        private void GetFilterOptions( RockContext rockContext, ConnectionRequestBoardData boardData )
        {
            boardData.FilterOptions.Connectors = GetConnectors( rockContext, boardData, false );
        }

        /// <summary>
        /// Gets the available "connector" people.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data containing info needed to get the connectors.</param>
        /// <param name="includeCurrentPerson">Whether to include the current person in the returned list of connectors.</param>
        /// <param name="personAliasIdToInclude">An optional, additional person to include in the list of connectors.</param>
        /// <returns>The available "connector" people.</returns>
        private List<ListItemBag> GetConnectors( RockContext rockContext, ConnectionRequestBoardData boardData, bool includeCurrentPerson, int? personAliasIdToInclude = null )
        {
            var connectors = new List<ListItemBag>();

            var campusId = boardData.Filters?.CampusId;

            if ( boardData.ConnectionOpportunity != null )
            {
                connectors.AddRange(
                    new ConnectionOpportunityConnectorGroupService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( g =>
                            g.ConnectionOpportunityId == boardData.ConnectionOpportunity.Id
                            && (
                                !campusId.HasValue
                                || !g.CampusId.HasValue
                                || g.CampusId.Value == campusId.Value
                            )
                        )
                        .SelectMany( g => g.ConnectorGroup.Members )
                        .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                        .Select( m => m.Person )
                        .Distinct()
                        .Where( p => p.Aliases.Any( a => a.AliasPersonId == p.Id ) )
                        .Select( p => new ListItemBag
                        {
                            Value = p.Aliases.First( a => a.AliasPersonId == p.Id ).Id.ToString(),
                            Text = $"{p.NickName} {p.LastName}"
                        } )
                        .ToList()
                );
            }

            var personAliasIdString = CurrentPersonAliasId.ToString();
            if ( includeCurrentPerson && CurrentPersonAliasId > 0 && !connectors.Any( c => c.Value == personAliasIdString ) )
            {
                connectors.Add( new ListItemBag
                {
                    Value = personAliasIdString,
                    Text = $"{CurrentPerson.NickName} {CurrentPerson.LastName}"
                } );
            }

            personAliasIdString = personAliasIdToInclude?.ToString();
            if ( personAliasIdToInclude.GetValueOrDefault() > 0 && !connectors.Any( c => c.Value == personAliasIdString ) )
            {
                var person = new PersonAliasService( rockContext ).GetPersonNoTracking( personAliasIdToInclude.Value );
                if ( person != null )
                {
                    connectors.Add( new ListItemBag
                    {
                        Value = personAliasIdString,
                        Text = $"{person.NickName} {person.LastName}"
                    } );
                }
            }

            return null;
        }

        private void GetFiltersFromURLOrPersonPreferences( RockContext rockContext, ConnectionRequestBoardData boardData )
        {
            //boardData.Filters.CampusId = GetEntityIdFromPageParameter<Campus>( PageParameterKey.CampusId, rockContext );

            //if ( boardData.ConnectionOpportunity == null )
            //{
            //    // The remainder of this method has to do with connection type-specific person preferences.
            //    // If we don't have a connection info, we don't have enough info to get/save these preferences.
            //    return;
            //}

            //int? campusId = null;
            //var campusIdParam = 
            //if ( campusIdParam.HasValue && boardData.ConnectionOpportunity != null )
            //{
            //    // Now that we have a connection opportunity, we can update the person's campus preference if needed.
            //    var key = boardData.GetPersonPreferenceKey( PersonPreferenceKey.CampusFilter );
            //    var personPrefCampusId = this.PersonPreferences.GetValue( key ).AsIntegerOrNull();
            //    if ( campusIdParam != personPrefCampusId )
            //    {
            //        this.PersonPreferences.SetValue( key, campusIdParam.ToString() );
            //    }
            //}
        }

        /// <summary>
        /// Gets the <see cref="IEntity"/> integer ID value if it can be parsed from page parameters, or <see langword="null"/> if not.
        /// <para>
        /// The page parameter's value may be an integer ID (if predictable IDs are allowed by site settings), a Guid, or an IdKey.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The <see cref="IEntity"/> type whose ID should be parsed.</typeparam>
        /// <param name="pageParameterKey">The key of the page parameter from which to parse the ID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="IEntity"/> integer ID value if it can be parsed from page parameters, or <see langword="null"/> if not.</returns>
        private int? GetEntityIdFromPageParameter<T>( string pageParameterKey, RockContext rockContext ) where T : IEntity
        {
            var entityKey = PageParameter( pageParameterKey );
            if ( entityKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var entityTypeId = EntityTypeCache.GetId( typeof( T ) );
            if ( !entityTypeId.HasValue )
            {
                return null;
            }

            return Reflection.GetEntityIdForEntityType( entityTypeId.Value, entityKey, !PageCache.Layout.Site.DisablePredictableIds, rockContext );
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// A runtime object representing the data needed for the block to operate.
        /// <para>
        /// This object is intended to be assembled using a combination of page parameter values, person preferences and existing
        /// database records; to be passed between private helper methods as needed, and ultimately sent back out the door in the
        /// form of view models.
        /// </para>
        /// </summary>
        private class ConnectionRequestBoardData
        {
            /// <summary>
            /// Gets or sets the allowed connection types.
            /// </summary>
            public List<ConnectionRequestBoardConnectionTypeBag> AllowedConnectionTypes { get; set; } = new List<ConnectionRequestBoardConnectionTypeBag>();

            /// <summary>
            /// Gets the allowed connection type IDs.
            /// </summary>
            public IEnumerable<int> AllowedConnectionTypeIds => this.AllowedConnectionTypes.Select( ct => ct.Id );

            /// <summary>
            /// Gets or sets the selected connection opportunity.
            /// </summary>
            public ConnectionOpportunity ConnectionOpportunity { get; set; }

            /// <summary>
            /// Gets or sets the selected connection request identifier, if a specific request should be opened.
            /// </summary>
            public int? ConnectionRequestId { get; set; }

            /// <summary>
            /// Gets or sets the [available] filter options.
            /// </summary>
            public ConnectionRequestBoardFilterOptionsBag FilterOptions { get; } = new ConnectionRequestBoardFilterOptionsBag();

            /// <summary>
            /// Gets or sets the [selected] filters.
            /// </summary>
            public ConnectionRequestBoardFiltersBag Filters { get; } = new ConnectionRequestBoardFiltersBag();

            /// <summary>
            /// Gets the appropriate person preference key for the specified subkey. Most keys will be connection type-specific.
            /// </summary>
            /// <param name="subkey">The subkey.</param>
            /// <returns>The appropriate person preference key for the specified subkey.</returns>
            public string GetPersonPreferenceKey( string subkey )
            {
                if ( subkey == PersonPreferenceKey.ConnectionOpportunityId )
                {
                    // This key - in particular - should span all connection types.
                    return subkey;
                }

                // The rest of the keys are connection type-specific.
                if ( this.ConnectionOpportunity == null )
                {
                    return null;
                }

                return $"{this.ConnectionOpportunity.ConnectionTypeId}-{subkey}";
            }
        }

        #endregion
    }
}

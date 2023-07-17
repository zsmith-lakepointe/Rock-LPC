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
using System.Text.RegularExpressions;

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
        "Connection Request Status Icons Template",
        Key = AttributeKey.ConnectionRequestStatusIconsTemplate,
        Description = "Lava Template that can be used to customize what is displayed for the status icons in the connection request grid.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        DefaultValue = ConnectionRequestStatusIconsTemplateDefaultValue,
        IsRequired = true,
        Order = 4 )]

    [LinkedPage(
        "Group Detail Page",
        Key = AttributeKey.GroupDetailPage,
        Description = "Page used to display group details.",
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        IsRequired = true,
        Order = 5 )]

    [LinkedPage(
        "SMS Link Page",
        Key = AttributeKey.SmsLinkPage,
        Description = "Page that will be linked for SMS enabled phones.",
        DefaultValue = Rock.SystemGuid.Page.NEW_COMMUNICATION,
        IsRequired = true,
        Order = 6 )]

    [BadgesField(
        "Badges",
        Key = AttributeKey.Badges,
        Description = "The badges to display in this block.",
        IsRequired = false,
        Order = 7 )]

    [CodeEditorField(
        "Lava Heading Template",
        Key = AttributeKey.LavaHeadingTemplate,
        Description = "The HTML Content to render above the person’s name. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        IsRequired = false,
        Order = 8 )]

    [CodeEditorField(
        "Lava Badge Bar",
        Key = AttributeKey.LavaBadgeBar,
        Description = "The HTML Content intended to be used as a kind of custom badge bar for the connection request. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        IsRequired = false,
        Order = 9 )]

    [ConnectionTypesField(
        "Connection Types",
        Key = AttributeKey.ConnectionTypes,
        Description = "Optional list of connection types to limit the display to (All will be displayed by default).",
        IsRequired = false,
        Order = 10 )]

    [BooleanField(
        "Limit to Assigned Connections",
        Key = AttributeKey.OnlyShowAssigned,
        Description = "When enabled, only requests assigned to the current person will be shown.",
        DefaultBooleanValue = false,
        IsRequired = true,
        Order = 11 )]

    [LinkedPage(
        "Connection Request History Page",
        Key = AttributeKey.ConnectionRequestHistoryPage,
        Description = "Page used to display history details.",
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        IsRequired = true,
        Order = 12 )]

    [LinkedPage(
        "Bulk Update Requests",
        Key = AttributeKey.BulkUpdateRequestsPage,
        Description = "Page used to update selected connection requests",
        DefaultValue = Rock.SystemGuid.Page.CONNECTION_REQUESTS_BULK_UPDATE,
        IsRequired = true,
        Order = 13 )]

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
            public const string Badges = "Badges";
            public const string BulkUpdateRequestsPage = "BulkUpdateRequestsPage";
            public const string ConnectionRequestHistoryPage = "ConnectionRequestHistoryPage";
            public const string ConnectionRequestStatusIconsTemplate = "ConnectionRequestStatusIconsTemplate";
            public const string ConnectionTypes = "ConnectionTypes";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string LavaBadgeBar = "LavaBadgeBar";
            public const string LavaHeadingTemplate = "LavaHeadingTemplate";
            public const string MaxCards = "MaxCards";
            public const string OnlyShowAssigned = "OnlyShowAssigned";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string SmsLinkPage = "SmsLinkPage";
            public const string WorkflowDetailPage = "WorkflowDetailPage";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
        }

        /// <summary>
        /// Keys for page parameters.
        /// </summary>
        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";                                  // Incoming
            public const string ConnectionOpportunityId = "ConnectionOpportunityId";    // Incoming
            public const string ConnectionRequestGuid = "ConnectionRequestGuid";                        // Outgoing
            public const string ConnectionRequestId = "ConnectionRequestId";            // Incoming
            public const string ConnectionTypeId = "ConnectionTypeId";                                  // Outgoing
            public const string EntitySetId = "EntitySetId";                                            // Outgoing
            public const string WorkflowId = "WorkflowId";                                              // Outgoing
        }

        /// <summary>
        /// Keys for person preferences.
        /// </summary>
        private static class PersonPreferenceKey
        {
            public const string CampusFilter = "CampusFilter";
            public const string ConnectionOpportunityId = "ConnectionOpportunityId";
            public const string ConnectorPersonAliasId = "ConnectorPersonAliasId";
            public const string DateRange = "DateRange";
            public const string LastActivities = "LastActivities";
            public const string PastDueOnly = "PastDueOnly";
            public const string Requester = "Requester";
            public const string SortBy = "SortBy";
            public const string States = "States";
            public const string Statuses = "Statuses";
            public const string ViewMode = "ViewMode";
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

            box.ConnectionTypes = boardData.AllowedConnectionTypeBags;
            box.SelectedOpportunity = GetSelectedConnectionOpportunity( boardData );
            box.MaxCardsPerColumn = GetAttributeValue( AttributeKey.MaxCards ).AsIntegerOrNull() ?? DefaultMaxCards;

            var statusIconsTemplate = GetAttributeValue( AttributeKey.ConnectionRequestStatusIconsTemplate );
            if ( statusIconsTemplate.IsNullOrWhiteSpace() )
            {
                statusIconsTemplate = ConnectionRequestStatusIconsTemplateDefaultValue;
            }

            box.StatusIconsTemplate = Regex.Replace( statusIconsTemplate, @"\s+", " " );
        }

        /// <summary>
        /// Gets the connection request board data needed for the board to operate.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="idOverrides">Optional identifiers to override page parameters and person preferences.</param>
        /// <returns>The connection request board data needed for the board to operate.</returns>
        private ConnectionRequestBoardData GetConnectionRequestBoardData( RockContext rockContext, Dictionary<string, int?> idOverrides = null )
        {
            var boardData = new ConnectionRequestBoardData();

            var block = new BlockService( rockContext ).Get( this.BlockId );
            block.LoadAttributes( rockContext );

            GetAllowedConnectionTypes( rockContext, boardData );
            GetConnectionAndCampusSelections( rockContext, boardData, idOverrides );
            GetFilterOptions( rockContext, boardData );
            GetFiltersFromPersonPreferences( boardData );

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
            var allowedConnectionTypeBags = new List<ConnectionRequestBoardConnectionTypeBag>();

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
                var connectionTypeBag = allowedConnectionTypeBags.FirstOrDefault( t => t.Id == opportunity.ConnectionType.Id );
                if ( connectionTypeBag == null )
                {
                    connectionTypeBag = new ConnectionRequestBoardConnectionTypeBag
                    {
                        Id = opportunity.ConnectionType.Id,
                        Name = opportunity.ConnectionType.Name,
                        IconCssClass = opportunity.ConnectionType.IconCssClass,
                        Order = opportunity.ConnectionType.Order,
                        ConnectionOpportunities = new List<ConnectionRequestBoardConnectionOpportunityBag>()
                    };

                    allowedConnectionTypeBags.Add( connectionTypeBag );
                }

                // Add the opportunity.
                connectionTypeBag.ConnectionOpportunities.Add( new ConnectionRequestBoardConnectionOpportunityBag
                {
                    Id = opportunity.Id,
                    PublicName = opportunity.PublicName,
                    IconCssClass = opportunity.IconCssClass,
                    ConnectionTypeName = connectionTypeBag.Name,
                    PhotoUrl = opportunity.PhotoUrl,
                    Order = opportunity.Order,
                    IsFavorite = favoriteOpportunityIds.Contains( opportunity.Id )
                } );
            }

            // Sort each type's opportunities.
            foreach ( var connectionTypeBag in allowedConnectionTypeBags )
            {
                connectionTypeBag.ConnectionOpportunities = connectionTypeBag.ConnectionOpportunities
                    .OrderBy( co => co.Order )
                    .ThenBy( co => co.PublicName )
                    .ToList();
            }

            // Sort and add the allowed types.
            boardData.AllowedConnectionTypeBags = allowedConnectionTypeBags
                .Where( ct => ct.ConnectionOpportunities.Any() )
                .OrderBy( ct => ct.Order )
                .ThenBy( ct => ct.Name )
                .ToList();
        }

        /// <summary>
        /// Gets the selected connection request, connection opportunity and campus, loading them onto the supplied
        /// <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data onto which to load the selected connection request, connection opportunity and campus.</param>
        /// <param name="idOverrides">Optional identifiers to override page parameters and person preferences.</param>
        private void GetConnectionAndCampusSelections( RockContext rockContext, ConnectionRequestBoardData boardData, Dictionary<string, int?> idOverrides = null )
        {
            int? connectionOpportunityId = null;

            var availableOpportunityIds = boardData.AllowedConnectionTypeBags
                .SelectMany( ct => ct.ConnectionOpportunities.Select( co => co.Id ) )
                .ToList();

            // If a connection request selection was provided, it takes priority since it's more specific.
            var selectedConnectionRequestId = GetEntityIdFromPageParameterOrOverride<ConnectionRequest>( PageParameterKey.ConnectionRequestId, rockContext, idOverrides );
            if ( selectedConnectionRequestId.HasValue )
            {
                var result = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( cr => cr.Id == selectedConnectionRequestId.Value )
                    .Select( cr => new
                    {
                        cr.ConnectionOpportunityId
                    } )
                    .FirstOrDefault();

                if ( result != null && availableOpportunityIds.Contains( result.ConnectionOpportunityId ) )
                {
                    connectionOpportunityId = result.ConnectionOpportunityId;
                    // This means we'll be auto-opening a specific connection request modal immediately.
                    boardData.ConnectionRequestId = selectedConnectionRequestId;
                }
            }

            // If not, was a connection opportunity selection provided?
            var selectedConnectionOpportunityId = GetEntityIdFromPageParameterOrOverride<ConnectionOpportunity>( PageParameterKey.ConnectionOpportunityId, rockContext, idOverrides );
            if ( !connectionOpportunityId.HasValue
                && selectedConnectionOpportunityId.HasValue
                && availableOpportunityIds.Contains( selectedConnectionOpportunityId.Value ) )
            {
                connectionOpportunityId = selectedConnectionOpportunityId;
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

            // If a campus selection was provided, it overrules any previous preference.
            var selectedCampusId = GetEntityIdFromPageParameterOrOverride<Campus>( PageParameterKey.CampusId, rockContext, idOverrides );
            if ( selectedCampusId.HasValue )
            {
                boardData.Filters.CampusId = selectedCampusId;

                // If different than the current preference, update preferences with this campus.
                if ( selectedCampusId != personPrefCampusId && personPrefKeyCampusFilter != null )
                {
                    this.PersonPreferences.SetValue( personPrefKeyCampusFilter, selectedCampusId.ToString() );
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
            GetCampuses( rockContext, boardData );
            GetConnectionStatuses( rockContext, boardData );
            GetConnectionStates( boardData );
            GetConnectionActivityTypes( rockContext, boardData );
            GetSortProperties( boardData );
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
                var connectorPeople = new ConnectionOpportunityConnectorGroupService( rockContext )
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
                        .Where( p => includeCurrentPerson || p.Id != CurrentPersonId )
                        .Where( p => p.Aliases.Any( a => a.AliasPersonId == p.Id ) )
                        .OrderBy( p => p.LastName )
                        .ThenBy( p => p.NickName )
                        .Select( p => new
                        {
                            p.Aliases.FirstOrDefault( a => a.AliasPersonId == p.Id ).Id,
                            p.NickName,
                            p.LastName
                        } )
                        .ToList();

                connectors.AddRange( connectorPeople
                    .Select( c => new ListItemBag
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.NickName} {c.LastName}"
                    } )
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

            return connectors;
        }

        /// <summary>
        /// Gets the available campuses and loads them onto the supplied <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data onto which to load the campuses.</param>
        private void GetCampuses( RockContext rockContext, ConnectionRequestBoardData boardData )
        {
            boardData.FilterOptions.Campuses = CampusCache.All( rockContext )
                .Where( c => c.IsActive != false )
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name )
                .Select( c => new ListItemBag
                {
                    Value = c.Id.ToString(),
                    Text = c.ShortCode.IsNullOrWhiteSpace()
                        ? c.Name
                        : c.ShortCode
                } )
                .ToList();

            // If there's only one campus, remove any previously-applied campus filtering.
            if ( boardData.FilterOptions.Campuses.Count <= 1 )
            {
                boardData.Filters.CampusId = null;
            }
        }

        /// <summary>
        /// Gets the available connection statuses for the selected connection opportunity and loads them onto the supplied
        /// <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data onto which to load the connection statuses.</param>
        private void GetConnectionStatuses( RockContext rockContext, ConnectionRequestBoardData boardData )
        {
            boardData.FilterOptions.ConnectionStatuses = boardData.ConnectionOpportunity == null
                ? null
                : new ConnectionOpportunityService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( co => co.Id == boardData.ConnectionOpportunity.Id )
                    .SelectMany( co => co.ConnectionType.ConnectionStatuses )
                    .Where( cs => cs.IsActive )
                    .OrderBy( cs => cs.Order )
                    .ThenByDescending( a => a.IsDefault )
                    .ThenBy( cs => cs.Name )
                    .Select( cs => new ListItemBag
                    {
                        Value = cs.Id.ToString(),
                        Text = cs.Name
                    } )
                    .ToList();
        }

        /// <summary>
        /// Gets the available connection states and loads them onto the supplied <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="boardData">The board data onto which to load the connection states.</param>
        private void GetConnectionStates( ConnectionRequestBoardData boardData )
        {
            boardData.FilterOptions.ConnectionStates = SettingsExtensions.GetListItemBagList<ConnectionState>();
        }

        /// <summary>
        /// Gets the available connection activity types for the selected connection opportunity and loads them onto the supplied
        /// <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="boardData">The board data onto which to load the connection activity types.</param>
        private void GetConnectionActivityTypes( RockContext rockContext, ConnectionRequestBoardData boardData )
        {
            boardData.FilterOptions.ConnectionActivityTypes = boardData.ConnectionOpportunity == null
                ? null
                : new ConnectionActivityTypeService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( t =>
                        t.ConnectionTypeId == boardData.ConnectionOpportunity.ConnectionTypeId
                        && t.IsActive
                    )
                    .OrderBy( t => t.Name )
                    .ThenBy( t => t.Id )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    } )
                    .ToList();
        }

        /// <summary>
        /// Gets the available sort properties and loads them onto the supplied <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="boardData">The board data onto which to load the sort properties.</param>
        private void GetSortProperties( ConnectionRequestBoardData boardData )
        {
            boardData.FilterOptions.SortProperties = new List<ConnectionRequestBoardSortPropertyBag>
            {
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.Order, Title = string.Empty },
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.Requestor, Title = "Requestor" },
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.Connector, Title = "Connector" },
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.DateAdded, Title = "Date Added", SubTitle = "Oldest First" },
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.DateAddedDesc, Title = "Date Added", SubTitle = "Newest First" },
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.LastActivity, Title = "Last Activity", SubTitle = "Oldest First" },
                new ConnectionRequestBoardSortPropertyBag { SortBy = ConnectionRequestViewModelSortProperty.LastActivityDesc, Title = "Last Activity", SubTitle = "Newest First" }
            };
        }

        /// <summary>
        /// Gets the selected filters from person preferences, loading them onto the supplied <see cref="ConnectionRequestBoardData"/> instance.
        /// </summary>
        /// <param name="boardData">The board data onto which to load the filters.</param>
        private void GetFiltersFromPersonPreferences( ConnectionRequestBoardData boardData )
        {
            if ( boardData.ConnectionOpportunity == null )
            {
                // These preferences are all connection type-specific, so if we don't have a connection opportunity (which will have a type)
                // loaded at this point, we can't load preferences.
                return;
            }

            boardData.Filters.ConnectorPersonAliasId = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.ConnectorPersonAliasId ) )
                .AsIntegerOrNull();

            boardData.Filters.RequesterPersonAliasId = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.Requester ) )
                .AsIntegerOrNull();

            // Campus ID has already been set.

            boardData.Filters.DateRange = RockDateTimeHelper.CreateSlidingDateRangeBagFromDelimitedValues(
                    this.PersonPreferences.GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.DateRange ) )
                );

            boardData.Filters.PastDueOnly = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.PastDueOnly ) )
                .AsBoolean();

            boardData.Filters.ConnectionStatuses = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.Statuses ) )
                .SplitDelimitedValues()
                .ToList();

            boardData.Filters.ConnectionStates = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.States ) )
                .SplitDelimitedValues()
                .ToList();

            boardData.Filters.ConnectionActivityTypes = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.LastActivities ) )
                .SplitDelimitedValues()
                .ToList();

            boardData.Filters.SortProperty = this.PersonPreferences
                .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.SortBy ) )
                .ConvertToEnumOrNull<ConnectionRequestViewModelSortProperty>() ?? ConnectionRequestViewModelSortProperty.Order;
        }

        /// <summary>
        /// Gets the selected connection opportunity and supporting information from the supplied <see cref="ConnectionRequestBoardData"/> instance
        /// and person preferences.
        /// </summary>
        /// <param name="boardData"></param>
        /// <returns>The selected connection opportunity and supporting information.</returns>
        private ConnectionRequestBoardSelectedOpportunityBag GetSelectedConnectionOpportunity( ConnectionRequestBoardData boardData )
        {
            var selectedOpportunity = new ConnectionRequestBoardSelectedOpportunityBag
            {
                ConnectionOpportunity = boardData.ConnectionOpportunityBag,
                ConnectionRequestId = boardData.ConnectionRequestId,
                FilterOptions = boardData.FilterOptions,
                Filters = boardData.Filters
            };

            if ( selectedOpportunity.ConnectionOpportunity != null )
            {
                selectedOpportunity.IsCardViewMode = this.PersonPreferences
                    .GetValue( boardData.GetPersonPreferenceKey( PersonPreferenceKey.ViewMode ) )
                    .AsBooleanOrNull() ?? true;
            }

            return selectedOpportunity;
        }

        /// <summary>
        /// Validates and selects the connection opportunity with the specified identifier.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="connectionOpportunityId">The identifier of the connection opportunity to select.</param>
        /// <returns>An object containing the validated / selected connection opportunity and supporting information.</returns>
        private ConnectionRequestBoardSelectedOpportunityBag ValidateAndSelectConnectionOpportunity( RockContext rockContext, int connectionOpportunityId )
        {
            var boardData = GetConnectionRequestBoardData( rockContext, new Dictionary<string, int?>
            {
                { PageParameterKey.ConnectionOpportunityId, connectionOpportunityId },
                { PageParameterKey.ConnectionRequestId, null }
            } );

            return GetSelectedConnectionOpportunity( boardData );
        }

        /// <summary>
        /// Gets the <see cref="IEntity"/> integer ID value if it exists in the override collection or can be parsed from page parameters,
        /// or <see langword="null"/> if not.
        /// <para>
        /// The page parameter's value may be an integer ID (if predictable IDs are allowed by site settings), a Guid, or an IdKey.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The <see cref="IEntity"/> type whose ID should be parsed.</typeparam>
        /// <param name="pageParameterKey">The key of the page parameter from which to parse the ID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="idOverrides">Optional identifiers to override page parameters.</param>
        /// <returns>The <see cref="IEntity"/> integer ID value if it exists in the override collection or can be parsed from page parameters,
        /// or <see langword="null"/> if not.</returns>
        private int? GetEntityIdFromPageParameterOrOverride<T>( string pageParameterKey, RockContext rockContext, Dictionary<string, int?> idOverrides = null ) where T : IEntity
        {
            if ( idOverrides?.TryGetValue( pageParameterKey, out int? id ) == true )
            {
                return id;
            }

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

        #region Block Actions

        /// <summary>
        /// Selects the specified connection opportunity.
        /// </summary>
        /// <param name="connectionOpportunityId">The identifier of the connection opportunity to select.</param>
        /// <returns>An object containing the selected connection opportunity and supporting information.</returns>
        [BlockAction]
        public BlockActionResult SelectConnectionOpportunity( int connectionOpportunityId )
        {
            using ( var rockContext = new RockContext() )
            {
                var selectedOpportunity = ValidateAndSelectConnectionOpportunity( rockContext, connectionOpportunityId );

                return ActionOk( selectedOpportunity );
            }
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
            /// Gets or sets the allowed connection type bags.
            /// </summary>
            public List<ConnectionRequestBoardConnectionTypeBag> AllowedConnectionTypeBags { get; set; } = new List<ConnectionRequestBoardConnectionTypeBag>();

            /// <summary>
            /// Gets the allowed connection type IDs.
            /// </summary>
            public IEnumerable<int> AllowedConnectionTypeIds => this.AllowedConnectionTypeBags?.Select( ct => ct.Id ) ?? new List<int>();

            /// <summary>
            /// Gets or sets the selected connection opportunity.
            /// </summary>
            public ConnectionOpportunity ConnectionOpportunity { get; set; }

            /// <summary>
            /// Gets the selected connection opportunity bag.
            /// </summary>
            public ConnectionRequestBoardConnectionOpportunityBag ConnectionOpportunityBag => this.ConnectionOpportunity == null
                ? null
                : this.AllowedConnectionTypeBags
                    ?.SelectMany( ct => ct.ConnectionOpportunities )
                    ?.FirstOrDefault( co => co.Id == this.ConnectionOpportunity.Id );

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

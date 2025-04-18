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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    #region Block Attributes
    [DisplayName( "Group Attendance Detail (Legacy)" )]
    [Category( "Groups" )]
    [Description( "Lists the group members for a specific occurrence date time and allows selecting if they attended or not." )]

    [BooleanField( "Allow Add",
        Description = "Should block support adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?",
        DefaultBooleanValue = true,
        Order = 0,
        Key = AttributeKey.AllowAdd )]
    [BooleanField( "Allow Adding Person",
        Description = "Should block support adding new people as attendees?",
        DefaultBooleanValue = false,
        Order = 1,
        Key = AttributeKey.AllowAddingPerson )]
    [CustomDropdownListField( "Add Person As",
        Description = "'Attendee' will only add the person to attendance. 'Group Member' will add them to the group with the default group role.",
        ListSource = "Attendee,Group Member",
        IsRequired = true,
        DefaultValue = "Attendee",
        Order = 2,
        Key = AttributeKey.AddPersonAs )]
    [LinkedPage( "Group Member Add Page",
        Description = "Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.GroupMemberAddPage )]
    [BooleanField( "Allow Campus Filter",
        Description = "Should block add an option to allow filtering people and attendance counts by campus?",
        DefaultBooleanValue = false,
        Order = 4,
        Key = AttributeKey.AllowCampusFilter )]
    [WorkflowTypeField( "Workflow",
        Description = "An optional workflow type to launch whenever attendance is saved. The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.",
        AllowMultiple = false,
        IsRequired = false,
        Order = 5,
        Key = AttributeKey.Workflow )]
    [MergeTemplateField( "Attendance Roster Template",
        IsRequired = false,
        Order = 6,
        Key = AttributeKey.AttendanceRosterTemplate )]
    [CodeEditorField( "Lava Template",
        Description = "An optional lava template to appear next to each person in the list.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        Order = 7,
        Key = AttributeKey.LavaTemplate )]
    [BooleanField( "Restrict Future Occurrence Date",
        Description = "Should user be prevented from selecting a future Occurrence date?",
        DefaultBooleanValue = false,
        Order = 8,
        Key = AttributeKey.RestrictFutureOccurrenceDate )]
    [BooleanField( "Show Notes", "Should the notes field be displayed?",
        DefaultBooleanValue = true,
        Order = 9,
        Key = AttributeKey.ShowNotes )]
    [TextField( "Attendance Note Label",
        Description = "The text to use to describe the notes",
        IsRequired = true,
        DefaultValue = "Notes",
        Order = 10,
        Key = AttributeKey.AttendanceNoteLabel )]
    [EnumsField( "Send Summary Email To",
        EnumSourceType = typeof( SendSummaryEmailType ),
        IsRequired = false,
        Order = 11,
        Key = AttributeKey.SendSummaryEmailTo )]
    [SystemCommunicationField( "Attendance Email",
        Description = "The System Email to use to send the attendance",
        IsRequired = false,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.ATTENDANCE_NOTIFICATION,
        Order = 12,
        Key = AttributeKey.AttendanceEmailTemplate )]
    [BooleanField( "Allow Sorting",
        Description = "Should the block allow sorting the Member's list by First Name or Last Name?",
        DefaultBooleanValue = true,
        Order = 13,
        Key = AttributeKey.AllowSorting )]
    [DefinedValueField( "Configured Attendance Types",
        Description = "The Attendance types that an occurrence can have. If no or one Attendance types selected none will be shown.",
        AllowMultiple = true,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CHECK_IN_ATTENDANCE_TYPES,
        IsRequired = false,
        DefaultValue = "",
        Order = 14,
        Key = AttributeKey.AttendanceTypes )]
    [TextField( "Attendance Type Label",
        Description = "The label that will be shown for the attendance types section.",
        DefaultValue = "Attendance Location",
        Order = 15,
        Key = AttributeKey.AttendanceTypeLabel )]
    #endregion
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.GROUP_ATTENDANCE_DETAIL )]
    public partial class GroupAttendanceDetail : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys for attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string AllowAdd = "AllowAdd";
            public const string AllowAddingPerson = "AllowAddingPerson";
            public const string AddPersonAs = "AddPersonAs";
            public const string GroupMemberAddPage = "GroupMemberAddPage";
            public const string AllowCampusFilter = "AllowCampusFilter";
            public const string Workflow = "Workflow";
            public const string AttendanceRosterTemplate = "AttendanceRosterTemplate";
            public const string LavaTemplate = "LavaTemplate";
            public const string RestrictFutureOccurrenceDate = "RestrictFutureOccurrenceDate";
            public const string ShowNotes = "ShowNotes";
            public const string AttendanceNoteLabel = "AttendanceNoteLabel";
            public const string SendSummaryEmailTo = "SendSummaryEmailTo";
            public const string AttendanceEmailTemplate = "AttendanceEmailTemplate";
            public const string AllowSorting = "AllowSorting";
            public const string AttendanceTypes = "AttendanceTypes";
            public const string AttendanceTypeLabel = "AttendanceTypeLabel";
        }

        /// <summary>
        /// Key for page parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupTypeIds = "GroupTypeIds";
            public const string OccurrenceId = "OccurrenceId";
            public const string Occurrence = "Occurrence";
            public const string Date = "Date";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
        }
        #endregion Keys

        #region Fields

        /// <summary>
        ///
        /// </summary>
        private enum SendSummaryEmailType
        {
            /// <summary>
            /// Group Leaders
            /// </summary>
            GroupLeaders = 1,

            /// <summary>
            /// All Group Members (note: all active group members)
            /// </summary>
            AllGroupMembers = 2,

            /// <summary>
            /// Parent Group Leaders
            /// </summary>
            ParentGroupLeaders = 3,

            /// <summary>
            /// Individual Entering Attendance
            /// </summary>
            IndividualEnteringAttendance = 4,

            /// <summary>
            /// Group Administrator
            /// </summary>
            GroupAdministrator = 5
        }

        #endregion

        #region Private Variables

        private RockContext _rockContext = null;
        private Group _group = null;
        private bool _canManageMembers = false;
        private bool _allowAdd = false;
        private bool _allowCampusFilter = false;
        private AttendanceOccurrence _occurrence = null;
        private List<GroupAttendanceAttendee> _attendees;
        private const string TOGGLE_SETTING = "Attendance_List_Sorting_Toggle";

        #endregion

        #region Control Methods

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            _attendees = ViewState["Attendees"] as List<GroupAttendanceAttendee>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RegisterScript();

            _rockContext = new RockContext();

            int groupId = PageParameter( PageParameterKey.GroupId ).AsInteger();
            _group = new GroupService( _rockContext )
                .Queryable( "GroupType,Schedule" ).AsNoTracking()
                .FirstOrDefault( g => g.Id == groupId );

            if ( _group != null && ( _group.IsAuthorized( Authorization.MANAGE_MEMBERS, CurrentPerson ) || _group.IsAuthorized( Authorization.EDIT, CurrentPerson ) ) )
            {
                lHeading.Text = _group.Name + " Attendance";
                _canManageMembers = true;
            }

            dpOccurrenceDate.AllowFutureDateSelection = !GetAttributeValue( AttributeKey.RestrictFutureOccurrenceDate ).AsBoolean();
            _allowAdd = GetAttributeValue( AttributeKey.AllowAdd ).AsBoolean();

            _allowCampusFilter = GetAttributeValue( AttributeKey.AllowCampusFilter ).AsBoolean();
            bddlCampus.Visible = _allowCampusFilter;
            if ( _allowCampusFilter )
            {
                bddlCampus.DataSource = CampusCache.All();
                bddlCampus.DataBind();
                bddlCampus.Items.Insert( 0, new ListItem( "All Campuses", "0" ) );
            }

            dtNotes.Label = GetAttributeValue( AttributeKey.AttendanceNoteLabel );
            dtNotes.Visible = GetAttributeValue( AttributeKey.ShowNotes ).AsBooleanOrNull() ?? true;
            tglSort.Visible = GetAttributeValue( AttributeKey.AllowSorting ).AsBooleanOrNull() ?? true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                tglSort.Checked = GetUserPreference( TOGGLE_SETTING ).AsBoolean( true );
            }

            if ( !_canManageMembers )
            {
                nbNotice.Heading = "Sorry";
                nbNotice.Text = "<p>You're not authorized to update the attendance for the selected group.</p>";
                nbNotice.NotificationBoxType = NotificationBoxType.Danger;
                nbNotice.Visible = true;
                pnlDetails.Visible = false;
            }
            else
            {
                _occurrence = GetOccurrence();
                if ( !Page.IsPostBack )
                {
                    if ( _allowCampusFilter )
                    {
                        var campus = CampusCache.Get( GetBlockUserPreference( "Campus" ).AsInteger() );
                        if ( campus != null )
                        {
                            bddlCampus.Title = campus.Name;
                            bddlCampus.SetValue( campus.Id );
                        }
                    }

                    BindLocations();
                    ShowDetails();
                }
                else
                {
                    if ( _attendees != null )
                    {
                        foreach ( var item in lvMembers.Items )
                        {
                            var hfMember = item.FindControl( "hfMember" ) as HiddenField;
                            var cbMember = item.FindControl( "cbMember" ) as CheckBox;

                            if ( hfMember != null && cbMember != null )
                            {
                                int personId = hfMember.ValueAsInt();

                                var attendance = _attendees.Where( a => a.PersonId == personId ).FirstOrDefault();
                                if ( attendance != null )
                                {
                                    attendance.Attended = cbMember.Checked;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["Attendees"] = _attendees;
            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( _group != null && _occurrence != null )
            {
                if ( SaveAttendance() )
                {
                    EmailAttendanceSummary();

                    var qryParams = new Dictionary<string, string> { { "GroupId", _group.Id.ToString() } };

                    var groupTypeIds = PageParameter( PageParameterKey.GroupTypeIds );
                    if ( !string.IsNullOrWhiteSpace( groupTypeIds ) )
                    {
                        qryParams.Add( "GroupTypeIds", groupTypeIds );
                    }

                    NavigateToParentPage( qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( _group != null )
            {
                var qryParams = new Dictionary<string, string> { { "GroupId", _group.Id.ToString() } };

                var groupTypeIds = PageParameter( PageParameterKey.GroupTypeIds );
                if ( !string.IsNullOrWhiteSpace( groupTypeIds ) )
                {
                    qryParams.Add( "GroupTypeIds", groupTypeIds );
                }

                NavigateToParentPage( qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbPrintAttendanceRoster control.
        /// NOTE: lbPrintAttendanceRoster is a full postback since we are returning a download of the roster
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPrintAttendanceRoster_Click( object sender, EventArgs e )
        {
            nbPrintRosterWarning.Visible = false;
            var rockContext = new RockContext();

            Dictionary<int, object> mergeObjectsDictionary = new Dictionary<int, object>();
            if ( _attendees != null )
            {
                var personIdList = _attendees.Select( a => a.PersonId ).ToList();
                var personList = new PersonService( rockContext ).GetByIds( personIdList );
                foreach ( var person in personList.OrderBy( a => a.LastName ).ThenBy( a => a.NickName ) )
                {
                    mergeObjectsDictionary.AddOrIgnore( person.Id, person );
                }
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "Group", this._group );
            mergeFields.Add( "AttendanceDate", this._occurrence.OccurrenceDate );

            var mergeTemplate = new MergeTemplateService( rockContext ).Get( this.GetAttributeValue( AttributeKey.AttendanceRosterTemplate ).AsGuid() );

            if ( mergeTemplate == null )
            {
                this.LogException( new Exception( "Error printing Attendance Roster: No merge template selected. Please configure an 'Attendance Roster Template' in the block settings." ) );
                nbPrintRosterWarning.Visible = true;
                nbPrintRosterWarning.Text = "Unable to print Attendance Roster: No merge template selected. Please configure an 'Attendance Roster Template' in the block settings.";
                return;
            }

            MergeTemplateType mergeTemplateType = mergeTemplate.GetMergeTemplateType();
            if ( mergeTemplateType == null )
            {
                this.LogException( new Exception( "Error printing Attendance Roster: Unable to determine Merge Template Type from the 'Attendance Roster Template' in the block settings." ) );
                nbPrintRosterWarning.Visible = true;
                nbPrintRosterWarning.Text = "Error printing Attendance Roster: Unable to determine Merge Template Type from the 'Attendance Roster Template' in the block settings.";
                return;
            }

            BinaryFile outputBinaryFileDoc = null;

            var mergeObjectList = mergeObjectsDictionary.Select( a => a.Value ).ToList();

            outputBinaryFileDoc = mergeTemplateType.CreateDocument( mergeTemplate, mergeObjectList, mergeFields );

            if ( mergeTemplateType.Exceptions != null && mergeTemplateType.Exceptions.Any() )
            {
                if ( mergeTemplateType.Exceptions.Count == 1 )
                {
                    this.LogException( mergeTemplateType.Exceptions[0] );
                }
                else if ( mergeTemplateType.Exceptions.Count > 50 )
                {
                    this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions for top 50.", mergeTemplate.Name ), mergeTemplateType.Exceptions.Take( 50 ).ToList() ) );
                }
                else
                {
                    this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions", mergeTemplate.Name ), mergeTemplateType.Exceptions.ToList() ) );
                }
            }

            var baseUrl = ResolveRockUrl( "~/GetFile.ashx" );
            var getFileUrl = $"{baseUrl}?Guid={outputBinaryFileDoc.Guid}&attachment=true";
            Response.Redirect( getFileUrl, false );
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindSchedules( ddlLocation.SelectedValueAsInt() );
        }

        /// <summary>
        /// Handles the SelectionChanged event of the bddlCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddlCampus_SelectionChanged( object sender, EventArgs e )
        {
            SetBlockUserPreference( "Campus", bddlCampus.SelectedValue );
            var campus = CampusCache.Get( bddlCampus.SelectedValueAsInt() ?? 0 );
            bddlCampus.Title = campus != null ? campus.Name : "All Campuses";
            BindAttendees();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppAddPerson_SelectPerson( object sender, EventArgs e )
        {
            string template = GetAttributeValue( AttributeKey.LavaTemplate );

            if ( ppAddPerson.PersonId.HasValue )
            {
                if ( !_attendees.Any( a => a.PersonId == ppAddPerson.PersonId.Value ) )
                {
                    var rockContext = new RockContext();
                    var person = new PersonService( rockContext ).Get( ppAddPerson.PersonId.Value );
                    if ( person != null )
                    {
                        string addPersonAs = GetAttributeValue( AttributeKey.AddPersonAs );
                        if ( !addPersonAs.IsNullOrWhiteSpace() && addPersonAs == "Group Member" )
                        {
                            AddPersonAsGroupMember( person, rockContext );
                        }

                        var attendee = new GroupAttendanceAttendee();
                        attendee.PersonId = person.Id;
                        attendee.NickName = person.NickName;
                        attendee.LastName = person.LastName;
                        attendee.Attended = true;
                        attendee.CampusIds = person.GetCampusIds();

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Person", person );
                        mergeFields.Add( "Attended", true );
                        attendee.MergedTemplate = template.ResolveMergeFields( mergeFields );
                        _attendees.Add( attendee );
                        BindAttendees();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvPendingMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvPendingMembers_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            if ( _group != null && e.CommandName == "Add" )
            {
                int personId = e.CommandArgument.ToString().AsInteger();

                var rockContext = new RockContext();

                foreach ( var groupMember in new GroupMemberService( rockContext )
                    .GetByGroupIdAndPersonId( _group.Id, personId ) )
                {
                    if ( groupMember.GroupMemberStatus == GroupMemberStatus.Pending )
                    {
                        groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                    }
                }

                rockContext.SaveChanges();

                ShowDetails();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddMember_Click( object sender, EventArgs e )
        {
            var personAddPage = GetAttributeValue( AttributeKey.GroupMemberAddPage );

            if ( !personAddPage.IsNullOrWhiteSpace() )
            {
                // Redirect to the add page provided
                if ( _group != null && _occurrence != null )
                {
                    if ( SaveAttendance() )
                    {
                        var queryParams = new Dictionary<string, string>();
                        queryParams.Add( "GroupId", _group.Id.ToString() );
                        queryParams.Add( "GroupName", _group.Name );
                        queryParams.Add( "ReturnUrl", Request.QueryString["returnUrl"] ?? Server.UrlEncode( Request.RawUrl ) );
                        NavigateToLinkedPage( AttributeKey.GroupMemberAddPage, queryParams );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the DataBinding event of the cbMember control.
        /// Set the Full Name Display of the cbMember check box
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbMember_DataBinding( object sender, EventArgs e )
        {
            var checkBox = sender as RockCheckBox;
            var parent = checkBox.Parent as ListViewDataItem;
            var data = parent.DataItem as GroupAttendanceAttendee;
            string displayName = string.Empty;

            if ( data != null )
            {
                if ( tglSort.Visible && tglSort.Checked )
                {
                    displayName = data.LastName + ", " + data.NickName;
                }
                else
                {
                    displayName = data.NickName + " " + data.LastName;
                }
                // LPC CODE to clarify nameless person records
                if (displayName.IsNullOrWhiteSpace() || displayName.Trim().Equals(","))
                {
                    displayName = string.Concat("Nameless Person", data.MobileNumber.IsNotNullOrWhiteSpace()  ? string.Format(" - {0}", data.MobileNumber) : String.Empty); 
                }
                // END OF LPC CODE

                checkBox.Text = string.Format( "{0} {1}", data.MergedTemplate, displayName );
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglSort UI control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglSort_CheckedChanged( object sender, EventArgs e )
        {
            SetUserPreference( TOGGLE_SETTING, tglSort.Checked.ToString() );
            BindAttendees();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Adds the person as group member.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        private void AddPersonAsGroupMember( Person person, RockContext rockContext )
        {
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            GroupTypeRole role = new GroupTypeRoleService( rockContext ).Get( _group.GroupType.DefaultGroupRoleId ?? 0 );

            var groupMember = new GroupMember { Id = 0 };
            groupMember.GroupId = _group.Id;

            // Check to see if the person is already a member of the group/role
            var existingGroupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId( _group.Id, person.Id, _group.GroupType.DefaultGroupRoleId ?? 0 );

            if ( existingGroupMember != null )
            {
                return;
            }

            groupMember.PersonId = person.Id;
            groupMember.GroupRoleId = role.Id;
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

            if ( groupMember.Id.Equals( 0 ) )
            {
                groupMemberService.Add( groupMember );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the occurrence items.
        /// </summary>
        private AttendanceOccurrence GetOccurrence()
        {
            AttendanceOccurrence occurrence = null;

            var occurrenceService = new AttendanceOccurrenceService( _rockContext );

            // Check to see if a occurrence id was specified on the query string, and if so, query for it
            int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();
            if ( occurrenceId.HasValue && occurrenceId.Value > 0 )
            {
                occurrence = occurrenceService.Get( occurrenceId.Value );

                // If we have a valid occurrence return it now (the date,location,schedule cannot be changed for an existing occurrence)
                if ( occurrence != null )
                {
                    return occurrence;
                }
            }

            // Set occurrence values from query string
            var occurrenceDate = PageParameter( PageParameterKey.Date ).AsDateTime() ?? PageParameter( PageParameterKey.Occurrence ).AsDateTime();
            var locationId = PageParameter( PageParameterKey.LocationId ).AsIntegerOrNull();
            var scheduleId = PageParameter( PageParameterKey.ScheduleId ).AsIntegerOrNull();

            if ( scheduleId == null )
            {
                // if no specific schedule was specified in the URL, use the group's scheduleId
                scheduleId = _group.ScheduleId;
            }

            // If this is a postback, check to see if date/location/schedule were updated
            if ( Page.IsPostBack && _allowAdd )
            {
                if ( dpOccurrenceDate.Visible && dpOccurrenceDate.SelectedDate.HasValue )
                {
                    occurrenceDate = dpOccurrenceDate.SelectedDate.Value;
                }

                if ( ddlLocation.Visible && ddlLocation.SelectedValueAsInt().HasValue )
                {
                    locationId = ddlLocation.SelectedValueAsInt().Value;
                }

                if ( ddlSchedule.Visible && ddlSchedule.SelectedValueAsInt().HasValue )
                {
                    scheduleId = ddlSchedule.SelectedValueAsInt().Value;
                }
            }

            if ( occurrence == null && occurrenceDate.HasValue )
            {
                // if no specific occurrenceId was specified, try to find a matching occurrence from Date, GroupId, Location, ScheduleId
                occurrence = occurrenceService.Get( occurrenceDate.Value.Date, _group.Id, locationId, scheduleId );
            }

            // If an occurrence date was included, but no occurrence was found with that date, and new
            // occurrences can be added, create a new one
            if ( occurrence == null && _allowAdd )
            {
                // Create a new occurrence record and return it
                return new AttendanceOccurrence
                {
                    Group = _group,
                    GroupId = _group.Id,
                    OccurrenceDate = occurrenceDate ?? RockDateTime.Today,
                    LocationId = locationId,
                    ScheduleId = scheduleId,
                };
            }

            return occurrence;
        }

        /// <summary>
        /// Binds the locations.
        /// </summary>
        private void BindLocations()
        {
            var locations = new Dictionary<int, string> { { 0, string.Empty } };

            if ( _group != null )
            {
                var locationPaths = new Dictionary<int, string>();
                var locationService = new LocationService( _rockContext );

                foreach ( var location in _group.GroupLocations
                    .Where( l =>
                        l.Location.Name != null &&
                        l.Location.Name != string.Empty )
                    .Select( l => l.Location ) )
                {
                    // Get location path
                    string parentLocationPath = string.Empty;
                    if ( location.ParentLocationId.HasValue )
                    {
                        var locId = location.ParentLocationId.Value;
                        if ( !locationPaths.ContainsKey( locId ) )
                        {
                            locationPaths.Add( locId, locationService.GetPath( locId ) );
                        }

                        parentLocationPath = locationPaths[locId];
                    }

                    if ( !locations.ContainsKey( location.Id ) )
                    {
                        locations.Add( location.Id, new List<string> { parentLocationPath, location.Name }.AsDelimited( " > " ) );
                    }
                }
            }

            if ( locations.Any() )
            {
                ddlLocation.DataSource = locations;
                ddlLocation.DataBind();
            }
        }

        /// <summary>
        /// Binds the schedules.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        private void BindSchedules( int? locationId )
        {
            var schedules = new Dictionary<int, string> { { 0, string.Empty } };

            if ( _group != null && locationId.HasValue )
            {
                _group.GroupLocations
                    .Where( l => l.LocationId == locationId.Value )
                    .SelectMany( l => l.Schedules )
                    .OrderBy( s => s.Name )
                    .ToList()
                    .ForEach( s => schedules.AddOrIgnore( s.Id, s.Name ) );
            }

            if ( schedules.Any() )
            {
                ddlSchedule.DataSource = schedules;
                ddlSchedule.DataBind();
            }

            ddlSchedule.Visible = ddlSchedule.Items.Count > 1;
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void ShowDetails()
        {
            if ( _occurrence == null )
            {
                nbNotice.Heading = "No Occurrences";
                nbNotice.Text = "<p>There are currently not any active occurrences for selected group to take attendance for.</p>";
                nbNotice.NotificationBoxType = NotificationBoxType.Warning;
                nbNotice.Visible = true;

                pnlDetails.Visible = false;
            }
            else
            {
                nbNotice.Visible = false;

                if ( PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull().HasValue )
                {
                    lOccurrenceDate.Visible = true;
                    lOccurrenceDate.Text = _occurrence.OccurrenceDate.ToShortDateString();
                    dpOccurrenceDate.Visible = false;

                    if ( _occurrence.LocationId.HasValue )
                    {
                        lLocation.Visible = true;
                        lLocation.Text = new LocationService( _rockContext ).GetPath( _occurrence.LocationId.Value );
                    }
                    else
                    {
                        lLocation.Visible = false;
                    }

                    ddlLocation.Visible = false;

                    lSchedule.Visible = _occurrence.Schedule != null;
                    lSchedule.Text = _occurrence.Schedule != null ? _occurrence.Schedule.Name : string.Empty;
                    ddlSchedule.Visible = false;
                }
                else
                {
                    lOccurrenceDate.Visible = false;
                    dpOccurrenceDate.Visible = true;
                    dpOccurrenceDate.SelectedDate = _occurrence.OccurrenceDate;

                    int? locationId = PageParameter( PageParameterKey.LocationId ).AsIntegerOrNull();
                    if ( locationId.HasValue )
                    {
                        lLocation.Visible = true;
                        lLocation.Text = new LocationService( _rockContext ).GetPath( locationId.Value );
                        ddlLocation.Visible = false;

                        Schedule schedule = null;
                        int? scheduleId = PageParameter( PageParameterKey.ScheduleId ).AsIntegerOrNull();
                        if ( scheduleId.HasValue )
                        {
                            schedule = new ScheduleService( _rockContext ).Get( scheduleId.Value );
                        }

                        if ( schedule != null )
                        {
                            lSchedule.Visible = true;
                            lSchedule.Text = schedule.Name;
                            ddlSchedule.Visible = false;
                        }
                        else
                        {
                            BindSchedules( locationId.Value );
                            lSchedule.Visible = false;
                            ddlSchedule.Visible = ddlSchedule.Items.Count > 1;
                        }
                    }
                    else
                    {
                        lLocation.Visible = false;
                        ddlLocation.Visible = ddlLocation.Items.Count > 1;

                        lSchedule.Visible = false;
                        ddlSchedule.Visible = ddlSchedule.Items.Count > 1;
                    }
                }

                BindAttendanceLocation();

                lMembers.Text = _group.GroupType.GroupMemberTerm.Pluralize();
                lPendingMembers.Text = "Pending " + lMembers.Text;

                List<int> attendedIds = new List<int>();

                // Load the attendance for the selected occurrence
                if ( _occurrence.Id > 0 )
                {
                    dtNotes.Text = _occurrence.Notes;

                    cbDidNotMeet.Checked = _occurrence.DidNotOccur ?? false;

                    // Get the list of people who attended
                    attendedIds = new AttendanceService( _rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.OccurrenceId == _occurrence.Id &&
                            a.DidAttend.HasValue &&
                            a.DidAttend.Value &&
                            a.PersonAlias != null )
                        .Select( a => a.PersonAlias.PersonId )
                        .Distinct()
                        .ToList();
                }

                var allowAddPerson = GetAttributeValue( AttributeKey.AllowAddingPerson ).AsBoolean();
                var addPersonAs = GetAttributeValue( AttributeKey.AddPersonAs );
                ppAddPerson.PersonName = string.Format( "Add New {0}", addPersonAs );
                if ( !GetAttributeValue( AttributeKey.GroupMemberAddPage ).IsNullOrWhiteSpace() )
                {
                    lbAddMember.Visible = allowAddPerson;
                    ppAddPerson.Visible = allowAddPerson && addPersonAs == "Attendee";
                }
                else
                {
                    ppAddPerson.Visible = allowAddPerson;
                }

                // Get the group members
                var groupMemberService = new GroupMemberService( _rockContext );

                // Add any existing active members not on that list
                var unattendedIds = groupMemberService
                    .Queryable().AsNoTracking()
                    .Where( m =>
                        m.GroupId == _group.Id &&
                        m.GroupMemberStatus == GroupMemberStatus.Active &&
                        !attendedIds.Contains( m.PersonId ) )
                    .Select( m => m.PersonId )
                    .ToList();

                string template = GetAttributeValue( AttributeKey.LavaTemplate );
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

				// LPC CODE
                var mobilePhoneDefinedValue = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), _rockContext);
                // END LPC CODE 
                
                // Bind the attendance roster
                // LPC MODIFIED
                _attendees = new PersonService( _rockContext )
                    .Queryable(new Rock.Model.PersonService.PersonQueryOptions() { IncludeNameless = true }).Include("PhoneNumbers").AsNoTracking()
                    .Where( p => attendedIds.Contains( p.Id ) || unattendedIds.Contains( p.Id ) )
                    .ToList()
                    .Select( p => new GroupAttendanceAttendee()
                    {
                        PersonId = p.Id,
                        NickName = p.NickName,
                        LastName = p.LastName,
                        Attended = attendedIds.Contains( p.Id ),
                        CampusIds = p.GetCampusIds(),
                        MobileNumber = p.PhoneNumbers.Where(pn => pn.NumberTypeValueId == mobilePhoneDefinedValue.Id).Select(pn => pn.NumberFormatted).FirstOrDefault(),
                        MergedTemplate = template.ResolveMergeFields( mergeFields.Union( new Dictionary<string, object>() { { "Person", p } } ).ToDictionary( x => x.Key, x => x.Value ) )
                    } )
                    .ToList();
                // END LPC MODIFIED

                BindAttendees();

                // Bind the pending members
                var pendingMembers = groupMemberService
                    .Queryable().AsNoTracking()
                    .Where( m =>
                        m.GroupId == _group.Id &&
                        m.GroupMemberStatus == GroupMemberStatus.Pending )
                    .OrderBy( m => m.Person.LastName )
                    .ThenBy( m => m.Person.NickName )
                    .Select( m => new
                    {
                        Id = m.PersonId,
                        FullName = m.Person.NickName + " " + m.Person.LastName
                    } )
                    .ToList();

                pnlPendingMembers.Visible = pendingMembers.Any();
                lvPendingMembers.DataSource = pendingMembers;
                lvPendingMembers.DataBind();
            }
        }

        /// <summary>
        /// Binds the attendees to the list.
        /// </summary>
        private void BindAttendees()
        {
            var campusAttendees = _attendees;
            if ( _allowCampusFilter )
            {
                var campus = CampusCache.Get( bddlCampus.SelectedValueAsInt() ?? 0 );
                if ( campus != null )
                {
                    campusAttendees = _attendees.Where( a => a.CampusIds.Contains( campus.Id ) ).ToList();
                }
            }

            int attendanceCount = campusAttendees.Where( a => a.Attended ).Count();
            lDidAttendCount.Visible = attendanceCount > 0;
            lDidAttendCount.Text = attendanceCount.ToString( "N0" );

            if ( tglSort.Visible && tglSort.Checked )
            {
                lvMembers.DataSource = campusAttendees.OrderBy( a => a.LastName ).ThenBy( a => a.NickName ).ToList();
            }
            else
            {
                lvMembers.DataSource = campusAttendees.OrderBy( a => a.NickName ).ThenBy( a => a.LastName ).ToList();
            }

            lvMembers.DataBind();

            // Reset the PersonPicker control selection.
            ppAddPerson.SetValue( null );
            ppAddPerson.PersonName = string.Format( "Add New {0}", GetAttributeValue( AttributeKey.AddPersonAs ) );
        }

        protected void RegisterScript()
        {
            string script = string.Format(
                @"

    Sys.Application.add_load(function () {{

        if ($('#{0}').is(':checked')) {{
            $('div.js-roster').hide();
        }}

        $('#{0}').on('click', function () {{
            if ($(this).is(':checked')) {{
                $('div.js-roster').hide('fast');
            }} else {{
                $('div.js-roster').show('fast');
            }}
        }});

        $('.js-add-member').off('click').on('click', function ( e ) {{
            e.preventDefault();
            var $a = $(this);
            var memberName = $(this).parent().find('span').html();
            Rock.dialogs.confirm('Add ' + memberName + ' to your group?', function (result) {{
                if (result) {{
                    window.location = $a.prop('href');
                }}
            }});
        }});

    }});

",
cbDidNotMeet.ClientID );

            ScriptManager.RegisterStartupScript( cbDidNotMeet, cbDidNotMeet.GetType(), "group-attendance-detail", script, true );
        }

        /// <summary>
        /// Method to save attendance for use in two separate areas.
        /// </summary>
        protected bool SaveAttendance()
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new AttendanceOccurrenceService( rockContext );
                var attendanceService = new AttendanceService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var locationService = new LocationService( rockContext );

                AttendanceOccurrence occurrence = null;

                if ( _occurrence.Id != 0 )
                {
                    occurrence = occurrenceService.Get( _occurrence.Id );
                }

                if ( occurrence == null )
                {
                    var existingOccurrence = occurrenceService.Get( _occurrence.OccurrenceDate, _group.Id, _occurrence.LocationId, _occurrence.ScheduleId );
                    if ( existingOccurrence != null )
                    {
                        nbNotice.Heading = "Occurrence Already Exists";
                        nbNotice.Text = "<p>An occurrence already exists for this group for the selected date, location, and schedule that you've selected. Please return to the list and select that occurrence to update it's attendance.</p>";
                        nbNotice.NotificationBoxType = NotificationBoxType.Danger;
                        nbNotice.Visible = true;

                        return false;
                    }
                    else
                    {
                        occurrence = new AttendanceOccurrence();
                        occurrence.GroupId = _occurrence.GroupId;
                        occurrence.LocationId = _occurrence.LocationId;
                        occurrence.ScheduleId = _occurrence.ScheduleId;
                        occurrence.OccurrenceDate = _occurrence.OccurrenceDate;
                        occurrenceService.Add( occurrence );
                    }
                }

                occurrence.Notes = GetAttributeValue( AttributeKey.ShowNotes ).AsBoolean() ? dtNotes.Text : string.Empty;
                occurrence.DidNotOccur = cbDidNotMeet.Checked;

                SetOccurrenceAttendanceTypeId( occurrence );

                var existingAttendees = occurrence.Attendees.ToList();

                // If did not meet was selected and this was a manually entered occurrence (not based on a schedule/location)
                // then just delete all the attendance records instead of tracking a 'did not meet' value
                if ( cbDidNotMeet.Checked && !_occurrence.ScheduleId.HasValue )
                {
                    foreach ( var attendance in existingAttendees )
                    {
                        attendanceService.Delete( attendance );
                    }
                }
                else
                {
                    int? campusId = locationService.GetCampusIdForLocation( _occurrence.LocationId ) ?? _group.CampusId;
                    if ( !campusId.HasValue && _allowCampusFilter )
                    {
                        var campus = CampusCache.Get( bddlCampus.SelectedValueAsInt() ?? 0 );
                        if ( campus != null )
                        {
                            campusId = campus.Id;
                        }
                    }

                    if ( cbDidNotMeet.Checked )
                    {
                        // If the occurrence is based on a schedule, set the did not meet flags
                        foreach ( var attendance in existingAttendees )
                        {
                            attendance.DidAttend = null;
                        }
                    }
                    else
                    {
                        _occurrence.Schedule = _occurrence.Schedule == null && _occurrence.ScheduleId.HasValue ? new ScheduleService( rockContext ).Get( _occurrence.ScheduleId.Value ) : _occurrence.Schedule;

                        cvAttendance.IsValid = _occurrence.IsValid;
                        if ( !cvAttendance.IsValid )
                        {
                            cvAttendance.ErrorMessage = _occurrence.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                            return false;
                        }

                        foreach ( var attendee in _attendees )
                        {
                            var attendance = existingAttendees
                                .Where( a => a.PersonAlias.PersonId == attendee.PersonId )
                                .FirstOrDefault();

                            if ( attendance == null )
                            {
                                int? personAliasId = personAliasService.GetPrimaryAliasId( attendee.PersonId );
                                if ( personAliasId.HasValue )
                                {
                                    attendance = new Attendance();
                                    attendance.PersonAliasId = personAliasId;
                                    attendance.CampusId = campusId;
                                    attendance.StartDateTime = _occurrence.Schedule != null && _occurrence.Schedule.HasSchedule() ? _occurrence.OccurrenceDate.Date.Add( _occurrence.Schedule.StartTimeOfDay ) : _occurrence.OccurrenceDate;
                                    attendance.DidAttend = attendee.Attended;

                                    // Check that the attendance record is valid
                                    cvAttendance.IsValid = attendance.IsValid;
                                    if ( !cvAttendance.IsValid )
                                    {
                                        cvAttendance.ErrorMessage = attendance.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                                        return false;
                                    }

                                    occurrence.Attendees.Add( attendance );
                                }
                            }
                            else
                            {
                                // Otherwise, only record that they attended -- don't change their attendance startDateTime
                                attendance.DidAttend = attendee.Attended;
                            }
                        }
                    }
                }

                rockContext.SaveChanges();

                if ( occurrence.LocationId.HasValue )
                {
                    Rock.CheckIn.KioskLocationAttendance.Remove( occurrence.LocationId.Value );
                }

                Guid? workflowTypeGuid = GetAttributeValue( AttributeKey.Workflow ).AsGuidOrNull();
                if ( workflowTypeGuid.HasValue )
                {
                    var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                    {
                        try
                        {
                            var workflow = Workflow.Activate( workflowType, _group.Name );

                            workflow.SetAttributeValue( "StartDateTime", _occurrence.OccurrenceDate.ToString( "o" ) );

                            if ( _group.Schedule != null )
                            {
                                workflow.SetAttributeValue( "Schedule", _group.Schedule.Guid.ToString() );
                            }

                            List<string> workflowErrors;
                            new WorkflowService( rockContext ).Process( workflow, _group, out workflowErrors );
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex, this.Context );
                        }
                    }
                }

                _occurrence.Id = occurrence.Id;
            }

            return true;
        }

        /// <summary>
        /// Method to email attendance summary.
        /// </summary>
        private void EmailAttendanceSummary()
        {
            try
            {
                var rockContext = new RockContext();
                var occurrence = new AttendanceOccurrenceService( rockContext ).Get( _occurrence.Id );
                var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeObjects.Add( "Group", _group );
                mergeObjects.Add( "AttendanceOccurrence", occurrence );
                mergeObjects.Add( "AttendanceNoteLabel", GetAttributeValue( AttributeKey.AttendanceNoteLabel ) );

                List<Person> recipients = new List<Person>();

                var notificationOptions = GetAttributeValue( AttributeKey.SendSummaryEmailTo ).SplitDelimitedValues().Select( a => a.ConvertToEnumOrNull<SendSummaryEmailType>() ).ToList();
                foreach ( var notificationOption in notificationOptions )
                {
                    if ( !notificationOption.HasValue )
                    {
                        continue;
                    }

                    switch ( notificationOption )
                    {
                        case SendSummaryEmailType.GroupLeaders:
                            var leaders = new GroupMemberService( _rockContext )
                                .Queryable( "Person" )
                                .AsNoTracking()
                                .Where( m => m.GroupId == _group.Id )
                                .Where( m => m.IsArchived == false )
                                .Where( m => m.GroupMemberStatus != GroupMemberStatus.Inactive )
                                .Where( m => m.GroupRole.IsLeader );

                            recipients.AddRange( leaders.Where( a => !string.IsNullOrEmpty( a.Person.Email ) ).Select( a => a.Person ) );
                            break;

                        case SendSummaryEmailType.AllGroupMembers:
                            var allGroupMembers = new GroupMemberService( _rockContext )
                                .Queryable( "Person" )
                                .AsNoTracking()
                                .Where( m => m.GroupId == _group.Id )
                                .Where( m => m.IsArchived == false )
                                .Where( m => m.GroupMemberStatus != GroupMemberStatus.Inactive );

                            recipients.AddRange( allGroupMembers.Where( a => !string.IsNullOrEmpty( a.Person.Email ) ).Select( a => a.Person ) );
                            break;

                        case SendSummaryEmailType.GroupAdministrator:
                            if ( _group.GroupType.ShowAdministrator && _group.GroupAdministratorPersonAliasId.HasValue && _group.GroupAdministratorPersonAlias.Person.Email.IsNotNullOrWhiteSpace() )
                            {
                                recipients.Add( _group.GroupAdministratorPersonAlias.Person );
                            }

                            break;

                        case SendSummaryEmailType.ParentGroupLeaders:
                            if ( _group.ParentGroupId.HasValue )
                            {
                                var parentLeaders = new GroupMemberService( _rockContext )
                                .Queryable( "Person" )
                                .AsNoTracking()
                                .Where( m => m.GroupId == _group.ParentGroupId.Value )
                                .Where( m => m.IsArchived == false )
                                .Where( m => m.GroupMemberStatus != GroupMemberStatus.Inactive )
                                .Where( m => m.GroupRole.IsLeader );

                                recipients.AddRange( parentLeaders.Where( a => !string.IsNullOrEmpty( a.Person.Email ) ).Select( a => a.Person ) );
                            }

                            break;

                        case SendSummaryEmailType.IndividualEnteringAttendance:
                            if ( !string.IsNullOrEmpty( this.CurrentPerson.Email ) )
                            {
                                recipients.Add( this.CurrentPerson );
                            }

                            break;

                        default:
                            break;
                    }
                }

                foreach ( var recipient in recipients )
                {
                    var emailMessage = new RockEmailMessage( GetAttributeValue( AttributeKey.AttendanceEmailTemplate ).AsGuid() );
                    emailMessage.AddRecipient( new RockEmailMessageRecipient( recipient, mergeObjects ) );
                    emailMessage.CreateCommunicationRecord = false;
                    emailMessage.Send();
                }
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex, Context, RockPage.PageId, RockPage.Site.Id, CurrentPersonAlias );
            }
        }

        private void BindAttendanceLocation()
        {
            var allowedAttendanceTypes = GetAttributeValues( AttributeKey.AttendanceTypes );
            if ( allowedAttendanceTypes.Any() )
            {
                rblAttendanceType.Visible = allowedAttendanceTypes.Count > 1;
                rblAttendanceType.Label = GetAttributeValue( AttributeKey.AttendanceTypeLabel );
                foreach ( var attendanceType in allowedAttendanceTypes )
                {
                    var cachedValue = DefinedValueCache.Get( attendanceType );
                    rblAttendanceType.Items.Add( new ListItem
                    {
                        Text = cachedValue.Value,
                        Value = cachedValue.Id.ToString(),
                        Selected = allowedAttendanceTypes.Count == 1 || ( _occurrence != null && _occurrence.AttendanceTypeValueId == cachedValue.Id )
                    } );
                }
            }
        }

        private void SetOccurrenceAttendanceTypeId( AttendanceOccurrence occurrence )
        {
            var allowedAttendanceTypes = GetAttributeValues( AttributeKey.AttendanceTypes );
            if ( allowedAttendanceTypes.Any() )
            {
                var allowedAttendanceTypeValues = allowedAttendanceTypes.Select( at => DefinedValueCache.Get( at ) );
                var selectedAttendanceTypeValue = rblAttendanceType.SelectedValueAsId();

                if ( selectedAttendanceTypeValue == null )
                {
                    occurrence.AttendanceTypeValueId = null;
                    return;
                }

                if ( allowedAttendanceTypeValues.Where( atv => atv.Id == selectedAttendanceTypeValue.Value ).Any() )
                {
                    occurrence.AttendanceTypeValueId = selectedAttendanceTypeValue;
                    return;
                }
            }
            occurrence.AttendanceTypeValueId = null;
        }
        #endregion

        #region Helper Classes

        [Serializable]
        public class GroupAttendanceAttendee
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the name of the nick.
            /// </summary>
            /// <value>
            /// The name of the nick.
            /// </value>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the full name.
            /// </summary>
            /// <value>
            /// The full name.
            /// </value>
            public string FullName
            {
                get { return NickName + " " + LastName; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="GroupAttendanceAttendee"/> is attended.
            /// </summary>
            /// <value>
            ///   <c>true</c> if attended; otherwise, <c>false</c>.
            /// </value>
            public bool Attended { get; set; }

            /// <summary>
            /// Gets or sets the campus ids that a person's families belong to.
            /// </summary>
            /// <value>
            /// The campus ids.
            /// </value>
            public List<int> CampusIds { get; set; }

			// LPC CODE
            public string MobileNumber { get; set; }
            // END LPC CODE

            public string MergedTemplate { get; set; }
        }

        #endregion
    }
}
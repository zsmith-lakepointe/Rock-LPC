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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_lakepointe.GroupScheduling
{
    [DisplayName( "LPC - Group Schedule Confirmation" )]
    [Category( "LPC > Group Scheduling" )]
    [Description( "Allows a person to confirm a schedule RSVP and view pending schedules.  Uses PersonActionIdentifier in 'Person' with action 'ScheduleConfirm' when supplied." )]

    [CodeEditorField( "Confirm Heading Template",
        Description = "Text to display when a person confirms a schedule RSVP. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = ConfirmHeadingTemplateDefaultValue,
        Order = 1,
        Key = AttributeKey.ConfirmHeadingTemplate )]

    [CodeEditorField( "Decline Heading Template",
        Description = "Heading to display when a person declines a schedule RSVP. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = DeclineHeadingTemplateDefaultValue,
        Order = 2,
        Key = AttributeKey.DeclineHeadingTemplate )]

    [CodeEditorField( "Decline Message Template",
        Description = "Message to display when a person declines a schedule RSVP. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = DeclineMessageTemplateDefaultValue,
        Order = 3,
        Key = AttributeKey.DeclineMessageTemplate )]

    [BooleanField( "Scheduler Receive Confirmation Emails",
        Description = "If checked, the scheduler will receive an email response for each confirmation or decline.",
        DefaultBooleanValue = false,
        Order = 4,
        Key = AttributeKey.SchedulerReceiveConfirmationEmails )]

    [BooleanField( "Require Decline Reasons",
        Description = "If checked, a person must choose one of the ‘Decline Reasons’ to submit their decline status.",
        DefaultBooleanValue = true,
        Order = 5,
        Key = AttributeKey.RequireDeclineReasons )]

    [BooleanField( "Enable Decline Note",
        Description = "If checked, a note will be shown for the person to elaborate on why they cannot attend.",
        DefaultBooleanValue = false,
        Order = 6,
        Key = AttributeKey.EnableDeclineNote )]

    [BooleanField( "Require Decline Note",
        Description = "If checked, a custom note response will be required in order to save their decline status.",
        DefaultBooleanValue = false,
        Order = 7,
        Key = AttributeKey.RequireDeclineNote )]

    [TextField( "Decline Note Title",
        Description = "A custom title for the decline elaboration note.",
        IsRequired = false,
        DefaultValue = "Please elaborate on why you cannot attend.",
        Order = 8,
        Key = AttributeKey.DeclineNoteTitle )]

    [SystemCommunicationField( "Scheduling Response Email",
        Description = "The system email that will be used for sending responses back to the scheduler.",
        IsRequired = false,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SCHEDULING_RESPONSE,
        Order = 9,
        Key = AttributeKey.SchedulingResponseEmail )]

    [ContextAware( typeof( Rock.Model.Person ) )]
    public partial class GroupScheduleConfirmation : Rock.Web.UI.RockBlock
    {
        protected class AttributeKey
        {
            public const string ConfirmHeadingTemplate = "ConfirmHeadingTemplate";
            public const string DeclineHeadingTemplate = "DeclineHeadingTemplate";
            public const string DeclineMessageTemplate = "DeclineMessageTemplate";
            public const string SchedulerReceiveConfirmationEmails = "SchedulerReceiveConfirmationEmails";
            public const string RequireDeclineReasons = "RequireDeclineReasons";
            public const string EnableDeclineNote = "EnableDeclineNote";
            public const string RequireDeclineNote = "RequireDeclineNote";
            public const string DeclineNoteTitle = "DeclineNoteTitle";
            public const string SchedulingResponseEmail = "SchedulingResponseEmail";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        protected const string ConfirmHeadingTemplateDefaultValue = @"<h2 class='margin-t-none'>{{ Person.NickName }}, you’re confirmed to serve.</h2><p>Thanks for letting us know.  You’re confirmed for:</p><p><b>{{ OccurrenceDate | Date:'dddd, MMMM d, yyyy' }}</b><br>{{ Group.Name }}<br>{{ ScheduledItem.Location.Name }} {{ScheduledItem.Schedule.Name }} <i class='text-success fa fa-check-circle'></i><br></p>
<p class='margin-b-lg'>Thanks again!<br>
{{ Scheduler.FullName }}</p>";

        protected const string DeclineHeadingTemplateDefaultValue = @"<h2 class='margin-t-none'>{{ Person.NickName }}, can’t make it?</h2><p>Thanks for letting us know.  We’ll try to schedule another person for:</p>
<p><b>{{ OccurrenceDate | Date:'dddd, MMMM d, yyyy' }}</b><br>
{{ Group.Name }}<br>
{{ ScheduledItem.Location.Name }} {{ ScheduledItem.Schedule.Name }}<br></p>";

        protected const string DeclineMessageTemplateDefaultValue = @"<div class='alert alert-success'><strong>Thank You</strong> We’ll try to schedule another person for: {{ ScheduledItem.Occurrence.Group.Name }}.</div>";

        #region Fields

        private Person _selectedPerson;

        #endregion

        #region Properties

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbError.Visible = false;

            if ( !Page.IsPostBack )
            {
                LoadBlockSettings();
                if ( HasRequiredParameters() )
                {
                    SetSelectedPersonId();
                    if ( _selectedPerson == null )
                    {
                        ShowNotAuthorizedMessage();
                        return;
                    }

                    GetAttendanceByAttendanceIdAndSelectedPersonId();
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            if ( this.Page.IsValid )
            {
                SetSelectedPersonId();

                if ( _selectedPerson == null )
                {
                    ShowNotAuthorizedMessage();
                    return;
                }

                UpdateAttendanceDeclineReasonAfterSubmit();

                if ( PageParameter( "ReturnUrl" ).IsNotNullOrWhiteSpace() )
                {
                    if ( PageParameter( "rckipid" ).IsNotNullOrWhiteSpace() )
                    {
                        var pageReference = new PageReference( PageParameter( "ReturnUrl" ) );
                        Dictionary<string, string> queryStrings = new Dictionary<string, string>() { { "rckipid", CurrentPerson.GetImpersonationToken( DateTime.Now.AddMinutes( 20 ), null, pageReference.PageId ) } };

                        NavigateToPage( PageParameter( "ReturnUrl" ).AsGuid(), queryStrings );
                    }
                    else
                    {
                        NavigateToPage( PageParameter( "ReturnUrl" ).AsGuid(), null );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmAttend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmAttend_Click( object sender, EventArgs e )
        {
            var btnConfirmAttend = sender as LinkButton;
            int? attendanceId = btnConfirmAttend.CommandArgument.AsIntegerOrNull();
            if ( attendanceId.HasValue )
            {
                var rockContext = new RockContext();
                new AttendanceService( rockContext ).ScheduledPersonConfirm( attendanceId.Value );
                rockContext.SaveChanges();
            }

            BindPendingConfirmations();
        }

        /// <summary>
        /// Handles the Click event of the btnDeclineAttend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeclineAttend_Click( object sender, EventArgs e )
        {
            var btnDeclineAttend = sender as LinkButton;
            int? attendanceId = btnDeclineAttend.CommandArgument.AsIntegerOrNull();
            if ( attendanceId.HasValue )
            {
                var rockContext = new RockContext();

                // Use the value selected in the drop down list if it was set.
                int? declineReasonValueId = ddlDeclineReason.SelectedItem.Value.AsIntegerOrNull();

                new AttendanceService( rockContext ).ScheduledPersonDecline( attendanceId.Value, declineReasonValueId );
                rockContext.SaveChanges();
                DetermineRecipientAndSendResponseEmails( attendanceId );
            }

            BindPendingConfirmations();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPendingConfirmations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPendingConfirmations_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var lPendingOccurrenceDetails = e.Item.FindControl( "lPendingOccurrenceDetails" ) as Literal;
            var lPendingOccurrenceTime = e.Item.FindControl( "lPendingOccurrenceTime" ) as Literal;
            var btnConfirmAttend = e.Item.FindControl( "btnConfirmAttend" ) as LinkButton;
            var btnDeclineAttend = e.Item.FindControl( "btnDeclineAttend" ) as LinkButton;
            var attendance = e.Item.DataItem as Attendance;

            lPendingOccurrenceDetails.Text = GetOccurrenceDetails( attendance );
            lPendingOccurrenceTime.Text = GetOccurrenceScheduleName( attendance );
            btnConfirmAttend.CommandName = "AttendanceId";
            btnConfirmAttend.CommandArgument = attendance.Id.ToString();
            btnDeclineAttend.CommandName = "AttendanceId";
            btnDeclineAttend.CommandArgument = attendance.Id.ToString();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Loads the decline reasons.
        /// </summary>
        private void LoadDeclineReasons()
        {
            var defineTypeGroupScheduleReason = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUP_SCHEDULE_DECLINE_REASON );
            var definedValues = defineTypeGroupScheduleReason.DefinedValues;

            ddlDeclineReason.DataSource = definedValues;
            ddlDeclineReason.DataBind();
            ddlDeclineReason.Items.Insert( 0, new ListItem() );
        }

        /// <summary>
        /// Binds the pending confirmations.
        /// </summary>
        private void BindPendingConfirmations()
        {
            var selectedPersonId = hfSelectedPersonId.Value.AsIntegerOrNull();

            if ( selectedPersonId == null )
            {
                return;
            }

            var rockContext = new RockContext();
            var qryPendingConfirmations = new AttendanceService( rockContext )
                .GetPendingScheduledConfirmations()
                .AsNoTracking()
                .Where( a => a.PersonAlias.PersonId == selectedPersonId )
                .OrderBy( a => a.Occurrence.OccurrenceDate );

            var pendingConfirmations = qryPendingConfirmations.ToList();
            if ( pendingConfirmations.Count > 0 )
            {
                rptPendingConfirmations.DataSource = pendingConfirmations;
                rptPendingConfirmations.DataBind();
                pnlPendingConfirmation.Visible = true;
            }
            else
            {
                pnlPendingConfirmation.Visible = false;
            }
        }

        /// <summary>
        /// Handles the not authorized.
        /// </summary>
        private void ShowNotAuthorizedMessage()
        {
            pnlPendingConfirmation.Visible = false;
            nbError.Visible = true;
        }

        /// <summary>
        /// Shows the decline message after submit.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        private void ShowDeclineMessageAfterSubmit( Attendance attendance, IDictionary<string, object> mergeFields )
        {
            nbError.Visible = false;
            lResponse.Text = GetAttributeValue( AttributeKey.DeclineMessageTemplate ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );
            lResponse.Visible = true;

            DetermineRecipientAndSendResponseEmails( attendance?.Id );
        }

        /// <summary>
        /// Shows the heading by is confirmed.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        private void ShowHeadingByIsConfirmed( Attendance attendance )
        {
            if ( attendance.Note.IsNotNullOrWhiteSpace() )
            {
                dtbDeclineReasonNote.Text = attendance.Note;
            }

            if ( attendance.DeclineReasonValueId != null )
            {
                ddlDeclineReason.SelectedValue = attendance.DeclineReasonValueId.ToString();
            }

            if ( PageParameter( "isConfirmed" ).AsBoolean() )
            {
                var mergeFields = MergeFields( attendance, attendance?.ScheduledByPersonAlias?.Person );
                ShowConfirmationHeading( mergeFields );
            }
            else
            {
                // we send decline email from submit button
                var mergeFields = MergeFields( attendance, attendance.Occurrence.Group?.ScheduleCancellationPersonAlias?.Person );
                ShowDeclineHeading( mergeFields );
            }

            lBlockTitle.Text = "Email Confirmation";
        }

        /// <summary>
        /// Shows the confirmation heading.
        /// </summary>
        /// <param name="mergeFields">The merge fields.</param>
        private void ShowConfirmationHeading( IDictionary<string, object> mergeFields )
        {
            lResponse.Text = GetAttributeValue( AttributeKey.ConfirmHeadingTemplate ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );
            lResponse.Visible = true;
            pnlDeclineReason.Visible = false;
            pnlPendingConfirmation.Visible = true;
        }

        /// <summary>
        /// Shows the decline heading.
        /// </summary>
        /// <param name="mergeFields">The merge fields.</param>
        private void ShowDeclineHeading( IDictionary<string, object> mergeFields )
        {
            pnlDeclineReason.Visible = true;
            lResponse.Text = GetAttributeValue( AttributeKey.DeclineHeadingTemplate ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );
            lResponse.Visible = true;
        }

        /// <summary>
        /// Determines whether [has required parameters].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has required parameters]; otherwise, <c>false</c>.
        /// </returns>
        private bool HasRequiredParameters()
        {
            if ( PageParameter( "attendanceId" ).IsNullOrWhiteSpace() )
            {
                ShowNotAuthorizedMessage();
                return false;
            }

            if ( PageParameter( "isConfirmed" ).AsBooleanOrNull() == null )
            {
                ShowIsConfirmedMissingParameterMessage();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Shows the is confirmed missing parameter message.
        /// </summary>
        private void ShowIsConfirmedMissingParameterMessage()
        {
            nbError.Title = "Sorry,";
            nbError.NotificationBoxType = NotificationBoxType.Warning;
            nbError.Text = "something is not right with the link you used to get here.";
            nbError.Visible = true;
        }

        /// <summary>
        /// Loads the block settings.
        /// </summary>
        private void LoadBlockSettings()
        {
            LoadDeclineReasons();

            // Decline reason drop down always visible
            ddlDeclineReason.Visible = true;

            // block setting drives if required
            ddlDeclineReason.Required = GetAttributeValue( AttributeKey.RequireDeclineReasons ).AsBoolean();
            this.btnSubmit.Visible = true;

            // decline Note
            dtbDeclineReasonNote.Label = GetAttributeValue( AttributeKey.DeclineNoteTitle ).ToString();
            dtbDeclineReasonNote.Visible = GetAttributeValue( AttributeKey.EnableDeclineNote ).AsBoolean();
            dtbDeclineReasonNote.Required = GetAttributeValue( AttributeKey.RequireDeclineNote ).AsBoolean();
        }

        /// <summary>
        /// Sets the attendance on load.
        /// </summary>
        private void GetAttendanceByAttendanceIdAndSelectedPersonId()
        {
            using ( var rockContext = new RockContext() )
            {
                // Is a person selected?
                if ( _selectedPerson == null )
                {
                    nbError.Visible = true;
                }
                else if ( CurrentPerson != null && _selectedPerson != CurrentPerson )
                {
                    nbError.Visible = true;
                    nbError.Title = "Note:";
                    nbError.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                    nbError.Text = string.Format( "You are setting and viewing the confirmations for {0}.", _selectedPerson.FullName );
                }

                var request = Context.Request;
                var attendanceId = GetAttendanceIdFromParameters();
                if ( attendanceId != null )
                {
                    var attendanceService = new AttendanceService( rockContext );

                    // make sure the attendance is for the currently logged in person
                    var attendance = attendanceService.Queryable().Where( a => a.Id == attendanceId.Value && a.PersonAlias.PersonId == _selectedPerson.Id ).FirstOrDefault();

                    if ( attendance == null )
                    {
                        ShowNotAuthorizedMessage();
                        return;
                    }

                    bool statusChanged = false;

                    bool isConfirmedParameter = this.PageParameter( "IsConfirmed" ).AsBoolean();
                    if ( isConfirmedParameter )
                    {
                        if ( !attendance.IsScheduledPersonConfirmed() )
                        {
                            attendanceService.ScheduledPersonConfirm( attendance.Id );
                            rockContext.SaveChanges();
                        }
                    }
                    else
                    {
                        if ( !attendance.IsScheduledPersonDeclined() )
                        {
                            attendanceService.ScheduledPersonDecline( attendance.Id, null );
                            rockContext.SaveChanges();
                        }
                    }

                    if ( statusChanged )
                    {
                        rockContext.SaveChanges();

                        // Only send Confirm if the status has changed and change is to Yes
                        if ( attendance.RSVP == RSVP.Yes )
                        {
                            DetermineRecipientAndSendResponseEmails( attendance?.Id );
                        }
                    }

                    ShowHeadingByIsConfirmed( attendance );
                }
                else
                {
                    ShowNotAuthorizedMessage();
                    return;
                }

                BindPendingConfirmations();
            }
        }

        /// <summary>
        /// Updates the attendance decline reason.
        /// </summary>
        private void UpdateAttendanceDeclineReasonAfterSubmit()
        {
            try
            {
                SetSelectedPersonId();
                var attendanceId = GetAttendanceIdFromParameters();

                if ( attendanceId != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var attendance = new AttendanceService( rockContext ).Queryable().Where( a => a.Id == attendanceId.Value && a.PersonAlias.PersonId == _selectedPerson.Id ).FirstOrDefault();
                        if ( attendance != null )
                        {
                            var declineResonId = ddlDeclineReason.SelectedItem.Value.AsInteger();

                            if ( declineResonId == 0 )
                            {
                                // set to blank and not required
                                attendance.DeclineReasonValueId = null;
                            }
                            else
                            {
                                attendance.DeclineReasonValueId = declineResonId;
                            }

                            if ( !dtbDeclineReasonNote.Text.IsNullOrWhiteSpace() )
                            {
                                attendance.Note = dtbDeclineReasonNote.Text;
                            }

                            rockContext.SaveChanges();
                        }

                        var mergeFields = MergeFields( attendance, this.ContextEntity<Person>() );
                        ShowDeclineMessageAfterSubmit( attendance, mergeFields );
                    }
                }

                pnlDeclineReason.Visible = false;
            }
            catch ( Exception ex )
            {
                // ignore but log
                ExceptionLogService.LogException( ex );
            }
        }

        /// <summary>
        /// Sets the person _selectedPersonId.
        /// </summary>
        private void SetSelectedPersonId()
        {
            var targetPerson = this.ContextEntity<Person>();
            if ( targetPerson != null )
            {
                _selectedPerson = targetPerson;
            }
            else
            {
                // Use the PersonActionIdentifier if given...
                string personKey = PageParameter( "Person" );
                if ( personKey.IsNotNullOrWhiteSpace() )
                {
                    _selectedPerson = new PersonService( new RockContext() ).GetByPersonActionIdentifier( personKey, "ScheduleConfirm" );
                    if ( _selectedPerson != null )
                    {
                        hfSelectedPersonId.Value = _selectedPerson.Id.ToString();
                    }
                }
                else
                {
                    // ...otherwise use the current person.
                    _selectedPerson = this.CurrentPerson;
                    hfSelectedPersonId.Value = this.CurrentPersonId.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the attendance identifier from parameters.
        /// </summary>
        /// <returns>The id if found or null otherwise.</returns>
        private int? GetAttendanceIdFromParameters()
        {
            if ( PageParameter( "attendanceId" ).IsNotNullOrWhiteSpace() )
            {
                var attendanceId = PageParameter( "attendanceId" ).AsIntegerOrNull();

                if ( attendanceId != null && _selectedPerson != null )
                {
                    return attendanceId;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the occurrence schedule's name.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        /// <returns>The name of the schedule</returns>
        protected string GetOccurrenceScheduleName( Attendance attendance )
        {
            return attendance.Occurrence.Schedule.Name;
        }

        /// <summary>
        /// Gets the occurrence details.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        /// <returns></returns>
        protected string GetOccurrenceDetails( Attendance attendance )
        {
            if ( attendance.Occurrence.GroupId == null && attendance.Occurrence.LocationId == null )
            {
                return attendance.Occurrence.OccurrenceDate.ToShortDateString();
            }

            if ( attendance.Occurrence.GroupId == null )
            {
                return string.Format( "{0} - {1}", attendance.Occurrence.OccurrenceDate.ToShortDateString(), attendance.Occurrence.Location );
            }

            if ( attendance.Occurrence.LocationId == null )
            {
                return attendance.Occurrence.OccurrenceDate.ToShortDateString();
            }

            return string.Format( "{0} - {1} - {2}", attendance.Occurrence.OccurrenceDate.ToShortDateString(), attendance.Occurrence.Group.Name, attendance.Occurrence.Location );
        }

        /// <summary>
        /// Determines the recipient and send confirmation email.
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        private void DetermineRecipientAndSendResponseEmails( int? attendanceId )
        {
            if ( !attendanceId.HasValue )
            {
                return;
            }

            var attendanceService = new AttendanceService( new RockContext() );
            var attendance = attendanceService.Get( attendanceId.Value );
            if ( attendance == null )
            {
                return;
            }

            try
            {
                // The scheduler receives email add as a recipient for both Confirmation and Decline
                if ( GetAttributeValue( AttributeKey.SchedulerReceiveConfirmationEmails ).AsBoolean() && attendance.ScheduledByPersonAlias != null && attendance.ScheduledByPersonAlias.Person.IsEmailActive )
                {
                    attendanceService.SendScheduledPersonResponseEmailToScheduler( attendance.Id, GetAttributeValue( AttributeKey.SchedulingResponseEmail ).AsGuid() );
                }

                // if attendance is decline (no) also send email to Schedule Cancellation Person
                if ( attendance.RSVP == RSVP.No )
                {
                    attendanceService.SendScheduledPersonDeclineEmail( attendance.Id, GetAttributeValue( AttributeKey.SchedulingResponseEmail ).AsGuid() );
                }
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex, Context, RockPage.PageId, RockPage.Site.Id, CurrentPersonAlias );
            }
        }

        /// <summary>
        /// Populates the merge fields.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        /// <param name="recipientPerson">The recipient person.</param>
        /// <returns></returns>
        private Dictionary<string, object> MergeFields( Attendance attendance, Person recipientPerson )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this._selectedPerson );
            var group = attendance.Occurrence.Group;
            mergeFields.Add( "Group", group );
            mergeFields.Add( "ScheduledItem", attendance );
            mergeFields.Add( "OccurrenceDate", attendance.Occurrence.OccurrenceDate );
            mergeFields.Add( "ScheduledStartTime", DateTime.Today.Add( attendance.Occurrence.Schedule.StartTimeOfDay ).ToString( "h:mm tt" ) );
            mergeFields.Add( "Person", attendance.PersonAlias.Person );
            mergeFields.Add( "Scheduler", attendance.ScheduledByPersonAlias.Person );

            // This would be Scheduler or Cancellation Person depending on which recipientPerson was specified
            mergeFields.Add( "Recipient", recipientPerson );

            return mergeFields;
        }

        #endregion
    }
}
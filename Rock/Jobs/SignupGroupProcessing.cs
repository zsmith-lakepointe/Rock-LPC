using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Quartz;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// This job will find active Signup Groups where the ProjectType is In-Person then find the next schedule date from it's schedules and finally send out the SystemCommunication configured on the group type.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisplayName( "Signup Group Processing" )]
    [Description( "This job will find active Signup Groups where the ProjectType is In-Person then find the next schedule date from it's schedules and finally send out the SystemCommunication configured on the group type." )]

    [DisallowConcurrentExecution]
    public class SignupGroupProcessing : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SignupGroupProcessing()
        {
        }

        /// <summary>
        /// Job that will send Signup group communications. Called by the <see cref="IScheduler" /> when an <see cref="ITrigger" />
        /// fires that is associated with the <see cref="IJob" />.
        /// </summary>
        /// <param name="context">The job's execution context.</param>
        public void Execute( IJobExecutionContext context )
        {
            try
            {
                ProcessJob( context );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
                throw;
            }
        }

        #region Job Methods

        /// <summary>
        /// Private method called by Execute() to process the job.  This method should be wrapped in a try/catch block to ensure than any exceptions are sent to the
        /// <see cref="ExceptionLogService"/>.
        /// </summary>
        /// <param name="context">The job's execution context.</param>
        private void ProcessJob( IJobExecutionContext context )
        {
            var signupGroupTypeGuid = SystemGuid.Group.GROUP_SIGNUP_GROUPS.AsGuid();
            var rockContext = new Data.RockContext();

            var groupTypeService = new GroupTypeService( rockContext );
            var signupGroupType = groupTypeService.Get( SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP.AsGuid() );

            List<Group> allValidGroups = null;
            if ( signupGroupType != null )
            {
                var systemCommunicationService = new SystemCommunicationService( rockContext );

                allValidGroups = rockContext.Groups
                     .Where( v => v.GroupTypeId == signupGroupType.Id && v.IsActive == true )?
                     .ToList();

                allValidGroups = allValidGroups.Where( group => IsInPersonProject( rockContext, group.Id ) )?.ToList();

                foreach ( var group in allValidGroups )
                {
                    var isRsvp = group.GroupType.EnableRSVP;
                    if ( !isRsvp )
                    {
                        continue;
                    }

                    // Uses the group setting or the groupType setting
                    var systemCommunicationId = group.RSVPReminderSystemCommunicationId ?? group.GroupType.RSVPReminderSystemCommunicationId ??  0;
                    if ( systemCommunicationId == 0 )
                    {
                        continue;
                    }

                    var systemCommunication = systemCommunicationService.Get( systemCommunicationId );
                    if ( systemCommunication == null )
                    {
                        continue;
                    }
                                        
                    var recipients = CreateRecipients( rockContext, group );

                    SendSignupEmail( systemCommunication, recipients );

                }
            }
        }


        /// <summary>
        /// Determines whether [is in person project] [the specified rock context].
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <returns><c>true</c> if [is in person project] [the specified rock context]; otherwise, <c>false</c>.</returns>
        private bool IsInPersonProject( RockContext rockContext, int groupId )
        {
            var attributeValue = GetAttributeValue( rockContext, SystemGuid.EntityType.GROUP, "ProjectType", groupId, "GroupTypeId" );

            var projectTypeGuid = attributeValue?.AttributeValue?.Value ?? attributeValue.DefaultValue;

            if ( projectTypeGuid?.ToUpper() == SystemGuid.DefinedValue.GROUP_PROJECT_TYPE_IN_PERSON )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the attribute value for the specified entity.
        /// </summary>
        private AttributeValueInfo GetAttributeValue( RockContext rockContext, string entityTypeGuid, string attributeKey, int entityId, string qualifierColumn = "" )
        {
            var entityTypeService = new EntityTypeService( rockContext );
            var attributeService = new AttributeService( rockContext );
            var attributeValueService = new AttributeValueService( rockContext );

            var groupEntityType = entityTypeService.Get( entityTypeGuid, false );

            var attribute = attributeService.GetByEntityTypeId( groupEntityType.Id, false )
                .First( v => v.Key == attributeKey && ( qualifierColumn == "" || v.EntityTypeQualifierColumn == qualifierColumn ) );

            if ( attribute != null )
            {
                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, entityId );

                return new AttributeValueInfo( attributeValue, attribute?.DefaultValue );
            }

            return null;
        }

        /// <summary>
        /// Sends the signup email.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="recipients">The recipients.</param>
        /// <returns>System.Int32.</returns>
        private int SendSignupEmail( SystemCommunication systemCommunication, List<SignupRecipientInfo> recipients )
        {

            var message = new RockEmailMessage( systemCommunication );
            message.SetRecipients( SignupRecipientInfo.GetEmailMessageRecipients( recipients ) );
            message.Send( out List<string> emailErrors );

            if ( !emailErrors.Any() )
            {
                return 1; // No error, this should be counted as a sent reminder.
            }

            return 0;
        }

        /// <summary>
        /// Creates the recipients.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <returns>System.Collections.Generic.List&lt;Rock.Jobs.SignupGroupProcessing.SignupRecipientConfiguration&gt;.</returns>
        private static List<SignupRecipientInfo> CreateRecipients( RockContext rockContext, Group group )
        {
            var recipients = new List<SignupRecipientInfo>();

            // Uses the group setting or the groupType setting
            var offsetDays = group.RSVPReminderOffsetDays ?? group.GroupType.RSVPReminderOffsetDays  ?? 0;

            // Default to 2 days prior if not set.
            if ( offsetDays == 0 )
            {
                offsetDays = 2;
            }

            foreach ( var groupMember in group?.Members )
            {
                foreach ( var groupMemberAssignment in groupMember?.GroupMemberAssignments )
                {
                    var person = groupMemberAssignment?.GroupMember?.Person;

                    // Make sure we have a valid group member with an active email
                    if ( person == null || !person.IsEmailActive || person.Email.IsNullOrWhiteSpace() )
                    {
                        continue;
                    }

                    foreach ( var groupLocation in groupMemberAssignment?.Location?.GroupLocations )
                    {
                        if ( groupLocation == null )
                        {
                            continue;
                        }

                        // Get the campus information for this group location
                        var campusService = new CampusService( rockContext );
                        var campusId = groupLocation.Location.CampusId ?? 0;
                        if ( campusId == 0 )
                        {
                            continue;
                        }

                        var campus = campusService.Get( campusId );
                        if ( campus == null )
                        {
                            continue;
                        }

                        // Get the schedule information for the campus location.
                        foreach ( var groupLocationScheduleConfig in groupLocation?.GroupLocationScheduleConfigs )
                        {
                            var sentDate = groupMemberAssignment.LastRSVPReminderSentDateTime ?? DateTime.MinValue;
                            var alreadySentToday = sentDate.Date == campus.CurrentDateTime.Date;

                            var schedule = groupLocationScheduleConfig.Schedule;

                            // The send date is the schedule next start date minus the offsetDays (i.e. if the schedule start date is 1/15 and the offset days is 2 the send date is 1/13).

                            var sendDate = schedule.GetNextStartDateTime( campus.CurrentDateTime )?.Subtract( TimeSpan.FromDays( offsetDays ) );

                            if ( schedule.IsActive && !alreadySentToday && sendDate.HasValue && sendDate.Value == campus.CurrentDateTime.Date )
                            {
                                var recipient = new SignupRecipientInfo( group, person, groupLocationScheduleConfig );
                                if ( recipients.Contains( recipient ) )
                                {
                                    continue;
                                }

                                recipients.Add( recipient );
                            }
                        }
                    }
                }
            }

            return recipients;
        }

        #endregion Job Methods

        #region Job Classes

        internal class AttributeValueInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AttributeValueInfo"/> class.
            /// </summary>
            /// <param name="attributeValue">The attribute value.</param>
            /// <param name="defaultValue">The default value.</param>
            internal AttributeValueInfo( AttributeValue attributeValue, string defaultValue )
            {
                AttributeValue = attributeValue;
                DefaultValue = defaultValue;
            }

            /// <summary>
            /// Gets the attribute value.
            /// </summary>
            /// <value>The attribute value.</value>
            internal AttributeValue AttributeValue { get; }

            /// <summary>
            /// Gets the default value.
            /// </summary>
            /// <value>The default value.</value>
            internal string DefaultValue { get; }
        }

        /// <summary>
        /// Class SignupRecipientInfo.
        /// </summary>
        internal class SignupRecipientInfo
        {
            #region Constructors

            internal SignupRecipientInfo( Group group, Person person, GroupLocationScheduleConfig groupLocationScheduleConfig )
            {
                Group = group;
                Person = person;
                GroupLocationScheduleConfig = groupLocationScheduleConfig;

                LavaMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, person );
                LavaMergeFields.Add( "Person", person );
                LavaMergeFields.Add( "Group", group );
                LavaMergeFields.Add( "GroupLocationScheduleConfig", groupLocationScheduleConfig );
            }

            #endregion Constructors

            /// <summary>
            /// Gets or sets the group.
            /// </summary>
            /// <value>The group.</value>
            internal Group Group { get; }

            /// <summary>
            /// Gets or sets the person.
            /// </summary>
            /// <value>The person.</value>
            internal Person Person { get; }

            /// <summary>
            /// Gets or sets the group location schedule configuration.
            /// </summary>
            /// <value>The group location schedule configuration.</value>
            internal GroupLocationScheduleConfig GroupLocationScheduleConfig { get; }

            /// <summary>
            /// Gets the lava merge fields.
            /// </summary>
            /// <value>The lava merge fields.</value>
            internal Dictionary<string, object> LavaMergeFields { get; }

            /// <summary>
            /// Converts to email message recipients.
            /// </summary>
            /// <param name="recipientInfo">The recipient information.</param>
            /// <returns>List&lt;RockEmailMessageRecipient&gt;.</returns>
            internal static List<RockEmailMessageRecipient> GetEmailMessageRecipients( List<SignupRecipientInfo> recipientInfo )
            {
                return recipientInfo.Select( v => new RockEmailMessageRecipient( v.Person, v.LavaMergeFields ) )?.ToList();
            }
        }

        #endregion Job Classes
    }
}

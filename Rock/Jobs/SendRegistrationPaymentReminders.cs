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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;

using Humanizer;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Sends payment reminders to registration contacts with an active balance
    /// </summary>
    [DisplayName( "Event Payment Reminders" )]
    [Description( "This job sends payment reminders to registration contacts with an active balance. For the reminder to be sent the registration template must have a 'Payment Reminder Time Span' configured. Also emails will not be sent to registrations where the instance close date is past the job's 'Cut-off Date' setting." )]

    [IntegerField("Cut-off Date", "The number of days past the registration close to send reminders. After this cut-off, reminders will need to be sent manually to prevent eternal reminders.", true, 30, key:"CutoffDate")]
    public class SendRegistrationPaymentReminders : RockJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendRegistrationPaymentReminders()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            
            // get registrations where
            //    + template is active
            //    + instance is active
            //    + template has a number of days between reminders
            //    + template as fields needed to send a reminder email
            //    + the registration has a cost
            //    + the registration has been closed within the last xx days (to prevent eternal nagging)

            using ( RockContext rockContext = new RockContext())
            {
                int sendCount = 0;
                int registrationInstanceCount = 0;

                var publicAppRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );

                RegistrationService registrationService = new RegistrationService( rockContext );

                var currentDate = RockDateTime.Today;
                var cutoffDays = GetAttributeValue( "CutoffDate" ).AsIntegerOrNull() ?? 30;

                // Do not filter registrations by template or instance cost, it will miss $0 registrations that have optional fees.
                var registrations = registrationService.Queryable( "RegistrationInstance" )
                                                .Where( r =>
                                                         r.RegistrationInstance.RegistrationTemplate.IsActive
                                                         && r.RegistrationInstance.IsActive == true
                                                         && (r.RegistrationInstance.RegistrationTemplate.PaymentReminderTimeSpan != null && r.RegistrationInstance.RegistrationTemplate.PaymentReminderTimeSpan != 0)
                                                         && r.RegistrationInstance.RegistrationTemplate.PaymentReminderEmailTemplate != null && r.RegistrationInstance.RegistrationTemplate.PaymentReminderEmailTemplate.Length > 0
                                                         && r.RegistrationInstance.RegistrationTemplate.PaymentReminderFromEmail != null && r.RegistrationInstance.RegistrationTemplate.PaymentReminderFromEmail.Length > 0
                                                         && r.RegistrationInstance.RegistrationTemplate.PaymentReminderSubject != null && r.RegistrationInstance.RegistrationTemplate.PaymentReminderSubject.Length > 0
                                                         && (r.RegistrationInstance.EndDateTime == null || currentDate <= System.Data.Entity.SqlServer.SqlFunctions.DateAdd("day", cutoffDays,  r.RegistrationInstance.EndDateTime) ) )
                                                 .ToList();

                registrationInstanceCount = registrations.Select( r => r.RegistrationInstance.Id ).Distinct().Count();

                var errors = new List<string>();
                foreach(var registration in registrations )
                {
                    if ( registration.DiscountedCost > registration.TotalPaid )
                    {
                        var reminderDate = RockDateTime.Now.AddDays( registration.RegistrationInstance.RegistrationTemplate.PaymentReminderTimeSpan.Value * -1 );

                        if ( registration.LastPaymentReminderDateTime < reminderDate )
                        {
                            Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
                            mergeObjects.Add( "Registration", registration );
                            mergeObjects.Add( "RegistrationInstance", registration.RegistrationInstance );

                            var emailMessage = new RockEmailMessage();
                            emailMessage.AdditionalMergeFields = mergeObjects;
                            emailMessage.AddRecipient( registration.GetConfirmationRecipient( mergeObjects ) );
                            emailMessage.FromEmail = registration.RegistrationInstance.RegistrationTemplate.PaymentReminderFromEmail;
                            emailMessage.FromName = registration.RegistrationInstance.RegistrationTemplate.PaymentReminderFromName;
                            emailMessage.Subject = registration.RegistrationInstance.RegistrationTemplate.PaymentReminderSubject;
                            emailMessage.Message = registration.RegistrationInstance.RegistrationTemplate.PaymentReminderEmailTemplate;
                            emailMessage.AppRoot = publicAppRoot;

                            // LPC CODE
                            registration.PersonAlias.Person.LoadAttributes();
                            string lang = registration.PersonAlias.Person.GetAttributeTextValue( "PreferredLanguage" );
                            if ( lang == "Spanish" || lang == "Español" )
                            {
                                emailMessage.Message += "<style>.EnglishText { display: none !important; }</style>";
                            }
                            else
                            {
                                emailMessage.Message += "<style>.SpanishText { display: none !important; }</style>";
                            }
                            // END LPC CODE

                            var emailErrors = new List<string>();
                            emailMessage.Send(out emailErrors);
                            errors.AddRange( emailErrors );

                            registration.LastPaymentReminderDateTime = RockDateTime.Now;
                            rockContext.SaveChanges();

                            if (!emailErrors.Any())
                            {
                                sendCount++;
                            }
                        }
                    }
                }

                this.Result =  string.Format("Sent {0} from {1}", "reminder".ToQuantity( sendCount ), "registration instances".ToQuantity(registrationInstanceCount) );
                if ( errors.Any() )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine();
                    sb.Append( string.Format( "{0} Errors: ", errors.Count() ) );
                    errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                    string errorMessage = sb.ToString();
                    this.Result += errorMessage;
                    var exception = new Exception( errorMessage );
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( exception, context2 );
                    throw exception;
                }
            }
        }

    }
}

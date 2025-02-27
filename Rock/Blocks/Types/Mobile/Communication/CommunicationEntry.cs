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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Communication.CommunicationEntry;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Tasks;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Communication
{
    /// <summary>
    /// The mobile adaptation of the web CommunicationEntry block.
    /// Implements the <see cref="Rock.Blocks.RockMobileBlockType" />
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    #region Block Attributes

    [BooleanField( "Enable Email",
        Description = "When enabled, show email as a selectable communication transport.",
        DefaultBooleanValue = true,
        Key = AttributeKey.EnableEmail,
        Order = 0 )]

    [BooleanField( "Enable SMS",
        Description = "When enabled, show SMS as a selectable communication transport.",
        DefaultBooleanValue = true,
        Key = AttributeKey.EnableSms,
        Order = 1 )]

    [BooleanField( "Show From Name",
        Description = "When enabled, a field will be shown to input From Name (email transport only).",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowFromName,
        Order = 2 )]

    [BooleanField( "Show Reply To",
        Description = "When enabled, a field will be shown to input Reply To (email transport only).",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowReplyTo,
        Order = 3 )]

    [BooleanField( "Show Send To Parents",
        Description = "When enabled, a toggle will show to enable an individual with the Age Classification of 'Child' to have the communication sent to their parents as well.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowSendToParents,
        Order = 4 )]

    [BooleanField( "Is Bulk",
        Description = "When enabled, the communication will be flagged as a bulk communication.",
        DefaultBooleanValue = false,
        Key = AttributeKey.IsBulk,
        Order = 5 )]

    [SystemPhoneNumberField( "Allowed SMS Numbers",
        Description = "Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ",
        IsRequired = false,
        AllowMultiple = true,
        Key = AttributeKey.AllowedSmsNumbers,
        Order = 6 )]

    [BooleanField( "Show only personal SMS number",
        Description = "Only SMS Numbers tied to the current individual will be shown. Those with ADMIN rights will see all SMS Numbers.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowOnlyPersonalSmsNumber,
        Order = 7 )]

    [BooleanField( "Hide personal SMS numbers",
        Description = "Only SMS Numbers that are not associated with a person. The numbers without an Assigned To Person value.",
        DefaultBooleanValue = false,
        Key = AttributeKey.HidePersonalSmsNumbers,
        Order = 8 )]

    [LinkedPage(
        "Person Profile Page",
        Description = "Page to link to when user taps on a person listed in the 'Failed to Deliver' section. PersonGuid is passed in the query string.",
        IsRequired = false,
        Key = AttributeKey.PersonProfilePage,
        Order = 9 )]

    #endregion

    [DisplayName( "Communication Entry" )]
    [Category( "Mobile > Communication" )]
    [Description( "Allows you to send communications to a set of recipients." )]
    [IconCssClass( "fa fa-comment-o" )]

    // This block uses a unique security action that determines whether someone can send out communications immediately or has
    // to submit them for approval.
    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve new communications." )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_COMMUNICATION_COMMUNICATIONENTRY_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "B0182DA2-82F7-4798-A48E-88EBE61F2109" )]
    public class CommunicationEntry : RockMobileBlockType
    {

        #region Attribute Keys

        /// <summary>
        /// The block setting attribute keys for the Communication Entry mobile block.
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The enable email attribute key.
            /// </summary>
            public const string EnableEmail = "EnableEmail";

            /// <summary>
            /// The enable SMS attribute key.
            /// </summary>
            public const string EnableSms = "EnableSms";

            /// <summary>
            /// The enable parent communication attribute key.
            /// </summary>
            public const string ShowSendToParents = "ShowSendToParents";

            /// <summary>
            /// The show from name attribute key.
            /// </summary>
            public const string ShowFromName = "ShowFromName";

            /// <summary>
            /// The show reply to attribute key.
            /// </summary>
            public const string ShowReplyTo = "ShowReplyTo";

            /// <summary>
            /// The is bulk attribute key.
            /// </summary>
            public const string IsBulk = "IsBulk";

            /// <summary>
            /// The allowed SMS numbers attribute key.
            /// </summary>
            public const string AllowedSmsNumbers = "AllowedSMSNumbers";

            /// <summary>
            /// The show only personal SMS number attribute key.
            /// </summary>
            public const string ShowOnlyPersonalSmsNumber = "ShowOnlyPersonalSmsNumber";

            /// <summary>
            /// The hide personal SMS numbers attribute key.
            /// </summary>
            public const string HidePersonalSmsNumbers = "HidePersonalSmsNumbers";

            /// <summary>
            /// The person profile page attribute key.
            /// </summary>
            public const string PersonProfilePage = "PersonProfilePage";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether email should be displayed as an option of communication medium.
        /// </summary>
        /// <value><c>true</c> if [enable email]; otherwise, <c>false</c>.</value>
        public bool EnableEmail => GetAttributeValue( AttributeKey.EnableEmail ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether SMS should be displayed as an option of communication medium.
        /// </summary>
        /// <value><c>true</c> if [enable SMS]; otherwise, <c>false</c>.</value>
        public bool EnableSms => GetAttributeValue( AttributeKey.EnableSms ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether we should show a toggle, used to send a communication to the parent of any children in the recipients.
        /// </summary>
        /// <value><c>true</c> if [enable parent communication]; otherwise, <c>false</c>.</value>
        public bool ShowSendToParents => GetAttributeValue( AttributeKey.ShowSendToParents ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether this instance is bulk.
        /// </summary>
        /// <value><c>true</c> if this instance is bulk; otherwise, <c>false</c>.</value>
        public bool IsBulk => GetAttributeValue( AttributeKey.IsBulk ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether we should show the from name field.
        /// </summary>
        /// <value><c>true</c> if [show from name]; otherwise, <c>false</c>.</value>
        public bool ShowFromName => GetAttributeValue( AttributeKey.ShowFromName ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether we should show the reply to field.
        /// </summary>
        /// <value><c>true</c> if [show from name]; otherwise, <c>false</c>.</value>
        public bool ShowReplyTo => GetAttributeValue( AttributeKey.ShowReplyTo ).AsBoolean();

        /// <summary>
        /// Gets the person profile page unique identifier.
        /// </summary>
        /// <value>
        /// The person profile page unique identifier.
        /// </value>
        protected Guid? PersonProfilePageGuid => GetAttributeValue( AttributeKey.PersonProfilePage ).AsGuidOrNull();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 5;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Communication.CommunicationEntry";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>A collection of string/object pairs.</returns>
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Communication.CommunicationEntry.Configuration
            {
                EnableEmail = EnableEmail,
                EnableSms = EnableSms,
                ShowSendToParents = ShowSendToParents,
                IsBulk = IsBulk,
                ShowFromName = ShowFromName,
                ShowReplyTo = ShowReplyTo,
                PersonProfilePageGuid = PersonProfilePageGuid
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets all phone numbers available to the currently logged in person.
        /// </summary>
        /// <returns>A collection of phone number objects or an HTTP error.</returns>
        [BlockAction]
        public BlockActionResult GetPhoneNumbers()
        {
            return ActionOk( LoadPhoneNumbers() );
        }

        /// <summary>
        /// Gets the recipients.
        /// </summary>
        /// <param name="entitySetGuid">The entity set unique identifier.</param>
        /// <returns>A BlockActionResult containing the list of recipients.</returns>
        [BlockAction]
        public BlockActionResult GetRecipients( Guid entitySetGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                // First load the entity set from the entity set guid.
                var entitySet = new EntitySetService( rockContext )
                    .GetNoTracking( entitySetGuid );

                // Our recipients really should always be a Person entity,
                // but just in case lets check for it.
                var personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;

                if ( entitySet.EntityTypeId != personEntityTypeId )
                {
                    return ActionBadRequest();
                }

                var personService = new PersonService( rockContext ).Queryable();

                // Load the actual "recipients".
                var entitySetItems = new EntitySetItemService( rockContext )
                    .GetByEntitySetId( entitySet.Id )
                    .Select( esi => new
                    {
                        esi.Guid,
                        esi.EntityId
                    } )
                    // We need to take our entity set item and retrieve the corresponding person from the EntityId.
                    .Join(
                        personService,
                        esi => esi.EntityId,
                        p => p.Id,
                        ( esi, p ) => new
                        {
                            p.NickName,
                            p.LastName,
                            p.Email,
                            p.PhoneNumbers,
                            PersonGuid = p.Guid,
                            p.PhotoId,
                            EntitySetItemGuid = esi.Guid,
                        }
                    )
                    .AsNoTracking()
                    .ToList();

                var recipientBag = entitySetItems.Select( a => new CommunicationRecipientResponseBag
                {
                    NickName = a.NickName,
                    LastName = a.LastName,
                    Email = a.Email,
                    PersonGuid = a.PersonGuid,
                    EntitySetItemGuid = a.EntitySetItemGuid,
                    PhotoUrl = a.PhotoId != null ? MobileHelper.BuildPublicApplicationRootUrl( $"GetImage.ashx?Id={a.PhotoId}&maxwidth=256&maxheight=256" ) : string.Empty,
                    SmsNumber = a.PhoneNumbers.GetFirstSmsNumber(),
                } ).ToList();

                return ActionOk( recipientBag );
            }
        }

        /// <summary>
        /// Deletes the recipient from entity set.
        /// </summary>
        /// <param name="entitySetGuid">The entity set unique identifier.</param>
        /// <param name="entitySetItemGuid">The entity set item unique identifier.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult DeleteRecipientFromEntitySet( Guid entitySetGuid, Guid entitySetItemGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                // Load our recipient.
                var entitySetItemService = new EntitySetItemService( rockContext );
                var entitySetItem = entitySetItemService.Queryable()
                    .Where( esi => esi.Guid == entitySetItemGuid && esi.EntitySet.Guid == entitySetGuid )
                    .FirstOrDefault();

                if ( entitySetItem == null )
                {
                    return ActionNotFound();
                }

                entitySetItemService.Delete( entitySetItem );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Sends the communication.
        /// </summary>
        /// <param name="sendCommunicationBag">The send communication bag.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult SendCommunication( SendCommunicationRequestBag sendCommunicationBag )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( RequestContext.CurrentPerson == null )
                {
                    return ActionForbidden();
                }

                return ActionOk( SendCommunicationInternal( sendCommunicationBag ) );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the phone numbers that the current person is allowed to access.
        /// </summary>
        /// <returns>A collection of objects representing the SMS phone numbers.</returns>
        private IEnumerable<PhoneNumberBag> LoadPhoneNumbers()
        {
            // First load up all of the available numbers.
            var smsNumbers = SystemPhoneNumberCache.All( false )
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .Where( spn => spn.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) );

            // Filter to the numbers that are specifically configured for use with this block.
            var selectedNumberGuids = GetAttributeValue( AttributeKey.AllowedSmsNumbers ).SplitDelimitedValues( true ).AsGuidList();
            if ( selectedNumberGuids.Any() )
            {
                smsNumbers = smsNumbers.Where( v => selectedNumberGuids.Contains( v.Guid ) );
            }

            // Filter personal numbers (any that have a response recipient) if the hide personal option is enabled.
            if ( GetAttributeValue( AttributeKey.HidePersonalSmsNumbers ).AsBoolean() )
            {
                smsNumbers = smsNumbers.Where( spn => !spn.AssignedToPersonAliasId.HasValue );
            }

            // Show only numbers 'tied to the current' individual...unless they have 'Admin rights'.
            if ( GetAttributeValue( AttributeKey.ShowOnlyPersonalSmsNumber ).AsBoolean() && !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                smsNumbers = smsNumbers.Where( spn => RequestContext.CurrentPerson.Aliases.Any( a => a.Id == spn.AssignedToPersonAliasId ) );
            }

            return smsNumbers
                .Select( n => new PhoneNumberBag
                {
                    Guid = n.Guid,
                    PhoneNumber = n.Number,
                    Description = n.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Creates and sends a new communication to an entity set of recipients..
        /// </summary>
        /// <param name="communicationBag">The details of the communication.</param>
        /// <returns>A string describing the result of the operation.</returns>
        private string SendCommunicationInternal( SendCommunicationRequestBag communicationBag )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return "You must be logged in to attempt to send a communication.";
            }

            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var personService = new PersonService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );

                // We are just assuming this is a new communication, since the ability to modify one does not exist on the shell.
                var communication = new Rock.Model.Communication() { Status = CommunicationStatus.Transient };
                communication.SenderPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId;
                communicationService.Add( communication );

                // Convert our recipients to CommunicationRecipients.
                var entitySetId = new EntitySetService( rockContext ).Get( communicationBag.EntitySetGuid ).Id;
                var recipientPeople = new EntitySetItemService( rockContext )
                    .GetByEntitySetId( entitySetId )
                    .Select( esi => new
                    {
                        esi.EntityId
                    } )
                    // Join the person that is in reference to the EntityId of the EntitySetItem.
                    .Join( personService.Queryable(),
                        e => e.EntityId,
                        p => p.Id,
                        ( e, p ) => p
                    )
                    // It is technically possible for a person to exist that doesn't have a PrimaryPersonAlias.
                    // Cleaned up nightly by the Rock Job, only would happen if someone was manually inserting data in the
                    // database.
                    .Join( personAliasService.Queryable(),
                        p => p.Id,
                        pa => pa.AliasPersonId,
                        ( p, pa ) => new
                        {
                            Person = p,
                            PrimaryAliasId = pa.Id
                        }
                    ).ToList();

                var parents = new Dictionary<int?, List<int>>();

                // If this is enabled, we're going to send the communication to the
                // 'parents' of any children in the recipients list. 
                if ( communicationBag.SendToParents )
                {
                    // This is knowingly obscure. We are actually just going to include all adult family members in the family, since there
                    // is no great way to distinguish a child's father/mother in Rock today. In other words, Uncle Joe will also
                    // receive the communication.
                    parents = personService
                       .GetParentsForChildren( recipientPeople.Select( a => a.Person ).ToList() )
                       // We join the person alias service here to prevent an extra query for it later.
                       .Join( personAliasService.Queryable(),
                            p => p.Id,
                            pa => pa.AliasPersonId,
                            ( p, pa ) => new
                            {
                                PrimaryAliasId = pa.Id,
                                PrimaryFamilyId = p.PrimaryFamilyId
                            } )
                       .GroupBy( p => p.PrimaryFamilyId )
                       .ToDictionary( kvp => kvp.Key, kvp => kvp.Select( a => a.PrimaryAliasId ).ToList() );
                }

                // We have to loop through each of our recipient objects to create a real CommunicationRecipient.
                foreach ( var recipientPerson in recipientPeople )
                {
                    // Checking for duplicates.
                    if ( communication.Recipients.Any( cr => cr.PersonAliasId == recipientPerson.PrimaryAliasId ) )
                    {
                        continue;
                    };

                    // Include any parents in the communication.
                    if ( parents.Any() )
                    {
                        // If the parents we retrieved earlier contains an entry of this person's family,
                        // we process adding them here.
                        if ( parents.TryGetValue( recipientPerson.Person.PrimaryFamilyId, out var parentAliasIds ) )
                        {
                            foreach ( var parentAliasId in parentAliasIds )
                            {
                                // If the parent already exists in the recipients, we don't want to duplicate the communication.
                                if ( communication.Recipients.Any( cr => cr.PersonAliasId == parentAliasId ) )
                                {
                                    continue;
                                }

                                // Add the parent to the communication.
                                communication.Recipients.Add( new CommunicationRecipient
                                {
                                    PersonAliasId = parentAliasId
                                } );
                            }
                        }
                    }

                    communication.Recipients.Add( new CommunicationRecipient
                    {
                        PersonAliasId = recipientPerson.PrimaryAliasId
                    } );
                }

                communication.CommunicationType = ( CommunicationType ) communicationBag.CommunicationType;
                communication.IsBulkCommunication = communicationBag.IsBulk;

                // If the communication type is email, we set the according properties.
                if ( communication.CommunicationType == CommunicationType.Email )
                {
                    var emailMediumEntityTypeId = EntityTypeCache.Get<Rock.Communication.Medium.Email>().Id;
                    foreach ( var recipient in communication.Recipients )
                    {
                        recipient.MediumEntityTypeId = emailMediumEntityTypeId;
                    }

                    // Structuring the communication.
                    communication.Subject = communicationBag.Subject;
                    communication.Message = communicationBag.Message;
                    communication.FromEmail = communicationBag.FromEmail;
                    communication.FromName = communicationBag.FromName;
                    communication.ReplyToEmail = communicationBag.ReplyTo;

                    // If there was a file attachment, add it to the communication.
                    if ( communicationBag.FileAttachmentGuid != null )
                    {
                        var binaryFileId = new BinaryFileService( rockContext ).Get( communicationBag.FileAttachmentGuid.Value )?.Id;

                        if ( binaryFileId != null )
                        {
                            communication.AddAttachment( new CommunicationAttachment
                            {
                                BinaryFileId = binaryFileId.Value
                            }, CommunicationType.Email );
                        }
                    }
                }
                else if ( communication.CommunicationType == CommunicationType.SMS )
                {
                    var smsMediumTypeId = EntityTypeCache.Get<Rock.Communication.Medium.Sms>().Id;
                    foreach ( var recipient in communication.Recipients )
                    {
                        recipient.MediumEntityTypeId = smsMediumTypeId;
                    }

                    // Structuring the communication.
                    communication.CommunicationType = CommunicationType.SMS;
                    communication.SmsFromSystemPhoneNumberId = SystemPhoneNumberCache.Get( communicationBag.FromNumberGuid )?.Id;
                    communication.SMSMessage = communicationBag.Message;

                    // If there was an image attachment, add it to the communication.
                    if ( communicationBag.ImageAttachmentGuid != null )
                    {
                        var binaryFileId = new BinaryFileService( rockContext ).Get( communicationBag.ImageAttachmentGuid.Value )?.Id;

                        if ( binaryFileId != null )
                        {
                            communication.AddAttachment( new CommunicationAttachment
                            {
                                BinaryFileId = binaryFileId.Value
                            }, CommunicationType.SMS );
                        }
                    }

                }

                string successMessage = string.Empty;

                // Save the communication prior to checking recipients.
                communication.Status = CommunicationStatus.Draft;
                rockContext.SaveChanges();

                // This block uses a unique 'Approve' security verb to either submit a communication for approval or
                // instantly queue a communication. Only required if the request exceeds the maximum number of recipients.
                if ( RequiresApproval( communication.Recipients.Count(), communicationBag.MaxRecipients ) && !BlockCache.IsAuthorized( Authorization.APPROVE, RequestContext.CurrentPerson ) )
                {
                    communication.Status = CommunicationStatus.PendingApproval;
                    successMessage = $"Your message to {communication.Recipients.Count} recipients has been submitted for approval.";
                }
                else
                {
                    communication.Status = CommunicationStatus.Approved;
                    communication.ReviewedDateTime = RockDateTime.Now;
                    communication.ReviewerPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId;

                    if ( communication.FutureSendDateTime.HasValue &&
                        communication.FutureSendDateTime > RockDateTime.Now )
                    {
                        successMessage = string.Format( "Communication will be sent {0}.",
                            communication.FutureSendDateTime.Value.ToRelativeDateString( 0 ) );
                    }
                    else
                    {
                        successMessage = $"Your message to {communication.Recipients.Count} recipients has been queued for sending and should be delivered in the next few minutes.";
                    }
                }

                rockContext.SaveChanges();

                // Send the approval email (if needed), now that we have a communication id.
                if ( communication.Status == CommunicationStatus.PendingApproval )
                {
                    var approvalTransactionMsg = new ProcessSendCommunicationApprovalEmail.Message()
                    {
                        CommunicationId = communication.Id
                    };
                    approvalTransactionMsg.Send();
                }

                // If the communication is already approved (via security), queue it up to send.
                if ( communication.Status == CommunicationStatus.Approved &&
                           ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value <= RockDateTime.Now ) )
                {
                    var transactionMsg = new ProcessSendCommunication.Message()
                    {
                        CommunicationId = communication.Id
                    };
                    transactionMsg.Send();
                }

                rockContext.SaveChanges();
                return successMessage;
            }
        }

        /// <summary>
        /// Determines whether or not the communication (based on count) requires approval before
        /// being sent out. If they have permission via the "Approve" security verb, this gets
        /// overridden.
        /// </summary>
        /// <param name="communicationRecipientCount">The communication recipient count.</param>
        /// <param name="maxRecipients">The maximum recipients.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool RequiresApproval( int communicationRecipientCount, int? maxRecipients )
        {
            if ( maxRecipients != null && communicationRecipientCount > maxRecipients )
            {
                return true;
            }

            return false;
        }

        #endregion

    }
}

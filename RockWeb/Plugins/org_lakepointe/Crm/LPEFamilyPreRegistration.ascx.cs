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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

using Rock.Web.UI.Controls;
using org.lakepointe.Checkin.Controls;
using System.Linq.Dynamic;
using System.Text;
using Humanizer;
using Newtonsoft.Json;

namespace RockWeb.Plugins.org_lakepointe.Crm
{
    [DisplayName( "LPC - LPE New Family Pre Registration" )]
    [Category( "LPC > CRM" )]
    [Description( "Provides a way to allow people to pre-register their families for LPE weekend check-in." )]

    [BooleanField( "Show Campus", "Should the campus field be displayed?", true, "", 0 )]
    [CampusField( "Default Campus", "An optional campus to use by default when adding a new family.", false, "", "", 1 )]
    [CustomDropdownListField( "Planned Visit Date", "How should the Planned Visit Date field be displayed (this value is only used when starting a workflow)?", HIDE_OPTIONAL_REQUIRED, false, "Optional", "", 2 )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "GroupTypeId", Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "Family Attributes", "The Family attributes that should be displayed", false, true, "", "", 3 )]
    [BooleanField( "Allow Updates", "If the person visiting this block is logged in, should the block be used to update their family? If not, a new family will always be created unless 'Auto Match' is enabled and the information entered matches an existing person.", false, "", 4 )]
    [BooleanField( "Auto Match", "Should this block attempt to match people to to current records in the database.", true, "", 5)]
    [BooleanField("Auto-Fill for Logged in Users", "Should this block attempt to populate fields based upon who is logged in to Rock?", true, "", 6, AUTOFILL_KEY)]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Connection Status", "The connection status that should be used when adding new people.", false, false, Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR, "", 7 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Record Status", "The record status that should be used when adding new people.", false, false, Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE, "", 8 )]
    [WorkflowTypeField( "Workflow Types", @"
        The workflow type(s) to launch when a family is added. The primary family will be passed to each workflow as the entity. Additionally if the workflow type has any of the 
        following attribute keys defined, those attribute values will also be set: ParentIds, ChildIds, PlannedVisitDate.
        ", true, false, "", "", 9 )]
    [CodeEditorField( "Redirect URL", @"
        The URL to redirect user to when they have completed the registration. The merge fields that are available includes 'Family', which is an object for the primary family 
        that is created/updated; 'RelatedChildren', which is a list of the children who have a relationship with the family, but are not in the family; 'ParentIds' which is a
        comma-delimited list of the person ids for each adult; 'ChildIds' which is a comma-delimited list of the person ids for each child; and 'PlannedVisitDate' which is 
        the value entered for the Planned Visit Date field if it was displayed.
        ", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "", "", 10 )]
    [BooleanField("Require Campus", "Require that a campus be selected", true, "", 11)]

    [CustomDropdownListField( "Suffix", "How should Suffix be displayed for adults?", HIDE_OPTIONAL, false, "Hide", "Adult Fields", 0, ADULT_SUFFIX_KEY )]
    [CustomDropdownListField( "Gender", "How should Gender be displayed for adults?", HIDE_OPTIONAL_REQUIRED, false, "Optional", "Adult Fields", 1, ADULT_GENDER_KEY )]
    [CustomDropdownListField( "Birth Date", "How should Birth Date be displayed for adults?", HIDE_OPTIONAL_REQUIRED, false, "Optional", "Adult Fields", 2, ADULT_BIRTHDATE_KEY )]
    [CustomDropdownListField( "Marital Status", "How should Marital Status be displayed for adults?", HIDE_OPTIONAL_REQUIRED, false, "Required", "Adult Fields", 3, ADULT_MARTIAL_STATUS_KEY )]
    [CustomDropdownListField( "Email", "How should Email be displayed for adults?", HIDE_OPTIONAL_REQUIRED, false, "Required", "Adult Fields", 4, ADULT_EMAIL_KEY )]
    [CustomDropdownListField( "Mobile Phone", "How should Mobile Phone be displayed for adults?", HIDE_OPTIONAL_REQUIRED, false, "Required", "Adult Fields", 5, ADULT_MOBILE_KEY )]
    [AttributeCategoryField( "Attribute Categories", "The adult Attribute Categories to display attributes from", true, "Rock.Model.Person", false, "", "Adult Fields", 6, ADULT_CATEGORIES_KEY )]
    [CustomDropdownListField( "Take Photo", "Should the option to take a photo be visible? Do not display take and upload on the same form.", HIDE_OPTIONAL, false, "Hide", "Adult Fields", 7, ADULT_TAKE_PHOTO_KEY )]
    [CustomDropdownListField("Upload Photo", "Should the option to upload a photo be visible? Do not display take and upload on the same form.", HIDE_OPTIONAL, false, "Hide", "Adult Fields", 8, ADULT_UPLOAD_PHOTO_KEY)]

    [TextField( "Adult Title", "How should adults be listed in the interface?", false, "Adulto", "", 6, ADULT_TITLE_KEY )]
    [TextField( "Progeny Title", "How should progeny be listed in the interface?", false, "Niño", "", 7, PROGENY_TITLE_KEY )]
    [CustomDropdownListField( "Suffix", "How should Suffix be displayed for children?", HIDE_OPTIONAL, false, "Hide", "Child Fields", 0, CHILD_SUFFIX_KEY )]
    [CustomDropdownListField( "Gender", "How should Gender be displayed for children?", HIDE_OPTIONAL_REQUIRED, false, "Optional", "Child Fields", 1, CHILD_GENDER_KEY )]
    [CustomDropdownListField( "Birth Date", "How should Birth Date be displayed for children?", HIDE_OPTIONAL_REQUIRED, false, "Required", "Child Fields", 2, CHILD_BIRTHDATE_KEY )]
    [CustomDropdownListField( "Email", "How should Email be displayed for children?", HIDE_OPTIONAL_REQUIRED, false, "Hide", "Child Fields", 4, CHILD_EMAIL_KEY )]
    [CustomDropdownListField( "Grade", "How should Grade be displayed for children?", HIDE_OPTIONAL_REQUIRED, false, "Optional", "Child Fields", 3, CHILD_GRADE_KEY )]
    [CustomDropdownListField("Email", "How should Email be displayed for children?  Be sure to seek legal guidance when collecting email addresses on minors.", HIDE_OPTIONAL_REQUIRED, false, "Hide", "Child Fields", 4, CHILD_EMAIL_KEY)]
    [CustomDropdownListField( "Mobile Phone", "How should Mobile Phone be displayed for children?", HIDE_OPTIONAL_REQUIRED, false, "Hide", "Child Fields", 5, CHILD_MOBILE_KEY )]
    [CustomDropdownListField("Communication Preference", "How should Communication Preference be displayed for children?", HIDE_OPTIONAL, false, "Hide", "Child Fields", 6, CHILD_COMMUNICATION_PREFERENCE)]
    [CustomDropdownListField( "Take Photo", "Should the option to take a photo be visible? Do not display take and upload on the same form.", HIDE_OPTIONAL, false, "Hide", "Child Fields", 7, CHILD_TAKE_PHOTO_KEY )]
    [CustomDropdownListField("Upload Photo", "Should the option to upload a photo be visible? Do not display take and upload on the same form.", HIDE_OPTIONAL, false, "Hide", "Child Fields", 8, CHILD_UPLOAD_PHOTO_KEY)]

    [AttributeCategoryField( "Attribute Categories", "The children Attribute Categories to display attributes from.", true, "Rock.Model.Person", false, "", "Child Fields", 8, CHILD_CATEGORIES_KEY )]

    [CodeEditorField("Acknowledgement Message", "Lava/HTML Acknoledgement message that need to be accepted before the user can submit the form. If blank acknowlegement will not be displayed.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false,"", "Acknowledgement", 0, ACKNOWLEDGEMENT_MESSAGE_KEY)]
    [TextField("Acknowledgement Accept Label", "The label for the Accept Acknowledgement message", false, "I Agree", "Acknowledgement", 1, ACKNOWLEDGEMENT_CHECKBOX_LABEL_KEY)]
    [LavaCommandsField("Enabled Lava Commands", "The Lava commands that should be enabled for the Acknowledgement Message.", false, "", "Acknowledgement", 2, ACKNOWLEDGEMENT_LAVA_COMMANDS_KEY)]

    [CustomEnhancedListField( "Relationship Types", "The possible child-to-adult relationships. The value 'Child' will always be included.", @"
        SELECT 
	        R.[Id] AS [Value],
	        R.[Name] AS [Text]
        FROM [GroupType] T
        INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
        WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
        AND R.[Name] <> 'Child'
        UNION ALL
        SELECT 0, 'Child'
        ORDER BY [Text]", false, "0", "Child Relationship", 0, "Relationships" )]
    [CustomEnhancedListField( "Same Immediate Family Relationships", "The relationships which indicate the child is in the same immediate family as the adult(s) rather than creating a new family for the child. In most cases, 'Child' will be the only value included in this list. Any values included in this list that are not in the Relationship Types list will be ignored.", @"
        SELECT 
	        R.[Id] AS [Value],
	        R.[Name] AS [Text]
        FROM [GroupType] T
        INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
        WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
        AND R.[Name] <> 'Child'
        UNION ALL
        SELECT 0, 'Child'
        ORDER BY [Text]", false, "0", "Child Relationship", 1, "FamilyRelationships" )]
    [CustomEnhancedListField( "Can Check-in Relationship", "Any relationships that, if selected, will also create an additional 'Can Check-in' relationship between the adult and the child. This is only necessary if the relationship (selected by the user) does not have the 'Allow Check-in' option.", @"
        SELECT 
	        R.[Id] AS [Value],
	        R.[Name] AS [Text]
        FROM [GroupType] T
        INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
        WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
        AND R.[Name] <> 'Child'
        UNION ALL
        SELECT 0, 'Child'
        ORDER BY [Text]", false, "", "Child Relationship", 2, "CanCheckinRelationships" )]
    [CustomEnhancedListField( "Temporary Can Check-in Relationship", "Any relationships that, if selected, will also create an additional 'Temporary Can Check-in' relationship between the adult and the child. This is only necessary if the relationship (selected by the user) does not have the 'Allow Check-in' option.", @"
        SELECT 
	        R.[Id] AS [Value],
	        R.[Name] AS [Text]
        FROM [GroupType] T
        INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
        WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
        AND R.[Name] <> 'Child'
        UNION ALL
        SELECT 0, 'Child'
        ORDER BY [Text]", false, "", "Child Relationship", 3, "TemporaryCanCheckinRelationships" )]

    public partial class LPEFamilyPreRegistration : RockBlock
    {
        private const string AUTOFILL_KEY = "Autofill";

        private const string ADULT_SUFFIX_KEY = "AdultSuffix";
        private const string ADULT_GENDER_KEY = "AdultGender";
        private const string ADULT_BIRTHDATE_KEY = "AdultBirthdate";
        private const string ADULT_MARTIAL_STATUS_KEY = "AdultMaritalStatus";
        private const string ADULT_EMAIL_KEY = "AdultEmail";
        private const string ADULT_MOBILE_KEY = "AdultMobilePhone";
        private const string ADULT_CATEGORIES_KEY = "AdultAttributeCategories";
        private const string ADULT_TAKE_PHOTO_KEY = "AdultTakePhoto";
        private const string ADULT_UPLOAD_PHOTO_KEY = "AdultUploadPhoto";

        private const string ADULT_TITLE_KEY = "AdultTitle";
        private const string PROGENY_TITLE_KEY = "ProgenyTitle";
        private const string CHILD_SUFFIX_KEY = "ChildSuffix";
        private const string CHILD_GENDER_KEY = "ChildGender";
        private const string CHILD_BIRTHDATE_KEY = "ChildBirthdate";
        private const string CHILD_EMAIL_KEY = "ChildEmail";
        private const string CHILD_COMMUNICATION_PREFERENCE = "ChildCommunicationPreference";
        private const string CHILD_GRADE_KEY = "ChildGrade";
        private const string CHILD_MOBILE_KEY = "ChildMobilePhone";
        private const string CHILD_CATEGORIES_KEY = "ChildAttributeCategories";
        private const string CHILD_TAKE_PHOTO_KEY = "ChildTakePhoto";
        private const string CHILD_UPLOAD_PHOTO_KEY = "ChildUploadPhoto";

        private const string ACKNOWLEDGEMENT_MESSAGE_KEY = "AcknowledgementMessage";
        private const string ACKNOWLEDGEMENT_CHECKBOX_LABEL_KEY = "AcknowledgementCheckboxLabel";
        private const string ACKNOWLEDGEMENT_LAVA_COMMANDS_KEY = "AcknowledgementLavaCommands";

        private const string HIDE_OPTIONAL_REQUIRED = "Hide,Optional,Required";
        private const string HIDE_OPTIONAL = "Hide,Optional";



        #region Fields

        private RockContext _rockContext = null;
        private Dictionary<int, string>  _relationshipTypes = new Dictionary<int, string>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the child members that have been added by user
        /// </summary>
        /// <value>
        /// The group members.
        /// </value>
        protected List<LPCPreRegistrationChild> Children { get; set; }
        protected string ValGroupName { get { return this.BlockValidationGroup; } }

        private string _path;
        public string Path
        {
            get
            {
                if ( _path.IsNullOrWhiteSpace() )
                {
                    _path = MapPath( ResolveRockUrl( "~/Plugins/org_lakepointe/Crm/FamilyPreRegistration/ChildRow.es.json" ) );
                }
                return _path;
            }
        }
        private Dictionary<string, string> _translation;
        public Dictionary<string, string> Translation
        {
            get
            {
                if ( _translation == null )
                {
                    _translation = new Dictionary<string, string>();
                    try
                    {
                        using ( StreamReader r = new StreamReader( Path ) )
                        {
                            string json = r.ReadToEnd();
                            _translation = JsonConvert.DeserializeObject<Dictionary<string, string>>( json );
                        }
                    }
                    catch ( FileNotFoundException ) { }
                }
                return _translation;
            }
            set
            {
                _translation = value;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            Children = ViewState["Children"] as List<LPCPreRegistrationChild> ?? new List<LPCPreRegistrationChild>();

            BuildAdultAttributes( false, null, null );
            BuildFamilyAttributes( false, null );
            CreateChildrenControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _rockContext = new RockContext();

            var progenyName = GetAttributeValue( PROGENY_TITLE_KEY );
            if ( progenyName.IsNotNullOrWhiteSpace() )
            {
                lProgenyLabel.Text = string.Format( @"<h3 class=""panel-title"">{0}</h3>", progenyName );
            }

            // Get the allowed relationship types that have been selected
            var selectedRelationshipTypeIds = GetAttributeValue( "Relationships" ).SplitDelimitedValues().AsIntegerList();
            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
            if ( knownRelationshipGroupType != null )
            {
                _relationshipTypes = knownRelationshipGroupType.Roles
                    .Where( r => selectedRelationshipTypeIds.Contains( r.Id ) )
                    .ToDictionary( k => k.Id, v => v.Name );
            }
            if ( selectedRelationshipTypeIds.Contains( 0 ) )
            {
                _relationshipTypes.Add( 0, Translation.GetValueOrDefault("Child", "Niño") );
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

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
                SetControls();
            }
            else
            {
                GetChildrenData();
            }

            ScriptManager.RegisterStartupScript( upnlContent, upnlContent.GetType(), "loadImages" + RockDateTime.Now.Ticks.ToString(), "loadImages();", true );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["Children"] = Children;

            return base.SaveViewState();
        }

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            string script = string.Format( @"
    testRequiredFields();

    $('#{0}').on('blur', function () {{
        testRequiredFields();
    }})

    $('#{1}').on('blur', function () {{
        testRequiredFields();
    }})

    function testRequiredFields() {{
        var hasValue = $('#{0}').val() != '' && $('#{1}').val() != '';
        enableRequiredFields( hasValue );

        if (hasValue) {{
            var required = $('#{2}').val() == 'True';
            if (required) {{
                $('#{3}').closest('.form-group').addClass('required');
            }} else {{
                $('#{3}').closest('.form-group').removeClass('required');
            }}

            required = $('#{4}').val() == 'True';
            if (required) {{
                var temp =  $('#{5}').closest('form-group');
                $('#{5}').closest('.form-group').addClass('required');
            }} else {{
                $('#{5}').closest('.form-group').removeClass('required');
            }}

            required = $('#{6}').val() == 'True';
            if (required) {{
                $('#{7}').closest('.form-group').addClass('required');
            }} else {{
                $('#{7}').closest('.form-group').removeClass('required');
            }}

            required = $('#{8}').val() == 'True';
            if (required) {{
                $('#{9}').closest('.form-group').addClass('required');
            }} else {{
                $('#{9}').closest('.form-group').removeClass('required');
            }}

            required = $('#{10}').val() == 'True';
            if (required) {{
                $('#{11}').closest('.form-group').addClass('required');
            }} else {{
                $('#{11}').closest('.form-group').removeClass('required');
            }}

            required = $('#{12}').val() == 'True';
            if (required) {{
                $('#{13}').closest('.form-group').addClass('required');
            }} else {{
                $('#{13}').closest('.form-group').removeClass('required');
            }}


        }} else {{
            $('#{3}').closest('.form-group').removeClass('required');
            $('#{5}').closest('.form-group').removeClass('required');
            $('#{7}').closest('.form-group').removeClass('required');
            $('#{9}').closest('.form-group').removeClass('required');
            $('#{11}').closest('.form-group').removeClass('required');
            $('#{13}').closest('.form-group').removeClass('required');
        }}
    }}
",
                tbFirstName2.ClientID,
                tbLastName2.ClientID,

                hfSuffixRequired.ClientID,
                dvpSuffix2.ClientID,

                hfGenderRequired.ClientID,
                ddlGender2.ClientID,

                hfBirthDateRequired.ClientID,
                dpBirthDate2.ClientID,

                hfMaritalStatusRequired.ClientID,
                dvpMaritalStatus2.ClientID,

                hfMobilePhoneRequired.ClientID,
                pnMobilePhone2.ClientID,

                hfEmailRequired.ClientID,
                tbEmail2.ClientID

            );

            ScriptManager.RegisterStartupScript( tbFirstName2, tbFirstName2.GetType(), "adult2-validation", script, true );

            var adultTitle = GetAttributeValue( ADULT_TITLE_KEY );
            if ( adultTitle != Translation.GetValueOrDefault( "Adult", "Adulto" ) ) // don't bother changing if it is the value that matches the json file
            {
                string patchAdult = $@"
  $('body :not(script)').contents().filter( function() {{
                    return this.nodeType === 3;
                }}).replaceWith( function() {{
                    return this.nodeValue.replace( '{Translation.GetValueOrDefault( "Adult", "Adulto" )}', '{adultTitle}' );
                }});
";
                ScriptManager.RegisterStartupScript( upnlContent, upnlContent.GetType(), "patchAdult", patchAdult, true );
            }

            var childTitle = GetAttributeValue( PROGENY_TITLE_KEY );
            
            var patchChild = new StringBuilder();

            // Fix "Add" button
            patchChild.Append( $@"
$('body :not(script)').contents().filter( function() {{
                return this.nodeType === 3;
            }}).replaceWith( function() {{
                return this.nodeValue.replace( 'Add Child', '{Translation.GetValueOrDefault( "LabelAddChild", "Añadir Niño" )}' );
            }});");
            ScriptManager.RegisterStartupScript( upnlContent, upnlContent.GetType(), "patchChild", patchChild.ToString(), true );
            
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetControls();
        }

        [System.Web.Services.WebMethod()]
        [System.Web.Script.Services.ScriptMethod()]
        protected void lbCamera_Click( object sender, EventArgs e )
        {
            var lb = sender as LinkButton;
            if ( lb != null )
            {
                IndexedSnap( lb.ID );
            }
        }

        private void ChildRow_Click( object sender, LpcChildClickEventArgs e )
        {
            ScriptManager.RegisterStartupScript( dlgCamera, dlgCamera.GetType(), "callSnapImage" + RockDateTime.Now.Ticks.ToString(), String.Format( "snapImage({0},{1});", e.HiddenField, e.Canvas ), true );
            dlgCamera.Title = Translation.GetValueOrDefault( "HeaderTakePhoto", "Tomar Foto de Persona" );
            dlgCamera.Show();
        }

        private void IndexedSnap( String id )
        {
            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match( id, @"lbCamera([0-9]+)" );
            if (m.Success)
            {
                ScriptManager.RegisterStartupScript( dlgCamera, dlgCamera.GetType(), "callSnapImage" + RockDateTime.Now.Ticks.ToString(), String.Format("snapImage(hfImage{0},canvas{0});", m.Groups[1].Value), true );
                dlgCamera.Title = Translation.GetValueOrDefault( "HeaderTakePhoto", "Tomar Foto de Persona" );
                dlgCamera.Show();
            }
        }

        /// <summary>
        /// Handles the AddChildClick event of the prChildren control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void prChildren_AddChildClick( object sender, EventArgs e )
        {
            AddChild();
            CreateChildrenControls( true );
        }

        /// <summary>
        /// Handles the DeleteClick event of the ChildRow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void ChildRow_DeleteClick( object sender, EventArgs e )
        {
            var row = sender as PreRegistrationChildRow;
            var child = Children.FirstOrDefault( m => m.Guid.Equals( row.PersonGuid ) );
            if ( child != null )
            {
                Children.Remove( child );
            }

            CreateChildrenControls( true );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if (!ValidateInfo())
            {
                var script = string.Format("$('#{0}').val('');", lbSave.ClientID);
                ScriptManager.RegisterStartupScript(upnlContent, upnlContent.GetType(), "UpdateSaveLinkValue" + DateTime.Now.Ticks, script, true);

                return;
            }
            // Get some system values
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var adultRoleId = familyGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
            var childRoleId = familyGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;

            var recordTypePersonId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            var recordStatusValue = DefinedValueCache.Get( GetAttributeValue( "RecordStatus" ).AsGuid() ) ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            var connectionStatusValue = DefinedValueCache.Get( GetAttributeValue( "ConnectionStatus" ).AsGuid() ) ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() );

            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
            var canCheckInRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid() );
            var temporaryCanCheckInRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == "EFAEE6AE-6889-43D8-84F2-25154AACEF69".AsGuid() );
            var knownRelationshipChild = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CHILD.AsGuid() );
            var knownRelationshipParent = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_PARENT.AsGuid() );
            var knownRelationshipOwnerRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();

            // ...and some block settings
            var familyRelationships = GetAttributeValue( "FamilyRelationships" ).SplitDelimitedValues().AsIntegerList();
            var canCheckinRelationships = GetAttributeValue( "CanCheckinRelationships" ).SplitDelimitedValues().AsIntegerList();
            var temporaryCanCheckinRelationships = GetAttributeValue( "TemporaryCanCheckinRelationships" ).SplitDelimitedValues().AsIntegerList();
            var showChildMobilePhone = GetAttributeValue( CHILD_MOBILE_KEY ) != "Hide";
            var showChildEmailAddress = GetAttributeValue(CHILD_EMAIL_KEY) != "Hide";

            // ...and some service objects
            var personService = new PersonService( _rockContext );
            var groupService = new GroupService( _rockContext );
            var groupMemberService = new GroupMemberService( _rockContext );
            var groupLocationService = new GroupLocationService( _rockContext );

            // Check to see if we're viewing an existing family
            Group primaryFamily = null;
            Guid? familyGuid = hfFamilyGuid.Value.AsGuidOrNull();
            if ( familyGuid.HasValue )
            {
                primaryFamily = groupService.Get( familyGuid.Value );
            }

            // If editing an existing family, we'll need to handle any children/relationships that they remove
            bool processRemovals = primaryFamily != null && GetAttributeValue( "AllowUpdates" ).AsBoolean();

            // If editing an existing family, we should also save any empty family values (campus, address)
            bool saveEmptyValues = primaryFamily != null;

            // Save the adults
            var adultIds = new List<int>();
            SaveAdult( ref primaryFamily, adultIds, 1, hfAdultGuid1, tbFirstName1, tbLastName1, dvpSuffix1, ddlGender1, dpBirthDate1, dvpMaritalStatus1, tbEmail1, pnMobilePhone1, cbPermissionToText1, phAttributes1, hfImage1, uploadPhoto1);
            SaveAdult( ref primaryFamily, adultIds, 2, hfAdultGuid2, tbFirstName2, tbLastName2, dvpSuffix2, ddlGender2, dpBirthDate2, dvpMaritalStatus2, tbEmail2, pnMobilePhone2, cbPermissionToText2, phAttributes2, hfImage2, uploadPhoto2);

            // If two adults were entered, let's check to see if we should assume they're married
            if ( adultIds.Count == 2 )
            {
                var marriedStatusValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() );
                if ( marriedStatusValue != null )
                {
                    var adults = personService.Queryable().Where( p => adultIds.Contains( p.Id ) ).ToList();

                    // as long as neither of the adults has a marital status
                    if ( !adults.Any( a => a.MaritalStatusValueId.HasValue ) )
                    {
                        // Set them all to married
                        foreach ( var adult in adults )
                        {
                            adult.MaritalStatusValueId = marriedStatusValue.Id;
                        }
                        _rockContext.SaveChanges();
                    }
                }
            }

            if (primaryFamily == null)
            {
                primaryFamily = CreateNewFamily(familyGroupType.Id, (tbLastName1.Text.IsNotNullOrWhiteSpace() ? tbLastName1.Text : tbLastName2.Text));
                groupService.Add(primaryFamily);
                saveEmptyValues = true;
            }

            // Handle 3 save cases for a family's campus in a defined order
            // Set as Default
            Guid? campusGuid = GetAttributeValue("DefaultCampus").AsGuidOrNull();
            if (campusGuid.HasValue)
            {
                var defaultCampus = CampusCache.Get(campusGuid.Value);
                if (defaultCampus != null)
                {
                    primaryFamily.CampusId = defaultCampus.Id;
                }
            }

            // If selector is visible, use that
            if (cpCampus.Visible)
            {
                primaryFamily.CampusId = cpCampus.SelectedValueAsInt();
            }

            // If query string is passed and the selector is hidden, default to that
            int? queryStrCampus = Request.QueryString["campus"].AsIntegerOrNull();
            if (queryStrCampus.HasValue && !cpCampus.Visible)
            {
                // Validate query string input
                var confirmedCampus = CampusCache.Get(queryStrCampus.Value);
                if (confirmedCampus != null)
                {
                    primaryFamily.CampusId = confirmedCampus.Id;
                }
                    
            }

            // Save the family
            _rockContext.SaveChanges();

            // Make sure adults are part of the primary family, and if not, add them.
            foreach( int id in adultIds )
            {
                var currentFamilyMember = primaryFamily.Members.FirstOrDefault( m => m.PersonId == id );
                if ( currentFamilyMember == null )
                {
                    currentFamilyMember = new GroupMember
                    {
                        GroupId = primaryFamily.Id,
                        PersonId = id,
                        GroupRoleId = adultRoleId,
                        GroupMemberStatus = GroupMemberStatus.Active
                    };

                    groupMemberService.Add( currentFamilyMember );

                    _rockContext.SaveChanges();
                }
            }

            // Save the family address
            var homeLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
            if ( homeLocationType != null )
            {
                // Find a location record for the address that was entered
                var loc = new Rock.Model.Location();
                acAddress.GetValues( loc );
                if ( acAddress.Street1.IsNotNullOrWhiteSpace() && loc.City.IsNotNullOrWhiteSpace() )
                {
                    loc = new LocationService( _rockContext ).Get(
                        loc.Street1, loc.Street2, loc.City, loc.State, loc.PostalCode, loc.Country, primaryFamily, true );
                }
                else
                {
                    loc = null;
                }

                // Check to see if family has an existing home address
                var groupLocation = primaryFamily.GroupLocations
                    .FirstOrDefault( l =>
                        l.GroupLocationTypeValueId.HasValue &&
                        l.GroupLocationTypeValueId.Value == homeLocationType.Id );

                if ( loc != null )
                {
                    if ( groupLocation == null || groupLocation.LocationId != loc.Id )
                    {
                        // If family does not currently have a home address or it is different than the one entered, add a new address (move old address to prev)
                        GroupService.AddNewGroupAddress( _rockContext, primaryFamily, homeLocationType.Guid.ToString(), loc, true, string.Empty, true, true );
                    }
                }
                else
                {
                    if ( groupLocation != null && saveEmptyValues )
                    {
                        // If an address was not entered, and family has one on record, update it to be a previous address
                        var prevLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() );
                        if ( prevLocationType != null )
                        {
                            groupLocation.GroupLocationTypeValueId = prevLocationType.Id;
                        }
                    }
                }

                _rockContext.SaveChanges();
            }

            // Save any family attribute values
            primaryFamily.LoadAttributes( _rockContext );
            Helper.GetEditValues( phFamilyAttributes, primaryFamily );
            primaryFamily.SaveAttributeValues( _rockContext );

            // Get the adult known relationship groups
            var knownRelationshipGroupIds = groupMemberService.Queryable()
                .Where( m =>
                    m.GroupRole.Guid == knownRelationshipOwnerRoleGuid &&
                    adultIds.Contains( m.PersonId ) )
                .Select( m => m.GroupId )
                .ToList();

            // Variables for tracking the new children/relationships that should exist
            var newChildIds = new List<int>();
            var newRelationships = new Dictionary<int, List<int>>();

            // Reload the primary family
            primaryFamily = groupService.Get( primaryFamily.Id );

            // Loop through each of the children
            var newFamilyIds = new Dictionary<string, int>();
            foreach ( var child in Children )
            {
                // Save the child's person information
                Rock.Model.Person person = personService.Get( child.Guid );

                // If person was not found, Look for existing person in same family with same name and birthdate
                if ( person == null && child.BirthDate.HasValue )
                {
                    var possibleMatch = new Rock.Model.Person { NickName = child.NickName, LastName = child.LastName };
                    possibleMatch.SetBirthDate( child.BirthDate );
                    person = primaryFamily.MatchingFamilyMember( possibleMatch  );
                }

                // Otherwise create a new person
                if ( person == null )
                {
                    person = new Rock.Model.Person();
                    personService.Add( person );

                    person.Guid = child.Guid;
                    person.FirstName = child.NickName.FixCase();
                    person.LastName = child.LastName.FixCase();
                    person.RecordTypeValueId = recordTypePersonId;
                    person.RecordStatusValueId = recordStatusValue != null ? recordStatusValue.Id : (int?)null;
                    person.ConnectionStatusValueId = connectionStatusValue != null ? connectionStatusValue.Id : (int?)null;
                }
                else
                {
                    person.NickName = child.NickName;
                    person.LastName = child.LastName;
                }

                if ( child.SuffixValueId.HasValue )
                {
                    person.SuffixValueId = child.SuffixValueId;
                }

                if ( child.Gender != Gender.Unknown )
                {
                    person.Gender = child.Gender;
                }

                if ( child.BirthDate.HasValue )
                {
                    person.SetBirthDate( child.BirthDate );
                }

                if ( child.GradeOffset.HasValue )
                {
                    person.GradeOffset = child.GradeOffset;
                }

                // Save the email address
                if (showChildEmailAddress && child.EmailAddress.IsNotNullOrWhiteSpace())
                {
                    person.Email = child.EmailAddress;
                }

                // Keep record of current photo for cleanup
                int? orphanedPhotoId = null;

                // Save photo taken in the browser
                if (child.TakenImage.Length > 23 && GetAttributeValue(CHILD_TAKE_PHOTO_KEY) != "Hide")
                {
                    // Always create a new BinaryFile record of IsTemporary when a file is uploaded
                    var binaryFileService = new BinaryFileService(_rockContext);
                    Guid fileTypeGuid = Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid();
                    BinaryFileType binaryFileType = new BinaryFileTypeService(_rockContext).Get(fileTypeGuid);

                    var binaryFile = new BinaryFile();

                    binaryFileService.Add(binaryFile);

                    byte[] imageBytes = Convert.FromBase64String(child.TakenImage.Substring(23));
                    MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                    binaryFile.ContentStream = ms;

                    // Assume file is temporary unless specified otherwise so that files that don't end up getting used will get cleaned up
                    binaryFile.IsTemporary = GetAttributeValue("IsTemporary").AsBooleanOrNull() ?? true;
                    binaryFile.BinaryFileTypeId = binaryFileType.Id;
                    binaryFile.MimeType = "image/jpeg";
                    binaryFile.FileName = "new picture";

                    _rockContext.SaveChanges();

                    orphanedPhotoId = person.PhotoId;
                    person.PhotoId = binaryFile.Id;
                }
                else
                {
                    _rockContext.SaveChanges();
                }

                // Save photo that is uploaded
                if (child.UploadedImage != null && person.PhotoId != child.UploadedImage && GetAttributeValue(CHILD_UPLOAD_PHOTO_KEY) != "Hide")
                {
                    orphanedPhotoId = person.PhotoId;
                    person.PhotoId = child.UploadedImage;
                }

                // Cleanup old photo if replaced (code taken from PublicProfileEdit)
                if (orphanedPhotoId.HasValue)
                {
                    BinaryFileService binaryFileService = new BinaryFileService(_rockContext);
                    var binaryFile = binaryFileService.Get(orphanedPhotoId.Value);
                    if (binaryFile != null)
                    {
                        // marked the old images as IsTemporary so they will get cleaned up later
                        binaryFile.IsTemporary = true;
                        _rockContext.SaveChanges();
                    }
                }

                // If they used the ImageEditor, and cropped it, the un-cropped file is still in BinaryFile. So clean it up
                if (child.PreCropBinaryFileId.HasValue)
                {
                    if (child.PreCropBinaryFileId != person.PhotoId)
                    {
                        BinaryFileService binaryFileService = new BinaryFileService(_rockContext);
                        var binaryFile = binaryFileService.Get(child.PreCropBinaryFileId.Value);
                        if (binaryFile != null && binaryFile.IsTemporary)
                        {
                            string errorMessage;
                            if (binaryFileService.CanDelete(binaryFile, out errorMessage))
                            {
                                binaryFileService.Delete(binaryFile);
                                _rockContext.SaveChanges();
                            }
                        }
                    }
                }

                // Save the mobile phone number
                if ( showChildMobilePhone && child.MobilePhoneNumber.IsNotNullOrWhiteSpace() )
                {
                    SavePhoneNumber( person.Id, child.MobilePhoneNumber, child.MobileCountryCode );
                }

                // Save the attributes for the child
                person.LoadAttributes();
                // Save the miscellaneous attributes
                foreach( var keyVal in child.AttributeValues )
                {
                    if ( keyVal.Value.IsNotNullOrWhiteSpace() )
                    {
                        person.SetAttributeValue( keyVal.Key, keyVal.Value );
                    }
                }
                // Save the allergy attribute
                if (child.HasAllergy == true)
                {
                    person.SetAttributeValue("Allergy", child.Allergy);
                }
                else
                {
                    person.SetAttributeValue("Allergy", string.Empty);
                }
                person.SaveAttributeValues( _rockContext );

                // Get the child's current family state
                bool inPrimaryFamily = primaryFamily.Members.Any( m => m.PersonId == person.Id );

                // Get what the family/relationship state should be for the child
                bool shouldBeInPrimaryFamily = familyRelationships.Contains( child.RelationshipType ?? 0 );
                int? newRelationshipId = shouldBeInPrimaryFamily ? (int?)null : child.RelationshipType;

                // Check to see if child needs to be added to the primary family or not
                if ( shouldBeInPrimaryFamily )
                {
                    // If so, add to list of children
                    newChildIds.Add( person.Id );

                    if ( !inPrimaryFamily )
                    {
                        var familyMember = new GroupMember();
                        familyMember.GroupId = primaryFamily.Id;
                        familyMember.PersonId = person.Id;
                        familyMember.GroupRoleId = childRoleId;
                        familyMember.GroupMemberStatus = GroupMemberStatus.Active;

                        groupMemberService.Add( familyMember );

                        _rockContext.SaveChanges();
                    }

                    foreach (var adultId in adultIds)
                    {
                        groupMemberService.CreateKnownRelationship( adultId, person.Id, canCheckInRole.Id );
                        newRelationships.AddOrIgnore( person.Id, new List<int>() );
                        newRelationships[person.Id].Add( canCheckInRole.Id );
                    }
                }
                else
                {
                    // Make sure they have another family
                    EnsurePersonInOtherFamily( familyGroupType.Id, primaryFamily.Id, person.Id, person.LastName, childRoleId, newFamilyIds );

                    // If the selected relationship for this person should also create the can-check in relationship, make sure to add it
                    if ( canCheckinRelationships.Contains( child.RelationshipType ?? -1 ) )
                    {
                        foreach ( var adultId in adultIds )
                        {
                            groupMemberService.CreateKnownRelationship( adultId, person.Id, canCheckInRole.Id );
                            newRelationships.AddOrIgnore( person.Id, new List<int>() );
                            newRelationships[person.Id].Add( canCheckInRole.Id );
                        }
                    }

                    // If the selected relationship for this person should also create the temporary can-check in relationship, make sure to add it
                    if (temporaryCanCheckinRelationships.Contains( child.RelationshipType ?? -1 ))
                    {
                        foreach (var adultId in adultIds)
                        {
                            groupMemberService.CreateKnownRelationship( adultId, person.Id, temporaryCanCheckInRole.Id );
                            newRelationships.AddOrIgnore( person.Id, new List<int>() );
                            newRelationships[person.Id].Add( temporaryCanCheckInRole.Id );
                        }
                    }
                }

                // Check to see if child needs to be removed from the primary family
                if ( !shouldBeInPrimaryFamily && inPrimaryFamily )
                {
                    RemovePersonFromFamily( familyGroupType.Id, primaryFamily.Id, person.Id );
                }

                // If child has a relationship type, make sure they belong to a family, and ensure that they have that relationship with each adult
                if ( newRelationshipId.HasValue )
                {
                    foreach ( var adultId in adultIds )
                    {
                        groupMemberService.CreateKnownRelationship( adultId, person.Id, newRelationshipId.Value );
                        newRelationships.AddOrIgnore( person.Id, new List<int>() );
                        newRelationships[person.Id].Add( newRelationshipId.Value );
                    }
                }
            }

            // If editing an existing family, check for any people that need to be removed from the family or relationships
            if ( processRemovals )
            {
                // Find all the existing children that were removed and make sure they have another family, and then remove them from this family.
                var na = new Dictionary<string, int>();
                foreach ( var removedChild in groupMemberService.Queryable()
                    .Where( m =>
                        m.GroupId == primaryFamily.Id &&
                        m.GroupRoleId == childRoleId &&
                        !newChildIds.Contains( m.PersonId ) ) )
                {
                    EnsurePersonInOtherFamily( familyGroupType.Id, primaryFamily.Id, removedChild.Id, removedChild.Person.LastName, childRoleId, na );
                    groupMemberService.Delete( removedChild );
                }
                _rockContext.SaveChanges();

                // Find all the existing relationships that were removed and delete them
                var roleIds = _relationshipTypes.Select( r => r.Key ).ToList();
                foreach ( var groupMember in new PersonService( _rockContext )
                    .GetRelatedPeople( adultIds, roleIds ) )
                {
                    if ( !newRelationships.ContainsKey(groupMember.PersonId) || !newRelationships[groupMember.PersonId].Contains( groupMember.GroupRoleId ) )
                    {
                        foreach ( var adultId in adultIds )
                        {
                            groupMemberService.DeleteKnownRelationship( adultId, groupMember.PersonId, groupMember.GroupRoleId );
                            if ( canCheckinRelationships.Contains( groupMember.GroupRoleId ) )
                            {
                                groupMemberService.DeleteKnownRelationship( adultId, groupMember.PersonId, canCheckInRole.Id );
                            }

                            if (temporaryCanCheckinRelationships.Contains( groupMember.GroupRoleId ))
                            {
                                groupMemberService.DeleteKnownRelationship( adultId, groupMember.PersonId, temporaryCanCheckInRole.Id );
                            }
                        }
                    }
                }
            }

            List<Guid> workflows = GetAttributeValue( "WorkflowTypes" ).SplitDelimitedValues().AsGuidList();
            string redirectUrl = GetAttributeValue( "RedirectURL" );
            if ( workflows.Any() || redirectUrl.IsNotNullOrWhiteSpace() )
            {
                var family = groupService.Get( primaryFamily.Id );

                var childIds = new List<int>( newChildIds );
                childIds.AddRange( newRelationships.Select( r => r.Key ).ToList() );

                // Create parameters
                var parameters = new Dictionary<string, string>();
                parameters.Add( "ParentIds", adultIds.AsDelimited( "," ) );
                parameters.Add( "ChildIds", childIds.AsDelimited( "," ) );

                if ( pnlPlannedDate.Visible )
                {
                    DateTime? visitDate = dpPlannedDate.SelectedDate;
                    if ( visitDate.HasValue )
                    {
                        parameters.Add( "PlannedVisitDate", visitDate.Value.ToString( "o" ) );
                    }
                }

                // Look for any workflows
                if ( workflows.Any() )
                {
                    // Launch all the workflows
                    foreach ( var wfGuid in workflows )
                    {
                        family.LaunchWorkflow( wfGuid, family.Name, parameters, null );
                    }
                }

                if ( redirectUrl.IsNotNullOrWhiteSpace() )
                {
                    var relatedPersonIds = newRelationships.Select( r => r.Key ).ToList();
                    var relatedChildren = personService.Queryable().Where( p => relatedPersonIds.Contains( p.Id ) ).ToList();

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( "Family", family );
                    mergeFields.Add( "RelatedChildren", relatedChildren );
                    foreach( var keyval in parameters )
                    {
                        mergeFields.Add( keyval.Key, keyval.Value );
                    }

                    var url = ResolveUrl( redirectUrl.ResolveMergeFields( mergeFields ) );

                    Response.Redirect( url, false );
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            string redirectUrl = GetAttributeValue( "RedirectURL" );
            if (redirectUrl.IsNotNullOrWhiteSpace())
            {
                var url = ResolveUrl( redirectUrl );

                Response.Redirect( url, false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        #endregion

        #region Methods

        private void LoadAcknoledgement()
        {
            var ackMessage = GetAttributeValue(ACKNOWLEDGEMENT_MESSAGE_KEY);

            if (ackMessage.IsNullOrWhiteSpace())
            {
                pnlAcknowledgement.Visible = false;
                rcbIAgree.Required = false;
                return;
            }

            pnlAcknowledgement.Visible = true;
            rcbIAgree.Visible = true;
            rcbIAgree.Required = true;
            rcbIAgree.RequiredErrorMessage = "Please accept acknlowedgement.";

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage);
            lAcknowledgement.Text = ackMessage.ResolveMergeFields(mergeFields, GetAttributeValue(ACKNOWLEDGEMENT_LAVA_COMMANDS_KEY));
            rcbIAgree.Label = GetAttributeValue(ACKNOWLEDGEMENT_CHECKBOX_LABEL_KEY);
        }

        /// <summary>
        /// Sets the initial visibility/required properties of controls based on block attribute values
        /// </summary>
        private void SetControls()
        {
            pnlVisit.Visible = true;

            // Campus 
            if ( GetAttributeValue( "ShowCampus" ).AsBoolean() )
            {
                pnlCampus.Visible = true;
                cpCampus.Required = GetAttributeValue("RequireCampus").AsBoolean();
            }
            else
            {
                pnlCampus.Visible = false;
            }
            cpCampus.Campuses = CampusCache.All(false);

            // Planned Visit Date
            dpPlannedDate.Required = SetControl( "PlannedVisitDate", pnlPlannedDate, null );

            // Visit Info
            pnlVisit.Visible = pnlCampus.Visible || pnlPlannedDate.Visible;

            // Adult Suffix
            bool isRequired = SetControl( ADULT_SUFFIX_KEY, pnlSuffix1, pnlSuffix2 );
            dvpSuffix1.Required = isRequired;
            hfSuffixRequired.Value = isRequired.ToStringSafe();
            var suffixDt = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() );
            dvpSuffix1.DefinedTypeId = suffixDt.Id;
            dvpSuffix2.DefinedTypeId = suffixDt.Id;

            // Adult Gender
            isRequired = SetControl( ADULT_GENDER_KEY, pnlGender1, pnlGender2 );
            ddlGender1.Required = isRequired;
            hfGenderRequired.Value = isRequired.ToStringSafe();
            ddlGender1.BindToEnum<Gender>( true, new Gender[] { Gender.Unknown } );
            ddlGender2.BindToEnum<Gender>( true, new Gender[] { Gender.Unknown } );

            // Adult Birthdate
            isRequired = SetControl( ADULT_BIRTHDATE_KEY, pnlBirthDate1, pnlBirthDate2 );
            dpBirthDate1.Required = isRequired;
            hfBirthDateRequired.Value = isRequired.ToStringSafe();

            // Adult Marital Status
            isRequired = SetControl( ADULT_MARTIAL_STATUS_KEY, pnlMaritalStatus1, pnlMaritalStatus2 );
            dvpMaritalStatus1.Required = isRequired;
            hfMaritalStatusRequired.Value = isRequired.ToStringSafe();
            var maritalStatusDt = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() );
            dvpMaritalStatus1.DefinedTypeId = maritalStatusDt.Id;
            dvpMaritalStatus2.DefinedTypeId = maritalStatusDt.Id;

            // Adult Email
            isRequired = SetControl( ADULT_EMAIL_KEY, pnlEmail1, pnlEmail2 );
            tbEmail1.Required = isRequired;
            hfEmailRequired.Value = isRequired.ToStringSafe();

            // Adult Mobile Phone
            isRequired = SetControl( ADULT_MOBILE_KEY, pnlMobilePhone1, pnlMobilePhone2 );
            pnMobilePhone1.Required = isRequired;

            hfMobilePhoneRequired.Value = isRequired.ToStringSafe();

            // Photo
            var hidePhoto = GetAttributeValue(ADULT_TAKE_PHOTO_KEY) == "Hide";
            if (hidePhoto)
            {
                pnlAPhoto1.Visible = false;
                pnlAPhoto2.Visible = false;
            }

            var hideUploadPhoto = GetAttributeValue(ADULT_UPLOAD_PHOTO_KEY) == "Hide";
            if (hideUploadPhoto)
            {
                pnlA1UploadPhoto.Visible = false;
                pnlA2UploadPhoto.Visible = false;
            }

            BuildAdultAttributes(false, null, null);

            // Check for Current Family
            if (GetAttributeValue(AUTOFILL_KEY).AsBoolean())
            {
                SetCurrentFamilyValues();
                CreateChildrenControls(true);
            }

            LoadAcknoledgement();
        }

        /// <summary>
        /// Sets the current family values.
        /// </summary>
        private void SetCurrentFamilyValues()
        {
            Group family = null;
            Rock.Model.Person adult1 = null;
            Rock.Model.Person adult2 = null;

            // If there is a logged in person, attempt to find their family and spouse.
            if ( GetAttributeValue("AllowUpdates").AsBoolean() && CurrentPerson != null )
            {
                Rock.Model.Person spouse = null;

                // Get all their families
                var families = CurrentPerson.GetFamilies( _rockContext );
                if ( families.Any() )
                {
                    // Get their spouse
                    spouse = CurrentPerson.GetSpouse( _rockContext );
                    if ( spouse != null )
                    {
                        // If spouse was found, find the first family that spouse belongs to also.
                        family = families.Where( f => f.Members.Any( m => m.PersonId == spouse.Id ) ).FirstOrDefault();
                        if ( family == null )
                        {
                            // If there was not family with spouse, something went wrong and assume there is no spouse.
                            spouse = null;
                        }
                    }

                    // If we didn't find a family yet (by checking spouses family), assume the first family.
                    if ( family == null )
                    {
                        family = families.FirstOrDefault();
                    }

                    // Assume Adult1 is the current person
                    adult1 = CurrentPerson;
                    if ( spouse != null )
                    {
                        // and Adult2 is the spouse
                        adult2 = spouse;

                        // However, if spouse is actually head of family, make them Adult1 and current person Adult2
                        var headOfFamilyId = family.Members
                            .OrderBy( m => m.GroupRole.Order )
                            .ThenBy( m => m.Person.Gender )
                            .Select( m => m.PersonId )
                            .FirstOrDefault();
                        if ( headOfFamilyId != 0 && headOfFamilyId == spouse.Id )
                        {
                            adult1 = spouse;
                            adult2 = CurrentPerson;
                        }
                    }
                }
            }

            // Set First Adult's Values
            hfAdultGuid1.Value = adult1 != null ? adult1.Id.ToString() : string.Empty;

            lFirstName1.Visible = adult1 != null;
            tbFirstName1.Visible = adult1 == null;
            lFirstName1.Text = adult1 != null ? adult1.NickName : String.Empty;
            tbFirstName1.Text = adult1 != null ? adult1.NickName : String.Empty;

            lLastName1.Visible = adult1 != null;
            tbLastName1.Visible = adult1 == null;
            lLastName1.Text = adult1 != null ? adult1.LastName : String.Empty;
            tbLastName1.Text = adult1 != null ? adult1.LastName : String.Empty;

            dvpSuffix1.SetValue( adult1 != null ? adult1.SuffixValueId : (int?)null );
            ddlGender1.SetValue( adult1 != null ? adult1.Gender.ConvertToInt() : 0 );
            dpBirthDate1.SelectedDate = ( adult1 != null ? adult1.BirthDate : (DateTime?)null );
            dvpMaritalStatus1.SetValue( adult1 != null ? adult1.MaritalStatusValueId : (int?)null );
            tbEmail1.Text = ( adult1 != null ? adult1.Email : string.Empty );
            SetPhoneNumber( adult1, pnMobilePhone1, cbPermissionToText1 );
            if ( adult1 != null)
            {
                uploadPhoto1.BinaryFileId = adult1.PhotoId;
            }

            // Set Second Adult's Values
            hfAdultGuid2.Value = adult2 != null ? adult2.Guid.ToString() : string.Empty;

            lFirstName2.Visible = adult2 != null;
            tbFirstName2.Visible = adult2 == null;
            lFirstName2.Text = adult2 != null ? adult2.NickName : String.Empty;
            tbFirstName2.Text = adult2 != null ? adult2.NickName : String.Empty;

            lLastName2.Visible = adult2 != null;
            tbLastName2.Visible = adult2 == null;
            lLastName2.Text = adult2 != null ? adult2.LastName : String.Empty;
            tbLastName2.Text = adult2 != null ? adult2.LastName : String.Empty;

            dvpSuffix2.SetValue( adult2 != null ? adult2.SuffixValueId : (int?)null );
            ddlGender2.SetValue( adult2 != null ? adult2.Gender.ConvertToInt() : 0 );
            dpBirthDate2.SelectedDate = ( adult2 != null ? adult2.BirthDate : (DateTime?)null );
            dvpMaritalStatus2.SetValue( adult2 != null ? adult2.MaritalStatusValueId : (int?)null );
            tbEmail2.Text = ( adult2 != null ? adult2.Email : string.Empty );
            SetPhoneNumber( adult2, pnMobilePhone2, cbPermissionToText2 );
            if (adult2 != null)
            {
                uploadPhoto2.BinaryFileId = adult2.PhotoId;
            }

            Children = new List<LPCPreRegistrationChild>();

            if ( family != null )
            {
                // Set the campus from the existing family
                cpCampus.SetValue(family.CampusId);
                
                // Set the address from the family
                var homeLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                if ( homeLocationType != null )
                {
                    var location = family.GroupLocations
                        .Where( l =>
                            l.GroupLocationTypeValueId.HasValue &&
                            l.GroupLocationTypeValueId.Value == homeLocationType.Id )
                        .Select( l => l.Location )
                        .FirstOrDefault();
                    acAddress.SetValues( location );
                }
                else
                {
                    acAddress.SetValues( null );
                }

                // Find all the children in the family
                var childRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
                foreach( var groupMember in family.Members
                    .Where( m => m.GroupRole.Guid == childRoleGuid )
                    .OrderByDescending( m => m.Person.Age ) )
                {
                    var child = new LPCPreRegistrationChild( groupMember.Person );
                    child.RelationshipType = 0;
                    Children.Add( child );
                }

                // Find all the related people.
                var adultIds = new List<int>();
                if ( adult1 != null )
                {
                    adultIds.Add( adult1.Id );
                }
                if ( adult2 != null )
                {
                    adultIds.Add( adult2.Id );
                }
                var roleIds = _relationshipTypes.Select( r => r.Key ).ToList();
                foreach ( var groupMember in new PersonService( _rockContext )
                    .GetRelatedPeople( adultIds, roleIds )
                    .OrderBy( m => m.GroupRole.Order )
                    .ThenByDescending( m => m.Person.Age ) )
                {
                    if ( !Children.Any( c => c.Id == groupMember.PersonId ) )
                    {
                        var child = new LPCPreRegistrationChild( groupMember.Person );
                        child.RelationshipType = groupMember.GroupRoleId;
                        Children.Add( child );
                    }
                }

                hfFamilyGuid.Value = family.Guid.ToString();
            }
            else
            {
                // Set campus to the default, no family currently exists
                Guid? campusGuid = GetAttributeValue("DefaultCampus").AsGuidOrNull();
                if (campusGuid.HasValue)
                {
                    var defaultCampus = CampusCache.Get(campusGuid.Value);
                    if (defaultCampus != null)
                    {
                        cpCampus.SetValue(defaultCampus.Id);
                    }
                }
                
                // Clear the address
                acAddress.SetValues( null );

                hfFamilyGuid.Value = string.Empty;
            }

            // If a query string for the campus is passed, use that instead of current or default
            int? queryStrCampus = Request.QueryString["campus"].AsIntegerOrNull();
            if (queryStrCampus.HasValue)
            {
                // Validate query string input
                var confirmedCampus = CampusCache.Get(queryStrCampus.Value);
                if (confirmedCampus != null)
                {
                    cpCampus.SetValue(confirmedCampus.Id);
                }
                
            }

            // Adult Attributes
            BuildAdultAttributes( true, adult1, adult2 );

            // Family Attributes
            BuildFamilyAttributes( true, family );
        }

        /// <summary>
        /// Helper method to set visibility of adult controls and return if it's required or not.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="adultControl1">The adult control1.</param>
        /// <param name="adultControl2">The adult control2.</param>
        /// <returns></returns>
        private bool SetControl( string attributeKey, WebControl adultControl1, WebControl adultControl2 )
        {
            string attributeValue = GetAttributeValue( attributeKey );
            if ( adultControl1 != null )
            {
                adultControl1.Visible = attributeValue != "Hide";
            }
            if ( adultControl2 != null )
            {
                adultControl2.Visible = attributeValue != "Hide";
            }
            return attributeValue == "Required";
        }

        /// <summary>
        /// Builds the adult attributes.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildAdultAttributes( bool setValues, Rock.Model.Person adult1, Rock.Model.Person adult2 )
        {
            phAttributes1.Controls.Clear();
            phAttributes2.Controls.Clear();

            var attributeList = GetCategoryAttributeList( ADULT_CATEGORIES_KEY );

            if ( adult1 != null )
            {
                adult1.LoadAttributes();
            }
            if ( adult2 != null )
            {
                adult2.LoadAttributes();
            }

            foreach( var attribute in attributeList )
            {
                string value1 = adult1 != null ? adult1.GetAttributeValue( attribute.Key ) : string.Empty;
                var div1 = new HtmlGenericControl( "Div" );
                phAttributes1.Controls.Add( div1 );
                div1.AddCssClass( "col-sm-3" );
                var ctrl1 = attribute.AddControl( div1.Controls, value1, this.BlockValidationGroup, setValues, true, attribute.IsRequired, null, null, null, string.Format( "attribute_field_{0}_1", attribute.Id ) );

                string value2 = adult2 != null ? adult2.GetAttributeValue( attribute.Key ) : string.Empty;
                var div2 = new HtmlGenericControl( "Div" );
                phAttributes2.Controls.Add( div2 );
                div2.AddCssClass( "col-sm-3" );
                var ctrl2 = attribute.AddControl( div2.Controls, value2, this.BlockValidationGroup, setValues, true, attribute.IsRequired, null, null, null, string.Format( "attribute_field_{0}_2", attribute.Id ) );
            }
        }

        /// <summary>
        /// Builds the family attributes.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildFamilyAttributes( bool setValues, Group family )
        {
            phFamilyAttributes.Controls.Clear();

            var attributeList = GetAttributeList( "FamilyAttributes" );

            if ( family != null )
            {
                family.LoadAttributes();
            }

            foreach ( var attribute in attributeList )
            {
                string value = family != null ? family.GetAttributeValue( attribute.Key ) : string.Empty;
                attribute.AddControl( phFamilyAttributes.Controls, value, this.BlockValidationGroup, setValues, true, attribute.IsRequired );
            }
        }

        /// <summary>
        /// Helper method to set a phone number control's value.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="pnb">The PNB.</param>
        private void SetPhoneNumber(Rock.Model.Person person, PhoneNumberBox pnb, RockCheckBox cbto )
        {
            if ( person != null )
            {
                var pn = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                if ( pn != null )
                {
                    pnb.CountryCode = pn.CountryCode;
                    pnb.Number = pn.ToString();
                    cbto.Checked = pn.IsMessagingEnabled;
                }
                else
                {
                    pnb.CountryCode = PhoneNumber.DefaultCountryCode();
                    pnb.Number = string.Empty;
                    cbto.Checked = true;
                }
            }
            else
            {
                pnb.CountryCode = PhoneNumber.DefaultCountryCode();
                pnb.Number = string.Empty;
                cbto.Checked = true;
            }
        }

        /// <summary>
        /// Adds a new child.
        /// </summary>
        private void AddChild()
        {
            var person = new Rock.Model.Person();
            person.Guid = Guid.NewGuid();
            person.Gender = Gender.Unknown;
            person.LastName = tbLastName1.Text;
            person.GradeOffset = null;

            var child = new LPCPreRegistrationChild( person );

            Children.Add( child );
        }

        /// <summary>
        /// Creates the children controls.
        /// </summary>
        private void CreateChildrenControls( bool setSelection )
        {
            prChildren.ClearRows();

            var showSuffix = GetAttributeValue( CHILD_SUFFIX_KEY ) != "Hide";
            var showGender = GetAttributeValue( CHILD_GENDER_KEY ) != "Hide";
            var requireGender = GetAttributeValue( CHILD_GENDER_KEY ) == "Required";
            var showBirthDate = GetAttributeValue( CHILD_BIRTHDATE_KEY ) != "Hide";
            var requireBirthDate = GetAttributeValue( CHILD_BIRTHDATE_KEY ) == "Required";
            var showEmail = GetAttributeValue( CHILD_EMAIL_KEY ) != "Hide";
            var requireEmail = GetAttributeValue( CHILD_EMAIL_KEY ) == "Required";
            var showGrade = GetAttributeValue( CHILD_GRADE_KEY ) != "Hide";
            var requireGrade = GetAttributeValue( CHILD_GRADE_KEY ) == "Required";
            var showMobilePhone = GetAttributeValue( CHILD_MOBILE_KEY ) != "Hide";
            var requireMobilePhone = GetAttributeValue( CHILD_MOBILE_KEY ) == "Required";
            var showPhoto = GetAttributeValue( CHILD_TAKE_PHOTO_KEY ) != "Hide";
            var showUploadPhoto = GetAttributeValue(CHILD_UPLOAD_PHOTO_KEY) != "Hide";
            var showEmailAddress = GetAttributeValue(CHILD_EMAIL_KEY) != "Hide";
            var requireEmailAddress = GetAttributeValue(CHILD_EMAIL_KEY) == "Required";
            var showCommunicationPreference = GetAttributeValue(CHILD_COMMUNICATION_PREFERENCE) != "Hide";
            var attributeList = GetCategoryAttributeList( CHILD_CATEGORIES_KEY );

            int index = 3; // adults are 1 and 2
            foreach ( var child in Children )
            {
                if ( child != null )
                {
                    var childRow = new LPCPreRegistrationChildRow( index++, Path );
                    
                    childRow.ValidationGroup = this.BlockValidationGroup;

                    prChildren.Controls.Add( childRow );

                    childRow.Click += ChildRow_Click;
                    childRow.DeleteClick += ChildRow_DeleteClick;
                    string childGuidString = child.Guid.ToString().Replace( "-", "_" );
                    childRow.ID = string.Format( "row_{0}", childGuidString );
                    childRow.PersonId = child.Id;
                    childRow.PersonGuid = child.Guid;

                    childRow.Caption = GetAttributeValue( PROGENY_TITLE_KEY );

                    childRow.ShowSuffix = showSuffix;
                    childRow.ShowGender = showGender;
                    childRow.RequireGender = requireGender;
                    childRow.ShowBirthDate = showBirthDate;
                    childRow.RequireBirthDate = requireBirthDate;
                    childRow.ShowEmail = showEmail;
                    childRow.RequireEmail = requireEmail;
                    childRow.ShowGrade = showGrade;
                    childRow.RequireGrade = requireGrade;
                    childRow.ShowMobilePhone = showMobilePhone;
                    childRow.RequireMobilePhone = requireMobilePhone;
                    childRow.ShowEmailAddress = showEmailAddress;
                    childRow.RequireEmailAddress = requireEmailAddress;
                    childRow.RelationshipTypeList = _relationshipTypes;
                    childRow.AttributeList = attributeList;
                    childRow.ShowImage = showPhoto;
                    childRow.ShowUploadImage = showUploadPhoto;
                    childRow.ShowCommunicationPreference = showCommunicationPreference;

                    childRow.ShowRace = false;
                    childRow.RequireRace = false;
                    childRow.ShowEthnicity = false;
                    childRow.RequireEthnicity = false;
                    childRow.ShowProfilePhoto = false;

                    childRow.ValidationGroup = BlockValidationGroup;

                    if ( setSelection )
                    {
                        childRow.NickName = child.NickName;
                        childRow.LastName = child.LastName;
                        childRow.SuffixValueId = child.SuffixValueId;
                        childRow.Gender = child.Gender;
                        childRow.BirthDate = child.BirthDate;
                        childRow.GradeOffset = child.GradeOffset;
                        childRow.RelationshipType = child.RelationshipType;
                        childRow.MobilePhone = child.MobilePhoneNumber;
                        childRow.MobilePhoneCountryCode = child.MobileCountryCode;
                        childRow.EmailAddress = child.EmailAddress;

                        childRow.Allergy = child.Allergy;
                        childRow.HasAllergy = child.HasAllergy;

                        childRow.SetAttributeValues( child );

                        childRow.hfImage.Value = child.TakenImage;
                        childRow.ieUploadedImage.BinaryFileId = child.UploadedImage;
                    }
                }
            }
        }

        private void SaveAdult( ref Group primaryFamily, List<int> adultIds, int adultNumber,
            HiddenField hfAdultGuid,
            RockTextBox tbFirstName,
            RockTextBox tbLastName,
            DefinedValuePicker dvpSuffix,
            RockDropDownList ddlGender,
            DatePicker dpBirthDate,
            DefinedValuePicker dvpMaritalStatus,
            EmailBox tbEmail,
            PhoneNumberBox pnMobilePhone,
            RockCheckBox cbOkayToText,
            DynamicPlaceholder phAttributes,
            HiddenField hfAdultImage,
            ImageEditor imgPhoto)        {
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );

            var recordTypePersonId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            var recordStatusValue = DefinedValueCache.Get( GetAttributeValue( "RecordStatus" ).AsGuid() ) ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            var connectionStatusValue = DefinedValueCache.Get( GetAttributeValue( "ConnectionStatus" ).AsGuid() ) ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() );

            var showSuffix = GetAttributeValue( ADULT_SUFFIX_KEY ) != "Hide";
            var showGender = GetAttributeValue( ADULT_GENDER_KEY ) != "Hide";
            var showBirthDate = GetAttributeValue( ADULT_BIRTHDATE_KEY ) != "Hide";
            var showMaritalStatus = GetAttributeValue( "AdultMaritalStatus" ) != "Hide";
            var showEmail = GetAttributeValue( ADULT_EMAIL_KEY ) != "Hide";
            var showMobilePhone = GetAttributeValue( ADULT_MOBILE_KEY ) != "Hide";
            var showPhoto = GetAttributeValue( ADULT_TAKE_PHOTO_KEY ) != "Hide";
            var showUploadPhoto = GetAttributeValue(ADULT_UPLOAD_PHOTO_KEY) != "Hide";
            bool autoMatch = GetAttributeValue( "AutoMatch" ).AsBoolean();

            var personService = new PersonService( _rockContext );

            // Get the adult if we're editing an existing person
            Rock.Model.Person adult = null;
            Guid? adultGuid = hfAdultGuid.Value.AsGuidOrNull();
            if ( adultGuid.HasValue )
            {
                adult = personService.Get( adultGuid.Value );
            }

            // Check to see if this is an existing person, or a name was entered for this adult
            if ( adult != null || ( tbFirstName.Text.IsNotNullOrWhiteSpace() && tbLastName.Text.IsNotNullOrWhiteSpace() ) )
            {
                // Flag indicating if empty values should be saved to person record (Should not do this if a matched record was found)
                bool saveEmptyValues = true;

                // If not editing an existing person, attempt to match them to existing (if configured to do so)
                if ( adult == null && showEmail && autoMatch )
                {
                    var gender = ddlGender.SelectedValueAsEnumOrNull<Gender>();
                    int? suffixValueId = dvpSuffix.SelectedValueAsInt();
                    var birthDate = dpBirthDate.SelectedDate;

                    var personQuery = new PersonService.PersonMatchQuery( tbFirstName.Text.Trim(), tbLastName.Text.Trim(), tbEmail.Text.Trim(), pnMobilePhone.Text.Trim(), gender, birthDate, suffixValueId );

                    
                    adult = personService.FindPerson( personQuery, true );
                    if ( adult != null )
                    {
                        saveEmptyValues = false;
                        if ( primaryFamily == null )
                        {
                            primaryFamily = adult.GetFamily( _rockContext );
                        }
                    }
                }

                // If this is a new person, add them.
                if ( adult == null )
                {
                    adult = new Rock.Model.Person();
                    personService.Add( adult );

                    adult.FirstName = tbFirstName.Text.FixCase();
                    adult.LastName = tbLastName.Text.FixCase();
                    adult.RecordTypeValueId = recordTypePersonId;
                    adult.RecordStatusValueId = recordStatusValue != null ? recordStatusValue.Id : (int?)null;
                    adult.ConnectionStatusValueId = connectionStatusValue != null ? connectionStatusValue.Id : (int?)null;
                }

                // Set the properties from UI
                if ( showSuffix )
                {
                    int? suffix = dvpSuffix.SelectedValueAsInt();
                    if ( suffix.HasValue || saveEmptyValues )
                    {
                        adult.SuffixValueId = suffix;
                    }
                }

                if ( showGender )
                {
                    var gender = ddlGender.SelectedValueAsEnumOrNull<Gender>();
                    if ( gender.HasValue || saveEmptyValues )
                    {
                        adult.Gender = gender ?? Gender.Unknown;
                    }
                }

                if ( showBirthDate )
                {
                    var birthDate = dpBirthDate.SelectedDate;
                    if ( birthDate.HasValue || saveEmptyValues )
                    {
                        adult.SetBirthDate( birthDate );
                    }
                }

                if ( showMaritalStatus )
                {
                    int? maritalStatus = dvpMaritalStatus.SelectedValueAsInt();
                    if ( maritalStatus.HasValue || saveEmptyValues )
                    {
                        adult.MaritalStatusValueId = maritalStatus;
                    }
                }

                if ( showEmail )
                {
                    if ( tbEmail.Text.IsNotNullOrWhiteSpace() || saveEmptyValues )
                    {
                        adult.Email = tbEmail.Text;
                    }
                }

                // Save the person
                _rockContext.SaveChanges();

                // Keep record of current photo for cleanup
                int? orphanedPhotoId = null;

                // Save photo that is taken in the browser
                if (hfAdultImage.Value.Length > 23 && GetAttributeValue(ADULT_TAKE_PHOTO_KEY) != "Hide")
                {
                    // Always create a new BinaryFile record of IsTemporary when a file is uploaded
                    var binaryFileService = new BinaryFileService( _rockContext );
                    Guid fileTypeGuid = Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid();
                    BinaryFileType binaryFileType = new BinaryFileTypeService( _rockContext ).Get( fileTypeGuid );

                    var binaryFile = new BinaryFile();

                    binaryFileService.Add( binaryFile );
                    var uploadedFile = hfAdultImage.Value.Substring( 23 );

                    byte[] imageBytes = Convert.FromBase64String( uploadedFile );
                    MemoryStream ms = new MemoryStream( imageBytes, 0, imageBytes.Length );
                    binaryFile.ContentStream = ms;

                    // Assume file is temporary unless specified otherwise so that files that don't end up getting used will get cleaned up
                    binaryFile.IsTemporary = GetAttributeValue( "IsTemporary" ).AsBooleanOrNull() ?? true;
                    binaryFile.BinaryFileTypeId = binaryFileType.Id;
                    binaryFile.MimeType = "image/jpeg";
                    binaryFile.FileName = "new picture";

                    _rockContext.SaveChanges();

                    orphanedPhotoId = adult.PhotoId;
                    adult.PhotoId = binaryFile.Id;
                }

                // Save photo that is uploaded
                if (imgPhoto.BinaryFileId != null && adult.PhotoId != imgPhoto.BinaryFileId && GetAttributeValue(ADULT_UPLOAD_PHOTO_KEY) != "Hide")
                {
                    orphanedPhotoId = adult.PhotoId;
                    adult.PhotoId = imgPhoto.BinaryFileId;
                }

                // Save the mobile phone number
                if ( showMobilePhone )
                {
                    bool isTextOkay = cbOkayToText.Checked;
                    SavePhoneNumber( adult.Id, pnMobilePhone, isTextOkay );
                }

                // Save any attribute values
                adult.LoadAttributes( _rockContext );
                GetAdultAttributeValues( phAttributes, adult, adultNumber, saveEmptyValues );
                adult.SaveAttributeValues( _rockContext );


                adultIds.Add( adult.Id );


                // Cleanup old photo if replaced (code taken from PublicProfileEdit)
                if (orphanedPhotoId.HasValue)
                {
                    BinaryFileService binaryFileService = new BinaryFileService(_rockContext);
                    var binaryFile = binaryFileService.Get(orphanedPhotoId.Value);
                    if (binaryFile != null)
                    {
                        // marked the old images as IsTemporary so they will get cleaned up later
                        binaryFile.IsTemporary = true;
                        _rockContext.SaveChanges();
                    }
                }

                // If they used the ImageEditor, and cropped it, the un-cropped file is still in BinaryFile. So clean it up
                if (imgPhoto.CropBinaryFileId.HasValue)
                {
                    if (imgPhoto.CropBinaryFileId != adult.PhotoId)
                    {
                        BinaryFileService binaryFileService = new BinaryFileService(_rockContext);
                        var binaryFile = binaryFileService.Get(imgPhoto.CropBinaryFileId.Value);
                        if (binaryFile != null && binaryFile.IsTemporary)
                        {
                            string errorMessage;
                            if (binaryFileService.CanDelete(binaryFile, out errorMessage))
                            {
                                binaryFileService.Delete(binaryFile);
                                _rockContext.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the adult attribute values.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="person">The person.</param>
        /// <param name="adultNumber">The adult number.</param>
        private void GetAdultAttributeValues( Control parentControl, Rock.Model.Person person, int adultNumber, bool setEmptyValue )
        {
            if ( person.Attributes != null )
            {
                foreach ( var attribute in person.Attributes )
                {
                    Control control = parentControl.FindControl( string.Format( "attribute_field_{0}_{1}", attribute.Value.Id, adultNumber ) );
                    if ( control != null )
                    {
                        var value = new AttributeValueCache();
                        value.Value = attribute.Value.FieldType.Field.GetEditValue( control, attribute.Value.QualifierValues );
                        if ( setEmptyValue || value.Value.IsNotNullOrWhiteSpace() )
                        {
                            person.AttributeValues[attribute.Key] = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the children data.
        /// </summary>
        private void GetChildrenData()
        {
            Children = new List<LPCPreRegistrationChild>();

            foreach( var childRow in prChildren.ChildRows )
            {
                var lpcChildRow = childRow as LPCPreRegistrationChildRow;
                if (lpcChildRow != null)
                {
                    var person = new Rock.Model.Person();
                    person.Id = lpcChildRow.PersonId;
                    person.Guid = lpcChildRow.PersonGuid ?? Guid.NewGuid();
                    person.NickName = lpcChildRow.NickName;
                    person.LastName = lpcChildRow.LastName;
                    person.SuffixValueId = lpcChildRow.SuffixValueId;
                    person.Gender = lpcChildRow.Gender;
                    person.SetBirthDate( lpcChildRow.BirthDate );
                    person.Email = lpcChildRow.Email;
                    person.GradeOffset = lpcChildRow.GradeOffset;

                    person.LoadAttributes();

                    var child = new LPCPreRegistrationChild( person );

                    child.MobilePhoneNumber = lpcChildRow.MobilePhone;
                    child.MobileCountryCode = lpcChildRow.MobilePhoneCountryCode;
                    child.EmailAddress = childRow.EmailAddress;

                    child.RelationshipType = lpcChildRow.RelationshipType;

                    var attributeKeys = GetCategoryAttributeList( CHILD_CATEGORIES_KEY ).Select( a => a.Key ).ToList();
                    child.AttributeValues = person.AttributeValues
                        .Where( a => attributeKeys.Contains( a.Key ) )
                        .ToDictionary( v => v.Key, v => v.Value.Value );

                    lpcChildRow.GetAttributeValues( child );

                    child.TakenImage = lpcChildRow.hfImage.Value;
                    child.UploadedImage = lpcChildRow.ieUploadedImage.BinaryFileId;
                    child.PreCropBinaryFileId = lpcChildRow.ieUploadedImage.CropBinaryFileId;

                    child.HasAllergy = lpcChildRow.HasAllergy;
                    child.Allergy = lpcChildRow.Allergy;

                    Children.Add( child );
                }
            }
        }

        /// <summary>
        /// Gets the attributes based on a block setting of attribute categories (adult/child attributes).
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        private List<AttributeCache> GetCategoryAttributeList( string attributeKey )
        {
            var AttributeList = new List<AttributeCache>();
            foreach ( Guid categoryGuid in GetAttributeValue( attributeKey ).SplitDelimitedValues( false ).AsGuidList() )
            {
                var category = CategoryCache.Get( categoryGuid );
                if ( category != null )
                {
                    foreach ( var attribute in new AttributeService( _rockContext ).GetByCategoryId( category.Id, false ) )
                    {
                        if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                        {
                            AttributeList.Add( AttributeCache.Get( attribute ) );
                        }
                    }
                }
            }

            return AttributeList;
        }

        /// <summary>
        /// Gets the attributes based on a block setting of attributes (family attributes).
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        private List<AttributeCache> GetAttributeList( string attributeKey )
        {
            var AttributeList = new List<AttributeCache>();
            foreach ( Guid attributeGuid in GetAttributeValue( attributeKey ).SplitDelimitedValues( false ).AsGuidList() )
            {
                var attribute = AttributeCache.Get( attributeGuid );
                if ( attribute != null && attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    AttributeList.Add( attribute );
                }
            }

            return AttributeList;
        }

        /// <summary>
        /// Validates the information being updated
        /// </summary>
        /// <returns></returns>
        private bool ValidateInfo()
        {
            var errorMessages = new List<string>();

            if ( tbFirstName1.Text.IsNullOrWhiteSpace() && tbFirstName2.Text.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( Translation.GetValueOrDefault( "ErrorAdultName", "El nombre de al menos un adulto es necesario.") );
            }

            if (
                ( tbFirstName1.Text.IsNotNullOrWhiteSpace() && tbLastName1.Text.IsNullOrWhiteSpace() ) ||
                ( tbLastName1.Text.IsNullOrWhiteSpace() && tbLastName1.Text.IsNotNullOrWhiteSpace() ) ||
                ( tbFirstName2.Text.IsNotNullOrWhiteSpace() && tbLastName2.Text.IsNullOrWhiteSpace() ) ||
                ( tbLastName2.Text.IsNullOrWhiteSpace() && tbLastName2.Text.IsNotNullOrWhiteSpace() ) 
            )
            {
                errorMessages.Add( Translation.GetValueOrDefault( "ErrorAdultFullName", "Nombre y Apellido es necesario por cada persona") );
            }

            ValidateRequiredField( ADULT_GENDER_KEY, Translation.GetValueOrDefault( "ErrorAdultGender", "Género es necesario para cada adulto."), ddlGender1.SelectedValueAsEnumOrNull<Gender>() != null, ddlGender2.SelectedValueAsEnumOrNull<Gender>() != null, errorMessages );
            ValidateRequiredField( ADULT_BIRTHDATE_KEY, Translation.GetValueOrDefault( "ErrorAdultBirthdate", "Fecha de Nacimiento es necesario para cada adulto."), dpBirthDate1.SelectedDate != null, dpBirthDate2.SelectedDate != null, errorMessages );
            ValidateRequiredField( ADULT_EMAIL_KEY, Translation.GetValueOrDefault( "ErrorAdultEmail", "Email es necesario para cada adulto."), tbEmail1.Text.IsNotNullOrWhiteSpace(), tbEmail2.Text.IsNotNullOrWhiteSpace(), errorMessages );
            ValidateRequiredField( ADULT_MOBILE_KEY, Translation.GetValueOrDefault( "ErrorAdultPhone", "Télefono necesario para cada adulto."), PhoneNumber.CleanNumber( pnMobilePhone1.Number ).IsNotNullOrWhiteSpace(), PhoneNumber.CleanNumber( pnMobilePhone2.Number ).IsNotNullOrWhiteSpace(), errorMessages );

            if (pnlAcknowledgement.Visible && !rcbIAgree.Checked)
            {
                errorMessages.Add( Translation.GetValueOrDefault( "ErrorAdultAcknowledgement", "Reconocimiento es necesario.") );
            }

            foreach (var child in Children)
            {
                // If the child has an allergy and the allergy field is empty
                if (child.HasAllergy == true && child.Allergy.IsNullOrWhiteSpace())
                {
                    errorMessages.Add( Translation.GetValueOrDefault( "ErrorAllergy", "Cada niño debe dejar saber si es alérgico si o no") );
                }
            }

            if ( errorMessages.Any() )
            {
                nbError.Title = Translation.GetValueOrDefault( "ErrorsHeading", "Por favor, corrija el siguiente:");
                nbError.Text = string.Format("<ul style=\"list-style-type:disc;\" ><li>{0}</li></ul>", errorMessages.AsDelimited( "</li><li>" ) );
                nbError.Visible = true;

                ScrollToTop();

                return false;
            }

            return true;
        }

        private void ScrollToTop(int intPosY = 0)
        {
            string strScript = @"var manager = Sys.WebForms.PageRequestManager.getInstance(); 
            manager.add_beginRequest(beginRequest); 
            function beginRequest() 
            { 
                manager._scrollPosition = null; 
            }
            window.scroll(0, " + intPosY.ToString() + ");";

            ScriptManager.RegisterStartupScript(this, this.GetType(), string.Empty, strScript, true);

            return;
        }

        /// <summary>
        /// Validates the required field.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="adult1HasValue">if set to <c>true</c> [adult1 has value].</param>
        /// <param name="adult2HasValue">if set to <c>true</c> [adult2 has value].</param>
        /// <param name="errorMessages">The error messages.</param>
        private void ValidateRequiredField( string attributeKey, string errorMessage, bool adult1HasValue, bool adult2HasValue, List<String> errorMessages )
        {
            if ( GetAttributeValue( attributeKey ) == "Required" )
            {
                if (
                    ( tbFirstName1.Text.IsNotNullOrWhiteSpace() && !adult1HasValue ) ||
                    ( tbFirstName2.Text.IsNotNullOrWhiteSpace() && !adult2HasValue )
                )
                {
                    errorMessages.Add( errorMessage );
                }
            }
        }

        /// <summary>
        /// Creates a new family group.
        /// </summary>
        /// <param name="familyGroupTypeId">The family group type identifier.</param>
        /// <returns></returns>
        private Group CreateNewFamily( int familyGroupTypeId, string lastName )
        {
            // If we don't have an existing family, create a new family
            var family = new Group();
            family.Name = lastName.FixCase() + " Family";
            family.GroupTypeId = familyGroupTypeId;
            return family;
        }

        /// <summary>
        /// Ensures the person in other family.
        /// </summary>
        /// <param name="familyGroupTypeId">The family group type identifier.</param>
        /// <param name="primaryfamilyId">The primaryfamily identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="childRoleId">The child role identifier.</param>
        /// <param name="newFamilyIds">The new family ids.</param>
        private void EnsurePersonInOtherFamily( int familyGroupTypeId, int primaryfamilyId, int personId, string lastName, int childRoleId, Dictionary<string, int> newFamilyIds )
        {
            var groupMemberService = new GroupMemberService( _rockContext );

            // Get any other family memberships.
            if ( groupMemberService.Queryable()
                .Any( m =>
                    m.Group.GroupTypeId == familyGroupTypeId &&
                    m.PersonId == personId &&
                    m.GroupId != primaryfamilyId ) )
            {
                // They have other families, so just return
                return;
            }

            // Check to see if we've already created a family with someone who has same last name
            string key = lastName.ToLower();
            int? newFamilyId = newFamilyIds.ContainsKey( key ) ? newFamilyIds[key] : (int?)null;

            // If not, create a new family
            if ( !newFamilyId.HasValue )
            {
                var family = CreateNewFamily( familyGroupTypeId, lastName );
                new GroupService( _rockContext ).Add( family );
                _rockContext.SaveChanges();

                newFamilyId = family.Id;
                newFamilyIds.Add( key, family.Id );
            }

            // Add the person to the family
            var familyMember = new GroupMember();
            familyMember.GroupId = newFamilyId.Value;
            familyMember.PersonId = personId;
            familyMember.GroupRoleId = childRoleId;
            familyMember.GroupMemberStatus = GroupMemberStatus.Active;

            groupMemberService.Add( familyMember );
            _rockContext.SaveChanges();
        }

        /// <summary>
        /// Removes a person from a family.
        /// </summary>
        /// <param name="familyGroupTypeId">The family group type identifier.</param>
        /// <param name="familyId">The family identifier.</param>
        /// <param name="personId">The person identifier.</param>
        private void RemovePersonFromFamily( int familyGroupTypeId, int familyId, int personId )
        {
            var groupMemberService = new GroupMemberService( _rockContext );

            // Get all their current group memberships.
            var groupMembers = groupMemberService.Queryable()
                .Where( m =>
                    m.Group.GroupTypeId == familyGroupTypeId &&
                    m.PersonId == personId )
                .ToList();

            // Find their membership in current family, if not found, skip processing, as something is amiss.
            var currentFamilyMembership = groupMembers.FirstOrDefault( m => m.GroupId == familyId );
            if ( currentFamilyMembership != null )
            {
                // If the person does not currently belong to any other families, we'll have to create a new family for them, and move them to that new group
                if ( !groupMembers.Where( m => m.GroupId != familyId ).Any() )
                {
                    var newGroup = new Group();
                    newGroup.Name = currentFamilyMembership.Person.LastName + " Family";
                    newGroup.GroupTypeId = familyGroupTypeId;
                    newGroup.CampusId = currentFamilyMembership.Group.CampusId;
                    new GroupService( _rockContext ).Add( newGroup );
                    _rockContext.SaveChanges();

                    // If person's previous giving group was this family, set it to their new family id
                    if ( currentFamilyMembership.Person.GivingGroupId.HasValue && currentFamilyMembership.Person.GivingGroupId == currentFamilyMembership.GroupId )
                    {
                        currentFamilyMembership.Person.GivingGroupId = newGroup.Id;
                    }

                    currentFamilyMembership.Group = newGroup;
                    _rockContext.SaveChanges();
                }
                else
                {
                    // Otherwise, just remove them from the current family
                    groupMemberService.Delete( currentFamilyMembership );
                    _rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Saves the phone number.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="pnb">The PNB.</param>
        private void SavePhoneNumber( int personId, PhoneNumberBox pnb, bool isTextOkay = false )
        {
            SavePhoneNumber( personId, pnb.Number, pnb.CountryCode, isTextOkay );
        }

        /// <summary>
        /// Saves the phone number.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="number">The number.</param>
        /// <param name="countryCode">The country code.</param>
        private void SavePhoneNumber( int personId, string number, string countryCode, bool isTextOkay = false )
        {

            string phone = PhoneNumber.CleanNumber( number );

            var phoneNumberService = new PhoneNumberService( _rockContext );

            var phType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            if ( phType != null )
            {
                var phoneNumber = phoneNumberService.Queryable()
                    .Where( n =>
                        n.PersonId == personId &&
                        n.NumberTypeValueId.HasValue &&
                        n.NumberTypeValueId.Value == phType.Id )
                    .FirstOrDefault();

                if ( phone.IsNotNullOrWhiteSpace() )
                {
                    if ( phoneNumber == null )
                    {
                        phoneNumber = new PhoneNumber();
                        phoneNumberService.Add( phoneNumber );
                        phoneNumber.IsMessagingEnabled = isTextOkay;
                        phoneNumber.PersonId = personId;
                        phoneNumber.NumberTypeValueId = phType.Id;
                    }

                    phoneNumber.CountryCode = PhoneNumber.CleanNumber( countryCode );
                    phoneNumber.Number = phone;
                }
                else
                {
                    if ( phoneNumber != null )
                    {
                        phoneNumberService.Delete( phoneNumber );
                    }
                }

                _rockContext.SaveChanges();
            }
        }

        #endregion
    }
}
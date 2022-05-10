using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using static Rock.Field.Types.BooleanFieldType;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Adds a new group for the configured GroupType, Parent Group, etc.
    /// </summary>
    [ActionCategory( "Groups" )]
    [Description( "Adds a new group for the configured GroupType, Parent Group, etc." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Add Group" )]

    [GroupTypeField( "Group Type",
        Description = "The group type to create the group with.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.GroupType )]
    [WorkflowAttribute( "Parent Group Attribute",
        Description = "The workflow attribute that contains the parent group.",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.ParentGroupAttribute,
       FieldTypeClassNames = new string[] { "Rock.Field.Types.GroupFieldType" } )]
    [WorkflowAttribute( "Campus Attribute",
        Description = "The workflow attribute that contains the campus to apply to the group.",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.CampusAttribute,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.CampusFieldType" } )]
    [WorkflowTextOrAttribute( "Group Name", "Attribute Value",
        Description = "The group name or attribute that contains the value for the name. {{ Lava }}.",
        IsRequired = true,
        Order = 3,
        Key = AttributeKey.GroupNameAttribute,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Group Description", "Attribute Value",
        Description = "The group description or attribute that contains the value for the description.",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.GroupDescriptionAttribute,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType" } )]
    [BooleanField( "Is Security Role",
        Description = "Indicates the group will be used as a security role.",
        ControlType = BooleanControlType.Checkbox,
        Order = 5,
        Key = AttributeKey.IsSecurityRole )]
    public class GroupAdd : ActionComponent
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string GroupType = "GroupType";
            public const string ParentGroupAttribute = "ParentGroupAttribute";
            public const string CampusAttribute = "CampusAttribute";
            public const string GroupNameAttribute = "GroupNameAttribute";
            public const string GroupDescriptionAttribute = "GroupDescriptionAttribute";
            public const string IsSecurityRole = "IsSecurityRole";
        }

        #endregion

        #region Error Messages

        private static class ErrorMessages
        {
            public const string NoGroupTypeProvided = "No group type was provided.";
            public const string NoGroupProvided = "No group was provided.";
        }

        #endregion Error Messages

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                var groupTypeResult = GetGroupType( rockContext, action );
                
                var groupResult = GetGroup( rockContext, action );

                return true;
            }
            catch ( GroupAddWorkflowActionAttributeResultException wfex )
            {
                errorMessages.Add( wfex.Message );
            }

            return false;
        }

        #region Action Methods

        /// <summary>
        /// Gets the type of the group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <returns>GroupType.</returns>
        private GroupType GetGroupType( RockContext rockContext, WorkflowAction action )
        {
            var groupTypeGuid = GetAttributeValue( action, AttributeKey.GroupType ).AsGuidOrNull();

            if ( !groupTypeGuid.HasValue )
            {
                ThrowErrorMessage( ErrorMessages.NoGroupTypeProvided );
            }

            var groupTypeService = new GroupTypeService( rockContext );

            var groupType = groupTypeService.Get( groupTypeGuid.Value );
            if ( groupType == null )
            {
                ThrowErrorMessage( ErrorMessages.NoGroupTypeProvided );
            }

            return groupType;
        }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <returns>Group.</returns>
        private Group GetGroup( RockContext rockContext, WorkflowAction action )
        {
            var parentGroupAttributeGuid = GetAttributeValue( action, AttributeKey.ParentGroupAttribute ).AsGuidOrNull();

            if ( !parentGroupAttributeGuid.HasValue )
            {
                ThrowErrorMessage( ErrorMessages.NoGroupProvided );
            }

            var attributeGroup = AttributeCache.Get( parentGroupAttributeGuid.Value, rockContext );
            if ( attributeGroup == null )
            {
                ThrowErrorMessage( ErrorMessages.NoGroupProvided );
            }

            var groupGuid = action.GetWorkflowAttributeValue( parentGroupAttributeGuid.Value ).AsGuidOrNull();

            if ( !groupGuid.HasValue )
            {
                ThrowErrorMessage( ErrorMessages.NoGroupProvided );
            }

            var group = new GroupService( rockContext ).Get( groupGuid.Value );

            return  group ;
        }

        /// <summary>
        /// Throws the error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="Rock.Workflow.Action.GroupAdd.GroupAddWorkflowActionAttributeResultException"></exception>
        private void ThrowErrorMessage( string errorMessage )
        {
            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                throw new GroupAddWorkflowActionAttributeResultException( errorMessage );
            }
        }

        #endregion Action Methods

        #region Action Classes

        private class GroupAddWorkflowActionAttributeResultException : Exception
        {
            internal GroupAddWorkflowActionAttributeResultException( string message ) : base( message )
            {

            }
        }

        #endregion Action Classes
    }
}

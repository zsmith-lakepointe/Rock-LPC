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
using System.Collections.Generic;
using Rock.Data;
using Rock.Model;
using System.Linq;
using Rock.Web.Cache;
using System;

namespace Rock.Tests.Integration
{
    public static partial class TestDataHelper
    {
        public static class Crm
        {
            public static Site GetInternalSite( RockContext rockContext = null )
            {
                rockContext = GetActiveRockContext( rockContext );
                var siteService = new SiteService( rockContext );

                var internalSite = siteService.Get( SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
                return internalSite;
            }

            public static List<Page> GetInternalSitePages( RockContext rockContext = null )
            {
                rockContext = GetActiveRockContext( rockContext );
                var pageService = new PageService( rockContext );

                var internalSite = GetInternalSite( rockContext );
                var pages = pageService.Queryable()
                    .Where( p => p.Layout != null && p.Layout.SiteId == internalSite.Id )
                    .ToList();

                return pages;
            }

            #region Group Actions

            public class AddGroupArgs
            {
                //public RockContext DataContext { get; set; }
                public bool ReplaceIfExists { get; set; }
                public string ForeignKey { get; set; }
                public string GroupTypeIdentifier { get; set; }
                public string ParentGroupIdentifier { get; set; }
                public string GroupName { get; set; }
                public string GroupGuid { get; set; }
                public List<GroupMember> GroupMembers { get; set; }
                public string CampusIdentifier { get; set; }

            }

            public class AddGroupActionResult
            {
                public Group Group;
                public int AffectedItemCount;
            }

            /// <summary>
            /// Add a new Group.
            /// </summary>
            /// <param name="args"></param>
            /// <returns></returns>
            public static AddGroupActionResult AddGroup( AddGroupArgs args )
            {
                var result = new AddGroupActionResult();
                Group group = null;

                // Use a new context because the Group will be immediately persisted.
                var rockContext = new RockContext();
                rockContext.WrapTransaction( () =>
                {
                    var groupService = new GroupService( rockContext );
                    var groupTypeService = new GroupTypeService( rockContext );
                    var groupType = groupTypeService.Queryable().GetByIdentifierOrThrow( args.GroupTypeIdentifier );

                    // Get optional parent Group.
                    Guid? parentGroupGuid = null;
                    if ( !string.IsNullOrWhiteSpace( args.ParentGroupIdentifier ) )
                    {
                        parentGroupGuid = groupService.Queryable()
                            .GetByIdentifierOrThrow( args.ParentGroupIdentifier )
                            .Guid;
                    }

                    // Get optional Campus.
                    int? campusId = null;
                    if ( !string.IsNullOrWhiteSpace( args.CampusIdentifier ) )
                    {
                        var campusService = new CampusService( rockContext );
                        campusId = campusService.Queryable()
                            .GetByIdentifier( args.CampusIdentifier )
                            .Id;
                    }

                    var groupGuid = args.GroupGuid.AsGuidOrNull();

                    if ( groupGuid != null )
                    {
                        var existingGroup = groupService.Queryable().FirstOrDefault( g => g.Guid == groupGuid );
                        if ( existingGroup != null )
                        {
                            if ( !args.ReplaceIfExists )
                            {
                                return;
                            }
                            DeleteGroup( rockContext, args.GroupGuid );
                            rockContext.SaveChanges();
                        }
                    }

                    group = GroupService.SaveNewGroup( rockContext,
                        groupType.Id,
                        parentGroupGuid,
                        args.GroupName,
                        args.GroupMembers ?? new List<GroupMember>(),
                        campusId,
                        savePersonAttributes: true );

                    group.Guid = args.GroupGuid.AsGuidOrNull() ?? Guid.NewGuid();
                    group.ForeignKey = args.ForeignKey;

                    rockContext.SaveChanges();
                } );

                result.Group = group;
                result.AffectedItemCount = ( group == null ? 0 : 1 );
                return result;
            }

            public static bool DeleteGroup( RockContext rockContext, string groupIdentifier )
            {
                rockContext.WrapTransaction( () =>
                {
                    var groupService = new GroupService( rockContext );
                    var group = groupService.Get( groupIdentifier );

                    groupService.Delete( group, removeFromAuthTables: true );

                    rockContext.SaveChanges();
                } );

                return true;
            }

            #endregion

            #region Group Member Actions

            public class AddGroupMemberArgs
            {
                public bool ReplaceIfExists { get; set; }
                public string ForeignKey { get; set; }
                public string GroupIdentifier { get; set; }
                public string PersonIdentifiers { get; set; }
                public string GroupRoleIdentifier { get; set; }
            }

            public static List<GroupMember> AddGroupMembers( RockContext rockContext, AddGroupMemberArgs args )
            {
                var groupMembers = new List<GroupMember>();

                rockContext.WrapTransaction( () =>
                {
                    var groupService = new GroupService( rockContext );
                    var groupRoleService = new GroupService( rockContext );

                    var group = groupService.Get( args.GroupIdentifier );
                    AssertRockEntityIsNotNull( group, args.GroupIdentifier );

                    var groupId = group.Id;
                    var groupGuid = group.Guid;
                    var groupTypeId = group.GroupTypeId;

                    var roleId = args.GroupRoleIdentifier.AsIntegerOrNull() ?? 0;
                    var roleGuid = args.GroupRoleIdentifier.AsGuidOrNull();

                    var groupTypeRoleService = new GroupTypeRoleService( rockContext );
                    var role = groupTypeRoleService.Queryable()
                        .FirstOrDefault( r => ( r.GroupTypeId == groupTypeId )
                            && ( r.Id == roleId || r.Guid == roleGuid || r.Name == args.GroupRoleIdentifier ) );
                    AssertRockEntityIsNotNull( role, args.GroupRoleIdentifier );

                    var personService = new PersonService( rockContext );
                    var groupMemberService = new GroupMemberService( rockContext );

                    var personIdentifierList = args.PersonIdentifiers.SplitDelimitedValues( "," );
                    foreach ( var personIdentifier in personIdentifierList )
                    {
                        var person = personService.Get( personIdentifier );
                        AssertRockEntityIsNotNull( person, personIdentifier );

                        var groupMember = new GroupMember
                        {
                            ForeignKey = args.ForeignKey,
                            GroupId = groupId,
                            GroupTypeId = groupTypeId,
                            PersonId = person.Id,
                            GroupRoleId = role.Id
                        };

                        groupMemberService.Add( groupMember );
                        groupMembers.Add( groupMember );
                    }
                    //rockContext.SaveChanges( disablePrePostProcessing:true );
                } );

                return groupMembers;
            }

            #endregion

            #region Group Requirements

            public class AddGroupRequirementArgs
            {
                public bool ReplaceIfExists { get; set; }
                public string ForeignKey { get; set; }
                public string GroupIdentifier { get; set; }
                //public string PersonIdentifier { get; set; }

                public string GroupRoleIdentifier { get; set; }
                public string GroupRequirementTypeIdentifier { get; set; }

                public bool MustMeetRequirementToAddMember { get; set; }

                public bool AllowLeadersToOverride { get; set; }

                public AppliesToAgeClassification? AppliesToAgeClassification { get; set; }
                public string AppliesToDataViewIdentifier { get; set; }

                public DateTime? DueDate { get; set; }
            }

            public static GroupRequirement AddGroupRequirement( RockContext rockContext, AddGroupRequirementArgs args )
            {
                GroupRequirement groupRequirement = null;

                //rockContext.WrapTransaction( () =>
                //{
                    groupRequirement = AddGroupRequirementInternal( rockContext, args );
                //} );

                return groupRequirement;
            }

            private static GroupRequirement AddGroupRequirementInternal( RockContext rockContext, AddGroupRequirementArgs args )
            {
                GroupRequirement groupRequirement = null;

                //rockContext.WrapTransaction( () =>
                //{
                    groupRequirement = new GroupRequirement();
                    groupRequirement.Guid = Guid.NewGuid();

                    // Requirement Type
                    var requirementTypeService = new GroupRequirementTypeService( rockContext );
                    var groupRequirementType = requirementTypeService.AsNoFilter() //.Queryable()
                        .GetByIdentifierOrThrow( args.GroupRequirementTypeIdentifier );
                    groupRequirement.GroupRequirementTypeId = groupRequirementType.Id;

                    try
                    {
                    // Group
                    DebugHelper.SQLLoggingStart();
                        var groupService = new GroupService( rockContext );
                        var group = groupService.AsNoFilter()
                            .GetByIdentifierOrThrow( args.GroupIdentifier );
                        groupRequirement.GroupId = group.Id;
                    
                }
                    catch (Exception ex)
                    {
                        int i = 0;
                    }
                DebugHelper.SQLLoggingStop();

                // Group Role
                if ( !string.IsNullOrWhiteSpace( args.GroupRoleIdentifier ) )
                    {
                        var groupTypeRoleService = new GroupTypeRoleService( rockContext );
                        var groupRole = groupTypeRoleService.Queryable().GetByIdentifierOrThrow( args.GroupRoleIdentifier );
                        groupRequirement.GroupRoleId = groupRole.Id;
                    }

                    // Applies To
                    if ( args.AppliesToAgeClassification != null )
                    {
                        groupRequirement.AppliesToAgeClassification = args.AppliesToAgeClassification.Value;
                    }

                    if ( !string.IsNullOrWhiteSpace( args.AppliesToDataViewIdentifier ) )
                    {
                        var dataService = new DataViewService( rockContext );
                        var dataView = dataService.Queryable().GetByIdentifier( args.AppliesToDataViewIdentifier );
                        groupRequirement.AppliesToDataViewId = dataView.Id;
                    }

                    // Additional Settings
                    groupRequirement.MustMeetRequirementToAddMember = args.MustMeetRequirementToAddMember;
                    groupRequirement.AllowLeadersToOverride = args.AllowLeadersToOverride;

                    if ( groupRequirementType.DueDateType == DueDateType.ConfiguredDate )
                    {
                        groupRequirement.DueDateStaticDate = args.DueDate;
                    }

                    //if ( groupRequirement.GroupRequirementType.DueDateType == DueDateType.GroupAttribute )
                    //{
                    //    // Set this due date attribute if it exists.
                    //    var groupDueDateAttributes = AttributeCache.AllForEntityType<Group>().Where( a => a.Id == ddlDueDateGroupAttribute.SelectedValue.AsIntegerOrNull() );
                    //    if ( groupDueDateAttributes.Any() )
                    //    {
                    //        groupRequirement.DueDateAttributeId = groupDueDateAttributes.First().Id;
                    //    }
                    //}

                    // Make sure we aren't adding a duplicate group requirement (same group requirement type and role)
                    //var duplicateGroupRequirement = this.GroupRequirementsState.Any( a =>
                    //    a.GroupRequirementTypeId == groupRequirement.GroupRequirementTypeId
                    //    && a.GroupRoleId == groupRequirement.GroupRoleId
                    //    && a.Guid != groupRequirement.Guid );

                    //if ( duplicateGroupRequirement )
                    //{
                    //    nbDuplicateGroupRequirement.Text = string.Format(
                    //        "This group already has a group requirement of {0} {1}",
                    //        groupRequirement.GroupRequirementType.Name,
                    //        groupRequirement.GroupRoleId.HasValue ? "for group role " + groupRequirement.GroupRole.Name : string.Empty );
                    //    nbDuplicateGroupRequirement.Visible = true;
                    //    this.GroupRequirementsState.Remove( groupRequirement );
                    //    return;
                    //}


                    var groupRequirementService = new GroupRequirementService( rockContext );
                    groupRequirementService.Add( groupRequirement );

                    //rockContext.SaveChanges();
                //} );

                return groupRequirement;
            }

            #endregion

            /*
            var groupRequirement = this.GroupRequirementsState.FirstOrDefault( a => a.Guid == groupRequirementGuid );
                        if ( groupRequirement == null )
                        {
                            groupRequirement = new GroupRequirement();
                            groupRequirement.Guid = Guid.NewGuid();
                            this.GroupRequirementsState.Add( groupRequirement );
                        }

                        groupRequirement.GroupRequirementTypeId = ddlGroupRequirementType.SelectedValue.AsInteger();
                        groupRequirement.GroupRequirementType = new GroupRequirementTypeService( rockContext ).Get( groupRequirement.GroupRequirementTypeId );
                        groupRequirement.GroupRoleId = grpGroupRequirementGroupRole.GroupRoleId;
                        groupRequirement.MustMeetRequirementToAddMember = cbMembersMustMeetRequirementOnAdd.Checked;
                        if ( groupRequirement.GroupRoleId.HasValue )
                        {
                            groupRequirement.GroupRole = new GroupTypeRoleService( rockContext ).Get( groupRequirement.GroupRoleId.Value );
                        }
                        else
                        {
                            groupRequirement.GroupRole = null;
                        }

                        groupRequirement.AppliesToAgeClassification = rblAppliesToAgeClassification.SelectedValue.ConvertToEnum<AppliesToAgeClassification>();
                        groupRequirement.AppliesToDataViewId = dvpAppliesToDataView.SelectedValueAsId();
                        groupRequirement.AllowLeadersToOverride = cbAllowLeadersToOverride.Checked;

                        if ( groupRequirement.GroupRequirementType.DueDateType == DueDateType.ConfiguredDate )
                        {
                            groupRequirement.DueDateStaticDate = dpDueDate.SelectedDate;
                        }

                        if ( groupRequirement.GroupRequirementType.DueDateType == DueDateType.GroupAttribute )
                        {
                            // Set this due date attribute if it exists.
                            var groupDueDateAttributes = AttributeCache.AllForEntityType<Group>().Where( a => a.Id == ddlDueDateGroupAttribute.SelectedValue.AsIntegerOrNull() );
                            if ( groupDueDateAttributes.Any() )
                            {
                                groupRequirement.DueDateAttributeId = groupDueDateAttributes.First().Id;
                            }
                        }

                        // Make sure we aren't adding a duplicate group requirement (same group requirement type and role)
                        var duplicateGroupRequirement = this.GroupRequirementsState.Any( a =>
                            a.GroupRequirementTypeId == groupRequirement.GroupRequirementTypeId
                            && a.GroupRoleId == groupRequirement.GroupRoleId
                            && a.Guid != groupRequirement.Guid );

                        if ( duplicateGroupRequirement )
                        {
                            nbDuplicateGroupRequirement.Text = string.Format(
                                "This group already has a group requirement of {0} {1}",
                                groupRequirement.GroupRequirementType.Name,
                                groupRequirement.GroupRoleId.HasValue ? "for group role " + groupRequirement.GroupRole.Name : string.Empty );
                            nbDuplicateGroupRequirement.Visible = true;
                            this.GroupRequirementsState.Remove( groupRequirement );
                            return;
                        }
                        else
             */
        }
    }
}

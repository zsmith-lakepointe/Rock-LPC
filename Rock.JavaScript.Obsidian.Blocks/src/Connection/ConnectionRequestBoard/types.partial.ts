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

import { ConnectionState } from "@Obsidian/Enums/Connection/connectionState";
import { GroupMemberStatus } from "@Obsidian/Enums/Group/groupMemberStatus";
import { CampusBag } from "@Obsidian/ViewModels/Blocks/Core/CampusDetail/campusBag";

// #region v1 API Request View Models

/**
 * The information needed to request connection status view models.
 * For v1 API Endpoint: https://github.com/SparkDevNetwork/Rock/blob/1e1558e2d222df7a3f7bcf36ff4e419a7e9b7a64/Rock.Rest/Controllers/ConnectionRequestsController.Partial.cs#L268
 */
export interface IConnectionStatusViewModelsRequest {
    campusId?: number | null,
    connectorPersonAliasId?: number | null,
    requesterPersonAliasId?: number | null,
    minDate?: string | null,
    maxDate?: string | null,
    delimitedStatusIds?: string | null,
    delimitedConnectionStates?: string | null,
    delimitedLastActivityTypeIds?: string | null,
    sortProperty?: number | null,
    maxRequestsPerStatus?: number | null,
    pastDueOnly: boolean
}

// #endregion v1 API Request View Models

// #region v1 API Response View Models

/**
 * Information about a connection status (columns).
 * Representation of: https://github.com/SparkDevNetwork/Rock/blob/1e1558e2d222df7a3f7bcf36ff4e419a7e9b7a64/Rock/Model/Connection/ConnectionRequest/ConnectionStatusViewModel.cs#L25
 */
export interface IConnectionStatusViewModel {
    id: number;
    name?: string | null;
    highlightColor?: string | null;
    requestCount: number;
    requests?: IConnectionRequestViewModel[] | null;
}

/**
 * Information about a connection request (cards & grid rows).
 * Representation of: https://github.com/SparkDevNetwork/Rock/blob/1e1558e2d222df7a3f7bcf36ff4e419a7e9b7a64/Rock/Model/Connection/ConnectionRequest/ConnectionRequestViewModel.cs#L28
 */
export interface IConnectionRequestViewModel {
    id: number;
    placementGroupId?: number | null;
    placementGroupRoleId?: number | null;
    placementGroupMemberStatus?: GroupMemberStatus | null;
    placementGroupRoleName?: string | null;
    comments?: string | null;
    personId: number;
    personAliasId: number;
    personEmail?: string | null;
    personNickName?: string | null;
    personLastName?: string | null;
    personPhotoId?: number | null;
    personPhones?: IPhoneViewModel[] | null;
    campus?: CampusBag | null;
    campusId?: number | null;
    campusName?: string | null;
    campusCode?: string | null;
    connectorPhotoId?: number | null;
    connectorPersonNickName?: string | null;
    connectorPersonLastName?: string | null;
    connectorPersonId?: number | null;
    connectorPersonAliasId?: number | null;
    statusId: number;
    connectionOpportunityId: number;
    connectionTypeId: number;
    statusName?: string | null;
    statusHighlightColor?: string | null;
    isStatusCritical: boolean;
    activityCount: number;
    lastActivityDate?: string | null;
    dateOpened?: string | null;
    groupName?: string | null;
    lastActivityTypeName?: string | null;
    lastActivityTypeId?: number | null;
    order: number;
    connectionState: ConnectionState;
    isAssignedToYou: boolean;
    isCritical: boolean;
    isIdle: boolean;
    isUnassigned: boolean;
    followupDate?: string | null;
    statusIconsHtml?: string | null;
    canConnect: boolean;
    canCurrentUserEdit: boolean;
    requestAttributes?: string | null;
    stateLabel?: string | null;
    statusLabelClass?: string | null;
    activityCountText?: string | null;
    lastActivityText?: string | null;
    connectorPersonFullname?: string | null;
    personFullname?: string | null;
    personPhotoUrl?: string | null;
    connectorPhotoUrl?: string | null;
    campusHtml?: string | null;
    daysSinceOpening?: number | null;
    daysSinceOpeningShortText?: string | null;
    daysOrWeeksSinceOpeningText?: string | null;
    daysSinceOpeningLongText?: string | null;
    daysSinceLastActivity?: number | null;
    daysSinceLastActivityShortText?: string | null;
    daysSinceLastActivityLongText?: string | null;
    groupNameWithRoleAndStatus?: string | null;
}

/**
 * Information about a person's phone number.
 * Representation of: https://github.com/SparkDevNetwork/Rock/blob/1e1558e2d222df7a3f7bcf36ff4e419a7e9b7a64/Rock/Model/Connection/ConnectionRequest/ConnectionRequestViewModel.cs#L607
 */
export interface IPhoneViewModel {
    phoneType?: string | null;
    formattedPhoneNumber?: string | null;
    isMessagingEnabled: boolean;
}

// #endregion v1 API Response View Models

/**
 * Keys for page parameters.
 */
export const enum PageParameterKey {
    CampusId = "CampusId",
    ConnectionOpportunityId = "ConnectionOpportunityId",
    ConnectionRequestId = "ConnectionRequestId"
}

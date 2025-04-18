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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration
{
    public static partial class TestDataHelper
    {
        public static class Interactions
        {
            public class CreatePageViewInteractionActionArgs
            {
                public Guid? Guid;
                public string ForeignKey;
                public DateTime ViewDateTime;
                public int SiteId;
                public int PageId;
                public string UserAgentString;
                public string BrowserIpAddress;
                public Guid? BrowserSessionGuid;
                public string RequestUrl;
                public int? UserPersonAliasId;
            }

            /// <summary>
            /// Remove the specified interaction.
            /// </summary>
            /// <param name="interactionIdentifier"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            public static bool DeleteInteraction( string interactionIdentifier, RockContext context )
            {
                var service = new InteractionService( context );
                var interaction = service.Get( interactionIdentifier );

                if ( interaction == null )
                {
                    return false;
                }

                return service.Delete( interaction );
            }

            /// <summary>
            /// Create an interaction for a Page View.
            /// </summary>
            /// <param name="actionInfo"></param>
            /// <param name="rockContext"></param>
            /// <returns></returns>
            public static Interaction CreatePageViewInteraction( CreatePageViewInteractionActionArgs actionInfo, RockContext rockContext )
            {
                string deviceApplication;
                string deviceOs;
                string deviceClientType;

                TestDataHelper.Web.ParseUserAgentString( actionInfo.UserAgentString,
                    out deviceOs,
                    out deviceApplication,
                    out deviceClientType );

                var interactionService = new InteractionService( rockContext );

                // Get the Interaction Channel: Internal Site
                var dvWebsiteChannelType = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE );
                var interactionChannelId = InteractionChannelCache.GetChannelIdByTypeIdAndEntityId( dvWebsiteChannelType.Id,
                    actionInfo.SiteId,
                    channelName: null,
                    componentEntityTypeId: null,
                    interactionEntityTypeId: null );

                // Get the Interaction Component: Page
                var interactionComponentId = InteractionComponentCache.GetComponentIdByChannelIdAndEntityId( interactionChannelId,
                    actionInfo.PageId,
                    componentName: null );

                var interaction = interactionService.CreateInteraction( interactionComponentId,
                    actionInfo.PageId,
                    operation: "View",
                    $"Browser Session {actionInfo.BrowserSessionGuid}",
                    actionInfo.RequestUrl,
                    actionInfo.UserPersonAliasId,
                    actionInfo.ViewDateTime,
                    deviceApplication,
                    deviceOs,
                    deviceClientType,
                    deviceTypeData: "",
                    actionInfo.BrowserIpAddress,
                    actionInfo.BrowserSessionGuid );

                interaction.ForeignKey = actionInfo.ForeignKey;

                return interaction;
            }
        }
    }
}

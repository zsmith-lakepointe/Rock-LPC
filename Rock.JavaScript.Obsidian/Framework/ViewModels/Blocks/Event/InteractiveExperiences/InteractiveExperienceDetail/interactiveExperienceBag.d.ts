//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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

import { InteractiveExperienceCampusBehavior } from "@Obsidian/Enums/Event/interactiveExperienceCampusBehavior";
import { InteractiveExperiencePushNotificationType } from "@Obsidian/Enums/Event/interactiveExperiencePushNotificationType";
import { InteractiveExperienceActionBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceActionBag";
import { InteractiveExperienceScheduleBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceScheduleBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

/**
 * Class InteractiveExperienceBag.
 * Implements the Rock.ViewModels.Utility.EntityBagBase
 */
export type InteractiveExperienceBag = {
    /** Gets or sets the background color of the action. */
    actionBackgroundColor?: string | null;

    /** Gets or sets the action background image binary file. */
    actionBackgroundImageBinaryFile?: ListItemBag | null;

    /** Gets or sets the custom css for the action. */
    actionCustomCss?: string | null;

    /** Gets or sets the primary button color of the action. */
    actionPrimaryButtonColor?: string | null;

    /** Gets or sets the primary button text color of the action. */
    actionPrimaryButtonTextColor?: string | null;

    /** Gets or sets the InteractiveExperienceActions  for this Interactive Experience. */
    actions?: InteractiveExperienceActionBag[] | null;

    /** Gets or sets the secondary button color of the action. */
    actionSecondaryButtonColor?: string | null;

    /** Gets or sets the secondary button text color of the action. */
    actionSecondaryButtonTextColor?: string | null;

    /** Gets or sets the text color of the action. */
    actionTextColor?: string | null;

    /** Gets or sets the attributes. */
    attributes?: Record<string, PublicAttributeBag> | null;

    /** Gets or sets the attribute values. */
    attributeValues?: Record<string, string> | null;

    /** Gets or sets the accent color for the audience. */
    audienceAccentColor?: string | null;

    /** Gets or sets the background color for the audience. */
    audienceBackgroundColor?: string | null;

    /** Gets or sets the audience background image binary file. */
    audienceBackgroundImageBinaryFile?: ListItemBag | null;

    /** Gets or sets the custom css for the audience. */
    audienceCustomCss?: string | null;

    /** Gets or sets the primary color for the audience. */
    audiencePrimaryColor?: string | null;

    /** Gets or sets the secondary color for the audience. */
    audienceSecondaryColor?: string | null;

    /** Gets or sets the text color for the audience. */
    audienceTextColor?: string | null;

    /**
     * The behavior of campus choices for this experience. See the description
     * of the individual enum values for specific functionality.
     */
    campusBehavior: InteractiveExperienceCampusBehavior;

    /**
     * The default campus to use when recording an Interaction if no other
     * campus could be determined.
     */
    defaultCampus?: ListItemBag | null;

    /** Gets or sets the Description of the Rock.Model.InteractiveExperience */
    description?: string | null;

    /** Gets or sets the lava template to use after the experience has ended. */
    experienceEndedTemplate?: string | null;

    /** Gets or sets the identifier key of this entity. */
    idKey?: string | null;

    /** Gets or sets the IsActive flag for the Rock.Model.InteractiveExperience. */
    isActive: boolean;

    /** Gets or sets the Name of the InteractiveExperience. This property is required. */
    name?: string | null;

    /** Gets or sets the no action header image binary file. */
    noActionHeaderImageBinaryFile?: ListItemBag | null;

    /** Gets or sets the no action message. */
    noActionMessage?: string | null;

    /** Gets or sets the no action title. */
    noActionTitle?: string | null;

    /** Gets or sets the photo binary file. */
    photoBinaryFile?: ListItemBag | null;

    /** Gets or sets the Public Label of the InteractiveExperience. */
    publicLabel?: string | null;

    /** Gets or sets the detail message of the push notification. */
    pushNotificationDetail?: string | null;

    /** Gets or sets the title of the push notification. */
    pushNotificationTitle?: string | null;

    /** Gets or sets the push notification type. */
    pushNotificationType: InteractiveExperiencePushNotificationType;

    /** Gets or sets the schedules associated with this interactive experience. */
    schedules?: InteractiveExperienceScheduleBag[] | null;

    /** Gets or sets the welcome header image binary file. */
    welcomeHeaderImageBinaryFile?: ListItemBag | null;

    /** Gets or sets the welcome message. */
    welcomeMessage?: string | null;

    /** Gets or sets the welcome title. */
    welcomeTitle?: string | null;
};

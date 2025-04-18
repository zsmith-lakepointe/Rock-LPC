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

import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

/** Badge View Model */
export type BadgeBag = {
    /** Gets or sets the attributes. */
    attributes?: Record<string, PublicAttributeBag> | null;

    /** Gets or sets the attribute values. */
    attributeValues?: Record<string, string> | null;

    /** Gets or sets the Id of the badge component entity type */
    badgeComponentEntityTypeId: number;

    /** Gets or sets the created by person alias identifier. */
    createdByPersonAliasId?: number | null;

    /** Gets or sets the created date time. */
    createdDateTime?: string | null;

    /** Gets or sets a description of the badge. */
    description?: string | null;

    /** Gets or sets the EntityTypeId of the Rock.Model.EntityType that this Badge describes. */
    entityTypeId?: number | null;

    /** Gets or sets the entity type qualifier column that contains the value (see Rock.Model.Badge.EntityTypeQualifierValue) that is used narrow the scope of the Badge to a subset or specific instance of an EntityType. */
    entityTypeQualifierColumn?: string | null;

    /** Gets or sets the entity type qualifier value that is used to narrow the scope of the Badge to a subset or specific instance of an EntityType. */
    entityTypeQualifierValue?: string | null;

    /** Gets or sets the identifier key of this entity. */
    idKey?: string | null;

    /** Gets or sets a flag indicating if this item is active or not. */
    isActive: boolean;

    /** Gets or sets the modified by person alias identifier. */
    modifiedByPersonAliasId?: number | null;

    /** Gets or sets the modified date time. */
    modifiedDateTime?: string | null;

    /** Gets or sets the given Name of the badge. This value is an alternate key and is required. */
    name?: string | null;

    /** Gets or sets the order. */
    order: number;
};

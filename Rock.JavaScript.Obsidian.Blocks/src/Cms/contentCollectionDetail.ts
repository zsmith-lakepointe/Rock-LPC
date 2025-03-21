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

import { computed, defineComponent, ref } from "vue";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { EntityType } from "@Obsidian/SystemGuids/entityType";
import DetailBlock from "@Obsidian/Templates/detailBlock";
import { DetailPanelMode } from "@Obsidian/Enums/Controls/detailPanelMode";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";
import ContentSources from "./ContentCollectionDetail/contentSources.partial";
import EditPanel from "./ContentCollectionDetail/editPanel.partial";
import SearchFilters from "./ContentCollectionDetail/searchFilters.partial";
import ViewPanel from "./ContentCollectionDetail/viewPanel.partial";
import { getSecurityGrant, provideSecurityGrant, refreshDetailAttributes, useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { debounce } from "@Obsidian/Utility/util";
import { NavigationUrlKey } from "./ContentCollectionDetail/types.partial";
import { DetailBlockBox } from "@Obsidian/ViewModels/Blocks/detailBlockBox";
import { ContentCollectionBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/contentCollectionBag";
import { ContentCollectionDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/contentCollectionDetailOptionsBag";
import { alert } from "@Obsidian/Utility/dialogs";

export default defineComponent({
    name: "Cms.ContentCollectionView",

    components: {
        NotificationBox,
        ContentSources,
        SearchFilters,
        EditPanel,
        DetailBlock,
        ViewPanel
    },

    setup() {
        const config = useConfigurationValues<DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag>>();
        const invokeBlockAction = useInvokeBlockAction();
        const securityGrant = getSecurityGrant(config.securityGrantToken);

        // #region Values

        const blockError = ref("");
        const errorMessage = ref("");

        const contentCollectionViewBag = ref(config.entity);
        const contentCollectionEditBag = ref<ContentCollectionBag | null>(null);

        const panelMode = ref<DetailPanelMode>(DetailPanelMode.View);

        const isContentSourcesActive = ref(true);

        // The properties that are being edited in the UI. This is used to
        // inform the server which incoming values have valid data in them.
        const validProperties = [
            "attributeValues",
            "description",
            "enableRequestFilters",
            "enableSegments",
            "filterSettings",
            "lastIndexDateTime",
            "lastIndexItemCount",
            "collectionKey",
            "name",
            "trendingEnabled",
            "trendingGravity",
            "trendingMaxItems",
            "trendingWindowDay",
        ];

        const refreshAttributesDebounce = debounce(() => refreshDetailAttributes(contentCollectionEditBag, validProperties, invokeBlockAction), undefined, true);

        const isIndexRebuilding = ref(false);

        // #endregion

        // #region Computed Values

        /**
         * The entity name to display in the block panel.
         */
        const panelName = computed((): string => {
            return contentCollectionViewBag.value?.name ?? "";
        });

        /**
         * The identifier key value for this entity.
         */
        const entityKey = computed((): string => {
            return contentCollectionViewBag.value?.idKey ?? "";
        });

        /**
         * Additional labels to display in the block panel.
         */
        const blockLabels = computed((): PanelAction[] | null => {
            const labels: PanelAction[] = [];

            if (panelMode.value !== DetailPanelMode.View) {
                return null;
            }

            return labels;
        });

        const isEditable = computed((): boolean => {
            return config.isEditable === true;
        });

        const options = computed((): ContentCollectionDetailOptionsBag => {
            return config.options ?? {};
        });

        const isViewing = computed((): boolean => {
            return panelMode.value === DetailPanelMode.View;
        });

        const contentSourcesNavClass = computed((): string => {
            return isContentSourcesActive.value ? "active" : "";
        });

        const searchFiltersNavClass = computed((): string => {
            return isContentSourcesActive.value ? "" : "active";
        });

        const footerSecondaryActions = computed((): PanelAction[] => {
            const actions: PanelAction[] = [];

            if (config.isEditable) {
                actions.push({
                    type: "default",
                    title: "Rebuild Index",
                    iconCssClass: !isIndexRebuilding.value ? "fa fa-download" : "fa fa-cog fa-spin",
                    disabled: isIndexRebuilding.value,
                    handler: onRebuildIndex
                });
            }

            return actions;
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        /**
         * Event handler for the Cancel button being clicked while in Edit mode.
         * Handles redirect to parent page if creating a new entity.
         *
         * @returns true if the panel should leave edit mode; false if it should stay in edit mode; or a string containing a redirect URL.
         */
        const onCancelEdit = async (): Promise<boolean | string> => {
            if (!contentCollectionEditBag.value?.idKey) {
                if (config.navigationUrls?.[NavigationUrlKey.ParentPage]) {
                    return config.navigationUrls[NavigationUrlKey.ParentPage];
                }

                return false;
            }

            return true;
        };

        /**
         * Event handler for the Delete button being clicked. Sends the
         * delete request to the server and then redirects to the target page.
         *
         * @returns false if it should stay on the page; or a string containing a redirect URL.
         */
        const onDelete = async (): Promise<false | string> => {
            errorMessage.value = "";

            const result = await invokeBlockAction<string>("Delete", {
                key: contentCollectionViewBag.value?.idKey
            });

            if (result.isSuccess && result.data) {
                return result.data;
            }
            else {
                errorMessage.value = result.errorMessage ?? "Unknown error while trying to delete content collection.";

                return false;
            }
        };

        /**
         * Event handler for the Edit button being clicked. Request the edit
         * details from the server and then enter edit mode.
         *
         * @returns true if the panel should enter edit mode; otherwise false.
         */
        const onEdit = async (): Promise<boolean> => {
            const result = await invokeBlockAction<DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag>>("Edit", {
                key: contentCollectionViewBag.value?.idKey
            });

            if (result.isSuccess && result.data && result.data.entity) {
                contentCollectionEditBag.value = result.data.entity;

                return true;
            }
            else {
                return false;
            }
        };

        /**
         * Event handler for when a value has changed that has an associated
         * C# property name. This is used to detect changes to values that
         * might cause qualified attributes to either show up or not show up.
         *
         * @param propertyName The name of the C# property that was changed.
         */
        const onPropertyChanged = (propertyName: string): void => {
            // If we don't have any qualified attribute properties or this property
            // is not one of them then do nothing.
            if (!config.qualifiedAttributeProperties || !config.qualifiedAttributeProperties.some(n => n.toLowerCase() === propertyName.toLowerCase())) {
                return;
            }

            refreshAttributesDebounce();
        };

        /**
         * Event handler for the panel's Save event. Send the data to the server
         * to be saved and then leave edit mode or redirect to target page.
         *
         * @returns true if the panel should leave edit mode; false if it should stay in edit mode; or a string containing a redirect URL.
         */
        const onSave = async (): Promise<boolean | string> => {
            errorMessage.value = "";

            const data: DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag> = {
                entity: contentCollectionEditBag.value,
                isEditable: true,
                validProperties: validProperties
            };

            const result = await invokeBlockAction<ContentCollectionBag | string>("Save", {
                box: data
            });

            if (result.isSuccess && result.data) {
                if (result.statusCode === 200 && typeof result.data === "object") {
                    contentCollectionViewBag.value = result.data;

                    return true;
                }
                else if (result.statusCode === 201 && typeof result.data === "string") {
                    return result.data;
                }
            }

            errorMessage.value = result.errorMessage ?? "Unknown error while trying to save content collection.";

            return false;
        };

        /**
         * Event handler for the nav menu to switch to the Content Sources tab.
         */
        const onContentSourcesNav = (): void => {
            isContentSourcesActive.value = true;
        };

        /**
         * Event handler for the nav menu to switch to the Search Filters tab.
         */
        const onSearchFiltersNav = (): void => {
            isContentSourcesActive.value = false;
        };

        /**
         * Event handler for the footer action to request the document index
         * to be rebuilt.
         */
        const onRebuildIndex = async (): Promise<void> => {
            if (isIndexRebuilding.value) {
                return;
            }

            isIndexRebuilding.value = true;

            const result = await invokeBlockAction<string>("RebuildIndex", {
                key: contentCollectionViewBag.value?.idKey
            });

            isIndexRebuilding.value = false;

            if (result.isSuccess && result.data) {
                await alert(result.data);
            }
            else {
                await alert(result.errorMessage || "Unknown error while trying to rebuild the index.");
            }
        };

        // #endregion

        provideSecurityGrant(securityGrant);

        // Handle any initial error conditions or the need to go into edit mode.
        if (config.errorMessage) {
            blockError.value = config.errorMessage;
        }
        else if (!config.entity) {
            blockError.value = "The specified content collection could not be viewed.";
        }
        else if (!config.entity.idKey) {
            contentCollectionEditBag.value = config.entity;
            panelMode.value = DetailPanelMode.Add;
        }

        return {
            contentCollectionViewBag,
            contentCollectionEditBag,
            contentSourcesNavClass,
            blockError,
            blockLabels,
            entityKey,
            entityTypeGuid: EntityType.ContentCollection,
            errorMessage,
            isContentSourcesActive,
            isEditable,
            isViewing,
            onCancelEdit,
            onContentSourcesNav,
            onDelete,
            onEdit,
            onPropertyChanged,
            onSave,
            onSearchFiltersNav,
            options,
            panelMode,
            panelName,
            searchFiltersNavClass,
            footerSecondaryActions
        };
    },

    template: `
<NotificationBox v-if="blockError" alertType="warning" v-text="blockError" />

<NotificationBox v-if="errorMessage" alertType="danger" v-text="errorMessage" />

<v-style>
    .content-collection-detail .label-container > .label + .label {
        margin-left: 8px;
    }
    .content-collection-detail .content-collection-trending-state > span + span {
        margin-left: 8px;
    }

    .content-collection-detail .collection-source {
        display: flex;
        min-height: 64px;
        border-radius: 8px;
        border: 1px solid #c4c4c4;
        overflow: clip;
        align-items: center;
    }

    .content-collection-detail .collection-source + .collection-source {
        margin-top: 16px;
    }

    .content-collection-detail .collection-source > .bar {
        width: 8px;
        align-self: stretch;
    }

    .content-collection-detail .collection-source > .icon {
        margin: 0px 8px;
        width: 34px;
        height: 34px;
        border-radius: 17px;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .content-collection-detail .collection-source > .title {
        flex: 1 0 0;
    }

    .content-collection-detail .collection-source > .title > .text {
        font-weight: bold;
    }

    .content-collection-detail .collection-source > .title > .secondary-text {
        color: #737475;
        font-size: 0.8em;
    }

    /* Overrides to fix panel-body targets. */
    .content-collection-detail .panel-body .collection-source > .actions {
        margin: initial;
        border: initial;
    }

    .content-collection-detail .collection-source > .actions > .item-count {
        margin-right: 12px;
    }

    .content-collection-detail .search-filter-row {
        display: flex;
    }

    .content-collection-detail .search-filter-row + .search-filter-row {
        border-top: 1px solid #dfe0e1;
        padding-top: 24px;
    }

    .content-collection-detail .search-filter-icon {
        width: 48px;
        text-align: center;
        font-size: 20px;
    }

    .content-collection-detail .search-filter-content {
        flex: 1 0 0;
    }

    .content-collection-detail .search-filter-title > .title {
        font-weight: bold;
    }

    .content-collection-detail .search-filter-description {
    }

    .content-collection-detail .search-filter-content > fieldset {
        margin-top: 24px;
        display: flex;
        flex-wrap: wrap;
    }

    .content-collection-detail .search-filter-content > fieldset > dl {
        flex: 1 0 33.33%;
    }
</v-style>

<div v-if="!blockError">
    <DetailBlock v-model:mode="panelMode"
        :name="panelName"
        :labels="blockLabels"
        :entityKey="entityKey"
        :entityTypeGuid="entityTypeGuid"
        entityTypeName="ContentCollection"
        :isAuditHidden="false"
        :isBadgesVisible="true"
        :isDeleteVisible="isEditable"
        :isEditVisible="isEditable"
        :isFollowVisible="false"
        :isSecurityHidden="true"
        :footerSecondaryActions="footerSecondaryActions"
        @cancelEdit="onCancelEdit"
        @delete="onDelete"
        @edit="onEdit"
        @save="onSave">
        <template #view>
            <ViewPanel :modelValue="contentCollectionViewBag" :options="options" />
        </template>

        <template #edit>
            <EditPanel v-model="contentCollectionEditBag" :options="options" @propertyChanged="onPropertyChanged" />
        </template>
    </DetailBlock>

    <div v-if="isViewing">
        <ul class="nav nav-pills nav-sm margin-b-md">
            <li :class="contentSourcesNavClass" role="presentation"><a href="#" @click.prevent="onContentSourcesNav">Content Sources</a></li>
            <li :class="searchFiltersNavClass" role="presentation"><a href="#" @click.prevent="onSearchFiltersNav">Search Filters</a></li>
        </ul>

        <ContentSources v-if="isContentSourcesActive" v-model="contentCollectionViewBag" />
        <SearchFilters v-else v-model="contentCollectionViewBag" />
    </div>
</div>
`
});

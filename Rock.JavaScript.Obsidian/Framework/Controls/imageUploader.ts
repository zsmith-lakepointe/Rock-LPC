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

import { Guid } from "@Obsidian/Types";
import { computed, defineComponent, PropType, ref, watch } from "vue";
import NotificationBox from "./notificationBox.obs";
import { BinaryFiletype } from "@Obsidian/SystemGuids/binaryFiletype";
import { uploadBinaryFile } from "@Obsidian/Utility/http";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "ImageUploader",

    components: {
        NotificationBox,
        RockFormField
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | null>,
            required: false
        },

        binaryFileTypeGuid: {
            type: String as PropType<Guid>,
            default: BinaryFiletype.Default
        },

        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        uploadAsTemporary: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        uploadButtonText: {
            type: String as PropType<string>,
            default: "Upload"
        },

        showDeleteButton: {
            type: Boolean as PropType<boolean>,
            default: true
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const fileGuid = ref<Guid>(props.modelValue?.value ?? "");
        const fileName = ref(props.modelValue?.text ?? "");

        // Variables related to the progress of uploading a new file.
        const isUploading = ref(false);
        const uploadProgressText = ref("");
        const uploadErrorMessage = ref("");

        // Element references used to interact directly with the DOM.
        const fileInputElement = ref<HTMLInputElement | null>(null);
        const dropZoneElement = ref<HTMLElement | null>(null);

        /** The URL used to download the file. */
        const fileUrl = computed((): string | null => {
            if (fileGuid.value) {
                return `/GetFile.ashx?guid=${fileGuid.value}`;
            }

            return null;
        });

        /** The CSS styles to use for the thumbnail element. */
        const thumbnailStyle = computed((): Record<string, string> => {
            const imageUrl = fileGuid.value ? `/GetImage.ashx?guid=${fileGuid.value}&width=500` : "/Assets/Images/no-picture.svg";

            return {
                backgroundImage: `url('${imageUrl}')`,
                backgroundSize: "cover",
                backgroundPosition: "50%"
            };
        });

        /** True if the delete button should be visible. */
        const isDeleteVisible = computed((): boolean => {
            return props.showDeleteButton && !!fileGuid.value;
        });

        /**
         * Upload the specified file into Rock with the current settings.
         *
         * @param file The file to be uploaded.
         */
        const uploadFile = async (file: File): Promise<void> => {
            // Update the UI to reflect that we are currently uploading.
            isUploading.value = true;
            uploadProgressText.value = "0%";
            uploadErrorMessage.value = "";

            try {
                // Perform the actual file upload using the helper function.
                const result = await uploadBinaryFile(file, props.binaryFileTypeGuid || BinaryFiletype.Default, {
                    baseUrl: "/ImageUploader.ashx",
                    isTemporary: props.uploadAsTemporary,
                    progress: (progress, total, percent) => {
                        uploadProgressText.value = `${percent}%`;
                    }
                });

                fileGuid.value = result.value ?? "";
                fileName.value = result.text ?? "";
            }
            catch (e) {
                // Show any error message we got.
                uploadErrorMessage.value = String(e);
            }
            finally {
                // Clear the uploading progress whether success or failure.
                isUploading.value = false;
            }
        };

        /**
         * Event handler for when the individual clicks to manually select a file
         * to be uploaded into Rock.
         */
        const onSelectFileClick = (): void => {
            if (!isUploading.value) {
                fileInputElement.value?.click();
            }
        };

        /**
         * Event handler for when the remove file button is clicked.
         */
        const onRemoveFileClick = (): void => {
            fileGuid.value = "";
            fileName.value = "";
        };

        /**
         * Event handler for when the file input has a new file selected. This
         * is triggered for manual selection only, not drag and drop.
         */
        const onFileChange = (): void => {
            if (isUploading.value) {
                return;
            }

            if (fileInputElement.value && fileInputElement.value.files && fileInputElement.value.files.length > 0) {
                uploadFile(fileInputElement.value.files[0]);
            }
        };

        /**
         * Event handler for when the file input has been cleared. This is
         * probably not actually needed since the control is hidden but including
         * it just in case.
         */
        const onFileRemove = (): void => {
            if (isUploading.value) {
                return;
            }

            fileGuid.value = "";
            fileName.value = "";
        };

        // Watch for the drop zone element to become available. Once we have it
        // register for the drag and drop events to support dropping a file onto
        // the file uploader component.
        watch(dropZoneElement, () => {
            if (dropZoneElement.value) {
                // Register the dragover event so we can indicate that we will
                // accept a file dropped on us.
                dropZoneElement.value.addEventListener("dragover", event => {
                    if (!isUploading.value && event.dataTransfer) {
                        event.stopPropagation();
                        event.preventDefault();

                        if (event.dataTransfer.items.length === 1 && event.dataTransfer.items[0].kind === "file") {
                            event.dataTransfer.dropEffect = "copy";
                        }
                        else {
                            event.dataTransfer.dropEffect = "none";
                        }
                    }
                });

                // Register the drop event so we can begin the upload for the
                // file that was dropped on us.
                dropZoneElement.value.addEventListener("drop", event => {
                    if (!isUploading.value && event.dataTransfer && event.dataTransfer.files.length > 0) {
                        event.stopPropagation();
                        event.preventDefault();

                        uploadFile(event.dataTransfer.files[0]);
                    }
                });
            }
        });

        // Watch for changes to the model value and update our internal values.
        watch(() => props.modelValue, () => {
            fileGuid.value = props.modelValue?.value ?? "";
            fileName.value = props.modelValue?.text ?? "";
        });

        // Watch for changes to our internal values and update the model value.
        watch([fileGuid, fileName], () => {
            let newValue: ListItemBag | undefined = undefined;

            if (fileGuid.value) {
                newValue = {
                    value: fileGuid.value,
                    text: fileName.value
                };
            }

            emit("update:modelValue", newValue);
        });

        return {
            dropZoneElement,
            fileGuid,
            fileInputElement,
            fileName,
            fileUrl,
            isDeleteVisible,
            isUploading,
            onFileChange,
            onFileRemove,
            onRemoveFileClick,
            onSelectFileClick,
            thumbnailStyle,
            uploadErrorMessage,
            uploadProgressText
        };
    },

    template: `
<RockFormField
    :modelValue="fileGuid"
    formGroupClasses="image-uploader"
    name="imageuploader">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <NotificationBox v-if="uploadErrorMessage" alertType="warning">
                <strong><i class="fa fa-exclamation-triangle"></i> Warning </strong>
                <span>{{ uploadErrorMessage }}</span>
            </NotificationBox>

            <div ref="dropZoneElement" :id="uniqueId" class="imageupload-group" :style="{pointerEvents: disabled ? 'none' : null}" @click="onSelectFileClick">
                <div class="imageupload-thumbnail" style="width: 100px; height: 100px;">
                    <a v-if="fileUrl" :class="thumbnailClass" :href="fileUrl" target="_blank" rel="noopener noreferrer" @click.stop>
                        <div class="imageupload-thumbnail-image" :style="thumbnailStyle"></div>
                    </a>
                    <div v-else class="imageupload-thumbnail-image" :style="thumbnailStyle"></div>

                    <div v-if="isDeleteVisible" class="imageupload-remove">
                        <a v-if="fileGuid" href="#" class="remove-file" title="Remove File" @click.prevent.stop="onRemoveFileClick">
                            <i class="fa fa-times"></i>
                        </a>
                    </div>
                </div>

                <div v-if="isUploading" class="upload-progress">
                    <i class="fa fa-refresh fa-spin fa-3x"></i>
                    <div>{{ uploadProgressText }}</div>
                </div>

                <div class="imageupload-dropzone">
                    <span>{{ uploadButtonText }}</span>
                    <input ref="fileInputElement" type="file" style="display: none;" @change="onFileChange" @remove="OnFileRemove" />
                </div>
            </div>
        </div>
    </template>
</RockFormField>
`
});

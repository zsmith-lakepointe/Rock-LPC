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

import { Component } from "vue";
import { defineAsyncComponent } from "@Obsidian/Utility/component";
import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";
import { FieldTypeBase } from "./fieldType";
import { stringComparisonTypes } from "@Obsidian/Core/Reporting/comparisonType";
import { escapeHtml, truncate } from "@Obsidian/Utility/stringUtils";

export const enum ConfigurationValueKey {
    BinaryFileType = "binaryFileType"
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./audioUrlFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./audioUrlFieldComponents")).ConfigurationComponent;
});

export class AudioUrlFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        return value ?? "";
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getSupportedComparisonTypes(): ComparisonType {
        return stringComparisonTypes;
    }

    public override getHtmlValue(value: string, _configurationValues: Record<string, string>): string {
        const html = `
<audio
    src='${value}'
    class='img img-responsive js-media-audio'
    controls>
</audio>

<script>
    Rock.controls.mediaPlayer.initialize();
</script>
        `;

        return html;
    }

    public override getCondensedHtmlValue(value: string, _configurationValues: Record<string, string>): string {
        if(!value) {
            return value;
        }

        const condensedValue = `<a href="${this.encodeXml(value)}">${truncate(value, 100)}</a>`;
        return `${escapeHtml(condensedValue)}`;
    }

    // Turns a string into a properly XML Encoded string.
    private encodeXml(str: string): string {
        let encoded = "";
        for (let i = 0; i < str.length; i++) {
            const chr = str.charAt(i);
            if (chr === "<") {
                encoded += "&lt;";
            }
            else if (chr === ">") {
                encoded += "&gt;";
            }
            else if (chr === "&") {
                encoded += "&amp;";
            }
            else if (chr === '"') {
                encoded += "&quot;";
            }
            else if (chr == "'") {
                encoded += "&apos;";
            }
            else if (chr == "\n") {
                encoded += "&#xA;";
            }
            else if (chr == "\r") {
                encoded += "&#xD;";
            }
            else if (chr == "\t") {
                encoded += "&#x9;";
            }
            else {
                encoded += chr;
            }
        }

        return encoded;
    }
}

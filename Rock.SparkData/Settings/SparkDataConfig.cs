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
using Rock.Extension;
using Rock.SystemKey;
using Rock.Web;

namespace Rock.SparkData.Settings
{
    /// <summary>
    /// Base class for SparkDataConfig.  This is the class which is serialized for database storage.  This
    /// class should initialize all members with default values directly (inline or in the constructor) and
    /// all members will be serialized.
    /// </summary>
    public class SparkDataConfigBase
    {
        /// <summary>
        /// The number of message to keep.
        /// </summary>
        private const int MESSAGE_RETENTION_LIMIT = 30;

        /// <summary>
        /// Gets or sets the NCOA settings.
        /// </summary>
        /// <value>
        /// The NCOA settings.
        /// </value>
        public NcoaSettings NcoaSettings { get; set; } = new NcoaSettings();

        /// <summary>
        /// Gets or sets the spark data API key.
        /// </summary>
        /// <value>
        /// The spark data API key.
        /// </value>
        public string SparkDataApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the global notification application group identifier.
        /// </summary>
        /// <value>
        /// The global notification application group identifier.
        /// </value>
        public int? GlobalNotificationApplicationGroupId { get; set; } = null;

        /// <summary>
        /// Gets or sets the messages that communicate to the block.
        /// </summary>
        /// <value>
        /// The messages.
        /// </value>
        public FixedSizeList<string> Messages { get; set; } = new FixedSizeList<string>( MESSAGE_RETENTION_LIMIT );

        /// <summary>
        /// Constructor.
        /// </summary>
        internal SparkDataConfigBase()
        {
            // Property or field members may be initialized here, if they are too complicated to initialize inline.
        }
    }

    /// <summary>
    /// Settings for SparkData.  This is a singleton class and it should not define any members which need
    /// to get saved. See <seealso cref="SparkDataConfigBase"/> for more detail.
    /// </summary>
    public sealed class SparkDataConfig : SparkDataConfigBase
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static SparkDataConfig Instance => _instance;

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static readonly SparkDataConfig _instance = new SparkDataConfig();

        /// <summary>
        /// Private (singleton) constructor.
        /// </summary>
        private SparkDataConfig() : base()
        {
            Load();
        }

        /// <summary>
        /// Loads the SparkData configuration data from the database.
        /// </summary>
        /// <returns></returns>
        private void Load()
        {
            var systemValue = SystemSettings.GetValue( SystemSetting.SPARK_DATA );
            if ( systemValue.IsNotNullOrWhiteSpace() )
            {
                var baseConfig = systemValue.FromJsonOrNull<SparkDataConfigBase>();
                _instance.NcoaSettings = baseConfig.NcoaSettings;
                _instance.SparkDataApiKey = baseConfig.SparkDataApiKey;
                _instance.Messages = baseConfig.Messages;
            }
        }

        /// <summary>
        /// Saves the SparkData configuration data to the database.
        /// </summary>
        public void Save()
        {
            var configRecord = new SparkDataConfigBase
            {
                NcoaSettings = Instance.NcoaSettings,
                SparkDataApiKey = Instance.SparkDataApiKey,
                Messages = Instance.Messages
            };

            SystemSettings.SetValue( SystemSetting.SPARK_DATA, configRecord.ToJson() );
        }

        /// <summary>
        /// The spark server
        /// 
        /// rockrms.com now redirects to use SSL. I changed the address below to be HTTPS instead of
        /// HTTP. Using HTTP was causing a redirect response for each request made. Requests that were
        /// GETs worked fine. POSTs did not work because .NET redirect handling changes the request
        /// method to GET. This caused the request to be a GET to an endpoint programmed to only accept
        /// POST and thus it failed.
        /// 
        /// See note by gregmac on Nov 15, 2017 about 302s:
        /// https://github.com/restsharp/RestSharp/issues/562
        /// </summary>
        internal const string SPARK_SERVER = "https://www.rockrms.com";
        //internal const string SPARK_SERVER = "http://localhost:57822";

        /// <summary>
        /// The minimum addresses required to run NCOA
        /// </summary>
        public const int NCOA_MIN_ADDRESSES = 50;
    }
}

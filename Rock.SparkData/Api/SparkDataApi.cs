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
using System;
using System.Net.Http;
using System.Web.Http;

using RestSharp;

using Rock.SparkData.Settings;

namespace Rock.SparkData.Api
{
    /// <summary>
    ///API Calls to Spark Data server. 
    /// </summary>
    public sealed partial class SparkDataApi
    {
        /// <summary>
        /// Gets the client singleton instance.
        /// </summary>
        public static SparkDataApi Instance => _instance;

        /// <summary>
        /// The client singleton instance.
        /// </summary>
        private static readonly SparkDataApi _instance = new SparkDataApi();

        /// <summary>
        /// Shared <see cref="RestClient"/>.  All SparkData Api clients should use this.
        /// </summary>
        private RestClient _client;

        /// <summary>
        /// Private (singleton) constructor.
        /// </summary>
        private SparkDataApi()
        {
            _client = new RestClient( SparkDataConfig.SPARK_SERVER );
            _client.AddDefaultHeader( "Rock-Version", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
        }

        #region Public Methods

        /// <summary>
        /// Checks if the account is valid on the Spark server.
        /// </summary>
        public AccountStatus CheckAccount()
        {
            try
            {
                var request = GetNewJsonRequest( "api/SparkData/ValidateAccount", Method.GET );
                request.AddParameter( "sparkDataApiKey", SparkDataConfig.Instance.SparkDataApiKey );

                var response = _client.Get<AccountStatus>( request );
                if ( response.StatusCode.IsErrorResponse() )
                {
                    ThrowHttpResponseException( response );
                }

                return response.Data;
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with Spark server failed: Could not authenticate Spark Data account. Possible cause is the Spark Server API server is down.", ex );
            }
        }

        /// <summary>
        /// Gets the price.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns>Price as string</returns>
        public string GetPrice( string service )
        {
            try
            {
                var request = GetNewJsonRequest( "api/SparkData/GetPrice", Method.GET );
                request.AddParameter( "service", service );

                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode.IsErrorResponse() )
                {
                    ThrowHttpResponseException( response );
                }

                return response.Content.Trim( '"' );
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with Spark server failed: Could not get price of service. Possible cause is the Spark Server API server is down.", ex );
            }
        }

        /// <summary>
        /// Initiates the report on the Spark server.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        /// <param name="numberRecords">The number records.</param>
        /// <param name="personFullName">The person that initiated the request.</param>
        /// <returns>Return the organization name and transaction key.</returns>
        internal GroupNameTransactionKey NcoaInitiateReport( int? numberRecords, string personFullName = null )
        {
            try
            {
                string url;
                url = $"api/SparkData/Ncoa/InitiateReport/{SparkDataConfig.Instance.SparkDataApiKey}/{numberRecords ?? 0}";

                var request = GetNewJsonRequest( url, Method.POST );
                request.AddHeader( "personFullName", personFullName.ToStringSafe() );

                var response = _client.Post<GroupNameTransactionKey>( request );
                if ( response.StatusCode.IsErrorResponse() )
                {
                    ThrowHttpResponseException( response );
                }

                return response.Data;
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with Spark server failed: Could not initiate the NCOA report. Possible cause is the Spark Server API server is down.", ex );
            }
        }

        /// <summary>
        /// Gets the credentials from the Spark server.
        /// </summary>
        /// <returns>The username and password</returns>
        internal UsernamePassword GetNcoaApiCredentials()
        {
            try
            {
                var request = GetNewJsonRequest( $"api/SparkData/Ncoa/GetCredentials/{SparkDataConfig.Instance.SparkDataApiKey}", Method.GET );

                var response = _client.Get<UsernamePassword>( request );
                if ( response.StatusCode.IsErrorResponse() )
                {
                    ThrowHttpResponseException( response );
                }

                return response.Data;
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with Spark server failed: Could not get the NCOA credentials. Possible cause is the Spark Server API server is down.", ex );
            }
        }

        /// <summary>
        /// Sent Complete report message to Spark server.
        /// </summary>
        /// <param name="reportKey">The report key.</param>
        /// <param name="exportFileKey">The export file key.</param>
        /// <returns>Return true if successful</returns>
        internal bool NcoaCompleteReport( string reportKey, string exportFileKey )
        {
            try
            {
                var request = GetNewJsonRequest( $"api/SparkData/Ncoa/CompleteReport/{SparkDataConfig.Instance.SparkDataApiKey}/{reportKey}/{exportFileKey}", Method.POST );

                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode.IsErrorResponse() )
                {
                    ThrowHttpResponseException( response );
                }

                return response.Content.AsBoolean();
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with Spark server failed: Could not set Spark report to complete. Possible cause is the Spark Server API server is down.", ex );
            }
        }

        /// <summary>
        /// Inform Spark server that NCOA failed and the job will try again to get NCOA data.
        /// </summary>
        /// <param name="reportKey">The report key.</param>
        /// <returns>Return the organization name and transaction key.</returns>
        internal GroupNameTransactionKey NcoaRetryReport( string reportKey )
        {
            try
            {
                var request = GetNewJsonRequest( $"api/SparkData/Ncoa/RetryReport/{SparkDataConfig.Instance.SparkDataApiKey}/{reportKey}", Method.POST );

                var response = _client.Post<GroupNameTransactionKey>( request );
                if ( response.StatusCode.IsErrorResponse() )
                {
                    ThrowHttpResponseException( response );
                }

                return response.Data;
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with Spark server failed: Could not set Spark initiate a retry. Possible cause is the Spark Server API server is down.", ex );
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Throws an <see cref="HttpResponseException"/> with the status code and content of the supplied <see cref="IRestResponse"/>.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="content"></param>
        private static void ThrowHttpResponseException( IRestResponse response )
        {
            throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
            {
                Content = new StringContent( response.Content )
            } );
        }

        /// <summary>
        /// Creates a new <see cref="RestRequest"/> with <see cref="DataFormat.Json"/>.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        private static RestRequest GetNewJsonRequest( string resource, Method method )
        {
            return new RestRequest( resource, method )
            {
                RequestFormat = DataFormat.Json
            };
        }

        #endregion Private Methods
    }
}

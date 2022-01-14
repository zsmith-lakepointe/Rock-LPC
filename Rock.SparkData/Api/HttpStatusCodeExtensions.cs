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
using System.Net;

namespace Rock.SparkData.Api
{
    internal static class HttpStatusCodeExtensions
    {
        /// <summary>
        /// Returns [true] if the <see cref="HttpStatusCode"/> is NOT <see cref="HttpStatusCode.OK"/> or <see cref="HttpStatusCode.Accepted"/>.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        internal static bool IsErrorResponse( this HttpStatusCode statusCode )
        {
            return statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.Accepted;
        }
    }
}

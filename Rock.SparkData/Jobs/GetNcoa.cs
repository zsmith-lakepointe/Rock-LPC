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
using System.ComponentModel;
using System.Text;
using System.Web;

using Quartz;

using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.SparkData.Api;
using Rock.SparkData.Settings;

namespace Rock.SparkData.Jobs
{
    /// <summary>
    /// Job to get a National Change of Address (NCOA) report for all active people's addresses.
    /// </summary>
    [DisplayName( "Get National Change of Address (NCOA)" )]
    [Description( "Job that gets National Change of Address (NCOA) data." )]

    [DisallowConcurrentExecution]
    public class GetNcoa : IJob
    {
        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GetNcoa()
        {
        }

        /// <summary>
        /// Job to get a National Change of Address (NCOA) report for all active people's addresses.
        ///
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            Exception exception = null;
            // Get the job setting(s)
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var sparkDataConfig = SparkDataConfig.Instance;

            if ( !sparkDataConfig.NcoaSettings.IsEnabled || !sparkDataConfig.NcoaSettings.IsValid() )
            {
                return;
            }

            try
            {
                Guid? sparkDataApiKeyGuid = sparkDataConfig.SparkDataApiKey.AsGuidOrNull();
                if ( sparkDataApiKeyGuid == null )
                {
                    exception = new NoRetryException( $"Spark Data API Key '{sparkDataConfig.SparkDataApiKey.ToStringSafe()}' is empty or invalid. The Spark Data API Key can be configured in System Settings > Spark Data Settings." );
                    return;
                }

                switch ( sparkDataConfig.NcoaSettings.CurrentReportStatus )
                {
                    case "":
                    case null:
                        if ( sparkDataConfig.NcoaSettings.RecurringEnabled )
                        {
                            StatusStart();
                        }

                        break;
                    case "Start":
                        StatusStart();
                        break;
                    case "Failed":
                        StatusFailed();
                        break;
                    case "Pending: Report":
                        StatusPendingReport();
                        break;
                    case "Pending: Export":
                        StatusPendingExport();
                        break;
                    case "Complete":
                        StatusComplete();
                        break;
                }
            }
            catch ( Exception ex )
            {
                exception = ex;
            }
            finally
            {
                if ( exception != null )
                {
                    context.Result = $"NCOA Job failed: {exception.Message}";

                    if ( exception is NoRetryException || exception is NoRetryAggregateException )
                    {
                        sparkDataConfig.NcoaSettings.CurrentReportStatus = "Complete";
                        sparkDataConfig.NcoaSettings.LastRunDate = RockDateTime.Now;
                    }
                    else
                    {
                        sparkDataConfig.NcoaSettings.CurrentReportStatus = "Failed";
                    }

                    StringBuilder sb = new StringBuilder( $"NOCA job failed: {RockDateTime.Now.ToString()} - {exception.Message}" );
                    Exception innerException = exception;
                    while ( innerException.InnerException != null )
                    {
                        innerException = innerException.InnerException;
                        sb.AppendLine( innerException.Message );
                    }

                    sparkDataConfig.Messages.Add( sb.ToString() );
                    sparkDataConfig.Save();

                    try
                    {
                        NcoaUtility.SendNotification( "failed" );
                    }
                    catch { }

                    Exception ex = new AggregateException( "NCOA job failed.", exception );
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( ex, context2 );
                    throw ex;
                }
                else
                {
                    string msg;
                    if ( sparkDataConfig.NcoaSettings.CurrentReportStatus == "Complete" )
                    {
                        using ( RockContext rockContext = new RockContext() )
                        {
                            NcoaHistoryService ncoaHistoryService = new NcoaHistoryService( rockContext );
                            msg = $"NCOA request processed, {ncoaHistoryService.Count()} {( ncoaHistoryService.Count() == 1 ? "address" : "addresses" )} processed, {ncoaHistoryService.MovedCount()} {( ncoaHistoryService.MovedCount() > 1 ? "were" : "was" )} marked as 'moved'";
                        }
                    }
                    else
                    {
                        msg = $"Job complete. NCOA status: {sparkDataConfig.NcoaSettings.CurrentReportStatus}";
                    }

                    context.Result = msg;
                    sparkDataConfig.Messages.Add( $"{msg}: {RockDateTime.Now.ToString()}" );
                    sparkDataConfig.Save();
                }
            }
        }

        /// <summary>
        /// Current State is Failed. If recurring is enabled, retry.
        /// </summary>
        private void StatusFailed()
        {
            StatusStart();
        }

        /// <summary>
        /// Current state is start. Start NCOA
        /// </summary>
        private static void StatusStart()
        {
            var sparkDataConfig = SparkDataConfig.Instance;

            if ( sparkDataConfig.NcoaSettings.IsAckPrice && sparkDataConfig.NcoaSettings.IsAcceptedTerms )
            {
                NcoaUtility.Start();
            }
            else
            {
                if ( !sparkDataConfig.NcoaSettings.IsAckPrice && !sparkDataConfig.NcoaSettings.IsAcceptedTerms )
                {
                    throw new NoRetryException( "The NCOA terms of service have not been accepted." );
                }
                else if ( !sparkDataConfig.NcoaSettings.IsAcceptedTerms )
                {
                    throw new NoRetryException( "The NCOA terms of service have not been accepted." );
                }
                else
                {
                    throw new NoRetryException( "The price of the NCOA service has not been acknowledged." );
                }
            }
        }

        /// <summary>
        /// Current state is complete. Check if recurring is enabled and recurring interval have been reached,
        /// and if so set the state back to Start.
        /// </summary>
        private void StatusComplete()
        {
            var sparkDataConfig = SparkDataConfig.Instance;
            bool isScheduledToRun = false;

            var hasRunDate = sparkDataConfig.NcoaSettings.IsEnabled && sparkDataConfig.NcoaSettings.LastRunDate.HasValue;
            if ( hasRunDate )
            {
                var nextRunDate = sparkDataConfig.NcoaSettings.LastRunDate.Value.AddDays( sparkDataConfig.NcoaSettings.RecurrenceInterval );
                isScheduledToRun = ( sparkDataConfig.NcoaSettings.RecurringEnabled && nextRunDate < RockDateTime.Now );
            }

            if ( isScheduledToRun )
            {
                sparkDataConfig.NcoaSettings.CurrentReportStatus = "Start";
                sparkDataConfig.NcoaSettings.PersonFullName = null;
                sparkDataConfig.Save();
                StatusStart();
            }
        }

        /// <summary>
        /// Current state is pending report. Try to resume a pending report.
        /// </summary>
        private void StatusPendingReport()
        {
            NcoaUtility.PendingReport();
        }

        /// <summary>
        /// Current state is pending export report. Try to resume a pending export report.
        /// </summary>
        private void StatusPendingExport()
        {
            NcoaUtility.PendingExport();
        }
    }
}


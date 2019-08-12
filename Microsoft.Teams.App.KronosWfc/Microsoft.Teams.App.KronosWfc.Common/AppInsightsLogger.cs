//-----------------------------------------------------------------------
// <copyright file="AppInsightsLogger.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Common
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    
    /// <summary>
    /// AppInsights Logger class
    /// </summary>
    public class AppInsightsLogger
    {
        /// <summary>
        /// telemetry client field
        /// </summary>
        private static Lazy<TelemetryClient> appInsightsClient;

        /// <summary>
        /// Initializes static members of the <see cref="AppInsightsLogger" /> class
        /// </summary>
        static AppInsightsLogger()
        {
            TelemetryConfiguration.Active.InstrumentationKey = AppSettings.Instance.AppInsightKey;
        }

        /// <summary>
        /// Gets Singleton instance to telemetry client to log insights
        /// </summary>
        public static TelemetryClient AppInsightsClient
        {
            get
            {
                if (appInsightsClient == null)
                {
                    appInsightsClient = new Lazy<TelemetryClient>(() => new TelemetryClient());
                }

                return appInsightsClient.Value;
            }
        }

        /// <summary>
        /// Used to log info to app insights
        /// </summary>
        /// <param name="message">info message</param>
        public static void Info(string message)
        {
            var properties = new Dictionary<string, string> { { "message", message } };
            AppInsightsClient.TrackEvent("Info", properties);
        }

        /// <summary>
        /// Used to log warning messages to app insights
        /// </summary>
        /// <param name="message">warning message</param>
        public static void Warn(string message)
        {
            var properties = new Dictionary<string, string> { { "message", message } };
            AppInsightsClient.TrackEvent("Warn", properties);
        }

        /// <summary>
        ///  Used to store debug details to insights
        /// </summary>
        /// <param name="message">debug message</param>
        public static void Debug(string message)
        {
            var properties = new Dictionary<string, string> { { "message", message } };
            AppInsightsClient.TrackEvent("Debug", properties);
        }

        /// <summary>
        /// Used to log error message along with exception trace to the app insights
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="ex">Error message along with exception object</param>
        public static void Error(string message, Exception ex)
        {
            var properties = new Dictionary<string, string> { { "message", message } };
            AppInsightsClient.TrackException(ex, properties);
        }

        /// <summary>
        /// Used to log error message to app insights
        /// </summary>
        /// <param name="message">error message string</param>
        public static void Error(string message)
        {
            var properties = new Dictionary<string, string> { { "message", message } };
            Exception ex = new Exception(message);
            AppInsightsClient.TrackException(ex, properties);
        }

        /// <summary>
        /// Used to log exception in app insights
        /// </summary>
        /// <param name="ex">Execption object</param>
        public static void Error(Exception ex)
        {
            AppInsightsClient.TrackException(ex);
        }

        /// <summary>
        /// Custom event trace
        /// </summary>
        /// <param name="eventName">event name</param>
        /// <param name="properties">properties to be logged optional</param>
        /// <param name="metrics">metrics optional</param>
        public static void CustomEventTrace(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            if (AppSettings.Instance.LogInsightsFlag == "1")
            {
                AppInsightsClient.TrackEvent(eventName, properties, metrics);
            }
        }
    }
}

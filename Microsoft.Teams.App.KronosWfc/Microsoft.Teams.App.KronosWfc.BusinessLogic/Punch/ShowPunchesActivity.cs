//-----------------------------------------------------------------------
// <copyright file="ShowPunchesActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Punch
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Punch.ShowPunches;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.ShowPunches;

    /// <summary>
    /// Show punches activity class
    /// </summary>
    [Serializable]
    public class ShowPunchesActivity : IShowPunchesActivity
    {
        /// <summary>
        /// Azure table storage helper
        /// </summary>
        private readonly IAzureTableStorageHelper _azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowPunchesActivity" /> class
        /// </summary>
        /// <param name="azureTableStorageHelper">Azure table storage helper</param>
        public ShowPunchesActivity(IAzureTableStorageHelper azureTableStorageHelper)
        {
            this._azureTableStorageHelper = azureTableStorageHelper;
        }

        /// <summary>
        /// Shows punches
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <param name="personNumber">Person number</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="payPeriodMonthly">Monthly pay period</param>
        /// <returns>Punches response</returns>
        public async Task<Response> ShowPunches(string tenantId, string jSession, string personNumber, string startDate, string endDate)
        {
            string xmlScheduleRequest = this.ShowPunchesRequest(personNumber, startDate, endDate);
            TenantMapEntity tenantMapEntity = await this._azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlScheduleRequest, ApiConstants.SoapEnvClose, jSession);

            Response addPunchResponse = this.ShowPunchesProcessResponse(tupleResponse.Item1);
            return addPunchResponse;
        }

        /// <summary>
        /// Shows punches process response
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>Punches process response</returns>
        public Response ShowPunchesProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Response>(xResponse.ToString());
        }

        /// <summary>
        /// Calculates pay period
        /// </summary>
        /// <param name="localTimestamp">Local timestamp</param>
        /// <param name="payPeriodMonthly">Monthly pay period</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        private static void CalculatePayPeriod(DateTimeOffset? localTimestamp, bool payPeriodMonthly, out string startDate, out string endDate)
        {
            if (!payPeriodMonthly)
            {
                var currentDay = (int)localTimestamp.Value.Date.DayOfWeek;
                var startWeek = localTimestamp.Value.Date.AddDays(-currentDay);
                endDate = startWeek.AddDays(6).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                startDate = startWeek.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                var beginDate = localTimestamp.Value.Date;
                var daysLeft = DateTime.DaysInMonth(beginDate.Year, beginDate.Month) - beginDate.Day;
                startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                endDate = localTimestamp.Value.Date.AddDays(daysLeft).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Shows punches request
        /// </summary>
        /// <param name="personNumber">Person Number</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="payPeriodMonthly">Monthly pay period</param>
        /// <returns>Punches request string</returns>
        private string ShowPunchesRequest(string personNumber, string startDate, string endDate)
        {
            Request request = new Request
            {
                Timesheet = new Models.RequestEntities.Punch.ShowPunches.Timesheet
                {
                    Employee = new Models.RequestEntities.Punch.ShowPunches.Employee
                    {
                        PersonIdentity = new Models.RequestEntities.Punch.ShowPunches.PersonIdentity
                        {
                            PersonNumber = personNumber
                        }
                    },
                    Period = new Models.RequestEntities.Punch.ShowPunches.Period
                    {
                        TimeFramePeriod = new Models.RequestEntities.Punch.ShowPunches.TimeFramePeriod
                        {
                            PeriodDateSpan = $"{startDate} - {endDate}"
                        }
                    }
                },
                Action = ApiConstants.LoadAction
            };

            return XmlConvertHelper.XmlSerialize(request);
        }
    }
}

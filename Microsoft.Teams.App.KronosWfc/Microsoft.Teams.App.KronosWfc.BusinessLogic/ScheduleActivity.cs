//-----------------------------------------------------------------------
// <copyright file="ScheduleActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Core;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Schedule;
    using ScheduleRequest = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Schedule;

    /// <summary>
    /// Schedule activity class
    /// </summary>
    [Serializable]
    public class ScheduleActivity : IScheduleActivity
    {
        /// <summary>
        /// Azure table storage helper
        /// </summary>
        private readonly IAzureTableStorageHelper azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleActivity" /> class
        /// </summary>
        /// <param name="azureTableStorageHelper">Azure table storage helper</param>
        public ScheduleActivity(IAzureTableStorageHelper azureTableStorageHelper)
        {
            this.azureTableStorageHelper = azureTableStorageHelper;
        }

        /// <summary>
        /// Shows schedule
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">jSession object</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="personNumber">Person Number</param>
        /// <returns>Schedule response</returns>
        public async Task<Response> ShowSchedule(string tenantId, string jSession, string startDate, string endDate, string personNumber)
        {
            string xmlScheduleRequest = this.CreateScheduleRequest(startDate, endDate, personNumber);
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlScheduleRequest, ApiConstants.SoapEnvClose, jSession);

            Response scheduleResponse = this.ProcessResponse(tupleResponse.Item1);
            scheduleResponse.Jsession = tupleResponse.Item2;
            return scheduleResponse;
        }

        /// <summary>
        /// Creates schedule request
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="personNumber">Person Number</param>
        /// <returns>Schedule request string</returns>
        public string CreateScheduleRequest(string startDate, string endDate, string personNumber)
        {
            ScheduleRequest.Request rq = new ScheduleRequest.Request()
            {
                Action = ApiConstants.LoadAction,
                Schedule = new ScheduleRequest.Schedule()
                {
                            Employees = new List<ScheduleRequest.PersonIdentity>(), QueryDateSpan = $"{startDate} - {endDate}"
                }
            };

            rq.Schedule.Employees.Add(new ScheduleRequest.PersonIdentity()
            {
                PersonNumber = personNumber
            });

            return rq.XmlSerialize();
        }

        /// <summary>
        /// Read the xml response into Response object
        /// </summary>
        /// <param name="strResponse">xml response string</param>
        /// <returns>Response object</returns>
        public Response ProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Response>(xResponse.ToString());
        }
    }
}

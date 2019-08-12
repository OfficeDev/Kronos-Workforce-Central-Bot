//-----------------------------------------------------------------------
// <copyright file="UpcomingShiftsActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Shifts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.HyperFind;
    using UpcomingShifts = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Shifts.UpcomingShifts;
    using ScheduleRequest = Models.RequestEntities.Schedule;

    /// <summary>
    /// Upcoming shifts activity class
    /// </summary>
    [Serializable]
    public class UpcomingShiftsActivity : IUpcomingShiftsActivity
    {
        /// <summary>
        /// Azure Table storage helper
        /// </summary>
        private readonly IAzureTableStorageHelper azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpcomingShiftsActivity" /> class
        /// </summary>
        /// <param name="azureTableStorageHelper">Azure table storage helper</param>
        public UpcomingShiftsActivity(IAzureTableStorageHelper azureTableStorageHelper)
        {
            this.azureTableStorageHelper = azureTableStorageHelper;
        }

        /// <summary>
        /// Shows upcoming shifts
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">jSession Object</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="personNumber">Person number</param>
        /// <param name="employees">Employees object</param>
        /// <returns>Upcoming shifts response</returns>
        public async Task<UpcomingShifts.Response> ShowUpcomingShifts(string tenantId, string jSession, string startDate, string endDate, string personNumber, List<ResponseHyperFindResult> employees = null)
        {
            string xmlScheduleRequest = string.Empty;

            if (employees == null)
            {
                xmlScheduleRequest = this.CreateUpcomingShiftsRequest(startDate, endDate, personNumber);
            }
            else
            {
                xmlScheduleRequest = this.CreateUpcomingShiftsRequestEmployees(startDate, endDate, personNumber, employees);
            }
            
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlScheduleRequest, ApiConstants.SoapEnvClose, jSession);

            UpcomingShifts.Response scheduleResponse = this.ProcessResponse(tupleResponse.Item1);
            scheduleResponse.Jsession = tupleResponse.Item2;
            return scheduleResponse;
        }

        /// <summary>
        /// Creates upcoming shifts request
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="personNumber">Person number</param>
        /// <returns>Shift request string</returns>
        public string CreateUpcomingShiftsRequest(string startDate, string endDate, string personNumber)
        {
            ScheduleRequest.Request rq = new ScheduleRequest.Request()
            {
                Action = ApiConstants.LoadAction,
                Schedule = new ScheduleRequest.Schedule()
                {
                    Employees = new List<ScheduleRequest.PersonIdentity>(),
                    QueryDateSpan = $"{startDate} - {endDate}"
                }
            };

            rq.Schedule.Employees.Add(new ScheduleRequest.PersonIdentity()
            {
                PersonNumber = personNumber
            });

            return rq.XmlSerialize();
        }

        /// <summary>
        /// Creates upcoming shifts request for employees
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="personNumber">Person Number</param>
        /// <param name="employees">Employees object</param>
        /// <returns>Shifts request string</returns>
        public string CreateUpcomingShiftsRequestEmployees(string startDate, string endDate, string personNumber, List<ResponseHyperFindResult> employees)
        {
            ScheduleRequest.Request rq = new ScheduleRequest.Request()
            {
                Action = ApiConstants.LoadAction,
                Schedule = new ScheduleRequest.Schedule()
                {
                    Employees = new List<ScheduleRequest.PersonIdentity>(),
                    QueryDateSpan = $"{startDate} - {endDate}"
                }
            };

            var scheduledEmployees = employees.ConvertAll(x => new ScheduleRequest.PersonIdentity { PersonNumber = x.PersonNumber });
            rq.Schedule.Employees.AddRange(scheduledEmployees);
            
            return rq.XmlSerialize();
        }

        /// <summary>
        /// Read the xml response into Response object
        /// </summary>
        /// <param name="strResponse">xml response string</param>
        /// <returns>Response object</returns>
        public UpcomingShifts.Response ProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<UpcomingShifts.Response>(xResponse.ToString());
        }
    }
}

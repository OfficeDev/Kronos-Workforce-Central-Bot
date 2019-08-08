//-----------------------------------------------------------------------
// <copyright file="SupervisorViewTimeOffActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.SupervisorViewTimeOff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.Common;
    using HyperfindResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.HyperFind;
    using TimeOffRequest = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.TimeOffRequests;
    using TimeOffResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOffRequests;

    /// <summary>
    /// Supervisor view time off Activity Class.
    /// </summary>
    [Serializable]
    public class SupervisorViewTimeOffActivity : ISupervisorViewTimeOffActivity
    {
        /// <summary>
        /// Azure table storage helper.
        /// </summary>
        private readonly AzureTableStorageHelper azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupervisorViewTimeOffActivity" /> class.
        /// </summary>
        public SupervisorViewTimeOffActivity()
        {
            this.azureTableStorageHelper = new AzureTableStorageHelper();
        }

        /// <summary>
        /// Get list of time off requested for employees.
        /// </summary>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="jSession">jSessionID.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <param name="employees">List of employees.</param>
        /// <returns>Task.</returns>
        public async Task<TimeOffResponse.Response> GetTimeOffRequest(string tenantId, string jSession, string startDate, string endDate, List<HyperfindResponse.ResponseHyperFindResult> employees)
        {
            string xmlTimeOffRequest = this.CreateRequest(employees, startDate, endDate);
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlTimeOffRequest, ApiConstants.SoapEnvClose, jSession);

            TimeOffResponse.Response timeOffResponse = this.ProcessResponse(tupleResponse.Item1);

            return timeOffResponse;
        }

        /// <summary>
        /// Create XML to add advanced time off request.
        /// </summary>
        /// <param name="employees">List of employees.</param>
        /// <param name="startdate">Start date.</param>
        /// <param name="enddate">End date.</param>
        /// <returns>Advanced time of request.</returns>
        public string CreateRequest(List<HyperfindResponse.ResponseHyperFindResult> employees, string startdate, string enddate)
        {
            TimeOffRequest.Request rq = new TimeOffRequest.Request
            {
                Action = ApiConstants.RetrieveWithDetails,
                RequestMgmt = new TimeOffRequest.RequestMgmt
                {
                    QueryDateSpan = $"{startdate}-{enddate}",
                    RequestFor = "TOR",
                    Employees = new TimeOffRequest.Employees
                    {
                        PersonIdentity = new List<TimeOffRequest.PersonIdentity>(),
                    },
                },
            };

            foreach (var item in employees)
            {
                rq.RequestMgmt.Employees.PersonIdentity.Add(new TimeOffRequest.PersonIdentity { PersonNumber = item.PersonNumber });
            }

            return rq.XmlSerialize<TimeOffRequest.Request>();
        }

        /// <summary>
        /// Read the xml response into Response object.
        /// </summary>
        /// <param name="strResponse">xml response string.</param>
        /// <returns>Response object.</returns>
        public TimeOffResponse.Response ProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<TimeOffResponse.Response>(xResponse.ToString());
        }
    }
}

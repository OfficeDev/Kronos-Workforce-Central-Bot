//-----------------------------------------------------------------------
// <copyright file="HoursWorkedActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Hours
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Hours;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Hours;

    /// <summary>
    /// Hours worked activity class
    /// </summary>
    [Serializable]
    public class HoursWorkedActivity : IHoursWorkedActivity
    {
        /// <summary>
        /// Azure Table Storage Helper
        /// </summary>
        private readonly IAzureTableStorageHelper _azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HoursWorkedActivity" /> class.
        /// </summary>
        /// <param name="azureTableStorageHelper">Azure table storage helper</param>
        public HoursWorkedActivity(IAzureTableStorageHelper azureTableStorageHelper)
        {
            this._azureTableStorageHelper = azureTableStorageHelper;
        }

        /// <summary>
        /// Shows hours worked
        /// </summary>
        /// <param name="tenantId">Tensnt ID</param>
        /// <param name="response">Takes Response</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="payPeriodMonthly">Monthly Pay period</param>
        /// <returns> Shows hours worked response</returns>
        public async Task<Response> ShowHoursWorked(string tenantId, LoginResponse response, string startDate, string endDate, string personNumber = "")
        {
            if (string.IsNullOrEmpty(personNumber))
            {
                personNumber = response.PersonNumber;
            }

            string xmlScheduleRequest = this.ShowHoursWorkedRequest(personNumber, startDate, endDate);
            TenantMapEntity tenantMapEntity = await this._azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlScheduleRequest, ApiConstants.SoapEnvClose, response.JsessionID);

            Response addPunchResponse = this.ShowHoursWorkedProcessResponse(tupleResponse.Item1);
            return addPunchResponse;
        }

        /// <summary>
        /// Shows hours worked process response
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>Worked process response</returns>
        public Response ShowHoursWorkedProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Response>(xResponse.ToString());
        }

        /// <summary>
        /// Shows hours worked request
        /// </summary>
        /// <param name="personNumber">Person Name</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <returns>Hours worked request</returns>
        private string ShowHoursWorkedRequest(string personNumber, string startDate, string endDate)
        {
            Request request = new Request
            {
                Timesheet = new Models.RequestEntities.Hours.Timesheet
                {
                    Employee = new Models.RequestEntities.Hours.Employee
                    {
                        PersonIdentity = new Models.RequestEntities.Hours.PersonIdentity
                        {
                            PersonNumber = personNumber
                        }
                    },
                    Period = new Models.RequestEntities.Hours.Period
                    {
                        TimeFramePeriod = new Models.RequestEntities.Hours.TimeFramePeriod
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

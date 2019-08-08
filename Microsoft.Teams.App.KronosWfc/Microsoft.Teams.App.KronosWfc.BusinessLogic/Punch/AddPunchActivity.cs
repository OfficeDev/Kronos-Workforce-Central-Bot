//-----------------------------------------------------------------------
// <copyright file="AddPunchActivity.cs" company="Microsoft">
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
    using addReq = Models.RequestEntities.Punch.AddPunch;
    using addRes = Models.ResponseEntities.Punch.AddPunch;
    using workRuleReq = Models.RequestEntities.Punch.WorkRuleTransfer;
    using workRuleRes = Models.ResponseEntities.Punch.WorkRuleTransfer;

    /// <summary>
    /// Add punch activity class
    /// </summary>
    [Serializable]
    public class AddPunchActivity : IAddPunchActivity
    {
        /// <summary>
        /// Azure table storage helper
        /// </summary>
        readonly IAzureTableStorageHelper _azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddPunchActivity" /> class
        /// </summary>
        /// <param name="azureTableStorageHelper">Azure table storage helper</param>
        public AddPunchActivity(IAzureTableStorageHelper azureTableStorageHelper)
        {
            this._azureTableStorageHelper = azureTableStorageHelper;
        }

        /// <summary>
        /// Add Punch
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <param name="personNumber">Person number</param>
        /// <param name="localTimestamp">Local Timestamp</param>
        /// <param name="workRuleName">Workrule name</param>
        /// <returns>punch response</returns>
        public async Task<addRes.Response> AddPunch(string tenantId, string jSession, string personNumber, DateTimeOffset? localTimestamp, string workRuleName = "")
        {
            string xmlScheduleRequest = this.CreatePunchRequest(personNumber, localTimestamp, workRuleName);
            TenantMapEntity tenantMapEntity = await this._azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlScheduleRequest, ApiConstants.SoapEnvClose, jSession);

            addRes.Response addPunchResponse = this.AddPunchProcessResponse(tupleResponse.Item1);
            return addPunchResponse;
        }

        /// <summary>
        /// Creates punch request
        /// </summary>
        /// <param name="personNumber">Person number</param>
        /// <param name="localTimestamp">local timestamp</param>
        /// <param name="workRuleName">Workrule name</param>
        /// <returns>Punch request response</returns>
        public string CreatePunchRequest(string personNumber, DateTimeOffset? localTimestamp, string workRuleName)
        {
            var tzOffset = Timezone.GetLocalTimeZoneOffset(localTimestamp);
            var kTimeZone = Timezone.GetKronosTimeZone().Where(t => t.Contains(tzOffset)).FirstOrDefault();

            addReq.Request request = new addReq.Request
            {
                Punch = new addReq.Punch
                {
                    Date = localTimestamp.Value.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture),
                    Time = localTimestamp.Value.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                    KronosTimeZone = kTimeZone,
                    Employee = new addReq.Employee
                    {
                        PersonIdentity = new addReq.PersonIdentity
                        {
                            PersonNumber = personNumber
                        }
                    }
                },
                Action = ApiConstants.AddOnlyAction
            };

            if (!string.IsNullOrWhiteSpace(workRuleName))
            {
                request.Punch.WorkRuleName = workRuleName;
            }
                
            return XmlConvertHelper.XmlSerialize(request);
        }

        /// <summary>
        /// Loads all work rules
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <param name="personNumber">Person Number</param>
        /// <returns>Work rules response</returns>
        public async Task<workRuleRes.Response> LoadAllWorkRules(string tenantId, string jSession, string personNumber)
        {
            string xmlScheduleRequest = this.CreateWorkRuleRequest(personNumber);
            TenantMapEntity tenantMapEntity = await this._azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlScheduleRequest, ApiConstants.SoapEnvClose, jSession);

            workRuleRes.Response addPunchResponse = this.WorkRuleProcessResponse(tupleResponse.Item1);
            return addPunchResponse;
        }

        /// <summary>
        /// Creates work rule request
        /// </summary>
        /// <param name="personNumber">Person Number</param>
        /// <returns>workrule request string</returns>
        public string CreateWorkRuleRequest(string personNumber)
        {
            workRuleReq.Request request = new workRuleReq.Request
            {
                WorkRule = string.Empty,
                Action = ApiConstants.WorkRuleAction
            };

            return XmlConvertHelper.XmlSerialize(request);
        }

        /// <summary>
        /// Adds punch process response
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>Punch process string </returns>
        public addRes.Response AddPunchProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<addRes.Response>(xResponse.ToString());
        }

        /// <summary>
        /// Workrule process response
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>Workrule response</returns>
        public workRuleRes.Response WorkRuleProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<workRuleRes.Response>(xResponse.ToString());
        }
    }
}

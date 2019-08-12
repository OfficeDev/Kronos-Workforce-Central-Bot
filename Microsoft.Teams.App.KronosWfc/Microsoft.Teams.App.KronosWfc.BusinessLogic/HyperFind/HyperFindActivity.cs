//-----------------------------------------------------------------------
// <copyright file="HyperFindActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.HyperFind
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.HyperFind;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.HyperFind;
    
    /// <summary>
    /// Hyper Find Activity class
    /// </summary>
    [Serializable]
    public class HyperFindActivity : IHyperFindActivity 
    {
        /// <summary>
        /// Azure Table Storage Helper Interface
        /// </summary>
        private readonly IAzureTableStorageHelper azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperFindActivity" /> class.
        /// </summary>
        /// <param name="azureTableStorageHelper">Azure Table Storage helper</param>
        public HyperFindActivity(IAzureTableStorageHelper azureTableStorageHelper)
        {
            this.azureTableStorageHelper = azureTableStorageHelper;
        }

        /// <summary>
        /// Gets all home employees
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <returns>All home employees response</returns>
        public async Task<Response> GetHyperFindQueryValues(string tenantId, string jSession, string startDate, string endDate, string hyperFindQueryName, string visibilityCode)
        {
            string hyperFindRequest = this.CreateHyperFindRequest(startDate, endDate, hyperFindQueryName, visibilityCode);
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, hyperFindRequest, ApiConstants.SoapEnvClose, jSession);

            Response hyperFindResponse = this.ProcessResponse(tupleResponse.Item1);

            return hyperFindResponse;
        }

        /// <summary>
        /// Creates hyper find request
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <returns>Request string</returns>
        public string CreateHyperFindRequest(string startDate, string endDate, string hyperFindQueryName, string visibilityCode)
        {
            Request rq = new Request()
            {
                HyperFindQuery = new RequestHyperFindQuery()
                {
                    HyperFindQueryName = hyperFindQueryName,
                    VisibilityCode = visibilityCode,
                    QueryDateSpan = $"{startDate} -{endDate}"
                },
                Action = ApiConstants.RunQueryAction
            };
            return rq.XmlSerialize();
        }

        /// <summary>
        /// Process response class
        /// </summary>
        /// <param name="strResponse">String response</param>
        /// <returns>Process response</returns>
        public Response ProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Response>(xResponse.ToString());
        }
    }
}

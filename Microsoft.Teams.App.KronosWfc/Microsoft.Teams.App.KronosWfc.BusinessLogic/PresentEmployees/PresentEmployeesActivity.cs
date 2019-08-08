//-----------------------------------------------------------------------
// <copyright file="PresentEmployeesActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.PresentEmployees
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.PresentEmployees;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.PresentEmployees;

    /// <summary>
    /// Present Employees Activity class
    /// </summary>
    [Serializable]
    public class PresentEmployeesActivity : IPresentEmployeesActivity
    {
        /// <summary>
        /// Azure Table storage helper
        /// </summary>
        private readonly IAzureTableStorageHelper _azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PresentEmployeesActivity" /> class
        /// </summary>
        /// <param name="azureTableStorageHelper">Azure table storage helper</param>
        public PresentEmployeesActivity(IAzureTableStorageHelper azureTableStorageHelper)
        {
            this._azureTableStorageHelper = azureTableStorageHelper;
        }

        #region Get person information

        /// <summary>
        /// Gets person information
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <param name="personNumber">Person Number</param>
        /// <returns>Person information response</returns>
        public async Task<Response> GetPersonInformation(string tenantId, string jSession, string personNumber)
        {
            string xmlScheduleRequest = this.GetPersonInformationRequest(personNumber);
            TenantMapEntity tenantMapEntity = await this._azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlScheduleRequest, ApiConstants.SoapEnvClose, jSession);

            Response getPersonInformationResponse = this.GetResponse(tupleResponse.Item1);

            return getPersonInformationResponse;
        }
        
        #endregion

        /// <summary>
        /// Get Response
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>Response text</returns>
        public Response GetResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Response>(xResponse.ToString());
        }

        /// <summary>
        /// Get person information request
        /// </summary>
        /// <param name="personNumber">Person Number</param>
        /// <returns>Person information request</returns>
        private string GetPersonInformationRequest(string personNumber)
        {
            Request request = new Request
            {
                PersonInformation = new Models.RequestEntities.PresentEmployees.PersonInformation
                {
                    Identity = new Identity
                    {
                        PersonIdentity = new PersonIdentity
                        {
                            PersonNumber = personNumber
                        }
                    }
                },
                Action = ApiConstants.LoadAction
            };

            return XmlConvertHelper.XmlSerialize(request);
        }
    }
}

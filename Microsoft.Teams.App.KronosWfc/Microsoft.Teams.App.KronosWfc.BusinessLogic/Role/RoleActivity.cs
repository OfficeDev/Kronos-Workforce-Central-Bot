//-----------------------------------------------------------------------
// <copyright file="RoleActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Role
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.PersonInfo;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.PersonInfo;

    /// <summary>
    /// Role activity class
    /// </summary>
    [Serializable]
    public class RoleActivity : IRoleActivity
    {
        /// <summary>
        /// Azure table storage helper
        /// </summary>
        private readonly IAzureTableStorageHelper azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleActivity" /> class
        /// </summary>
        /// <param name="azureTableStorageHelper">Azure table storage helper</param>
        public RoleActivity(IAzureTableStorageHelper azureTableStorageHelper)
        {
            this.azureTableStorageHelper = azureTableStorageHelper;
        }

        /// <summary>
        /// Gets person information
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="personNumber">Person Number</param>
        /// <param name="jSession">jSession object</param>
        /// <returns>Person information response</returns>
        public async Task<Response> GetPersonInfo(string tenantId, string personNumber, string jSession)
        {
            string personInfoRequest = this.CreatePersonInfoRequest(personNumber);
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, personInfoRequest, ApiConstants.SoapEnvClose, jSession);

            Response personInfoResponse = this.ProcessResponse(tupleResponse.Item1);
           
            return personInfoResponse;
        }

        /// <summary>
        /// Creates Person information request
        /// </summary>
        /// <param name="personNumber">Person Number</param>
        /// <returns>Person information request</returns>
        public string CreatePersonInfoRequest(string personNumber)
        {
            Request rq = new Request()
            {
                PersonInformation = new Models.RequestEntities.PersonInfo.PersonInformation()
                {
                    Identity = new Models.RequestEntities.PersonInfo.Identity()
                    {
                        PersonID = new Models.RequestEntities.PersonInfo.PersonIdentity()
                        {
                            PersonNumber = personNumber
                        }
                    }
                },
                Action = ApiConstants.LoadAction
            };

            return rq.XmlSerialize();
        }

        /// <summary>
        /// Process response method
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>Process response</returns>
        public Response ProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Response>(xResponse.ToString());
        }
    }
}

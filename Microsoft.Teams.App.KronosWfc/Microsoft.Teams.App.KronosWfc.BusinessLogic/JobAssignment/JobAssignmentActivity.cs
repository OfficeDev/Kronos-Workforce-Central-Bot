//-----------------------------------------------------------------------
// <copyright file="JobAssignmentActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.JobAssignment
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.Common;

    /// <summary>
    /// Job assignment activity class 
    /// </summary>
    [Serializable]
    public class JobAssignmentActivity: IJobAssignmentActivity
    {
        /// <summary>
        /// Azure table storage helper
        /// </summary>
        private readonly IAzureTableStorageHelper azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobAssignmentActivity" /> class
        /// </summary>
        public JobAssignmentActivity(IAzureTableStorageHelper azureTableStorageHelper)
        {
            this.azureTableStorageHelper = azureTableStorageHelper;
        }

        /// <summary>
        /// Create job assignment request
        /// </summary>
        /// <param name="personNumber">Person Number</param>
        /// <returns>job assign request</returns>
        public string CreateJobAssignRequest(string personNumber)
        {
            Models.RequestEntities.JobAssignment.Request request = new Models.RequestEntities.JobAssignment.Request
            {
                JobAssign = new Models.RequestEntities.JobAssignment.JobAssignment
                {
                    Ident = new Models.RequestEntities.JobAssignment.Identity
                    {
                        PersonIdentit = new Models.RequestEntities.JobAssignment.PersonIdentity
                        {
                            PersonNumber = personNumber
                        }
                    }
                },
                Action = ApiConstants.LoadAction
            };
            return request.XmlSerialize();
        }

        /// <summary>
        /// Process Job Assign Response
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>Job Assignment Response</returns>
        public Models.ResponseEntities.JobAssignment.Response ProcessJobAssignResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Models.ResponseEntities.JobAssignment.Response>(xResponse.ToString());
        }

        /// <summary>
        /// Get Job Assignments
        /// </summary>
        /// <param name="personNumber">Person Number</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <returns>Job Assignment response</returns>
        public async Task<Models.ResponseEntities.JobAssignment.Response> getJobAssignment(string personNumber, string tenantId, string jSession)
        {
            try
            {
                string xmlJobAssignReq = this.CreateJobAssignRequest(personNumber);
                TenantMapEntity tenantMapEntity = await azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
                var tupleJobAssignResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlJobAssignReq, ApiConstants.SoapEnvClose, jSession);

                Models.ResponseEntities.JobAssignment.Response response = this.ProcessJobAssignResponse(tupleJobAssignResponse.Item1);

                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}

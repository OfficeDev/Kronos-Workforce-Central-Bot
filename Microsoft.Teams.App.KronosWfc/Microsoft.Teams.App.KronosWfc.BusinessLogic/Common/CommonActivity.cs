namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Logon;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Logon;
    using SubTypeParams = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SubTypeParams;

    [Serializable]
    public class CommonActivity : ICommonActivity
    {
        private readonly AzureTableStorageHelper azureTableStorageHelper;

        public CommonActivity()
        {
            this.azureTableStorageHelper = new AzureTableStorageHelper();
        }

        public async Task<Models.ResponseEntities.JobAssignment.Response> GetJobAssignment(string personNumber, string tenantId, string jSession)
        {
            try
            {
                string xmlJobAssignReq = this.CreateJobAssignRequest(personNumber);
                TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
                var tupleJobAssignResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlJobAssignReq, ApiConstants.SoapEnvClose, jSession);

                Models.ResponseEntities.JobAssignment.Response response = this.ProcessJobAssignResponse(tupleJobAssignResponse.Item1);

                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Models.ResponseEntities.PersonInformation.Response> GetPersonInformation(string tenantId, string superuserJSession, string PersonNumber)
        {
            try
            {
                string xmlJobAssignReq = this.CreatePIRequest(PersonNumber);
                TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
                var tupleJobAssignResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlJobAssignReq, ApiConstants.SoapEnvClose, superuserJSession);

                Models.ResponseEntities.PersonInformation.Response response = this.ProcessPIResponse(tupleJobAssignResponse.Item1);

                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> GetConversationId(string personNumber, string tenantId, string jSession, string channelId)
        {
            try
            {
                string xmlJobAssignReq = this.CreatePIRequest(personNumber);
                TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
                var tuplePIResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlJobAssignReq, ApiConstants.SoapEnvClose, jSession);

                Models.ResponseEntities.PersonInformation.Response response = this.ProcessPIResponse(tuplePIResponse.Item1);
                var conversationId = await this.azureTableStorageHelper.Retrive<BotUserEntity>(response.PersonInformation.SupervisorData.Supervisor.PersonNumber, tenantId, channelId, "KronosUserDetails");

                return conversationId;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> GetEmpConversationId(string personNumber, string tenantId, string jSession, string channelId)
        {
            try
            {
                var conversationId = await this.azureTableStorageHelper.Retrive<BotUserEntity>(personNumber, tenantId, channelId, AppSettings.Instance.KronosUserTableName);

                return conversationId;
            }
            catch (Exception)
            {
                throw;
            }
        }

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
                            PersonNumber = personNumber,
                        },
                    },
                },
                Action = ApiConstants.LoadAction,
            };
            return request.XmlSerialize();
        }

        public Models.ResponseEntities.JobAssignment.Response ProcessJobAssignResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Models.ResponseEntities.JobAssignment.Response>(xResponse.ToString());
        }

        public string CreatePIRequest(string personNumber)
        {
            Models.RequestEntities.PersonInformation.Request request = new Models.RequestEntities.PersonInformation.Request
            {
                PersonInformation = new Models.RequestEntities.PersonInformation.PersonInformation
                {
                    Identity = new Models.RequestEntities.PersonInformation.Identity
                    {
                        PersonIdentity = new Models.RequestEntities.PersonInformation.PersonIdentity { PersonNumber = personNumber },
                    },
                },
                Action = ApiConstants.LoadAction,

            };
            return request.XmlSerialize();
        }

        public Models.ResponseEntities.PersonInformation.Response ProcessPIResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Models.ResponseEntities.PersonInformation.Response>(xResponse.ToString());
        }

        public async Task<SubTypeParams.Response> GetRequestSubTypeParams(string tenantId, string jSession)
        {
            try
            {
                string xmlRequest = this.CreateSubtypeParamsRequest();
                TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
                var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlRequest, ApiConstants.SoapEnvClose, jSession);

                Models.ResponseEntities.SubTypeParams.Response response = this.ProcessRequestSubTypeParamsResponse(tupleResponse.Item1);

                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string CreateSubtypeParamsRequest()
        {
            Models.RequestEntities.SubTypeParams.Request request = new Models.RequestEntities.SubTypeParams.Request
            {
                RequestSubtype = new Models.RequestEntities.SubTypeParams.RequestSubtype(),
                Action = ApiConstants.RetrieveAllForUpdate,
            };
            return request.XmlSerialize();
        }

        public Models.ResponseEntities.SubTypeParams.Response ProcessRequestSubTypeParamsResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Models.ResponseEntities.SubTypeParams.Response>(xResponse.ToString());
        }

        public async Task<Response> LogonSuperUser(string tenantId)
        {
            try
            {
                Request request = new Request();
                LogonActivity logonActivity = new LogonActivity(request, this.azureTableStorageHelper);
                var logonResponse = await logonActivity.LogonSuperUser(tenantId);

                return logonResponse;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
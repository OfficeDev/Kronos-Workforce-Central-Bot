//-----------------------------------------------------------------------
// <copyright file="LogonActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Core;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Logon;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Logon;

    /// <summary>
    /// Logon Activity class.
    /// </summary>
    [Serializable]
    public class LogonActivity : ILogonActivity
    {
        /// <summary>
        /// Login request.
        /// </summary>
        private Request loginRequest;

        /// <summary>
        /// Azure table storage helper.
        /// </summary>
        private IAzureTableStorageHelper azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogonActivity" /> class.
        /// </summary>
        /// <param name="loginRequest">Login Request.</param>
        /// <param name="azureTableStorageHelper">Azure table storage helper.</param>
        public LogonActivity(Request loginRequest, IAzureTableStorageHelper azureTableStorageHelper)
        {
            this.loginRequest = loginRequest;
            this.azureTableStorageHelper = azureTableStorageHelper;
        }

        /// <summary>
        /// This method calls the logOn api to log the user to kronos.
        /// </summary>
        /// <param name="user">user request object.</param>
        /// <returns>Response object.</returns>
        public async Task<Response> Logon(User user)
        {
            try
            {
                string xmlLoginRequest = this.CreateLogOnRequest(user);
                TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, user.TenantId);
                var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlLoginRequest, ApiConstants.SoapEnvClose, string.Empty);

                Response logonResponse = this.ProcessResponse(tupleResponse.Item1);
                logonResponse.Jsession = tupleResponse.Item2;
                return logonResponse;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Used to create xml logon request string.
        /// </summary>
        /// <param name="user">User request object.</param>
        /// <returns>Logon request xml string.</returns>
        public string CreateLogOnRequest(User user)
        {
            this.loginRequest.Object = ApiConstants.System;
            this.loginRequest.Action = ApiConstants.LogonAction;
            this.loginRequest.Username = user.UserName;
            this.loginRequest.Password = user.Password;
            return this.loginRequest.XmlSerialize<Request>();
        }

        /// <summary>
        /// Read the xml response into Response object.
        /// </summary>
        /// <param name="strResponse">xml response string.</param>
        /// <returns>Response object.</returns>
        public Response ProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Response>(xResponse.ToString());
        }

        /// <summary>
        /// Install method.
        /// </summary>
        /// <param name="tennantId">Tenant ID.</param>
        /// <returns>Install response.</returns>
        public async Task<Response> Install(string tennantId)
        {
            Response installResponse = new Response();

            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tennantId);

            if (tenantMapEntity != null)
            {
                installResponse.Message = tenantMapEntity.EndpointUrl;
            }
            else
            {
                installResponse.ErrorCode = "404";
            }

            return installResponse;
        }

        /// <summary>
        /// logon super user method.
        /// </summary>
        /// <param name="tenantId">tenant Id.</param>
        /// <returns>Task.</returns>
        public async Task<Response> LogonSuperUser(string tenantId)
        {
            try
            {
                TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
                User user = new User();
                user.TenantId = tenantId;

                string userName = AppSettings.Instance.SuperuserUsernameUri;
                string userPassword = AppSettings.Instance.SuperuserPasswordUri;

                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userPassword))
                {
                    user.UserName = userName;//EncryptDecrypt.Decrypt256(tenantMapEntity.SuperUserName);
                    user.Password = userPassword;//EncryptDecrypt.Decrypt256(tenantMapEntity.SuperUserPwd);
                }

                string xmlLoginRequest = this.CreateLogOnRequest(user);

                var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlLoginRequest, ApiConstants.SoapEnvClose, string.Empty);

                Response logonResponse = this.ProcessResponse(tupleResponse.Item1);
                logonResponse.Jsession = tupleResponse.Item2;

                return logonResponse;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// get schedule url based on tenant.
        /// </summary>
        /// <param name="tenantId">tenant Id.</param>
        /// <returns>Task.</returns>
        public async Task<string> GetScheduleUrl(string tenantId)
        {
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            return string.IsNullOrEmpty(tenantMapEntity?.EndpointUrl) ? string.Empty : tenantMapEntity.EndpointUrl;
        }
    }
}
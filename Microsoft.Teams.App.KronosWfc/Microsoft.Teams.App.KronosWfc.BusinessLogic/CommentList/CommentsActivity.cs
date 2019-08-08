//-----------------------------------------------------------------------
// <copyright file="CommentsActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.CommentList
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Request = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.CommentList;
    using Response = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.CommentList;

    /// <summary>
    /// Comments Activity Class.
    /// </summary>
    [Serializable]
    public class CommentsActivity : ICommentsActivity
    {
        /// <summary>
        /// Azure table storage helper.
        /// </summary>
        private readonly AzureTableStorageHelper azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentsActivity" /> class.
        /// </summary>
        public CommentsActivity()
        {
            this.azureTableStorageHelper = new AzureTableStorageHelper();
        }

        /// <summary>
        /// Get list of comments to which note going to be attached.
        /// </summary>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="jSession">JSession Id of superuser.</param>
        /// <returns>Task.</returns>
        public async Task<Response.Response> GetComments(string tenantId, string jSession)
        {
            string xmlRequest = this.CreateRequest();
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlRequest, ApiConstants.SoapEnvClose, jSession);

            Response.Response res = this.ProcessResponse(tupleResponse.Item1);

            return res;
        }

        /// <summary>
        /// Create XML request.
        /// </summary>
        /// <returns>Serialized xml request. </returns>
        public string CreateRequest()
        {
            var rq = new Request.Request
            {
                Action = ApiConstants.LoadActiveComments,
                Comment = new Request.Comment { CommentCategory = ApiConstants.REQUESTS },
            };
            return rq.XmlSerialize<Request.Request>();
        }

        /// <summary>
        /// Read the xml response into Response object.
        /// </summary>
        /// <param name="strResponse">xml response string.</param>
        /// <returns>Response object.</returns>
        public Response.Response ProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Response.Response>(xResponse.ToString());
        }
    }
}

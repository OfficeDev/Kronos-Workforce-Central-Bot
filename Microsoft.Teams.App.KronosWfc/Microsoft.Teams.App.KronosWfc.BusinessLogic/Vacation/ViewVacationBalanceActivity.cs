//-----------------------------------------------------------------------
// <copyright file="ViewVacationBalanceActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Vacation
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Vacation.ViewBalance;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Vacation.ViewBalance;
    using req = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Vacation.ViewBalance;

    /// <summary>
    /// View vacation balane activity.
    /// </summary>
    [Serializable]
    public class ViewVacationBalanceActivity : IViewVacationBalanceActivity
    {
        /// <summary>
        /// Azure Table Storage Helper Interface.
        /// </summary>
        private readonly IAzureTableStorageHelper azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewVacationBalanceActivity" /> class.
        /// </summary>
        /// <param name="azureTableStorageHelper">Azure Table Storage helper.</param>
        public ViewVacationBalanceActivity(IAzureTableStorageHelper azureTableStorageHelper)
        {
            this.azureTableStorageHelper = azureTableStorageHelper;
        }

        /// <summary>
        /// View balance method.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="jSession">J Session.</param>
        /// <param name="personNumber">Person number.</param>
        /// <returns>Balance response.</returns>
        public async Task<Response> ViewBalance(string tenantId, string jSession, string personNumber)
        {
            string xmlScheduleRequest = this.CreateViewBalanceRequest(personNumber);
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlScheduleRequest, ApiConstants.SoapEnvClose, jSession);

            Response addPunchResponse = this.ProcessResponse(tupleResponse.Item1);
            return addPunchResponse;
        }

        /// <summary>
        /// Create view balance request class.
        /// </summary>
        /// <param name="personNumber">Person number</param>
        /// <returns>View balance request</returns>
        public string CreateViewBalanceRequest(string personNumber)
        {
            Request request = new Request
            {
                AccrualData = new req.AccrualData
                {
                    BalanceDate = DateTime.Now.Date.ToString("MM/d/yyyy", CultureInfo.InvariantCulture),
                    Employee = new req.Employee
                    {
                        PersonIdentity = new req.PersonIdentity
                        {
                            PersonNumber = personNumber
                        }
                    }
                },
                Action = ApiConstants.LoadAction
            };

            return XmlConvertHelper.XmlSerialize(request);
        }

        /// <summary>
        /// Process response method
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

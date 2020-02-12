
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Teams.App.KronosWfc.Common;
using Microsoft.Teams.App.KronosWfc.Configurator.Models;
using Microsoft.Teams.App.KronosWfc.Service.Core;

namespace Microsoft.Teams.App.KronosWfc.Configurator.Models
{
    public class PayCodeActivity
    {
        private readonly IApiHelper apiHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayCodeActivity"/> class.
        /// Initialize PayCodeActivity.
        /// </summary>
        /// <param name="telemetryClient">Telemetry initialize.</param>
        /// <param name="apiHelper">API helper to fetch tuple response by post soap requests.</param>
        public PayCodeActivity( IApiHelper apiHelper)
        {
            this.apiHelper = apiHelper;
        }

        /// <summary>
        /// Fetch kronos PayCodes.
        /// </summary>
        /// <param name="endPointUrl">Kronos url.</param>
        /// <param name="jSession">Kronos session.</param>
        /// <returns>List of kronos paycodes.</returns>
        public async Task<List<string>> FetchPayCodesAsync(string endPointUrl, string jSession)
        {
            string xmlScheduleRequest = string.Empty;

            xmlScheduleRequest = this.CreateLoadPayCodeRequest();

            var tupleResponse = await this.apiHelper.SendSoapPostRequest(
                endPointUrl,
                ApiConstants.SoapEnvOpen,
                xmlScheduleRequest,
                ApiConstants.SoapEnvClose,
                jSession).ConfigureAwait(false);

            Response scheduleResponse = this.ProcessResponse(tupleResponse.Item1);
            var payCodeList = scheduleResponse.PayCode.Where(c => c.ExcuseAbsenceFlag == "true" && c.IsVisibleFlag == "true" && c.ExcuseAbsenceFlag == "true" && c.DisplayOrder != "99999999").Select(x => x.PayCodeName).ToList();
            return payCodeList;
        }

        private string CreateLoadPayCodeRequest()
        {
            Request request = new Request()
            {
                Action = ApiConstants.LoadAllPayCodes,
                PayCode = string.Empty,
            };

            return request.XmlSerialize();
        }

        /// <summary>
        /// Read the xml response into Response object.
        /// </summary>
        /// <param name="strResponse">xml response string.</param>
        /// <returns>Response object.</returns>
        private Response ProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response, StringComparison.Ordinal));
            return XmlConvertHelper.DeserializeObject<Response>(xResponse.ToString());
        }

    }
}

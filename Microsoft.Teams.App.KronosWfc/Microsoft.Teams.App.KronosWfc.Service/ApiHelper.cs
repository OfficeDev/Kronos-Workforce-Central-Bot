//-----------------------------------------------------------------------
// <copyright file="ApiHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Common
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Teams.App.KronosWfc.Service.Core;
    
    /// <summary>
    /// API helper Class
    /// </summary>
    public sealed class ApiHelper : IApiHelper
    {
        /// <summary>
        /// API helper instance
        /// </summary>
        private static readonly Lazy<ApiHelper> instance = new Lazy<ApiHelper>(() => new ApiHelper());

        /// <summary>
        /// Prevents a default instance of the <see cref="ApiHelper" /> class from being created.
        /// </summary>
        private ApiHelper()
        {
        }
        
        /// <summary>
        /// Gets API Helper Instance
        /// </summary>
        public static ApiHelper Instance
        {
            get
            {
                return instance.Value;
            }
        }

        /// <summary>
        /// Send Soap Post request
        /// </summary>
        /// <param name="endpointUrl">End point URL</param>
        /// <param name="soapEnvOpen">Soap ENv open</param>
        /// <param name="reqXml">Request XML</param>
        /// <param name="soapEnvClose">Soap Env Close</param>
        /// <param name="jSession">J Session</param>
        /// <returns>Soap request response</returns>
        public async Task<Tuple<string, string>> SendSoapPostRequest(string endpointUrl, string soapEnvOpen, string reqXml, string soapEnvClose, string jSession)
        {
            string soapString = $"{soapEnvOpen}{reqXml}{soapEnvClose}";

            HttpResponseMessage response = await this.PostXmlRequest(endpointUrl, soapString, jSession);
            string content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(jSession))
            {
                jSession = response.Headers.Where(x => x.Key == "Set-Cookie").FirstOrDefault().Value.FirstOrDefault().ToString();
            }

            return new Tuple<string, string>(content, jSession);
        }

        /// <summary>
        /// Post XMl request
        /// </summary>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="xmlString">XML string</param>
        /// <param name="jSession">J Session</param>
        /// <returns>Response message</returns>
        public async Task<HttpResponseMessage> PostXmlRequest(string baseUrl, string xmlString, string jSession)
        {
            if (string.IsNullOrEmpty(jSession))
            {
                using (var httpClient = new HttpClient())
                {
                    var httpContent = new StringContent(xmlString, Encoding.UTF8, "text/xml");
                    httpContent.Headers.Add("SOAPAction", ApiConstants.SoapAction);
                    return await httpClient.PostAsync(baseUrl, httpContent);
                }
            }
            else
            {
                using (var httpClient = new HttpClient(new HttpClientHandler { UseCookies = false }))
                {
                    var httpContent = new StringContent(xmlString, Encoding.UTF8, "text/xml");
                    httpContent.Headers.Add("SOAPAction", ApiConstants.SoapAction);
                    httpContent.Headers.Add("Cookie", jSession);
                    return await httpClient.PostAsync(baseUrl, httpContent);
                }
            }
        }
    }
}

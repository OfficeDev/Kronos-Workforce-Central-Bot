//-----------------------------------------------------------------------
// <copyright file="IApiHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Service.Core
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// API Helper interface
    /// </summary>
    public interface IApiHelper
    {
        /// <summary>
        /// Sends SOAP post request
        /// </summary>
        /// <param name="endpointUrl">Endpoint URL</param>
        /// <param name="soapEnvOpen">SOAP env open</param>
        /// <param name="reqXml">request XML</param>
        /// <param name="soapEnvClose">Soap Env Close</param>
        /// <param name="jSession">J Session</param>
        /// <returns>Soap Post response</returns>
        Task<Tuple<string, string>> SendSoapPostRequest(string endpointUrl, string soapEnvOpen, string reqXml, string soapEnvClose, string jSession);
    }
}

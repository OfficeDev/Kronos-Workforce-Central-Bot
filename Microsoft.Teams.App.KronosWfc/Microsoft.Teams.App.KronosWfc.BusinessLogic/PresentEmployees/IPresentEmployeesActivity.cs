//-----------------------------------------------------------------------
// <copyright file="IPresentEmployeesActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.PresentEmployees
{
    using System.Threading.Tasks;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.PresentEmployees;

    /// <summary>
    /// Present Employees Activity interface
    /// </summary>
    public interface IPresentEmployeesActivity
    {
        /// <summary>
        /// Gets person information
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <param name="personNumber">Person Number</param>
        /// <returns>Person Information response</returns>
        Task<Response> GetPersonInformation(string tenantId, string jSession, string personNumber);

        /// <summary>
        /// Get Response
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>response string</returns>
        Response GetResponse(string strResponse);
    }
}
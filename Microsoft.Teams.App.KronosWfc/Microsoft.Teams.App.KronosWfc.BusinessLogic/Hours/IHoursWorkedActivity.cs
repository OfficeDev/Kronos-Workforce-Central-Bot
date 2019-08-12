//-----------------------------------------------------------------------
// <copyright file="IHoursWorkedActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Hours
{
    using System.Threading.Tasks;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Hours;

    /// <summary>
    /// Hours Worked Activity Interface
    /// </summary>
    public interface IHoursWorkedActivity
    {
        /// <summary>
        /// Shows hours worked process response
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>Hours worked process response</returns>
        Response ShowHoursWorkedProcessResponse(string strResponse);

        /// <summary>
        /// Shows hours worked
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="response">Takes Response</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="payPeriodMonthly">Monthly pay period</param>
        /// <returns>Hours worked response</returns>
        Task<Response> ShowHoursWorked(string tenantId, LoginResponse response, string startDate, string endDate, string personNumber = "");
    }
}
//-----------------------------------------------------------------------
// <copyright file="IShowPunchesActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Punch
{
    using System.Threading.Tasks;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.ShowPunches;

    /// <summary>
    /// Show punches activity interface
    /// </summary>
    public interface IShowPunchesActivity
    {
        /// <summary>
        /// Show punches method
        /// </summary>
        /// <param name="tenantId">tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <param name="personNumber">Person number</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="payPeriodMonthly">Monthly pay period</param>
        /// <returns>Punches response</returns>
        Task<Response> ShowPunches(string tenantId, string jSession, string personNumber, string startDate, string endDate);

        /// <summary>
        /// Shows punches process response
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>Punches process response</returns>
        Response ShowPunchesProcessResponse(string strResponse);
    }
}
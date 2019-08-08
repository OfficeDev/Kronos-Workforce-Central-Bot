//-----------------------------------------------------------------------
// <copyright file="IScheduleActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Core
{
    using System.Threading.Tasks;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Schedule;
    
    /// <summary>
    /// Schedule activity interface
    /// </summary>
    public interface IScheduleActivity
    {
        /// <summary>
        /// Show schedule
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="personNumber">Person Number</param>
        /// <returns>Schedule Response</returns>
        Task<Response> ShowSchedule(string tenantId, string jSession, string startDate, string endDate, string personNumber);
    }
}

//-----------------------------------------------------------------------
// <copyright file="IUpcomingShiftsActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Shifts
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.HyperFind;
    using UpcomingShifts = Models.ResponseEntities.Shifts.UpcomingShifts;
    using ScheduleRequest = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Schedule;

    /// <summary>
    /// Upcoming shift activity interface
    /// </summary>
    public interface IUpcomingShiftsActivity
    {
        /// <summary>
        /// Shows upcoming shifts
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">jSession object</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="personNumber">Person Number</param>
        /// <param name="employees">Employees shift data</param>
        /// <returns>Upcoming shifts response</returns>
        Task<UpcomingShifts.Response> ShowUpcomingShifts(string tenantId, string jSession, string startDate, string endDate, string personNumber, List<ResponseHyperFindResult> employees = null);
    }
}
//-----------------------------------------------------------------------
// <copyright file="IHyperFindActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.HyperFind
{
    using System.Threading.Tasks;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.HyperFind;
    
    /// <summary>
    /// Hyper Find Activity Interface
    /// </summary>
    public interface IHyperFindActivity
    {
        /// <summary>
        /// Gets all home employees
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <returns>All home employees response</returns>
        Task<Response> GetHyperFindQueryValues(string tenantId, string jSession, string startDate, string endDate, string hyperFindQueryName, string visibilityCode);
    }
}
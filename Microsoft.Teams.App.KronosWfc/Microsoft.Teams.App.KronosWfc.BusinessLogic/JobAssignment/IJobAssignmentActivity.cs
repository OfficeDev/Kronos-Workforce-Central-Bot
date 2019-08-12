//-----------------------------------------------------------------------
// <copyright file="IJobAssignmentActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.JobAssignment
{
    using System.Threading.Tasks;

    /// <summary>
    /// Job assignment activity interface
    /// </summary>
    public interface IJobAssignmentActivity
    {

        /// <summary>
        /// Get Job Assignments
        /// </summary>
        /// <param name="personNumber">Person Number</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <returns>Job Assignment response</returns>
        Task<Models.ResponseEntities.JobAssignment.Response> getJobAssignment(string personNumber, string tenantId, string jSession);
    }
}
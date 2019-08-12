//-----------------------------------------------------------------------
// <copyright file="ILogonActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Core
{
    using System.Threading.Tasks;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Logon;
    
    /// <summary>
    /// Logon Activity Interface
    /// </summary>
    public interface ILogonActivity
    {
        /// <summary>
        /// Logon method
        /// </summary>
        /// <param name="user">User details</param>
        /// <returns>User Response</returns>
        Task<Response> Logon(User user);

        /// <summary>
        /// Install method
        /// </summary>
        /// <param name="tennantId">Tenant ID</param>
        /// <returns>Tenant Id response</returns>
        Task<Response> Install(string tennantId);
        /// <summary>
        /// LogonSuperUser method
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        Task<Response> LogonSuperUser(string tenantId);
        /// <summary>
        /// Get schedule tab url based on tenant.
        /// </summary>
        /// <param name="tenantId">tenant Id.</param>
        /// <returns></returns>
        Task<string> GetScheduleUrl(string tenantId);
    }
}

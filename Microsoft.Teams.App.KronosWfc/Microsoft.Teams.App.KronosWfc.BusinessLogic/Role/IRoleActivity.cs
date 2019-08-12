//-----------------------------------------------------------------------
// <copyright file="IRoleActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Role
{
    using System.Threading.Tasks;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.PersonInfo;
    
    /// <summary>
    /// Role activity interface
    /// </summary>
    public interface IRoleActivity
    {
        /// <summary>
        /// Gets person info
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="personNumber">Person Number</param>
        /// <param name="jSession">jSession object</param>
        /// <returns>Person info response</returns>
        Task<Response> GetPersonInfo(string tenantId, string personNumber, string jSession);
    }
}
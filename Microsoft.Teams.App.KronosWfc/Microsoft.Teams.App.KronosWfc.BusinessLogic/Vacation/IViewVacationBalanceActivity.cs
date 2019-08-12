//-----------------------------------------------------------------------
// <copyright file="IViewVacationBalanceActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Vacation
{
    using System.Threading.Tasks;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Vacation.ViewBalance;

    /// <summary>
    /// View Vacation Balance Activity Interface
    /// </summary>
    public interface IViewVacationBalanceActivity
    {
        /// <summary>
        /// Create view balance report
        /// </summary>
        /// <param name="personNumber">Person Number</param>
        /// <returns>balance request string</returns>
        string CreateViewBalanceRequest(string personNumber);

        /// <summary>
        /// Process Response
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>Process response</returns>
        Response ProcessResponse(string strResponse);

        /// <summary>
        /// View balance
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <param name="personNumber">Person Number</param>
        /// <returns>Balance response</returns>
        Task<Response> ViewBalance(string tenantId, string jSession, string personNumber);
    }
}
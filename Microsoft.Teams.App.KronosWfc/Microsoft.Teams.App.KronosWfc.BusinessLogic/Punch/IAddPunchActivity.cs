//-----------------------------------------------------------------------
// <copyright file="IAddPunchActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Punch
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.AddPunch;

    /// <summary>
    /// Add punch activity interface
    /// </summary>
    public interface IAddPunchActivity
    {
        /// <summary>
        /// Add punch
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <param name="personNumber">Person number</param>
        /// <param name="localTimestamp">Local Timestamp</param>
        /// <param name="workRuleName">Workrule name</param>
        /// <returns>Punch response</returns>
        Task<Response> AddPunch(string tenantId, string jSession, string personNumber, DateTimeOffset? localTimestamp, string workRuleName = "");

        /// <summary>
        /// Add punch process response
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>Punch response string</returns>
        Response AddPunchProcessResponse(string strResponse);

        /// <summary>
        /// Creates punch request
        /// </summary>
        /// <param name="personNumber">Person Number</param>
        /// <param name="localTimestamp">Local timestamp</param>
        /// <param name="workRuleName">Workrule name</param>
        /// <returns>Punch request string</returns>
        string CreatePunchRequest(string personNumber, DateTimeOffset? localTimestamp, string workRuleName);

        /// <summary>
        /// Creates workrule request
        /// </summary>
        /// <param name="personNumber">Person number</param>
        /// <returns>Workrule request string</returns>
        string CreateWorkRuleRequest(string personNumber);

        /// <summary>
        /// Loads all Work rules
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="jSession">J Session</param>
        /// <param name="personNumber">Person number</param>
        /// <returns>Work rule response</returns>
        Task<Models.ResponseEntities.Punch.WorkRuleTransfer.Response> LoadAllWorkRules(string tenantId, string jSession, string personNumber);

        /// <summary>
        /// Workrule process response
        /// </summary>
        /// <param name="strResponse">Response string</param>
        /// <returns>Work rule process response</returns>
        Models.ResponseEntities.Punch.WorkRuleTransfer.Response WorkRuleProcessResponse(string strResponse);
    }
}
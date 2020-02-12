//-----------------------------------------------------------------------
// <copyright file="OnLeaveDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Dialogs.OnLeave
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.HyperFind;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.SupervisorViewTimeOff;
    using Microsoft.Teams.App.KronosWfc.Cards.HeroCards;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.HyperFind;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Manager can see who is on leave today.
    /// </summary>
    [Serializable]
    public class OnLeaveDialog : IDialog<object>
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IHyperFindActivity hyperFindActivity;
        private readonly ISupervisorViewTimeOffActivity supervisorViewTimeOffActivity;
        private readonly IAzureTableStorageHelper azureTableStorageHelper;
        private LoginResponse response;
        private HeroLeaveCard heroLeaveCard;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnLeaveDialog"/> class.
        /// </summary>
        /// <param name="authenticationService">authentication service.</param>
        /// <param name="hyperFindActivity">hyperFind activity.</param>
        /// <param name="supervisorViewTimeOffActivity">time off activity.</param>
        /// <param name="azureTableStorageHelper">azure table storage helper.</param>
        /// <param name="response">login response.</param>
        /// <param name="heroLeaveCard">leave card.</param>
        public OnLeaveDialog(
            IAuthenticationService authenticationService,
            IHyperFindActivity hyperFindActivity,
            ISupervisorViewTimeOffActivity supervisorViewTimeOffActivity,
            IAzureTableStorageHelper azureTableStorageHelper,
            LoginResponse response,
            HeroLeaveCard heroLeaveCard)
        {
            this.authenticationService = authenticationService;
            this.hyperFindActivity = hyperFindActivity;
            this.supervisorViewTimeOffActivity = supervisorViewTimeOffActivity;
            this.azureTableStorageHelper = azureTableStorageHelper;
            this.response = response;
            this.heroLeaveCard = heroLeaveCard;
        }

        /// <inheritdoc/>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.ShowEmployeesonLeaveAsync);
            return Task.CompletedTask;
        }

        /// <summary>
        /// show employees on approved leave.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="result">awaitable result.</param>
        /// <returns>employee on leave card.</returns>
        private async Task ShowEmployeesonLeaveAsync(IDialogContext context, IAwaitable<string> result)
        {
            string message = await result;
            string jSession = string.Empty;
            string personNumber = string.Empty;
            string startDate = default(string);
            string endDate = default(string);
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                personNumber = this.response.PersonNumber;
                jSession = this.response.JsessionID;
            }

            AppInsightsLogger.CustomEventTrace("OnLeaveDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ShowEmployeesonLeaveAsync" }, { "Command", message } });

            // todays date
            startDate = context.Activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            endDate = context.Activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);

            // get person number from employee name
            Response hyperFindResponse = await this.hyperFindActivity.GetHyperFindQueryValues(tenantId, jSession, startDate, endDate, ApiConstants.ReportsToHyperFindQuery, ApiConstants.PersonalVisibilityCode);
            if (hyperFindResponse?.Status == ApiConstants.Success)
            {
               var leaveResult = await this.supervisorViewTimeOffActivity.GetTimeOffRequest(tenantId, jSession, startDate, endDate, hyperFindResponse?.HyperFindResult);
               if (leaveResult?.Status == ApiConstants.Success)
                {
                    var entitySick = string.Join(",", (await this.azureTableStorageHelper.GetRecordsBasedOnType(AppSettings.Instance.OvertimeMappingtableName, Constants.SickAZTS))
                        .Select(w => w.Properties["PayCodeName"].StringValue).ToArray());
                    var entityVacation = string.Join(",", (await this.azureTableStorageHelper.GetRecordsBasedOnType(AppSettings.Instance.OvertimeMappingtableName, Constants.VacationAZTS))
                        .Select(w => w.Properties["PayCodeName"].StringValue).ToArray());
                    var vacationResult = leaveResult?.RequestMgmt?.RequestItems?.GlobalTimeOffRequestItem?.FindAll(x => (entityVacation.ToLower().Contains(x.TimeOffPeriods?.TimeOffPeriod.PayCodeName.ToLowerInvariant()) || entitySick.ToLower().Contains(x.TimeOffPeriods?.TimeOffPeriod.PayCodeName.ToLowerInvariant())) && x.StatusName.ToLowerInvariant() == Constants.Approved.ToLowerInvariant());

                    Dictionary<string, string> resultData = new Dictionary<string, string>();
                    foreach (var v in vacationResult)
                    {
                        var employee = hyperFindResponse.HyperFindResult.Where(x => x.PersonNumber.Contains(v.CreatedByUser.PersonIdentity.PersonNumber)).FirstOrDefault();
                        resultData.Add(employee.FullName, v.TimeOffPeriods.TimeOffPeriod.PayCodeName);
                    }

                    await this.heroLeaveCard.ShowEmployeesonLeaveCard(context, resultData);
                }
            }

            context.Done(default(string));
        }
    }
}
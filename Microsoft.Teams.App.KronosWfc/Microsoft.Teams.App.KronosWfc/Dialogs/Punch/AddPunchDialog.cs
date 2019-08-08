//-----------------------------------------------------------------------
// <copyright file="AddPunchDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Dialogs.Punch
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Punch;
    using Microsoft.Teams.App.KronosWfc.Cards.HeroCards;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.AddPunch;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This dialog is used to add a punch.
    /// </summary>
    [Serializable]
    public class AddPunchDialog : IDialog<object>
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IAddPunchActivity addPunchActivity;
        private LoginResponse response;
        private HeroAddPunch addPunchCard;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddPunchDialog" /> class.
        /// </summary>
        /// <param name="response">LoginResponse object.</param>
        /// <param name="authenticationService">AuthenticationService object.</param>
        /// <param name="addPunchActivity">AddPunchActivity object.</param>
        /// <param name="addPunchCard">HeroAddPunch object.</param>
        public AddPunchDialog(
            LoginResponse response,
            IAuthenticationService authenticationService,
            IAddPunchActivity addPunchActivity,
            HeroAddPunch addPunchCard)
        {
            this.response = response;
            this.authenticationService = authenticationService;
            this.addPunchActivity = addPunchActivity;
            this.addPunchCard = addPunchCard;
        }

        /// <summary>
        /// The StartAsync method calls IDialogContext.Wait with the continuation delegate to specify
        /// the method that should be called when a new message is received.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>A task.</returns>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.AddPunch);
            return Task.CompletedTask;
        }

        private async Task AddPunch(IDialogContext context, IAwaitable<string> result)
        {
            var activity = context.Activity as Activity;
            string jSession = this.response.JsessionID;
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            string personNumber = string.Empty;

            context.PrivateConversationData.TryGetValue($"{context.Activity.From.Id}WorkRule", out string workRuleName);
            context.PrivateConversationData.TryGetValue($"{context.Activity.From.Id}AddPunch", out string addPunch);

            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                personNumber = this.response.PersonNumber;
            }

            var message = activity.Text?.ToLowerInvariant().Trim();
            AppInsightsLogger.CustomEventTrace("AddPunchDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "AddPunch" }, { "Command", message } });

            switch (message)
            {
                case string command when command.Contains(Constants.WorkRuleTransfer):
                    var response = await this.addPunchActivity.LoadAllWorkRules(tenantId, jSession, personNumber);
                    if (response?.Status == ApiConstants.Failure && response.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                    {
                        await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                        break;
                    }

                    await this.addPunchCard.ShowAllWorkRules(context, response);
                    break;

                case string command when command.Contains(Constants.RecentTransfer):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.RecentTransfer);
                    break;

                case string command when command.Contains(Constants.Punch):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.Punch);
                    break;

                case string command when command.Contains(Constants.WRTAdminAssistant.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTAdminAssistant, true);
                    break;
                case string command when command.Contains(Constants.WRTAdminAssistant.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTAdminAssistant, true);
                    break;
                case string command when command.Contains(Constants.WRTAdministration.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTAdministration, true);
                    break;
                case string command when command.Contains(Constants.WRTCA8hrDay.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTCA8hrDay, true);
                    break;
                case string command when command.Contains(Constants.WRTCA9HrDay.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTCA9HrDay, true);
                    break;
                case string command when command.Contains(Constants.WRTCAFullTime.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTCAFullTime, true);
                    break;
                case string command when command.Contains(Constants.WRTCallback.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTCallback, true);
                    break;
                case string command when command.Contains(Constants.WRTFullTimeExecutive.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTFullTimeExecutive, true);
                    break;
                case string command when command.Contains(Constants.WRTExecutive.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTExecutive, true);
                    break;
                case string command when command.Contains(Constants.WRTFullTime30Min.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTFullTime30Min, true);
                    break;
                case string command when command.Contains(Constants.WRTFullTime60MinNoZone.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTFullTime60MinNoZone, true);
                    break;
                case string command when command.Contains(Constants.WRTFullTime60Min.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTFullTime60Min, true);
                    break;
                case string command when command.Contains(Constants.WRTOnCall.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTOnCall, true);
                    break;
                case string command when command.Contains(Constants.WRTPartTime.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTPartTime, true);
                    break;
                case string command when command.Contains(Constants.WRTProfessionalHourly.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTProfessionalHourly, true);
                    break;
                case string command when command.Contains(Constants.WRTProfessionalSalariedWFS.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTProfessionalSalariedWFS, true);
                    break;
                case string command when command.Contains(Constants.WRTProfessionalSalaried.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTProfessionalSalaried, true);
                    break;
                case string command when command.Contains(Constants.WRTSalaried.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTSalaried, true);
                    break;
                case string command when command.Contains(Constants.WRTSupport.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTSupport, true);
                    break;
                case string command when command.Contains(Constants.WRTTechnicalService.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTTechnicalService, true);
                    break;
                case string command when command.Contains(Constants.WRTTraining.ToLowerInvariant().Trim()):
                    await this.addPunchCard.AddPunches(context, context.Activity.LocalTimestamp, Constants.WRTTraining, true);
                    break;
            }

            context.Done(default(Response));
        }
    }
}
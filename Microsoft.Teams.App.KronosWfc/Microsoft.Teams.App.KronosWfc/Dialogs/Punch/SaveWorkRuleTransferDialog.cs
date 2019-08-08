//-----------------------------------------------------------------------
// <copyright file="SaveWorkRuleTransferDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Dialogs.Punch
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Punch;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.AddPunch;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This dialog is used to save work rule transfer.
    /// </summary>
    [Serializable]
    public class SaveWorkRuleTransferDialog : IDialog<object>
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IAddPunchActivity addPunchActivity;
        private LoginResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveWorkRuleTransferDialog" /> class.
        /// </summary>
        /// <param name="response">LoginResponse object.</param>
        /// <param name="authenticationService">AuthenticationService object.</param>
        /// <param name="addPunchActivity">AddPunchActivity object.</param>
        public SaveWorkRuleTransferDialog(
            LoginResponse response,
            IAuthenticationService authenticationService,
            IAddPunchActivity addPunchActivity)
        {
            this.response = response;
            this.authenticationService = authenticationService;
            this.addPunchActivity = addPunchActivity;
        }

        /// <summary>
        /// The StartAsync method calls IDialogContext.Wait with the continuation delegate to specify
        /// the method that should be called when a new message is received.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>A task.</returns>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.SaveWorkRuleTransfer);
            return Task.CompletedTask;
        }

        private async Task SaveWorkRuleTransfer(IDialogContext context, IAwaitable<string> result)
        {
            var command = await result;
            context.PrivateConversationData.TryGetValue($"{context.Activity.From.Id}WorkRule", out string workRuleName);
            var activity = context.Activity as Activity;
            string jSession = this.response.JsessionID;
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            string personNumber = string.Empty;

            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                personNumber = this.response.PersonNumber;
            }

            AppInsightsLogger.CustomEventTrace("SaveWorkRuleTransferDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "SaveWorkRuleTransfer" }, { "Command", command } });

            if (command.ToLowerInvariant() == Constants.Yes)
            {
                var addPunchResponse = await this.addPunchActivity.AddPunch(tenantId, jSession, personNumber, context.Activity.LocalTimestamp, workRuleName);
                var error = await this.CheckErrorResponse(addPunchResponse, context);
                if (!error)
                {
                    await context.PostAsync(KronosResourceText.PunchWithTransferDone.Replace("{workRuleName}", workRuleName).Replace("{txt}", context.Activity.LocalTimestamp.Value.DateTime.ToString("dddd, dd MMMM yyyy h:mm tt", CultureInfo.InvariantCulture)));
                    context.PrivateConversationData.SetValue($"{context.Activity.From.Id}WorkRule", string.Empty);
                }
            }
            else if (command.ToLowerInvariant() == Constants.No)
            {
                await context.PostAsync(KronosResourceText.TransferCanceled);
            }

            context.Done(string.Empty);
        }

        private async Task<bool> CheckErrorResponse(Response response, IDialogContext context)
        {
            bool isError = false;
            if (response?.Status == ApiConstants.Failure)
            {
                isError = true;
                if (response.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }
                else
                {
                    await context.PostAsync(response?.Error.Message);
                }
            }

            return isError;
        }
    }
}
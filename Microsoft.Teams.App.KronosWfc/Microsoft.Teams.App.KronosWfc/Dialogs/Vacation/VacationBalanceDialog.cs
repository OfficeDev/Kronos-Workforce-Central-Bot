//-----------------------------------------------------------------------
// <copyright file="VacationBalanceDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Dialogs.Vacation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Vacation;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.VacationBalance;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.ViewTimeOffRequests;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOff.AddResponse;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Response = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Vacation.ViewBalance.Response;

    /// <summary>
    /// This dialog is used to display vacation balance.
    /// </summary>
    [Serializable]
    public class VacationBalanceDialog : IDialog<object>
    {
        private readonly IViewVacationBalanceActivity viewBalanceActivity;
        private readonly IAuthenticationService authenticationService;
        private readonly ITimeOffActivity timeOffActivity;
        private readonly AdaptiveVacationBalance carouselVacationBalance;
        private readonly ViewTimeOffRequestsCard cardObj;
        private LoginResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="VacationBalanceDialog" /> class.
        /// </summary>
        /// <param name="viewBalanceActivity">ViewVacationBalanceActivity object.</param>
        /// <param name="authenticationService">AuthenticationService object.</param>
        /// <param name="carouselVacationBalance">CarouselVacationBalance object.</param>
        /// <param name="response">LoginResponse object.</param>
        /// <param name="timeOffActivity">TimeOffActivity object.</param>
        /// <param name="card">NextVacationCard object.</param>
        public VacationBalanceDialog(
            IViewVacationBalanceActivity viewBalanceActivity,
            IAuthenticationService authenticationService,
            AdaptiveVacationBalance carouselVacationBalance,
            LoginResponse response,
            ITimeOffActivity timeOffActivity,
            ViewTimeOffRequestsCard card)
        {
            this.viewBalanceActivity = viewBalanceActivity;
            this.authenticationService = authenticationService;
            this.carouselVacationBalance = carouselVacationBalance;
            this.response = response;
            this.cardObj = card;
            this.timeOffActivity = timeOffActivity;
        }

        /// <summary>
        /// The StartAsync method calls IDialogContext.Wait with the continuation delegate to specify
        /// the method that should be called when a new message is received.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>A Task.</returns>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.ProcessRequest);
            return Task.CompletedTask;
        }

        /// <summary>
        /// get paginated list.
        /// </summary>
        /// <typeparam name="T">list type.</typeparam>
        /// <param name="list">list object.</param>
        /// <param name="page">page number.</param>
        /// <param name="pageSize">page size.</param>
        /// <returns>paginated list.</returns>
        public IList<T> GetPage<T>(IList<T> list, int page, int pageSize)
        {
            return list.Skip(page * pageSize).Take(pageSize).ToList();
        }

        /// <summary>
        /// Process recieved message.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">Awaitable string.</param>
        /// <returns>Task.</returns>
        private async Task ProcessRequest(IDialogContext context, IAwaitable<string> result)
        {
            var activity = context.Activity as Activity;
            var message = activity.Text?.Trim();

            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            string jSessionId = string.Empty;
            string personNumber = string.Empty;

            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                jSessionId = this.response.JsessionID;
                personNumber = this.response.PersonNumber;
            }

            AppInsightsLogger.CustomEventTrace("VacationBalanceDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ProcessRequest" }, { "Command", message } });

            if (message.ToLowerInvariant().Contains(Constants.NextVacation.ToLowerInvariant()))
            {
                await this.ProcessNextVacation(context, tenantId, jSessionId, personNumber, message);
            }
            else
            {
                await this.ViewVacationBalance(context, result, tenantId, jSessionId, personNumber);
            }

            context.Done(default(string));
        }

        /// <summary>
        /// View vacation balance.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">Awaitable string.</param>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="jSession">J Session.</param>
        /// <param name="personNumber">Person number of logged in user.</param>
        /// <returns>Task.</returns>
        private async Task ViewVacationBalance(IDialogContext context, IAwaitable<string> result, string tenantId, string jSession, string personNumber)
        {
            Response viewBalanceResponse = await this.viewBalanceActivity.ViewBalance(tenantId, jSession, personNumber);
            if (viewBalanceResponse?.Status == ApiConstants.Failure)
            {
                if (viewBalanceResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }
            }
            else
            {
                var reply = this.carouselVacationBalance.ShowVacationBalanceCard(context, viewBalanceResponse);
                if (reply.Attachments.Count > 0)
                {
                    await context.PostAsync(reply);
                }
                else
                {
                    await context.PostAsync(Constants.NoAccrualBalanceText);
                }
            }

            context.Done(viewBalanceResponse);
        }

        /// <summary>
        /// Process next vacation commands.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="jSession">Jsession Id.</param>
        /// <param name="personNumber">Person number of logged in user.</param>
        /// <param name="message">Activity message.</param>
        /// <returns>Task.</returns>
        private async Task ProcessNextVacation(IDialogContext context, string tenantId, string jSession, string personNumber, string message)
        {
            if (message.ToLowerInvariant() == Constants.NextVacation_Next.ToLowerInvariant())
            {
                await this.NextVacationOnNext(context);
            }
            else if (message.ToLowerInvariant() == Constants.NextVacation_Previous.ToLowerInvariant())
            {
                await this.NextVacationOnPrevious(context);
            }
            else
            {
                await this.ShowNextVacation(context, tenantId, jSession, personNumber);
            }

            context.Done(default(string));
        }

        /// <summary>
        /// Fetch upcoming time off requests and present card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="jSession">Jsession Id.</param>
        /// <param name="personNumber">Logged in user's person number.</param>
        /// <returns>Task.</returns>
        private async Task ShowNextVacation(IDialogContext context, string tenantId, string jSession, string personNumber)
        {
            string datePeriod = string.Empty;
            var obj = await this.timeOffActivity.getVacationsList(tenantId, jSession, personNumber, Constants.NextVacation);
            var items = obj.EmployeeRequestMgm.RequestItem.GlobalTimeOffRequestItms.ToList();
            var activity = context.Activity as Activity;

            Hashtable pages = new Hashtable();
            foreach (var item in items)
            {
                var sdt = item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().StartDate;
                var edt = item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().EndDate;
                DateTime.TryParse(sdt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate);
                DateTime.TryParse(edt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate);
                endDate = endDate.AddHours(23);
                endDate = endDate.AddMinutes(59);
                item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().sdt = startDate;
                item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().edt = endDate;
            }

            // items = items.Where(w => w.StatusName.ToLowerInvariant() == Constants.Approved.ToLowerInvariant() || w.StatusName.ToLowerInvariant() == Constants.Refused.ToLowerInvariant() || w.StatusName.ToLowerInvariant() == Constants.Submitted.ToLowerInvariant()).OrderBy(w => w.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().sdt).ToList();
            items = items.Where(w => w.StatusName.ToLowerInvariant() == Constants.Approved.ToLowerInvariant()).OrderBy(w => w.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().sdt).ToList();
            int count = (int)Math.Ceiling((double)items.Count / 5);
            count = count > 5 ? 5 : count;

            for (int i = 0; i < count; i++)
            {
                IList<EmployeeGlobalTimeOffRequestItem> perPageList = this.GetPage(items, i, 5);
                pages.Add((i + 1).ToString(), JsonConvert.SerializeObject(perPageList).ToString());
            }

            context.PrivateConversationData.SetValue("PagewiseNextVacation", pages);
            var currentPage = pages.Count > 0 ? JsonConvert.DeserializeObject<IList<EmployeeGlobalTimeOffRequestItem>>(Convert.ToString(pages["1"])) : new List<EmployeeGlobalTimeOffRequestItem>();
            var card = this.cardObj.GetCard(currentPage, pages.Count > 1 ? "TN" : "T", 1, 1);
            var reply = activity.CreateReply();
            reply.Attachments = new List<Attachment>
                {
                    new Attachment()
                    {
                        Content = card,
                        ContentType = "application/vnd.microsoft.card.adaptive",
                    },
                };
            await context.PostAsync(reply);
        }

        /// <summary>
        /// Get next 5 vacation requests.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>Task.</returns>
        private async Task NextVacationOnNext(IDialogContext context)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            var currentPage = Convert.ToInt32(token.SelectToken("CurrentPage"));
            Hashtable pages = context.PrivateConversationData.GetValue<Hashtable>("PagewiseNextVacation");
            var card = this.cardObj.GetCard(JsonConvert.DeserializeObject<IList<EmployeeGlobalTimeOffRequestItem>>(Convert.ToString(pages[(currentPage + 1).ToString()])), pages.Count > (currentPage + 1) ? "TNP" : "TP", currentPage + 1, 1);
            var conversationId = context.Activity.Conversation.Id;
            var activityId = context.Activity.ReplyToId;

            IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
            activity.Text = null;
            activity.Value = null;
            activity.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
            });
            await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
        }

        /// <summary>
        /// Get previous 5 vacation requests.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>Task.</returns>
        private async Task NextVacationOnPrevious(IDialogContext context)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            var currentPage = Convert.ToInt32(token.SelectToken("CurrentPage"));
            Hashtable pages = context.PrivateConversationData.GetValue<Hashtable>("PagewiseNextVacation");
            var card = this.cardObj.GetCard(JsonConvert.DeserializeObject<IList<EmployeeGlobalTimeOffRequestItem>>(Convert.ToString(pages[(currentPage - 1).ToString()])), (currentPage - 1) == 1 ? "TN" : "TNP", currentPage - 1, 1);
            var conversationId = context.Activity.Conversation.Id;
            var activityId = context.Activity.ReplyToId;

            IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
            activity.Text = null;
            activity.Value = null;
            activity.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
            });
            await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
        }
    }
}
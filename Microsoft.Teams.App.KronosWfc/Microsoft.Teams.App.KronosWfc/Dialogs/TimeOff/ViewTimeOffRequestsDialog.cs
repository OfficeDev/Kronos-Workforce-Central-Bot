//-----------------------------------------------------------------------
// <copyright file="ViewTimeOffRequestsDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Dialogs.TimeOff
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
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.ViewTimeOffRequests;
    using Microsoft.Teams.App.KronosWfc.Cards.CarouselCards;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOff.AddResponse;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// ViewTimeOffRequestsDialog class.
    /// </summary>
    [Serializable]
    public class ViewTimeOffRequestsDialog : IDialog<object>
    {
        private readonly IAuthenticationService authenticationService;
        private readonly ITimeOffActivity timeOffActivity;
        private readonly ViewTimeOffRequestsCard cardObj;
        private LoginResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTimeOffRequestsDialog"/> class.
        /// </summary>
        /// <param name="response">Login response object.</param>
        /// <param name="authenticationService">AuthenticationService.</param>
        /// <param name="timeOffActivity">TimeOffActivity.</param>
        /// <param name="card">NextVacationCard class object.</param>
        public ViewTimeOffRequestsDialog(LoginResponse response, IAuthenticationService authenticationService, ITimeOffActivity timeOffActivity, ViewTimeOffRequestsCard card)
        {
            this.cardObj = card;
            this.response = response;
            this.authenticationService = authenticationService;
            this.timeOffActivity = timeOffActivity;
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
        /// The StartAsync method calls IDialogContext.Wait with the continuation delegate to specify
        /// the method that should be called when a new message is received.
        /// </summary>
        /// <param name="context">IDialogContext object.</param>
        /// <returns>A task.</returns>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.ProcessRequest);
            return Task.CompletedTask;
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
            //var message = activity.Text?.Trim();
            string resultString = await result;
            string message = JsonConvert.DeserializeObject<Message>(resultString).message;
            var luisResult = JsonConvert.DeserializeObject<Message>(resultString).luisResult;
            // string jSessionId = JsonConvert.DeserializeObject<Message>(resultString).jID;

            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            string jSessionId = string.Empty;
            string personNumber = string.Empty;

            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                jSessionId = this.response.JsessionID;
                personNumber = this.response.PersonNumber;
            }

            AppInsightsLogger.CustomEventTrace("ViewTimeOffRequestsDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ProcessRequest" }, { "Command", message } });

            //  if (message.ToLowerInvariant().Contains(Constants.ShowAllTimeOff.ToLowerInvariant()))
            // {
            await this.ProcessShowAllTimeoff(context, tenantId, jSessionId, personNumber, message);
         //}
            context.Done(default(string));
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
        private async Task ProcessShowAllTimeoff(IDialogContext context, string tenantId, string jSession, string personNumber, string message)
        {
            if (message.ToLowerInvariant() == Constants.ShowAllVacation_Next.ToLowerInvariant())
            {
                await this.ShowAllTimeOffOnNext(context);
            }
            else if (message.ToLowerInvariant() == Constants.ShowAllVacation_Previous.ToLowerInvariant())
            {
                await this.ShowAllTimeOffOnPrevious(context);
            }
            else
            {
                await this.ShowAllTimeOff(context, tenantId, jSession, personNumber);
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
        private async Task ShowAllTimeOff(IDialogContext context, string tenantId, string jSession, string personNumber)
        {
            string datePeriod = string.Empty;
            var obj = await this.timeOffActivity.getVacationsList(tenantId, jSession, personNumber, Constants.ShowAllTimeOff);
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

            items = items.Where(w => w.StatusName.ToLowerInvariant() == Constants.Approved.ToLowerInvariant() || w.StatusName.ToLowerInvariant() == Constants.Refused.ToLowerInvariant() || w.StatusName.ToLowerInvariant() == Constants.Submitted.ToLowerInvariant()).OrderBy(w => w.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().sdt).ToList();
            int count = (int)Math.Ceiling((double)items.Count / 5);
            count = count > 5 ? 5 : count;

            for (int i = 0; i < count; i++)
            {
                IList<EmployeeGlobalTimeOffRequestItem> perPageList = this.GetPage(items, i, 5);
                pages.Add((i + 1).ToString(), JsonConvert.SerializeObject(perPageList).ToString());
            }

            context.PrivateConversationData.SetValue("PagewiseAllTimeOffRequests", pages);
            var currentPage = pages.Count > 0 ? JsonConvert.DeserializeObject<IList<EmployeeGlobalTimeOffRequestItem>>(Convert.ToString(pages["1"])) : new List<EmployeeGlobalTimeOffRequestItem>();
            var card = this.cardObj.GetCard(currentPage, pages.Count > 1 ? "TN" : "T", 1, 2);
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
        private async Task ShowAllTimeOffOnNext(IDialogContext context)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            var currentPage = Convert.ToInt32(token.SelectToken("CurrentPage"));
            Hashtable pages = context.PrivateConversationData.GetValue<Hashtable>("PagewiseAllTimeOffRequests");
            var card = this.cardObj.GetCard(JsonConvert.DeserializeObject<IList<EmployeeGlobalTimeOffRequestItem>>(Convert.ToString(pages[(currentPage + 1).ToString()])), pages.Count > (currentPage + 1) ? "TNP" : "TP", currentPage + 1, 2);
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
        private async Task ShowAllTimeOffOnPrevious(IDialogContext context)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            var currentPage = Convert.ToInt32(token.SelectToken("CurrentPage"));
            Hashtable pages = context.PrivateConversationData.GetValue<Hashtable>("PagewiseAllTimeOffRequests");
            var card = this.cardObj.GetCard(JsonConvert.DeserializeObject<IList<EmployeeGlobalTimeOffRequestItem>>(Convert.ToString(pages[(currentPage - 1).ToString()])), (currentPage - 1) == 1 ? "TN" : "TNP", currentPage - 1, 2);
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
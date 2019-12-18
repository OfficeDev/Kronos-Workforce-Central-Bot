//-----------------------------------------------------------------------
// <copyright file="MessagesController.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using AdaptiveCards;
    using Autofac;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.CommandHandling;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Dialogs;
    using Microsoft.Teams.App.KronosWfc.Dialogs.SupervisorViewTimeoff;
    using Microsoft.Teams.App.KronosWfc.Filters;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Provider;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Request = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Logon.Request;
    using Response = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Logon.Response;
    using TimeOffResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOffRequests;

    /// <summary>
    /// message controller class.
    /// </summary>
    [Route("api/messages")]
    [BotAuthentication(CredentialProviderType = typeof(CredentialProvider))]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// Default locale to use if client locale is missing.
        /// </summary>
        public const string DefaultLocale = "en-US";

        /// <summary>
        /// post request of the message extension.
        /// </summary>
        /// <param name="activity">activity object.</param>
        /// <param name="cancellationToken">cancellation token.</param>
        /// <returns>http response message.</returns>
        [CustomExceptionFilter]
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity, CancellationToken cancellationToken)
        {
            // Get the current culture info to use in resource files
            if (activity.Locale != null)
            {
            }

            // This name is sent by MS Teams to indicate sign in. We do this so that we can pass handling to the right logic in the dialog. You can
            // set this to be whatever string you want.
            if (activity.Name == Constants.VerifyState)
            {
                activity.Text = Constants.SignInCompleteText;
            }

            if (activity.Type == ActivityTypes.Message || activity.Name == Constants.VerifyState)
            {
                if (activity.ChannelId == Constants.ActivityChannelId)
                {
                    JObject tenant = activity.ChannelData as JObject;
                    string tenantId = tenant["tenant"].SelectToken("id").ToString();
                    AppInsightsLogger.CustomEventTrace("Received Input Message for Bot", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", activity.From.Id }, { "methodName", "Post" }, { "Capability", "Bot" } });
                }

                MicrosoftAppCredentials.TrustServiceUrl($"{activity.ServiceUrl}", DateTime.MaxValue);

                // Adding typing indicator to incoming message
                var typingReply = activity.CreateReply();
                typingReply.Type = ActivityTypes.Typing;
                IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(typingReply), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                await factory.MakeConnectorClient().Conversations.ReplyToActivityAsync(typingReply);

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                {
                    var dialog = scope.Resolve<IDialog<object>>();
                    await Conversation.SendAsync(activity, () => dialog, cancellationToken);
                }
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                {
                    var dialog = scope.Resolve<IDialog<object>>();
                    MicrosoftAppCredentials.TrustServiceUrl($"{activity.ServiceUrl}", DateTime.MaxValue);
                    await Conversation.SendAsync(activity, () => dialog);
                }
            }
            else if (activity.Type == ActivityTypes.Invoke)
            {
                return await this.HandleInvokeMessagesAsync(activity, cancellationToken);
            }

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        private async Task<HttpResponseMessage> HandleInvokeMessagesAsync(Activity activity, CancellationToken cancellationToken)
        {
            var activityValue = activity.Value.ToString();

            if (activity.Name == "task/fetch")
            {
                JToken data = JObject.Parse(activityValue).First.First;
                var message = Convert.ToString(data.SelectToken("text"));
                TaskEnvelope taskEnvelope = new TaskEnvelope();
                TaskInfo taskInfo = new TaskInfo();

                // Employee time off requests - pending request approval task module
                if (message.ToLowerInvariant() == Constants.TorListOpenModal.ToLowerInvariant())
                {
                    var card = this.GetTORApprovalCard(data);
                    Attachment attachment = new Attachment()
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = card,
                    };

                    taskInfo.Card = attachment;
                    taskInfo.Height = "large";
                    taskInfo.Width = "medium";
                    taskInfo.Title = KronosResourceText.ManagerViewTOR_Title;
                    taskInfo.CompletionBotId = AppSettings.Instance.MicrosoftAppId;
                    taskEnvelope.Task = new TaskDetail()
                    {
                        Type = "continue",
                        TaskInfo = taskInfo,
                    };
                    return this.Request.CreateResponse(HttpStatusCode.OK, taskEnvelope);
                }
            }
            else if (activity.Name == "task/submit")
            {
                JToken data = JObject.Parse(activityValue).First.First;
                try
                {
                    var message = Convert.ToString(data.SelectToken("msteams").SelectToken("text"));
                    if (message.ToLowerInvariant() == Constants.TORApproveRequest.ToLowerInvariant() || message.ToLowerInvariant() == Constants.TORRefuseRequest.ToLowerInvariant())
                    {
                        var tempAct = activity;
                        tempAct.Text = message;
                        tempAct.Type = ActivityTypes.Message;
                        MicrosoftAppCredentials.TrustServiceUrl($"{activity.ServiceUrl}", DateTime.MaxValue);

                        using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, tempAct))
                        {
                            var dialog = scope.Resolve<IDialog<object>>();
                            await Conversation.SendAsync(tempAct, () => dialog, cancellationToken);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// Time off request approval card.
        /// </summary>
        /// <param name="token">Data posted from request list card.</param>
        /// <returns>Approval adaptive card.</returns>
        private AdaptiveCard GetTORApprovalCard(JToken token)
        {
            var queryDateSpan = Convert.ToString(token.SelectToken("QueryDateSpan"));
            var requestId = Convert.ToString(token.SelectToken("RequestId"));
            var personNumber = Convert.ToString(token.SelectToken("PersonNumber"));
            var empName = Convert.ToString(token.SelectToken("EmpName"));
            var index = Convert.ToString(token.SelectToken("Index"));
            var currentPage = Convert.ToString(token.SelectToken("CurrentPage"));
            var date = Convert.ToString(token.SelectToken("Date"));
            var paycode = Convert.ToString(token.SelectToken("PayCode"));
            var timePeriod = Convert.ToString(token.SelectToken("TimePeriod"));
            var note = Convert.ToString(token.SelectToken("Note"));
            var comm = token.SelectToken("Comments");
            var conversationId = Convert.ToString(token.SelectToken("conversationId"));
            var activityId = Convert.ToString(token.SelectToken("activityId"));
            var json = File.ReadAllText(HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/SupervisorViewTimeOffRequests/Latest/AcceptRefuse.json"));
            json = json.Replace("{txt_Status}", KronosResourceText.Status).Replace("{txt_Employee}", KronosResourceText.Employee);
            json = json.Replace("{txt_Paycode}", KronosResourceText.PayCode).Replace("{txt_Date}", KronosResourceText.Date).Replace("{txt_TimePeriod}", KronosResourceText.TimePeriod);
            json = json.Replace("{txt_Note}", KronosResourceText.Note).Replace("{SelectComment}", KronosResourceText.SelectComment).Replace("{EnterNote}", KronosResourceText.EnterNote);
            json = json.Replace("{txt_Refuse}", KronosResourceText.Refuse);
            json = json.Replace("{Date}", date).Replace("{EmpName}", empName).Replace("{PayCode}", paycode).Replace("{Status}", KronosResourceText.Submitted);
            json = json.Replace("{TimePeriod}", timePeriod).Replace("{Note}", string.IsNullOrEmpty(note) ? $"({KronosResourceText.None})" : note);
            json = json.Replace("{ApproveCommand}", Constants.TORApproveRequest).Replace("{RefuseCommand}", Constants.TORRefuseRequest).Replace("{CurrentPage}", currentPage);
            json = json.Replace("{txt_Approve}", KronosResourceText.Approve).Replace("{txt_Refuse}", KronosResourceText.Refuse);
            json = json.Replace("{EnterNote}", KronosResourceText.EnterNote);
            json = json.Replace("{QueryDateSpan}", queryDateSpan).Replace("{RequestId}", requestId).Replace("{PersonNumber}", personNumber).Replace("{EmpName}", empName);
            json = json.Replace("{index}", index).Replace("{CurrentPage}", currentPage).Replace("{conversationId}", conversationId).Replace("{activityId}", activityId);

            var row = "{\"title\": \"{Text}\",\"value\": \"{Value}\"}";
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var item in comm)
            {
                var comment = item.Value<string>("CommentText");
                if (i == 0)
                {
                    sb.Append(row.Replace("{Text}", comment).Replace("{Value}", comment));
                }
                else
                {
                    sb.Append(", " + row.Replace("{Text}", comment).Replace("{Value}", comment));
                }

                i += 1;
            }

            json = json.Replace("{CommentRows}", sb.ToString());

            return AdaptiveCard.FromJson(json).Card;
        }
    }
}

//-----------------------------------------------------------------------
// <copyright file="TimeOffDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Dialogs.TimeOff
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams.Models;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.CommentList;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Common;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Role;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Time off dialog class for creating and approving time off requests.
    /// </summary>
    [Serializable]
    public class TimeOffDialog : IDialog<object>
    {
        private readonly IAuthenticationService authenticationService;
        private readonly ITimeOffActivity timeOffActivity;
        private readonly ICommonActivity commonActivity;
        private readonly ICommentsActivity commentsActivity;
        private readonly IRoleActivity roleActivity;
        private readonly IAzureTableStorageHelper azureTableStorageHelper;
        private readonly LoginResponse response;
        private readonly TimeOffRequestCard timeOffCard;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeOffDialog"/> class.
        /// </summary>
        /// <param name="azureTableStorageHelper">Azure table storage.</param>
        /// <param name="commentsActivity">Comments activity.</param>
        /// <param name="roleActivity">Tole activity.</param>
        /// <param name="response">Login response.</param>
        /// <param name="authenticationService">Authentication service.</param>
        /// <param name="timeOffActivity">Timeoff activity.</param>
        /// <param name="timeOffCard">Timeoff card.</param>
        /// <param name="commonActivity">Common activty.</param>
        public TimeOffDialog(IAzureTableStorageHelper azureTableStorageHelper, ICommentsActivity commentsActivity, IRoleActivity roleActivity, LoginResponse response, IAuthenticationService authenticationService, ITimeOffActivity timeOffActivity, TimeOffRequestCard timeOffCard, ICommonActivity commonActivity)
        {
            this.timeOffCard = timeOffCard;
            this.response = response;
            this.authenticationService = authenticationService;
            this.timeOffActivity = timeOffActivity;
            this.commonActivity = commonActivity;
            this.roleActivity = roleActivity;
            this.commentsActivity = commentsActivity;
            this.azureTableStorageHelper = azureTableStorageHelper;
        }

        /// <summary>
        /// The StartAsync method calls IDialogContext.Wait with the continuation delegate to specify
        /// the method that should be called when a new message is received.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>Task.</returns>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.ProcessRequest);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Send approval notification to supervisor.
        /// </summary>
        /// <param name="tenantId">Tenand Id.</param>
        /// <param name="personNumber">Person number of employee who raised time off request.</param>
        /// <param name="reqId">Request Id generated for submitted request.</param>
        /// <param name="conversationId">Supervisor conversation Id to send notification.</param>
        /// <param name="paycode">Pay code for request.</param>
        /// <param name="context">Dialog context.</param>
        /// <param name="timePeriod">Time period for request.</param>
        /// <param name="advancedTimeOff">Advanced time off request object ('null' for normal time off request).</param>
        /// <returns>Task.</returns>
        public async Task SendNotificationToSupervisor(string tenantId, string personNumber, string reqId, string conversationId, string paycode, IDialogContext context, string timePeriod, AdvancedTimeOff advancedTimeOff)
        {
            var superUserLogonRes = await this.authenticationService.LoginSuperUser((Activity)context.Activity);
            if (superUserLogonRes?.Status == ApiConstants.Success)
            {
                context.UserData.SetValue(context.Activity.From.Id + Constants.SuperUser, superUserLogonRes.Jsession);
            }

            try
            {
                context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);
                var activity = context.Activity as Activity;
                JToken token = JObject.Parse(activity.Value.ToString());
                DateTime sdt, edt;
                string note = null;
                if (advancedTimeOff == null)
                {
                    DateTime.TryParse((string)token.SelectToken("sdt"), CultureInfo.InvariantCulture, DateTimeStyles.None, out sdt);
                    DateTime.TryParse((string)token.SelectToken("edt"), CultureInfo.InvariantCulture, DateTimeStyles.None, out edt);
                }
                else
                {
                    note = advancedTimeOff.Note;
                    DateTime.TryParse(advancedTimeOff.sdt, CultureInfo.InvariantCulture, DateTimeStyles.None, out sdt);
                    DateTime.TryParse(advancedTimeOff.edt, CultureInfo.InvariantCulture, DateTimeStyles.None, out edt);
                }
                edt = edt.AddDays(1);
                var requests = await this.timeOffActivity.GetTimeOffRequestDetails(tenantId, superSession, sdt.ToString("M/d/yyyy", CultureInfo.InvariantCulture) + "-" + edt.ToString("M/d/yyyy", CultureInfo.InvariantCulture), personNumber);
                var filteredRequest = (from d in requests.RequestMgmt.RequestItems.GlobalTimeOffRequestItem where d.Id == reqId select d).FirstOrDefault();

                var requestorInfo = await this.roleActivity.GetPersonInfo(tenantId, personNumber, superSession);

                for (int i = 0; i < filteredRequest.RequestStatusChanges.RequestStatusChange.Count; i++)
                {
                    var personInfo = await this.roleActivity.GetPersonInfo(tenantId, filteredRequest.RequestStatusChanges.RequestStatusChange[i].User.PersonIdentity.PersonNumber, superSession);

                    filteredRequest.RequestStatusChanges.RequestStatusChange[i].User.PersonIdentity.PersonNumber = personInfo.PersonInformation.PersonDat.PersonDetail.FullName;
                }

                var comments = await this.commentsActivity.GetComments(tenantId, superSession);

                var channelData = new TeamsChannelData
                {
                    Tenant = new TenantInfo()
                    {
                        Id = tenantId,
                    },
                };

                var message = Activity.CreateMessageActivity();
                message.From = new ChannelAccount(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword);

                MicrosoftAppCredentials.TrustServiceUrl(context.Activity.ServiceUrl);
                message.ChannelData = channelData;
                message.ChannelId = Constants.ActivityChannelId;
                message.Conversation = new ConversationAccount(
                    conversationType: "personal",
                    id: conversationId,
                    isGroup: false);

                AdaptiveCard card = this.timeOffCard.GetSupervisorNotificationCard(context, reqId, personNumber, paycode, timePeriod, filteredRequest, note, requestorInfo.PersonInformation.PersonDat.PersonDetail.FullName, comments, advancedTimeOff);

                message.Attachments.Add(new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = card,
                });

                IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                await factory.MakeConnectorClient().Conversations.SendToConversationAsync((Activity)message);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Send approval response to employee and approval confirmation card to supervisor.
        /// </summary>
        /// <param name="tenantId">Tenand Id.</param>
        /// <param name="context">Dialog context.</param>
        /// <param name="status">Request status.</param>
        /// <param name="conversationId">Employee conversation Id to send notification.</param>
        /// <param name="note">Note by supervisor.</param>
        /// <returns>Task.</returns>
        public async Task SendApprovalResponseNotiToEmployee(string tenantId, IDialogContext context, string status, string conversationId, string note)
        {
            var superUserLogonRes = await this.authenticationService.LoginSuperUser((Activity)context.Activity);
            if (superUserLogonRes?.Status == ApiConstants.Success)
            {
                context.UserData.SetValue(context.Activity.From.Id + Constants.SuperUser, superUserLogonRes.Jsession);
            }

            try
            {
                context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);
                var activity = context.Activity as Activity;
                JToken token = JObject.Parse(activity.Value.ToString());
                string personNumber = (string)token.SelectToken("PersonNumber");
                string queryDateSpan = (string)token.SelectToken("QueryDateSpan");
                string requestId = (string)token.SelectToken("RequestId");
                string jSessionId = this.response?.JsessionID;

                var jobAssgnDetails = await this.commonActivity.GetJobAssignment(personNumber, tenantId, superSession);
                var reviewer = jobAssgnDetails.JobAssign.jobAssignDetData.JobAssignDet.SupervisorName;

                var requests = await this.timeOffActivity.GetTimeOffRequestDetails(tenantId, superSession, queryDateSpan, personNumber);
                var filteredRequest = (from d in requests.RequestMgmt.RequestItems.GlobalTimeOffRequestItem where d.Id == requestId select d).FirstOrDefault();

                for (int i = 0; i < filteredRequest.RequestStatusChanges.RequestStatusChange.Count; i++)
                {
                    var personInfo = await this.roleActivity.GetPersonInfo(tenantId, filteredRequest.RequestStatusChanges.RequestStatusChange[i].User.PersonIdentity.PersonNumber, superSession);

                    filteredRequest.RequestStatusChanges.RequestStatusChange[i].User.PersonIdentity.PersonNumber = personInfo.PersonInformation.PersonDat.PersonDetail.FullName;
                }

                var channelData = new TeamsChannelData
                {
                    Tenant = new TenantInfo()
                    {
                        Id = tenantId,
                    },
                };

                var message = Activity.CreateMessageActivity();
                message.From = new ChannelAccount(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword);

                MicrosoftAppCredentials.TrustServiceUrl(context.Activity.ServiceUrl);
                message.ChannelData = channelData;
                message.ChannelId = Constants.ActivityChannelId;
                message.Conversation = new ConversationAccount(
                    conversationType: "personal",
                    id: conversationId,
                    isGroup: false);

                AdaptiveCard card = this.timeOffCard.GetEmployeeNotificationCard(context, status, reviewer, note, filteredRequest);

                message.Attachments.Add(new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = card,
                });

                IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                await factory.MakeConnectorClient().Conversations.SendToConversationAsync((Activity)message);

                var convId = context.Activity.Conversation.Id;
                var activityId = context.Activity.ReplyToId;
                card = this.timeOffCard.GetApprovalConfirmationCard(context, status, reviewer, note, filteredRequest);
                activity.Text = null;
                activity.Value = null;
                activity.Attachments.Add(new Attachment()
                {
                    Content = card,
                    ContentType = "application/vnd.microsoft.card.adaptive",
                });
                await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(convId, activityId, activity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// process the command and call respective method.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">Awaitable string.</param>
        /// <returns>Time off request.</returns>
        private async Task ProcessRequest(IDialogContext context, IAwaitable<string> result)
        {
            var activity = context.Activity as Activity;
            var message = activity.Text?.ToLowerInvariant().Trim();
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            AppInsightsLogger.CustomEventTrace("TimeOffDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ProcessRequest" }, { "Command", message } });

            if (message.ToLowerInvariant().Equals(Constants.SubmitTimeOff.ToLowerInvariant()))
            {
                await this.SubmitTimeOffRequest(context);
            }
            else if (message.ToLowerInvariant().Equals(Constants.CancelTimeOff.ToLowerInvariant()))
            {
                string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/CancelTimeOffRequest.json");
                var json = File.ReadAllText(fullPath);
                var card = AdaptiveCard.FromJson(json).Card;

                var conversationId = context.Activity.Conversation.Id;
                var activityId = context.Activity.ReplyToId;

                IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));

                try
                {
                    context.PrivateConversationData.RemoveValue("AdvancedRequestObj");
                    activity.Text = null;
                    activity.Value = null;
                    activity.Attachments.Add(new Attachment()
                    {
                        Content = card,
                        ContentType = "application/vnd.microsoft.card.adaptive",
                    });
                    await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (message.ToLowerInvariant() == Constants.SubmitAdvancedTimeOff.ToLowerInvariant())
            {
                await this.SubmitAdvancedTimeOffRequest(context);
            }
            else if (message.ToLowerInvariant() == Constants.ApproveTimeoff.ToLowerInvariant() || message.ToLowerInvariant() == Constants.RefuseTimeoff.ToLowerInvariant())
            {
                await this.SubmitTimeOffApproval(context, message);
            }
            else if (message.ToLowerInvariant().Contains("advanced"))
            {
                await this.ProcessAdvancedTimeOffRequest(context);
            }
            else
            {
                var allPaycodes = await this.azureTableStorageHelper.GetTimeOffPaycodes(AppSettings.Instance.OvertimeMappingtableName);
                var filteredPaycodes = (from d in allPaycodes where d.RowKey.Contains(tenantId) select d).Select(w => w.Properties["PayCodeName"].StringValue).ToList();
                IMessageActivity reply = this.timeOffCard.GetBasicTimeOffRequestCard(context, filteredPaycodes);
                if (reply.Attachments.Count > 0)
                {
                    await context.PostAsync(reply);
                }
                else
                {
                    await context.PostAsync(Resources.KronosResourceText.UnableToProceed);
                }
            }

            context.Done(default(string));
        }

        /// <summary>
        /// Process advanced time off request commands and call respective methods.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>Task.</returns>
        private async Task ProcessAdvancedTimeOffRequest(IDialogContext context)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            AdaptiveCard card = new AdaptiveCard("1.0");
            var conversationId = context.Activity.Conversation.Id;
            var activityId = context.Activity.ReplyToId;
            var advancedObj = new AdvancedTimeOff();
            IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));

            if (activity.Text.ToLowerInvariant() == Constants.CreateAdvancedTimeOff.ToLowerInvariant())
            {
                if (context.PrivateConversationData.ContainsKey("AdvancedRequestObj"))
                {
                    advancedObj = context.PrivateConversationData.GetValue<AdvancedTimeOff>("AdvancedRequestObj");
                }

                card = this.timeOffCard.GetAdvancedTimeOffRequestCard(context, advancedObj);
            }
            else if (activity.Text.ToLowerInvariant() == Constants.AdvancedNext2.ToLowerInvariant())
            {
                if (context.PrivateConversationData.ContainsKey("AdvancedRequestObj"))
                {
                    advancedObj = context.PrivateConversationData.GetValue<AdvancedTimeOff>("AdvancedRequestObj");
                }

                advancedObj.sdt = (string)token.SelectToken("sdt");
                advancedObj.edt = (string)token.SelectToken("edt");
                context.PrivateConversationData.SetValue("AdvancedRequestObj", advancedObj);
                card = this.timeOffCard.OnNextGetDurationCard(context, advancedObj);
            }
            else if (activity.Text.ToLowerInvariant() == Constants.AdvancedNext3.ToLowerInvariant())
            {
                advancedObj = context.PrivateConversationData.GetValue<AdvancedTimeOff>("AdvancedRequestObj");
                advancedObj.duration = (string)token.SelectToken("duration");

                if (advancedObj.duration.ToLowerInvariant() == Constants.Hours.ToLowerInvariant())
                {
                    card = this.timeOffCard.OnNextGetHoursCard(context, advancedObj);
                }
                else
                {
                    advancedObj.StartTime = null;
                    advancedObj.EndTime = null;
                    var allPaycodes = await this.azureTableStorageHelper.GetTimeOffPaycodes(AppSettings.Instance.OvertimeMappingtableName);
                    var filteredPaycodes = (from d in allPaycodes where d.RowKey.Contains(tenantId) select d).Select(w => w.Properties["PayCodeName"].StringValue).ToList();
                    card = this.timeOffCard.OnNextGetDeductFromCard(context, advancedObj, filteredPaycodes);
                }

                context.PrivateConversationData.SetValue("AdvancedRequestObj", advancedObj);
            }
            else if (activity.Text.ToLowerInvariant() == Constants.AdvancedNext3FromHours.ToLowerInvariant())
            {
                advancedObj = context.PrivateConversationData.GetValue<AdvancedTimeOff>("AdvancedRequestObj");
                advancedObj.StartTime = (string)token.SelectToken("StartTime");
                advancedObj.EndTime = (string)token.SelectToken("EndTime");
                context.PrivateConversationData.SetValue("AdvancedRequestObj", advancedObj);
                var allPaycodes = await this.azureTableStorageHelper.GetTimeOffPaycodes(AppSettings.Instance.OvertimeMappingtableName);
                var filteredPaycodes = (from d in allPaycodes where d.RowKey.Contains(tenantId) select d).Select(w => w.Properties["PayCodeName"].StringValue).ToList();
                card = this.timeOffCard.OnNextGetDeductFromCard(context, advancedObj, filteredPaycodes);
            }
            else if (activity.Text.ToLowerInvariant() == Constants.AdvancedNext4.ToLowerInvariant())
            {
                var superUserLogonRes = await this.authenticationService.LoginSuperUser((Activity)context.Activity);
                if (superUserLogonRes?.Status == ApiConstants.Success)
                {
                    context.UserData.SetValue(context.Activity.From.Id + Constants.SuperUser, superUserLogonRes.Jsession);
                }

                advancedObj = context.PrivateConversationData.GetValue<AdvancedTimeOff>("AdvancedRequestObj");
                advancedObj.DeductFrom = (string)token.SelectToken("DeductFrom");

                context.PrivateConversationData.SetValue("AdvancedRequestObj", advancedObj);
                context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);
                var comments = await this.commentsActivity.GetComments(tenantId, superSession);

                card = this.timeOffCard.OnNextGetConfirmationCard(context, advancedObj, comments);
            }
            else if (activity.Text.ToLowerInvariant() == Constants.AdvancedBack3ToHours.ToLowerInvariant())
            {
                advancedObj = context.PrivateConversationData.GetValue<AdvancedTimeOff>("AdvancedRequestObj");
                advancedObj.DeductFrom = (string)token.SelectToken("DeductFrom");
                card = this.timeOffCard.OnBackGetHoursCard(context, advancedObj);
            }
            else if (activity.Text.ToLowerInvariant() == Constants.AdvancedBack3.ToLowerInvariant())
            {
                advancedObj = context.PrivateConversationData.GetValue<AdvancedTimeOff>("AdvancedRequestObj");
                context.PrivateConversationData.SetValue("AdvancedRequestObj", advancedObj);
                var allPaycodes = await this.azureTableStorageHelper.GetTimeOffPaycodes(AppSettings.Instance.OvertimeMappingtableName);
                var filteredPaycodes = (from d in allPaycodes where d.RowKey.Contains(tenantId) select d).Select(w => w.Properties["PayCodeName"].StringValue).ToList();
                card = this.timeOffCard.OnBackGetDeductFromCard(context, advancedObj, filteredPaycodes);
            }
            else if (activity.Text.ToLowerInvariant() == Constants.AdvancedBack2.ToLowerInvariant())
            {
                advancedObj = context.PrivateConversationData.GetValue<AdvancedTimeOff>("AdvancedRequestObj");
                if (advancedObj.duration.ToLowerInvariant() == Constants.Hours.ToLowerInvariant())
                {
                    advancedObj.StartTime = (string)token.SelectToken("StartTime");
                    advancedObj.EndTime = (string)token.SelectToken("EndTime");
                }
                else
                {
                    advancedObj.DeductFrom = (string)token.SelectToken("DeductFrom");
                }

                context.PrivateConversationData.SetValue("AdvancedRequestObj", advancedObj);
                card = this.timeOffCard.OnBackGetDurationCard(context, advancedObj);
            }
            else if (activity.Text.ToLowerInvariant() == Constants.AdvancedBack1.ToLowerInvariant())
            {
                advancedObj = context.PrivateConversationData.GetValue<AdvancedTimeOff>("AdvancedRequestObj");
                advancedObj.duration = (string)token.SelectToken("duration");
                context.PrivateConversationData.SetValue("AdvancedRequestObj", advancedObj);
                card = this.timeOffCard.OnBackGetDateRangeCard(context, advancedObj);
            }
            else
            {
                await context.PostAsync(Constants.CommandNotRecognized);
            }

            try
            {
                activity.Text = null;
                activity.Value = null;
                activity.Attachments.Add(new Attachment()
                {
                    Content = card,
                    ContentType = "application/vnd.microsoft.card.adaptive",
                });
                await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Create time off request draft and submit it.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>Task.</returns>
        private async Task SubmitTimeOffRequest(IDialogContext context)
        {
            var superUserLogonRes = await this.authenticationService.LoginSuperUser((Activity)context.Activity);
            if (superUserLogonRes?.Status == ApiConstants.Success)
            {
                context.UserData.SetValue(context.Activity.From.Id + Constants.SuperUser, superUserLogonRes.Jsession);
            }

            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();

            DateTime.TryParse((string)token.SelectToken("sdt"), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sdt);
            DateTime.TryParse((string)token.SelectToken("edt"), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime edt);
            string querySpan = sdt.ToString("M/d/yyyy", CultureInfo.InvariantCulture) + "-" + edt.ToString("M/d/yyyy", CultureInfo.InvariantCulture);
            edt = edt.AddHours(23);
            edt = edt.AddMinutes(59);
            var startDate = sdt.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            var endDate = edt.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);

            string jSessionId = this.response?.JsessionID;
            string personNumber = this.response?.PersonNumber;
            context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);
            Models.ResponseEntities.TimeOff.AddResponse.Response timeOffResponse = await this.timeOffActivity.TimeOffRequest(tenantId, jSessionId, startDate, endDate, personNumber, (string)token.SelectToken("reason"));
            if (timeOffResponse?.Status == ApiConstants.Failure)
            {
                if (timeOffResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }
            }

            if (timeOffResponse?.Status == ApiConstants.Success)
            {
                Models.ResponseEntities.TimeOff.SubmitResponse.Response submitTimeOffResponse = await this.timeOffActivity.SubmitTimeOffRequest(tenantId, jSessionId, personNumber, timeOffResponse.EmployeeRequestMgm.RequestItem.GlobalTimeOffRequestItms.FirstOrDefault().Id, querySpan);

                if (submitTimeOffResponse?.Status == ApiConstants.Failure)
                {
                    if (submitTimeOffResponse?.Error.DetailErrors.Error.FirstOrDefault().ErrorCode == "611")
                    {
                        await context.PostAsync(Resources.KronosResourceText.TimeOffRequestOverlap);
                    }
                    else
                    {
                        await context.PostAsync(Resources.KronosResourceText.UnableToProceed);
                    }
                }

                if (submitTimeOffResponse?.Status == ApiConstants.Success)
                {
                    var conversationId = context.Activity.Conversation.Id;
                    var activityId = context.Activity.ReplyToId;

                    IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                    var card = this.timeOffCard.ShowTimeOffSuccessCard(context, timeOffResponse, null);

                    var supervisorConversationId = await this.commonActivity.GetConversationId(personNumber, tenantId, superSession, activity.ChannelId);
                    if (supervisorConversationId != null)
                        {
                        await this.SendNotificationToSupervisor(tenantId, personNumber, timeOffResponse.EmployeeRequestMgm.RequestItem.GlobalTimeOffRequestItms.FirstOrDefault().Id, supervisorConversationId, (string)token.SelectToken("reason"), context, "Full Day", null);
                    }

                    try
                    {
                        activity.Text = null;
                        activity.Value = null;
                        activity.Attachments.Add(new Attachment()
                        {
                            Content = card,
                            ContentType = "application/vnd.microsoft.card.adaptive",
                        });
                        await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
                        if (supervisorConversationId == null)
                        {
                            await context.PostAsync(Resources.KronosResourceText.SupervisorNotFoundForNotification);
                        }
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    finally
                    {
                        context.PrivateConversationData.RemoveValue("AdvancedRequestObj");
                    }
                }
            }

            context.Done(default(string));
        }

        /// <summary>
        /// Create time off request draft and submit it.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>Task.</returns>
        private async Task SubmitAdvancedTimeOffRequest(IDialogContext context)
        {
            var superUserLogonRes = await this.authenticationService.LoginSuperUser((Activity)context.Activity);
            if (superUserLogonRes?.Status == ApiConstants.Success)
            {
                context.UserData.SetValue(context.Activity.From.Id + Constants.SuperUser, superUserLogonRes.Jsession);
            }

            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();

            var advancedObj = context.PrivateConversationData.GetValue<AdvancedTimeOff>("AdvancedRequestObj");
            advancedObj.Comment = (string)token.SelectToken("Comment");
            advancedObj.Note = (string)token.SelectToken("note");

            if (!string.IsNullOrEmpty(advancedObj.Note) && string.IsNullOrEmpty(advancedObj.Comment))
            {
                await context.PostAsync(Resources.KronosResourceText.CommentSelectionValidation);
            }
            else
            {
                context.PrivateConversationData.SetValue("AdvancedRequestObj", advancedObj);
                DateTime.TryParse(advancedObj.sdt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sdt);
                DateTime.TryParse(advancedObj.edt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime edt);

                edt = edt.AddHours(23);
                edt = edt.AddMinutes(59);
                string querySpan = sdt.ToString("M/d/yyyy", CultureInfo.InvariantCulture) + "-" + edt.ToString("M/d/yyyy", CultureInfo.InvariantCulture);

                var startDate = sdt.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                var endDate = edt.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);

                advancedObj.sdt = startDate;
                advancedObj.edt = endDate;

                string jSessionId = this.response?.JsessionID;
                string personNumber = this.response?.PersonNumber;
                context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);

                Models.ResponseEntities.TimeOff.AddResponse.Response timeOffResponse = await this.timeOffActivity.AdvancedTimeOffRequest(tenantId, jSessionId, personNumber, advancedObj);
                if (timeOffResponse?.Status == ApiConstants.Failure)
                {
                    if (timeOffResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                    {
                        await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                    }
                    else
                    {
                        await context.PostAsync(Resources.KronosResourceText.UnableToProceed);
                    }
                }

                if (timeOffResponse?.Status == ApiConstants.Success)
                {
                    Models.ResponseEntities.TimeOff.SubmitResponse.Response submitTimeOffResponse = await this.timeOffActivity.SubmitTimeOffRequest(tenantId, jSessionId, personNumber, timeOffResponse.EmployeeRequestMgm.RequestItem.GlobalTimeOffRequestItms.FirstOrDefault().Id, querySpan);

                    if (submitTimeOffResponse?.Status == ApiConstants.Failure)
                    {
                        if (submitTimeOffResponse.Error.DetailErrors.Error.FirstOrDefault().ErrorCode == "611")
                        {
                            await context.PostAsync(Resources.KronosResourceText.TimeOffRequestOverlap);
                        }
                        else
                        {
                            await context.PostAsync(Resources.KronosResourceText.UnableToProceed);
                        }
                    }

                    if (submitTimeOffResponse?.Status == ApiConstants.Success)
                    {
                        var conversationId = context.Activity.Conversation.Id;
                        var activityId = context.Activity.ReplyToId;

                        IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                        var card = this.timeOffCard.ShowTimeOffSuccessCard(context, timeOffResponse, advancedObj);
                        var supervisorConversationId = await this.commonActivity.GetConversationId(personNumber, tenantId, superSession, activity.ChannelId);
                        var timePeriod = advancedObj.duration.ToLowerInvariant() == Constants.FullDay.ToLowerInvariant() ? Constants.FullDay : advancedObj.duration.ToLowerInvariant() == Constants.HalfDay.ToLowerInvariant() ? Constants.HalfDay : advancedObj.duration.ToLowerInvariant() == Constants.FirstHalfDay.ToLowerInvariant() ? Constants.FirstHalfDay : Constants.Hours;
                        if (advancedObj.duration.ToLowerInvariant() == Constants.Hours.ToLowerInvariant())
                        {
                            var shr = Convert.ToInt32(advancedObj.StartTime.Split(' ')[0].Split(':')[0]);
                            var smin = Convert.ToInt32(advancedObj.StartTime.Split(' ')[0].Split(':')[1]);
                            var ehr = Convert.ToInt32(advancedObj.EndTime.Split(' ')[0].Split(':')[0]);
                            var emin = Convert.ToInt32(advancedObj.EndTime.Split(' ')[0].Split(':')[1]);
                            var stime = new DateTime(2000, 1, 1, shr, smin, 0);
                            var etime = new DateTime(2000, 1, 1, ehr, emin, 0);
                            timePeriod += " (" + stime.ToString("h:mm tt", CultureInfo.InvariantCulture) + " to " + etime.ToString("h:mm tt", CultureInfo.InvariantCulture) + ")";
                        }

                        var period = timeOffResponse.EmployeeRequestMgm.RequestItem.GlobalTimeOffRequestItms.FirstOrDefault().TimeOffPeriodsList.TimeOffPerd.FirstOrDefault();
                        var duration = period.Duration == Constants.full_day ? Constants.FullDay : period.Duration == Constants.half_day ? Constants.HalfDay : period.Duration == Constants.first_half_day ? Constants.FirstHalfDay : Constants.Hours;
                        if (supervisorConversationId != null)
                        {
                            await this.SendNotificationToSupervisor(tenantId, personNumber, timeOffResponse.EmployeeRequestMgm.RequestItem.GlobalTimeOffRequestItms.FirstOrDefault().Id, supervisorConversationId, advancedObj.DeductFrom, context, duration, advancedObj);
                        }

                        try
                        {
                            activity.Text = null;
                            activity.Value = null;
                            activity.Attachments.Add(new Attachment()
                            {
                                Content = card,
                                ContentType = "application/vnd.microsoft.card.adaptive",
                            });
                            await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
                            if (supervisorConversationId == null)
                            {
                                await context.PostAsync(Resources.KronosResourceText.SupervisorNotFoundForNotification);
                            }

                            context.PrivateConversationData.RemoveValue("AdvancedRequestObj");
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                }
            }

            context.Done(default(string));
        }

        /// <summary>
        /// Process time off request approval (Accept/Refuse) sent from supervisor.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="command">Command sent to bot.</param>
        /// <returns>Task.</returns>
        private async Task SubmitTimeOffApproval(IDialogContext context, string command)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();

            string jSessionId = this.response?.JsessionID;
            string personNumber = (string)token.SelectToken("PersonNumber");
            string querySpan = (string)token.SelectToken("QueryDateSpan");
            string reqId = (string)token.SelectToken("RequestId");
            string payCode = (string)token.SelectToken("PayCode");
            string note = (string)token.SelectToken("note");
            string comment = (string)token.SelectToken("Comment");

            if (!string.IsNullOrEmpty(note) && string.IsNullOrEmpty(comment))
            {
                await context.PostAsync(Resources.KronosResourceText.CommentSelectionValidation);
            }
            else
            {
                Models.ResponseEntities.TimeOff.SubmitResponse.Response response = await this.timeOffActivity.SubmitApproval(tenantId, jSessionId, reqId, personNumber, command, querySpan, comment, note);
                if (response?.Status == ApiConstants.Failure)
                {
                    if (response.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                    {
                        await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                    }
                    else
                    {
                        await context.PostAsync(Resources.KronosResourceText.UnableToProceed);
                    }
                }

                if (response?.Status == ApiConstants.Success)
                {
                    var empConversationId = await this.commonActivity.GetEmpConversationId(personNumber, tenantId, jSessionId, activity.ChannelId);

                    var status = command == Constants.ApproveTimeoff ? Resources.KronosResourceText.Approved : Resources.KronosResourceText.Refused;

                    if (empConversationId == null)
                    {
                        await context.PostAsync(Resources.KronosResourceText.EmpNotFoundForNotification);
                    }
                    else
                    {
                        await this.SendApprovalResponseNotiToEmployee(tenantId, context, status, empConversationId, note);
                    }

                    var reply = Resources.KronosResourceText.TimeOffApprovalReply;
                    reply = reply.Replace("{status}", status);
                    reply = reply.Replace("{paycode}", payCode);
                    await context.PostAsync(reply);
                }
            }

            context.Done(default(string));
        }
    }
}
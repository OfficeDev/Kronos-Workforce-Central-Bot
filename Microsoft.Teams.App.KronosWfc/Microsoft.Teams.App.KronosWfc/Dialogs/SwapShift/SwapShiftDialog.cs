//-----------------------------------------------------------------------
// <copyright file="SwapShiftDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Dialogs.SwapShift
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams.Models;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.CommentList;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Common;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Core;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.HyperFind;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Shifts;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.SwapShift;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.SwapShift;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Schedule;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UpcomingShift = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Shifts.UpcomingShifts;

    /// <summary>
    /// Dialog is used for swap shift process.
    /// </summary>
    [Serializable]
    public class SwapShiftDialog : IDialog<object>
    {
        private readonly IScheduleActivity scheduleActivity;
        private readonly ISwapShiftActivity swapShiftActivity;
        private readonly IAuthenticationService authenticationService;
        private readonly ICommentsActivity commentsActivity;
        private readonly ICommonActivity commonActivity;
        private readonly SwapShiftCard swapShiftCard;
        private readonly IUpcomingShiftsActivity upcomingShiftsActivity;
        private readonly IHyperFindActivity hyperFindActivity;
        private LoginResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwapShiftDialog"/> class.
        /// </summary>
        /// <param name="commentsActivity">Comments activity.</param>
        /// <param name="scheduleActivity">Schedule activity.</param>
        /// <param name="authenticationService">Authentication service.</param>
        /// <param name="swapShiftActivity">Swap shift activity.</param>
        /// <param name="commonActivity">Common activity.</param>
        /// <param name="response">Login response.</param>
        /// <param name="card">Swap shift card.</param>
        /// <param name="upcomingShiftsActivity">Upcoming shifts activity.</param>
        /// <param name="hyperFindActivity">Hyperfind activity.</param>
        public SwapShiftDialog(ICommentsActivity commentsActivity, IScheduleActivity scheduleActivity, IAuthenticationService authenticationService, ISwapShiftActivity swapShiftActivity, ICommonActivity commonActivity, LoginResponse response, SwapShiftCard card, IUpcomingShiftsActivity upcomingShiftsActivity, IHyperFindActivity hyperFindActivity)
        {
            this.scheduleActivity = scheduleActivity;
            this.authenticationService = authenticationService;
            this.response = response;
            this.swapShiftActivity = swapShiftActivity;
            this.swapShiftCard = card;
            this.commonActivity = commonActivity;
            this.upcomingShiftsActivity = upcomingShiftsActivity;
            this.hyperFindActivity = hyperFindActivity;
            this.commentsActivity = commentsActivity;
        }

        /// <inheritdoc/>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.ProcessCommand);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Send approval notifications.
        /// </summary>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="jsession">J Session.</param>
        /// <param name="context">Dialog context.</param>
        /// <param name="conversationId">Conversation Id to which notification has to be sent.</param>
        /// <param name="note">Note entered.</param>
        /// <param name="obj">Swap shift object.</param>
        /// <param name="approvalType">Approval type: 1-Employee, 2-Supervisor.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SendApprovalNotification(string tenantId, string jsession, IDialogContext context, string conversationId, string note, SwapShiftObj obj, int approvalType)
        {
            try
            {
                var activity = context.Activity as Activity;
                JToken token = JObject.Parse(activity.Value.ToString());
                context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);
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
                var comments = await this.commentsActivity.GetComments(tenantId, superSession);
                AdaptiveCard card = this.swapShiftCard.GetNotificationCard(context, obj, note, comments, approvalType);

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
                await context.PostAsync("Something went wrong while sending notification");
                throw;
            }
        }

        /// <summary>
        /// Send approval notification to employee after approval/rejection.
        /// </summary>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="jsession">J Session.</param>
        /// <param name="context">Dialog context.</param>
        /// <param name="status">Status: 1.Approved, 2.Refused.</param>
        /// <param name="conversationId">Employee conversation Id.</param>
        /// <param name="note">Note submitted.</param>
        /// <param name="obj">Swap shift object.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SendApprovalResponseNotiToEmployee(string tenantId, string jsession, IDialogContext context, string status, string conversationId, string note, SwapShiftObj obj)
        {
            try
            {
                var activity = context.Activity as Activity;
                JToken token = JObject.Parse(activity.Value.ToString());
                var details = await this.commonActivity.GetPersonInformation(tenantId, jsession, obj.RequestorPersonNumber);

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

                AdaptiveCard card = this.swapShiftCard.GetPostApprovalCard(context, obj, note, status);

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
                await context.PostAsync("Something went wrong while sending notification");
                throw;
            }
        }

        /// <summary>
        /// Confirm swap shift submit.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="obj">Swap shift object.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SwapShiftConfirmation(IDialogContext context, SwapShiftObj obj)
        {
            try
            {
                var activity = context.Activity as Activity;
                JToken token = JObject.Parse(activity.Value.ToString());
                JObject tenant = context.Activity.ChannelData as JObject;
                string tenantId = tenant["tenant"].SelectToken("id").ToString();

                AdaptiveCard card = this.swapShiftCard.GetSwapShiftCnfCard(context, obj);

                IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                var conversationId = context.Activity.Conversation.Id;
                var activityId = context.Activity.ReplyToId;

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

        private async Task ProcessCommand(IDialogContext context, IAwaitable<string> result)
        {
            var activity = context.Activity as Activity;
           // var message = await result;
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant?["tenant"].SelectToken("id").ToString();
            string resultString = await result;
            string message = JsonConvert.DeserializeObject<Message>(resultString).message;
            var luisResult = JsonConvert.DeserializeObject<Message>(resultString).luisResult;
            var luisMessage = JsonConvert.SerializeObject(new Message { luisResult = luisResult });
            AppInsightsLogger.CustomEventTrace("SwapShiftDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ProcessCommand" }, { "Command", message } });

            if (message == Constants.SwapShift)
            {
                await this.ShowShiftSelectionCard(context, Constants.SwapShift);
            }
            else if (message == Constants.SwapShiftNext2)
            {
                JToken token = JObject.Parse(activity.Value.ToString());
                var shift = Convert.ToString(token.SelectToken("SelectedShift"));
                var splitted = shift.Split('-');
                SwapShiftObj obj = new SwapShiftObj();
                DateTime shiftStart;
                DateTime shiftEnd;
                string startTime;
                string endTime;
                if (splitted[2].Length == 6)
                {
                    startTime = splitted[2].Insert(4, " ");
                }
                else
                {
                    startTime = splitted[2].Insert(5, " ");
                }

                if (splitted[3].Length == 6)
                {
                    endTime = splitted[3].Insert(4, " ");
                }
                else
                {
                    endTime = splitted[3].Insert(5, " ");
                }

                DateTime.TryParse(splitted[0] + " " + startTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out shiftStart);
                DateTime.TryParse(splitted[1] + " " + endTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out shiftEnd);

                obj.Emp1FromDateTime = shiftStart;
                obj.Emp1ToDateTime = shiftEnd;
                obj.RequestorPersonNumber = splitted[4];
                obj.RequestorName = splitted[5];
                obj.SelectedShiftToSwap = shift;

                context.PrivateConversationData.SetValue("SwapShiftObj", obj);
                await this.ShowSearchFilterCard(context, Constants.SwapShiftNext2);
            }
            else if (message == Constants.SwapShiftNext3)
            {
                JToken token = JObject.Parse(activity.Value.ToString());
                var selectedLocation = Convert.ToString(token.SelectToken("SelectedLocation"));
                var selectedJob = Convert.ToString(token.SelectToken("SelectedJob"));
                var selectedEmployee = Convert.ToString(token.SelectToken("SelectedEmployee"));
                var obj = context.PrivateConversationData.GetValue<SwapShiftObj>("SwapShiftObj");
                obj.SelectedEmployee = selectedEmployee;
                obj.SelectedLocation = selectedLocation;
                obj.SelectedJob = selectedJob;

                context.PrivateConversationData.SetValue("SwapShiftObj", obj);

                await this.ShowAvailableShiftsCard(context, Constants.SwapShiftNext3);
            }
            else if (message == Constants.SwapShiftBack1)
            {
                JToken token = JObject.Parse(activity.Value.ToString());
                var selectedLocation1 = Convert.ToString(token.SelectToken("SelectedLocation"));
                var selectedJob1 = Convert.ToString(token.SelectToken("SelectedJob"));
                var selectedEmployee1 = Convert.ToString(token.SelectToken("SelectedEmployee"));
                var obj = context.PrivateConversationData.GetValue<SwapShiftObj>("SwapShiftObj");
                obj.SelectedEmployee = selectedEmployee1;
                obj.SelectedLocation = selectedLocation1;
                obj.SelectedJob = selectedJob1;

                context.PrivateConversationData.SetValue("SwapShiftObj", obj);
                await this.ShowShiftSelectionCard(context, Constants.SwapShiftBack1);
            }
            else if (message == Constants.SwapShiftBack2)
            {
                JToken token = JObject.Parse(activity.Value.ToString());
                var selectedShiftToSwap = Convert.ToString(token.SelectToken("Choice1"));
                var obj = context.PrivateConversationData.GetValue<SwapShiftObj>("SwapShiftObj");
                obj.SelectedShiftToSwap = selectedShiftToSwap;

                context.PrivateConversationData.SetValue("SwapShiftObj", obj);
                await this.ShowSearchFilterCard(context, Constants.SwapShiftBack2);
            }
            else if (message == Constants.SwapShiftBack3)
            {
                await this.ShowAvailableShiftsCard(context, Constants.SwapShiftBack3);
            }
            else if (message == Constants.SwapShiftNext4)
            {
                JToken token = JObject.Parse(activity.Value.ToString());
                var selectedShiftToSwap = Convert.ToString(token.SelectToken("SelectedShift"));
                var obj = context.PrivateConversationData.GetValue<SwapShiftObj>("SwapShiftObj");
                obj.SelectedShiftToSwap = selectedShiftToSwap;
                var splitted = selectedShiftToSwap.Split('-');
                DateTime shiftStart1;
                DateTime shiftEnd1;
                string startTime1;
                string endTime1;
                if (splitted[2].Length == 6)
                {
                    startTime1 = splitted[2].Insert(4, " ");
                }
                else
                {
                    startTime1 = splitted[2].Insert(5, " ");
                }

                if (splitted[3].Length == 6)
                {
                    endTime1 = splitted[3].Insert(4, " ");
                }
                else
                {
                    endTime1 = splitted[3].Insert(5, " ");
                }

                DateTime.TryParse(splitted[0] + " " + startTime1, CultureInfo.InvariantCulture, DateTimeStyles.None, out shiftStart1);
                DateTime.TryParse(splitted[1] + " " + endTime1, CultureInfo.InvariantCulture, DateTimeStyles.None, out shiftEnd1);

                obj.Emp2FromDateTime = shiftStart1;
                obj.Emp2ToDateTime = shiftEnd1;
                obj.RequestedToPersonNumber = splitted[4];
                obj.RequestedToName = splitted[5];
                context.PrivateConversationData.SetValue("SwapShiftObj", obj);
                await this.SwapShiftConfirmation(context, obj);
            }
            else if (message == Constants.CancelSwapShift)
            {
                var conversationId = context.Activity.Conversation.Id;
                var activityId = context.Activity.ReplyToId;
                AdaptiveCard card = new AdaptiveCard("1.0")
                {
                    Body = new List<AdaptiveElement>
                        {
                            new AdaptiveContainer
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = KronosResourceText.CancelSwapShift,
                                        Wrap = true,
                                    },
                                },
                            },
                        },
                };
                IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                context.PrivateConversationData.RemoveValue("SwapShiftObj");
                activity.Text = null;
                activity.Value = null;
                activity.Attachments.Add(new Attachment()
                {
                    Content = card,
                    ContentType = "application/vnd.microsoft.card.adaptive",
                });
                await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
            }
            else if (message == Constants.SubmitSwapShift)
            {
                await this.SubmitSwapShiftRequest(context);
            }
            else if (message.Contains("approve") || message.Contains("refuse"))
            {
                await this.SubmitApproval(context, message);
            }
            else
            {
                await this.ShowShiftSelectionCard(context, Constants.SwapShift, luisResult);
            }

            context.Done(default(string));
        }

        private async Task ShowSearchFilterCard(IDialogContext context, string command)
        {
            var activity = context.Activity as Activity;
            string jSession = string.Empty;

            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            string personNumber = string.Empty;

            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                personNumber = this.response.PersonNumber;
                jSession = this.response.JsessionID;
            }

            var obj = context.PrivateConversationData.GetValue<SwapShiftObj>("SwapShiftObj");

            context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);

            var allJob = await this.swapShiftActivity.LoadAllJobs(tenantId, superSession, personNumber, obj.Emp1FromDateTime.ToString("M/dd/yyyy", CultureInfo.InvariantCulture), obj.Emp1FromDateTime.ToString("M/dd/yyyy", CultureInfo.InvariantCulture), obj.Emp1FromDateTime.ToString("hh:mmtt", CultureInfo.InvariantCulture), obj.Emp1ToDateTime.ToString("hh:mmtt", CultureInfo.InvariantCulture));

            Models.ResponseEntities.SwapShift.LoadEligibleEmployees.Response loadEligibleEmps;

            loadEligibleEmps = await this.swapShiftActivity.LoadEligibleEmployees(tenantId, superSession, personNumber, obj.Emp1FromDateTime.ToString("M/dd/yyyy", CultureInfo.InvariantCulture), obj.Emp1FromDateTime.ToString("hh:mmtt", CultureInfo.InvariantCulture), obj.Emp1ToDateTime.ToString("hh:mmtt", CultureInfo.InvariantCulture));

            var card = this.swapShiftCard.GetFilterCard(context, allJob, loadEligibleEmps);

            IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
            var conversationId = context.Activity.Conversation.Id;
            var activityId = context.Activity.ReplyToId;

            activity.Text = null;
            activity.Value = null;
            activity.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
            });
            await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);

            context.Done(default(string));
        }

        private async Task ShowAvailableShiftsCard(IDialogContext context, string command)
        {
            var activity = context.Activity as Activity;
            string jSession = string.Empty;
            JToken token = JObject.Parse(activity.Value.ToString());
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            string personNumber = string.Empty;

            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                personNumber = this.response.PersonNumber;
                jSession = this.response.JsessionID;
            }

            var obj = context.PrivateConversationData.GetValue<SwapShiftObj>("SwapShiftObj");

            IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
            var conversationId = context.Activity.Conversation.Id;
            var activityId = context.Activity.ReplyToId;

            List<ScheduleShift> scheduleShifts = new List<ScheduleShift>();
            List<Models.ResponseEntities.HyperFind.ResponseHyperFindResult> personIdentities = this.CreatePersonIdentities(obj.SelectedEmployee);

            context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);
            UpcomingShift.Response scheduleResponse = await this.upcomingShiftsActivity.ShowUpcomingShifts(tenantId, superSession, obj.Emp1FromDateTime.ToString("M/d/yyyy", CultureInfo.InvariantCulture), obj.Emp1ToDateTime.ToString("M/d/yyyy", CultureInfo.InvariantCulture), string.Empty, personIdentities);
            if (scheduleResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
            {
                await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
            }
            else
            {
                try
                {
                    var workingdaySchedule = scheduleResponse.Schedule.ScheduleItems.ScheduleShift;
                    var listOfEmps = await this.hyperFindActivity.GetHyperFindQueryValues(tenantId, superSession, obj.Emp1FromDateTime.ToString("M/d/yyyy", CultureInfo.InvariantCulture), obj.Emp1ToDateTime.ToString("M/d/yyyy", CultureInfo.InvariantCulture), ApiConstants.ReportsToHyperFindQuery, ApiConstants.PersonalVisibilityCode);
                    var card = this.swapShiftCard.GetAvailableShiftsCard(context, workingdaySchedule.ToList(), listOfEmps.HyperFindResult);
                    activity.Text = null;
                    activity.Value = null;
                    activity.Attachments.Add(new Attachment()
                    {
                        Content = card,
                        ContentType = "application/vnd.microsoft.card.adaptive",
                    });
                }
                catch (Exception)
                {
                    activity.Text = KronosResourceText.ScheduleError;
                    activity.Value = null;
                }
            }

            await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);

            context.Done(default(string));
        }

        private List<Models.ResponseEntities.HyperFind.ResponseHyperFindResult> CreatePersonIdentities(string personNumbers)
        {
            List<Models.ResponseEntities.HyperFind.ResponseHyperFindResult> personIdentities = new List<Models.ResponseEntities.HyperFind.ResponseHyperFindResult>();
            string[] personNos = personNumbers.Split(new char[] { ',' });

            foreach (var person in personNos)
            {
                Models.ResponseEntities.HyperFind.ResponseHyperFindResult personIdentity = new Models.ResponseEntities.HyperFind.ResponseHyperFindResult();
                personIdentity.PersonNumber = person;
                personIdentities.Add(personIdentity);
            }

            return personIdentities;
        }

        private async Task ShowShiftSelectionCard(IDialogContext context, string command, LuisResultModel luisResult = null)
        {

            string jSession = string.Empty;
            var reply = context.MakeMessage();
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            AdaptiveCard card;
            string startDate = "" ;
            string endDate = "" ;
            if (luisResult == null)
            {
                startDate = DateTime.Today.AddDays(1).ToString("M/d/yyyy", CultureInfo.InvariantCulture);
                DateTime start = context.Activity.LocalTimestamp.Value.DateTime.StartWeekDate(DayOfWeek.Sunday).AddDays(6);
                endDate = start.EndOfWeek().ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }
            else
            {
            var t = luisResult?.entities?.FirstOrDefault()?.resolution?.values?.FirstOrDefault();
            switch (t.type)
            {
                case "date":
                    startDate = DateTime.Parse(t.value).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                    endDate = DateTime.Parse(t.value).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                    break;
                case "daterange":
                    startDate = DateTime.Parse(t.start).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                    endDate = DateTime.Parse(t.end).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                    break;
                default:
                    await context.PostAsync("Could not recognise command.");
                    break;
            }
                //        string startDate = DateTime.Today.AddDays(1).ToString("M/d/yyyy", CultureInfo.InvariantCulture);
                //DateTime start = context.Activity.LocalTimestamp.Value.DateTime.StartWeekDate(DayOfWeek.Sunday).AddDays(6);
                //AdaptiveCard card;
                //string endDate = start.EndOfWeek().ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }

            // get person number
            string personNumber = string.Empty;
            string personName = string.Empty;
            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                personNumber = this.response.PersonNumber;
                personName = this.response.Name;
                jSession = this.response.JsessionID;
            }

            context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);

            UpcomingShift.Response scheduleResponse = await this.upcomingShiftsActivity.ShowUpcomingShifts(tenantId, superSession, startDate, endDate, personNumber);

            if (scheduleResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
            {
                await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
            }
            else if (scheduleResponse.Error?.ErrorCode == ApiConstants.UserUnauthorizedError)
            {
                await context.PostAsync(KronosResourceText.NoPermission);
            }
            else
            {
                try
                {
                    var workingdaySchedule = scheduleResponse.Schedule.ScheduleItems.ScheduleShift.ToList();
                    if (workingdaySchedule.Count() == 0)
                    {
                        card = this.swapShiftCard.GetShiftSelectionCard(context, null, null);
                    }
                    else
                    {
                        var listOfEmps = await this.hyperFindActivity.GetHyperFindQueryValues(tenantId, superSession, startDate, endDate, ApiConstants.ReportsToHyperFindQuery, ApiConstants.PersonalVisibilityCode);

                        card = this.swapShiftCard.GetShiftSelectionCard(context, workingdaySchedule, listOfEmps.HyperFindResult);
                    }
                }
                catch (Exception)
                {
                    card = this.swapShiftCard.GetShiftSelectionCard(context, null, null);
                }

                if (command.Contains("back"))
                {
                    var activity = context.Activity as Activity;
                    IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                    var conversationId = context.Activity.Conversation.Id;
                    var activityId = context.Activity.ReplyToId;

                    activity.Text = null;
                    activity.Value = null;
                    activity.Attachments.Add(new Attachment()
                    {
                        Content = card,
                        ContentType = "application/vnd.microsoft.card.adaptive",
                    });
                    await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
                }
                else
                {
                    reply.Attachments.Add(new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = card,
                    });
                    await context.PostAsync(reply);
                }
            }
        }

        private async Task SubmitSwapShiftRequest(IDialogContext context)
        {
            var activity = context.Activity as Activity;
            string jSession = string.Empty;
            JToken token = JObject.Parse(activity.Value.ToString());
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            string personNumber = string.Empty;
            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                personNumber = this.response.PersonNumber;
                jSession = this.response.JsessionID;
            }

            var obj = context.PrivateConversationData.GetValue<SwapShiftObj>("SwapShiftObj");
            obj.QueryDateSpan = obj.Emp1FromDateTime.ToString("M/d/yyyy", CultureInfo.InvariantCulture) + "-" + obj.Emp1ToDateTime.AddDays(10).ToString("M/d/yyyy", CultureInfo.InvariantCulture);
            context.PrivateConversationData.SetValue("SwapShiftObj", obj);

            context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);

            var result = await this.swapShiftActivity.DraftSwapShift(tenantId, superSession, obj);
            if (result?.Status == ApiConstants.Failure)
            {
                // check if authentication failure then send sign in card
                // User is not logged in - Send Sign in card
                if (result.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }
                else
                {
                    await context.PostAsync(result.Error.DetailErrors.Error.FirstOrDefault().Message);
                }
            }
            else
            {
                var reqId = result.EmployeeRequestMgm.RequestItem.EmployeeSwapShiftRequestItems.FirstOrDefault().Id;
                obj.RequestId = reqId;
                context.PrivateConversationData.SetValue("SwapShiftObj", obj);
                result = await this.swapShiftActivity.SubmitSwapShift(tenantId, superSession, obj.RequestorPersonNumber, reqId, obj.QueryDateSpan, null);

                if (result?.Status == ApiConstants.Failure)
                {
                    await context.PostAsync(result.Error.DetailErrors.Error.FirstOrDefault().Message);
                }
                else
                {
                    var conversationId = context.Activity.Conversation.Id;
                    var activityId = context.Activity.ReplyToId;

                    IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));

                    var conversationId1 = await this.commonActivity.GetEmpConversationId(obj.RequestedToPersonNumber, tenantId, superSession, activity.ChannelId);

                    if (conversationId1 != null)
                    {
                        await this.SendApprovalNotification(tenantId, superSession, context, conversationId1, null, obj, 1);
                    }

                    try
                    {
                        AdaptiveCard submitterCard = this.swapShiftCard.GetSwapSubmitCard(context, obj, null, Constants.Submitted);
                        activity.Text = null;
                        activity.Value = null;
                        activity.Attachments.Add(new Attachment()
                        {
                            Content = submitterCard,
                            ContentType = "application/vnd.microsoft.card.adaptive",
                        });
                        await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
                        if (conversationId1 == null)
                        {
                            await context.PostAsync(KronosResourceText.SwapShiftEmpNotFoundToNotify);
                        }

                        context.PrivateConversationData.RemoveValue("SwapShiftObj");
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }

            context.Done(default(string));
        }

        private async Task SubmitApproval(IDialogContext context, string command)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();

            string jSessionId = this.response?.JsessionID;
            var objToken = token.SelectToken("SwapShiftObj");
            var obj = this.ParseSwapShiftToken(objToken);
            string note = (string)token.SelectToken("note");
            string comment = (string)token.SelectToken("Comment");
            context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);
            var autoApprove = true;
            var status = string.Empty;
            if (command == Constants.ApproveSwapShiftByEmp)
            {
                var subTypeParams = await this.commonActivity.GetRequestSubTypeParams(tenantId, superSession);
                if (subTypeParams != null)
                {
                    var swapShiftParams = subTypeParams.RequestSubtype.Where(c => c.RequestTypeName == ApiConstants.SwapShift).FirstOrDefault();
                    if (swapShiftParams != null)
                    {
                        var ssAutoApproval = swapShiftParams.RequestParamValues.RequestParamValue.Where(w => w.Name == ApiConstants.SwapShiftAutoApproval).FirstOrDefault();
                        if (ssAutoApproval != null)
                        {
                            autoApprove = Convert.ToBoolean(ssAutoApproval.Value);
                        }
                    }
                }

                if (autoApprove == true)
                {
                    status = Constants.Approved.ToUpper();
                }
                else
                {
                    status = Constants.Submitted.ToUpper();
                }
            }
            else if (command == Constants.ApproveSwapShiftBySupervisor)
            {
                status = Constants.Approved.ToUpper();
            }
            else
            {
                status = Constants.Refused.ToUpper();
            }

            var response = await this.swapShiftActivity.SubmitApproval(tenantId, superSession, obj.RequestId, obj.RequestorPersonNumber, status, obj.QueryDateSpan, comment, note);
            if (response?.Status == ApiConstants.Failure)
            {
                if (response.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }
                else
                {
                    await context.PostAsync(response?.Error?.DetailErrors?.Error.FirstOrDefault().Message);
                }
            }

            if (response?.Status == ApiConstants.Success)
            {
                if (string.Equals(status, Constants.Submitted, StringComparison.OrdinalIgnoreCase))
                {
                    var requestor = await this.commonActivity.GetPersonInformation(tenantId, superSession, obj.RequestorPersonNumber);
                    var requestedTo = await this.commonActivity.GetPersonInformation(tenantId, superSession, obj.RequestedToPersonNumber);
                    var requestorSupervisorConvId = await this.commonActivity.GetEmpConversationId(requestor.PersonInformation.SupervisorData.Supervisor.PersonNumber, tenantId, jSessionId, activity.ChannelId);
                    var requestedToSupervisorConvId = await this.commonActivity.GetEmpConversationId(requestedTo.PersonInformation.SupervisorData.Supervisor.PersonNumber, tenantId, jSessionId, activity.ChannelId);
                    await context.PostAsync(KronosResourceText.YouApprovedSwapShiftRequest);
                    if (requestorSupervisorConvId != null && requestedToSupervisorConvId != null)
                    {
                        if (requestorSupervisorConvId != requestedToSupervisorConvId)
                        {
                            if (requestorSupervisorConvId != null)
                            {
                                await this.SendApprovalNotification(tenantId, superSession, context, requestorSupervisorConvId, note, obj, 2);
                            }

                            if (requestorSupervisorConvId != null)
                            {
                                await this.SendApprovalNotification(tenantId, superSession, context, requestedToSupervisorConvId, note, obj, 2);
                            }
                        }
                        else
                        {
                            if (requestorSupervisorConvId != null)
                            {
                                await this.SendApprovalNotification(tenantId, superSession, context, requestorSupervisorConvId, note, obj, 2);
                            }
                        }
                    }
                    else
                    {
                    }
                }

                if (string.Equals(status, Constants.Approved, StringComparison.OrdinalIgnoreCase))
                {
                    var requestor = await this.commonActivity.GetPersonInformation(tenantId, superSession, obj.RequestorPersonNumber);
                    var requestorSupervisorConvId = await this.commonActivity.GetEmpConversationId(obj.RequestorPersonNumber, tenantId, superSession, activity.ChannelId);
                    await this.SendApprovalResponseNotiToEmployee(tenantId, superSession, context, Constants.Accepted, requestorSupervisorConvId, note, obj);
                    await context.PostAsync(KronosResourceText.YouApprovedSwapShiftRequest);
                }

                if (string.Equals(status, Constants.Refused, StringComparison.OrdinalIgnoreCase))
                {
                    var requestor = await this.commonActivity.GetPersonInformation(tenantId, superSession, obj.RequestorPersonNumber);
                    var requestorSupervisorConvId = await this.commonActivity.GetEmpConversationId(obj.RequestorPersonNumber, tenantId, superSession, activity.ChannelId);
                    await this.SendApprovalResponseNotiToEmployee(tenantId, jSessionId, context, Constants.Refused, requestorSupervisorConvId, note, obj);
                    await context.PostAsync(KronosResourceText.YouRefusedSwapShiftRequest);
                }
            }

            context.Done(default(string));
        }

        private SwapShiftObj ParseSwapShiftToken(JToken obj)
        {
            SwapShiftObj ss = new SwapShiftObj
            {
                Emp1FromDateTime = Convert.ToDateTime(obj.SelectToken("Emp1FromDateTime")),
                Emp1ToDateTime = Convert.ToDateTime(obj.SelectToken("Emp1ToDateTime")),
                Emp2FromDateTime = Convert.ToDateTime(obj.SelectToken("Emp2FromDateTime")),
                Emp2ToDateTime = Convert.ToDateTime(obj.SelectToken("Emp2ToDateTime")),
                QueryDateSpan = Convert.ToString(obj.SelectToken("QueryDateSpan")),
                RequestedToName = Convert.ToString(obj.SelectToken("RequestedToName")),
                RequestedToPersonNumber = Convert.ToString(obj.SelectToken("RequestedToPersonNumber")),
                RequestId = Convert.ToString(obj.SelectToken("RequestId")),
                RequestorName = Convert.ToString(obj.SelectToken("RequestorName")),
                RequestorPersonNumber = Convert.ToString(obj.SelectToken("RequestorPersonNumber")),
                SelectedAvailableShift = Convert.ToString(obj.SelectToken("SelectedAvailableShift")),
                SelectedEmployee = Convert.ToString(obj.SelectToken("SelectedEmployee")),
                SelectedJob = Convert.ToString(obj.SelectToken("SelectedJob")),
                SelectedLocation = Convert.ToString(obj.SelectToken("SelectedLocation")),
                SelectedShiftToSwap = Convert.ToString(obj.SelectToken("SelectedShiftToSwap")),
            };

            return ss;
        }
    }
}
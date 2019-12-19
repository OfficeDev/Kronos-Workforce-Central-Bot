//-----------------------------------------------------------------------
// <copyright file="TeamOvertimesDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Dialogs.TeamOvertimes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Hours;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.HyperFind;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.JobAssignment;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Shifts;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.TeamOvertimesCard;
    using Microsoft.Teams.App.KronosWfc.Cards.CarouselCards;
    using Microsoft.Teams.App.KronosWfc.CommandHandling;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UpcomingShiftsAlias = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Shifts.UpcomingShifts;

    /// <summary>
    /// This dialog displays team overtimes data.
    /// </summary>
    [Serializable]
    public class TeamOvertimesDialog : IDialog<object>
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IHoursWorkedActivity hoursWorkedActivity;
        private readonly IHyperFindActivity hyperFindActivity;
        private readonly AuthenticateUser authenticateUser;
        private readonly IJobAssignmentActivity jobAssignment;
        private readonly IAzureTableStorageHelper azureTableStorageHelper;
        private readonly LoginResponse response;
        private readonly IUpcomingShiftsActivity upcomingShiftsActivity;
        private CarouselTeamOvertimes teamOvertimesCard;
        private TeamOvertimesCard adaptiveTeamOvertimesCard;
        private AdaptiveDateRange dateRangeCard;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamOvertimesDialog" /> class.
        /// </summary>
        /// <param name="response">LoginResponse object.</param>
        /// <param name="authenticationService">AuthenticationService object.</param>
        /// <param name="hoursWorkedActivity">HoursWorkedActivity object.</param>
        /// <param name="hyperFindActivity">HyperFindActivity object.</param>
        /// <param name="jobAssignment">JobAssignmentActivity object.</param>
        /// <param name="authenticateUser">AuthenticateUser object.</param>
        /// <param name="teamOvertimesCard">CarouselTeamOvertimes object.</param>
        /// <param name="azureTableStorageHelper">AzureTableStorageHelper object.</param>
        /// <param name="dateRangeCard">AdaptiveDateRange object.</param>
        /// <param name="adaptiveTeamOvertimesCard">TeamOvertimesCard object.</param>
        /// <param name="upcomingShiftsActivity">UpcomingShiftsActivity object.</param>
        public TeamOvertimesDialog(
            LoginResponse response,
            IAuthenticationService authenticationService,
            IHoursWorkedActivity hoursWorkedActivity,
            IHyperFindActivity hyperFindActivity,
            IJobAssignmentActivity jobAssignment,
            AuthenticateUser authenticateUser,
            CarouselTeamOvertimes teamOvertimesCard,
            IAzureTableStorageHelper azureTableStorageHelper,
            AdaptiveDateRange dateRangeCard,
            TeamOvertimesCard adaptiveTeamOvertimesCard,
            IUpcomingShiftsActivity upcomingShiftsActivity)
        {
            this.response = response;
            this.authenticationService = authenticationService;
            this.hoursWorkedActivity = hoursWorkedActivity;
            this.hyperFindActivity = hyperFindActivity;
            this.authenticateUser = authenticateUser;
            this.jobAssignment = jobAssignment;
            this.teamOvertimesCard = teamOvertimesCard;
            this.azureTableStorageHelper = azureTableStorageHelper;
            this.dateRangeCard = dateRangeCard;
            this.adaptiveTeamOvertimesCard = adaptiveTeamOvertimesCard;
            this.upcomingShiftsActivity = upcomingShiftsActivity;
        }

        /// <summary>
        /// The StartAsync method calls IDialogContext.Wait with the continuation delegate to specify
        /// the method that should be called when a new message is received.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>A task.</returns>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.ShowOvertimeEmployeesList);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Calculates pay period.
        /// </summary>
        /// <param name="localTimestamp">Local timestamp.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <param name="payPeriod">Pay period.</param>
        private static void CalculatePayPeriod(DateTimeOffset? localTimestamp, out string startDate, out string endDate, string payPeriod)
        {
            if (payPeriod == Constants.PreviousPayPeriodPunches)
            {
                var currentDay = (int)localTimestamp.Value.Date.DayOfWeek;
                var startWeek = localTimestamp.Value.Date.AddDays(-currentDay);
                endDate = startWeek.AddDays(-1).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                startDate = startWeek.AddDays(-7).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                var currentDay = (int)localTimestamp.Value.Date.DayOfWeek;
                var startWeek = localTimestamp.Value.Date.AddDays(-currentDay);
                endDate = startWeek.AddDays(currentDay).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                startDate = startWeek.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Method is used to show overtime employees list.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">Awaitable string.</param>
        /// <returns>A task.</returns>
        private async Task ShowOvertimeEmployeesList(IDialogContext context, IAwaitable<string> result)
        {
            var activity = context.Activity as Activity;
            var tenant = activity.ChannelData as JObject;
            string resultString = await result;
            string msg = JsonConvert.DeserializeObject<Message>(resultString).message;
            var luisResult = JsonConvert.DeserializeObject<Message>(resultString).luisResult;
            var tenantId = tenant["tenant"].SelectToken("id").ToString();
            var startDate = activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            var endDate = activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            var overtimeEmployeeList = new List<string>();
            var payPeriod = string.Empty;

            if (!context.UserData.TryGetValue(context.Activity.From.Id, out LoginResponse response))
            {
                response = this.response;
            }

            var message = activity.Text?.ToLowerInvariant().Trim();
            AppInsightsLogger.CustomEventTrace("TeamOvertimesDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ShowOvertimeEmployeesList" }, { "Command", message } });
            if (luisResult.entities.Count == 0)
            {
                switch (message)
                {
                    case string command when command.ToLowerInvariant().Contains(Constants.OvertimeNext.ToLowerInvariant()):
                        await this.Next(context);
                        break;
                    case string command when command.ToLowerInvariant().Contains(Constants.OvertimePrevious.ToLowerInvariant()):
                        await this.Previous(context);
                        break;
                    case string command when command.Contains(Constants.PreviousWeekTeamOvertimes):
                        CalculatePayPeriod(context.Activity.LocalTimestamp, out startDate, out endDate, Constants.PreviousPayPeriodPunches);
                        payPeriod = Constants.PreviousPayPeriodPunchesText;
                        await this.ShowOvertimeEmployees(context, tenantId, startDate, endDate, overtimeEmployeeList, payPeriod, response);
                        break;

                    case string command when command.Contains(Constants.DateRangeTeamOvertimes):
                        await this.dateRangeCard.ShowDateRange(context, Constants.SubmitDateRangeTeamOvertimes, Constants.TeamOvertimesDateRangeText);
                        break;

                    case string command when command.Contains(Constants.SubmitDateRangeTeamOvertimes):
                        dynamic value = activity.Value;
                        DateRange dateRange;

                        if (value != null)
                        {
                            dateRange = DateRange.Parse(value);
                            var results = new List<ValidationResult>();
                            bool valid = Validator.TryValidateObject(dateRange, new ValidationContext(dateRange, null, null), results, true);

                            if (!valid)
                            {
                                var errors = string.Join("\n", results.Select(o => " - " + o.ErrorMessage));
                                await context.PostAsync($"{Constants.DateRangeParseError}" + errors);
                                return;
                            }

                            startDate = DateTime.Parse(dateRange.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                            endDate = DateTime.Parse(dateRange.EndDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                        }

                        await this.ShowOvertimeEmployees(context, tenantId, startDate, endDate, overtimeEmployeeList, payPeriod, response);
                        break;

                    case string command when command.Contains(Constants.CurrentWeekTeamOvertimes) || command.Contains(Constants.TeamOvertimes.ToLowerInvariant()):
                        CalculatePayPeriod(context.Activity.LocalTimestamp, out startDate, out endDate, Constants.CurrentpayPeriodPunches);
                        payPeriod = Constants.CurrentpayPeriodPunchesText;
                        await this.ShowOvertimeEmployees(context, tenantId, startDate, endDate, overtimeEmployeeList, payPeriod, response);
                        break;
                }
            }
            else
            {
                var t = luisResult?.entities?.FirstOrDefault()?.resolution?.values?.FirstOrDefault();
                switch (t.type)
                {
                    case "date":
                        startDate = DateTime.Parse(t.value).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                        endDate = DateTime.Parse(t.value).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                        await this.ShowOvertimeEmployees(context, tenantId, startDate, endDate, overtimeEmployeeList, payPeriod, response);
                        break;
                    case "daterange":
                        startDate = DateTime.Parse(t.start).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                        endDate = DateTime.Parse(t.end).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                        await this.ShowOvertimeEmployees(context, tenantId, startDate, endDate, overtimeEmployeeList, payPeriod, response);
                        break;
                    default:
                        await context.PostAsync("Could not recognise command.");
                        break;
                }

            }
            context.Done(string.Empty);
        }

        private async Task ShowOvertimeEmployees(IDialogContext context, string tenantId, string startDate, string endDate, List<string> overtimeEmployeeList, string payPeriod, LoginResponse response)
        {
            var superUserLogonRes = await this.authenticationService.LoginSuperUser((Activity)context.Activity);
            if (superUserLogonRes?.Status == ApiConstants.Success)
            {
                context.UserData.SetValue(context.Activity.From.Id + Constants.SuperUser, superUserLogonRes.Jsession);
            }

            string personNumber = this.response?.PersonNumber;
            string isManager = await this.authenticateUser.IsUserManager(context);
            var activity = context.Activity as Activity;
            var message = activity.Text?.ToLowerInvariant().Trim();
            if (isManager.Equals(Constants.Yes))
            {
                Models.ResponseEntities.HyperFind.Response hyperFindResponse = await this.hyperFindActivity.GetHyperFindQueryValues(tenantId, response.JsessionID, startDate, endDate, ApiConstants.OvertimeHyperFindQuery, ApiConstants.PubilcVisibilityCode);

                if (hyperFindResponse?.Status == ApiConstants.Failure)
                {
                    // User is not logged in - Send Sign in card
                    if (hyperFindResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                    {
                        await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                    }
                    else
                    {
                        await context.PostAsync(hyperFindResponse.Error?.Message);
                    }
                }
                else
                {
                    context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);
                    if (hyperFindResponse.HyperFindResult.Count > 0)
                    {
                        var overtimeMappingEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<OvertimeMappingEntity>(Constants.ActivityChannelId, $"{tenantId}${Constants.TeamOvertimes}", AppSettings.Instance.OvertimeMappingtableName);

                        List<string> lst = new List<string>();
                        var emplist = hyperFindResponse.HyperFindResult.Where(w => w.PersonNumber != personNumber);

                        var tasks = emplist.Select(async emp =>
                        {
                            var showHoursWorkedResponse = await this.hoursWorkedActivity.ShowHoursWorked(tenantId, response, startDate, endDate, emp.PersonNumber);

                            // var totalHours = showPunchesResponse?.Timesheet?.DailyTotals?.DateTotals?
                            //    .Where(t => t.Totals != null && t.Totals.Total != null)
                            //    .SelectMany(x => x.Totals.Total.Where(f => f.PayCodeName.ToLowerInvariant().Contains(overtimeMappingEntity.PayCodeName.ToLowerInvariant())))
                            //    .Sum(x => Convert.ToDouble(x.AmountInTime.Replace(':', '.')));
                            var showHoursWorkedOrderedList = showHoursWorkedResponse?.Timesheet?.PeriodTotalData?.PeriodTotals?.Totals?.Total?.FindAll(x => x.PayCodeName == Constants.AllHours).Select(x => new { x.PayCodeName, x.AmountInTime }).ToList();
                            var allHours = showHoursWorkedOrderedList.FirstOrDefault(x => x.PayCodeName.Contains(Constants.AllHours))?.AmountInTime;

                            if (allHours != null)
                            {
                                var jobAssignent = await this.jobAssignment.getJobAssignment(Convert.ToString(emp.PersonNumber), tenantId, superSession);
                                var role = jobAssignent?.JobAssign?.PrimaryLaborAccList?.PrimaryLaborAcc?.OrganizationPath?.Split('/').LastOrDefault();
                                DateTime stDateTime = default(DateTime);
                                DateTime eDateTime = default(DateTime);
                                double assignedHours = default(double);
                                UpcomingShiftsAlias.Response scheduleResponse = await this.upcomingShiftsActivity.ShowUpcomingShifts(tenantId, superSession, startDate, endDate, emp.PersonNumber);
                                List<UpcomingShiftsAlias.ScheduleShift> scheduleShifts = scheduleResponse?.Schedule?.ScheduleItems?.ScheduleShift?.OrderBy(x => x.StartDate).ToList();
                                if (scheduleShifts != null)
                                {
                                    foreach (var scheduleShift in scheduleShifts)
                                    {
                                        var shiftSegment = scheduleShift.ShiftSegments.FirstOrDefault();
                                        stDateTime = DateTime.Parse($"{shiftSegment.StartDate} {shiftSegment.StartTime}", CultureInfo.InvariantCulture, DateTimeStyles.None);
                                        eDateTime = DateTime.Parse($"{shiftSegment.EndDate} {shiftSegment.EndTime}", CultureInfo.InvariantCulture, DateTimeStyles.None);
                                        assignedHours = assignedHours + Math.Abs(Math.Round((stDateTime - eDateTime).TotalHours, 2));
                                    }
                                }

                                // overtimeEmployeeList.Add($"{emp.FullName} - {totalHours} <br />");
                                overtimeEmployeeList.Add($"{emp.FullName} - {role} - {Convert.ToString(allHours).Replace(':', '.')}/{assignedHours}");
                            }
                        });
                        await Task.WhenAll(tasks);

                        var pagewiseOvertimes = this.GetPagewiseList(overtimeEmployeeList);
                        context.PrivateConversationData.SetValue("PagewiseOvertimes", pagewiseOvertimes);
                        var card = this.adaptiveTeamOvertimesCard.GetCard(context, payPeriod, startDate, endDate, 1);
                        if (message.Contains(Constants.PreviousWeekTeamOvertimes) || message.Contains(Constants.CurrentWeekTeamOvertimes))
                        {
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
                        else
                        {
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
                    }
                    else
                    {
                        var pagewiseOvertimes = this.GetPagewiseList(overtimeEmployeeList);
                        context.PrivateConversationData.SetValue("PagewiseOvertimes", pagewiseOvertimes);
                        var card = this.adaptiveTeamOvertimesCard.GetCard(context, payPeriod, startDate, endDate, 1);
                        if (message.Contains(Constants.PreviousWeekTeamOvertimes) || message.Contains(Constants.CurrentWeekTeamOvertimes))
                        {
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
                        else
                        {
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
                    }
                }
            }
            else if (isManager.Equals(Constants.No))
            {
                await context.PostAsync(KronosResourceText.NoPermission);
            }
        }

        private Hashtable GetPagewiseList(List<string> overtimeEmployeeList)
        {
            var pageSize = 5;
            var pageCount = Math.Ceiling((double)overtimeEmployeeList.Count / pageSize);
            pageCount = pageCount > pageSize ? pageSize : pageCount;
            Hashtable pages = new Hashtable();

            for (int i = 0; i < pageCount; i++)
            {
                var list = overtimeEmployeeList.Skip(pageSize * i).Take(pageSize).ToList();
                pages.Add(i.ToString(), JsonConvert.SerializeObject(list).ToString());
            }

            return pages;
        }

        private async Task Next(IDialogContext context)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            var currentPage = Convert.ToString(token.SelectToken("CurrentPage"));
            var payPeriod = Convert.ToString(token.SelectToken("PayPeriod"));
            var conversationId = context.Activity.Conversation.Id;
            var activityId = context.Activity.ReplyToId;
            var card = this.adaptiveTeamOvertimesCard.GetCard(context, payPeriod, string.Empty, string.Empty, Convert.ToInt32(currentPage) + 1);
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

        private async Task Previous(IDialogContext context)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            var currentPage = Convert.ToString(token.SelectToken("CurrentPage"));
            var payPeriod = Convert.ToString(token.SelectToken("PayPeriod"));
            var conversationId = context.Activity.Conversation.Id;
            var activityId = context.Activity.ReplyToId;
            var card = this.adaptiveTeamOvertimesCard.GetCard(context, payPeriod, string.Empty, string.Empty, Convert.ToInt32(currentPage) - 1);
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
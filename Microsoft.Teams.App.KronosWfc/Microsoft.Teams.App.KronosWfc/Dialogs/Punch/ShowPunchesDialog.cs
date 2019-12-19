//-----------------------------------------------------------------------
// <copyright file="ShowPunchesDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Dialogs.Punch
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Punch;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Shifts;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.Punches;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.ShowPunches;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UpcomingShiftsAlias = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Shifts.UpcomingShifts;

    /// <summary>
    /// This dialog displays list of punches.
    /// </summary>
    [Serializable]
    public class ShowPunchesDialog : IDialog<object>
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IShowPunchesActivity showPunchesActivity;
        private readonly IUpcomingShiftsActivity upcomingShiftsActivity;
        private readonly IAzureTableStorageHelper azureTableStorageHelper;
        private readonly LoginResponse response;
        private HeroShowPunches showPunchesCard;
        private AdaptiveShowPunches showPunchesDataCard;
        private AdaptiveDateRange dateRangeCard;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowPunchesDialog" /> class.
        /// </summary>
        /// <param name="response">LoginResponse object.</param>
        /// <param name="authenticationService">AuthenticationService object.</param>
        /// <param name="showPunchesActivity">ShowPunchesActivity object.</param>
        /// <param name="upcomingShiftsActivity">Upcoming shift object.</param>
        /// <param name="azureTableStorageHelper">Azure table storage object.</param>
        /// <param name="showPunchesCard">HeroShowPunches object.</param>
        /// <param name="showPunchesDataCard">AdaptiveShowPunches object.</param>
        /// <param name="dateRangeCard">AdaptiveDateRange object.</param>
        public ShowPunchesDialog(
            LoginResponse response,
            IAuthenticationService authenticationService,
            IShowPunchesActivity showPunchesActivity,
            IUpcomingShiftsActivity upcomingShiftsActivity,
            IAzureTableStorageHelper azureTableStorageHelper,
            HeroShowPunches showPunchesCard,
            AdaptiveShowPunches showPunchesDataCard,
            AdaptiveDateRange dateRangeCard)
        {
            this.response = response;
            this.authenticationService = authenticationService;
            this.showPunchesActivity = showPunchesActivity;
            this.upcomingShiftsActivity = upcomingShiftsActivity;
            this.azureTableStorageHelper = azureTableStorageHelper;
            this.showPunchesCard = showPunchesCard;
            this.showPunchesDataCard = showPunchesDataCard;
            this.dateRangeCard = dateRangeCard;
        }

        /// <summary>
        /// The StartAsync method calls IDialogContext.Wait with the continuation delegate to specify
        /// the method that should be called when a new message is received.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>A Task.</returns>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.ShowPunches);
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
            else if (payPeriod == Constants.CurrentpayPeriodPunches)
            {
                var currentDay = (int)localTimestamp.Value.Date.DayOfWeek;
                var startWeek = localTimestamp.Value.Date.AddDays(-currentDay);
                endDate = startWeek.AddDays(currentDay).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                startDate = startWeek.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                var beginDate = localTimestamp.Value.Date;
                var daysLeft = DateTime.DaysInMonth(beginDate.Year, beginDate.Month) - beginDate.Day;
                startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                endDate = localTimestamp.Value.Date.AddDays(daysLeft).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }
        }

        private async Task ShowPunches(IDialogContext context, IAwaitable<string> result)
        {

            string resultString = await result;
            string msg = JsonConvert.DeserializeObject<Message>(resultString).message;
            var luisResult = JsonConvert.DeserializeObject<Message>(resultString).luisResult;

            var activity = context.Activity as Activity;
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();

            string startDate = string.Empty;
            string endDate = string.Empty;
            var personNumber = string.Empty;
            var jSession = string.Empty;
            string allHours = string.Empty;
            double assignedHours = default(double);
            DateTime stDateTime = default(DateTime);
            DateTime eDateTime = default(DateTime);

            if (!context.UserData.TryGetValue(context.Activity.From.Id, out LoginResponse response))
            {
                response = this.response;
            }
            else
            {
                personNumber = response.PersonNumber;
                jSession = response.JsessionID;
            }

            var message = activity.Text?.ToLowerInvariant().Trim();
            Response showPunchesResponse = default(Response);
            var punchText = string.Empty;
            AppInsightsLogger.CustomEventTrace("ShowPunchesDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ShowPunches" }, { "Command", message } });
            if (luisResult.entities.Count == 0)
            {
                switch (message)
                {
                    case string command when command == Constants.PreviousPayPeriodPunches:
                        punchText = Constants.PreviousPayPeriodPunchesText;
                        CalculatePayPeriod(context.Activity.LocalTimestamp, out startDate, out endDate, Constants.PreviousPayPeriodPunches);
                        break;

                    case string command when command == Constants.CurrentpayPeriodPunches || command == Constants.Punches || command == Constants.ShowMeMyPunches:
                        punchText = Constants.CurrentpayPeriodPunchesText;
                        CalculatePayPeriod(context.Activity.LocalTimestamp, out startDate, out endDate, Constants.CurrentpayPeriodPunches);
                        break;

                    case string command when command.Contains(Constants.DateRangePunches):
                        await this.dateRangeCard.ShowDateRange(context, Constants.SubmitDateRangePunches, Constants.PunchesDateRangeText);
                        return;

                    case string command when command.Contains(Constants.SubmitDateRangePunches):
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

                        break;
                    default:
                        punchText = Constants.CurrentpayPeriodPunchesText;
                        CalculatePayPeriod(context.Activity.LocalTimestamp, out startDate, out endDate, Constants.CurrentpayPeriodPunches);
                        break;
                }
            }
            else
            {
                if ((luisResult?.entities?.FirstOrDefault()?.resolution?.values?.FirstOrDefault()) != null)
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
                }
                else
                {
                    // default shifts for today
                    startDate = context.Activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                    endDate = context.Activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                }
            }
            showPunchesResponse = await this.showPunchesActivity.ShowPunches(tenantId, jSession, personNumber, startDate, endDate);

            if (showPunchesResponse?.Status == ApiConstants.Success)
            {
                // get all hours worked and assigned hours
                context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);
                UpcomingShiftsAlias.Response scheduleResponse = await this.upcomingShiftsActivity.ShowUpcomingShifts(tenantId, superSession, startDate, endDate, personNumber);
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

                // Getting the list of recorded punches
                var showPunchesDataOrderedList = showPunchesResponse?.Timesheet?.TotaledSpans?.TotaledSpan?
                    .Where(x => x.InPunch.Punch.EnteredOnDate != null).ToList();

                // Getting all hours worked time
                var showHoursWorkedOrderedList = showPunchesResponse?.Timesheet?.PeriodTotalData?.PeriodTotals?.Totals?.Total?.FindAll(x => x.PayCodeName == Constants.AllHours).Select(x => new { x.PayCodeName, x.AmountInTime }).FirstOrDefault();
                allHours = string.IsNullOrEmpty(showHoursWorkedOrderedList?.AmountInTime) ? "0" : showHoursWorkedOrderedList?.AmountInTime.Replace(':', '.');

                await this.showPunchesDataCard.ShowPunchesData(context, showPunchesDataOrderedList, punchText, startDate, endDate, response, string.Format("{0:0.00}", assignedHours), allHours);
            }
            else
            {
                await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
            }

            context.Done(showPunchesResponse);
        }
    }
}
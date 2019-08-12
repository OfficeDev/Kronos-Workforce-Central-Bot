//-----------------------------------------------------------------------
// <copyright file="ShowHoursWorkedDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Dialogs.Hours
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Hours;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards;
    using Microsoft.Teams.App.KronosWfc.Cards.CarouselCards;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Hours;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This dialog is used to display total hours worked.
    /// </summary>
    [Serializable]
    public class ShowHoursWorkedDialog : IDialog<object>
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IHoursWorkedActivity hoursWorkedActivity;
        private readonly LoginResponse response;
        private readonly IAzureTableStorageHelper azureTableStorageHelper;
        private CarouselShowHoursWorked showHoursWorkedCard;
        private AdaptiveDateRange dateRangeCard;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowHoursWorkedDialog"/> class.
        /// </summary>
        /// <param name="response">LoginResponse object.</param>
        /// <param name="authenticationService">AuthenticationService object.</param>
        /// <param name="hoursWorkedActivity">HoursWorkedActivity object.</param>
        /// <param name="showHoursWorkedCard">CarouselShowHoursWorked object.</param>
        /// <param name="azureTableStorageHelper">AzureTableStorageHelper object.</param>
        /// <param name="dateRangeCard">AdaptiveDateRange object.</param>
        public ShowHoursWorkedDialog(
            LoginResponse response,
            IAuthenticationService authenticationService,
            IHoursWorkedActivity hoursWorkedActivity,
            CarouselShowHoursWorked showHoursWorkedCard,
            IAzureTableStorageHelper azureTableStorageHelper,
            AdaptiveDateRange dateRangeCard)
        {
            this.response = response;
            this.authenticationService = authenticationService;
            this.hoursWorkedActivity = hoursWorkedActivity;
            this.showHoursWorkedCard = showHoursWorkedCard;
            this.azureTableStorageHelper = azureTableStorageHelper;
            this.dateRangeCard = dateRangeCard;
        }

        /// <summary>
        /// The StartAsync method calls IDialogContext.Wait with the continuation delegate to specify
        /// the method that should be called when a new message is received.
        /// </summary>
        /// <param name="context">IDialogContext object.</param>
        /// <returns>A task.</returns>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.ShowHoursWorked);
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
            else if (payPeriod == Constants.TodayPunches)
            {
                var currentDay = localTimestamp.Value.Date;
                endDate = currentDay.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                startDate = currentDay.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                var beginDate = localTimestamp.Value.Date;
                var daysLeft = DateTime.DaysInMonth(beginDate.Year, beginDate.Month) - beginDate.Day;
                startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                endDate = localTimestamp.Value.Date.AddDays(daysLeft).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }
        }

        private async Task ShowHoursWorked(IDialogContext context, IAwaitable<string> result)
        {
            var activity = context.Activity as Activity;
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();

            string startDate = string.Empty;
            string endDate = string.Empty;
            bool status = false;

            if (!context.UserData.TryGetValue(context.Activity.From.Id, out LoginResponse response))
            {
                response = this.response;
            }

            var message = activity.Text?.ToLowerInvariant().Trim();
            Response showHoursWorkedResponse = default(Response);
            var punchText = string.Empty;

            AppInsightsLogger.CustomEventTrace("ShowHoursWorkedDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ShowHoursWorked" }, { "Command", message } });

            var overtimeMappingEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsyncFiltered<OvertimeMappingEntity>(Constants.ActivityChannelId, tenantId, AppSettings.Instance.OvertimeMappingtableName);
            var filteredResults = overtimeMappingEntity.Where(x => x.PayCodeType == Constants.TeamOvertimes || x.PayCodeType == Constants.Regular).Select(x => x.PayCodeName);

            switch (message)
            {
                case string command when command.Contains(Constants.PreviousPayPeriodHoursWorkedText):
                    punchText = Constants.PreviousPayPeriodPunchesText;
                    CalculatePayPeriod(context.Activity.LocalTimestamp, out startDate, out endDate, Constants.PreviousPayPeriodPunches);
                    break;

                case string command when command.Contains(Constants.DateRangeHoursWorked):
                    await this.dateRangeCard.ShowDateRange(context, Constants.SubmitDateRangeHoursWorked, Constants.HoursWorkedDateRangeText);
                    return;

                case string command when command.Contains(Constants.SubmitDateRangeHoursWorked):
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

                case string command when command.Contains(Constants.CurrentpayPeriodHoursWorkedText) || command.Contains(Constants.HowManyHoursWorked) || command.Contains(Constants.Hour):
                    punchText = Constants.CurrentpayPeriodPunchesText;
                    CalculatePayPeriod(context.Activity.LocalTimestamp, out startDate, out endDate, Constants.CurrentpayPeriodPunches);
                    break;
            }

            showHoursWorkedResponse = await this.hoursWorkedActivity.ShowHoursWorked(tenantId, response, startDate, endDate);
            status = this.CheckResponseStatus(context, showHoursWorkedResponse);

            if (status)
            {
                var showHoursWorkedOrderedList = showHoursWorkedResponse?.Timesheet?.PeriodTotalData?.PeriodTotals?.Totals?.Total?.FindAll(x => filteredResults.Contains(x.PayCodeName) || x.PayCodeName == Constants.AllHours).Select(x => new { x.PayCodeName, x.AmountInTime }).ToList();
                var regular = showHoursWorkedOrderedList?.FirstOrDefault(x => x.PayCodeName.Contains(Constants.Regular))?.AmountInTime ?? "0:00";
                var overtime = showHoursWorkedOrderedList?.FirstOrDefault(x => x.PayCodeName.Contains(Constants.TeamOvertimes))?.AmountInTime ?? "0:00";
                var allHours = showHoursWorkedOrderedList?.FirstOrDefault(x => x.PayCodeName.Contains(Constants.AllHours))?.AmountInTime;
                await this.showHoursWorkedCard.ShowHoursWorkedData(context, showHoursWorkedResponse, punchText, startDate, endDate, regular, overtime, allHours);
            }
            else
            {
                await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
            }

            context.Done(showHoursWorkedResponse);
        }

        private bool CheckResponseStatus(IDialogContext context, Response showHoursWorkedResponse)
        {
            bool successStatus = false;

            if (showHoursWorkedResponse?.Status == ApiConstants.Failure)
            {
                if (!(showHoursWorkedResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError))
                {
                    successStatus = true;
                }
            }
            else
            {
                successStatus = true;
            }

            return successStatus;
        }
    }
}
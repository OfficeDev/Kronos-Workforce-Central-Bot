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
    using Newtonsoft.Json;
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

            string resultString = "";
            try
            {
                resultString = await result;

                string msg = JsonConvert.DeserializeObject<Message>(resultString).message;
                var luisResult = JsonConvert.DeserializeObject<Message>(resultString).luisResult;
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
                if (luisResult.entities.Count == 0)
                {
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
                }

                showHoursWorkedResponse = await this.hoursWorkedActivity.ShowHoursWorked(tenantId, response, startDate, endDate);
                status = this.CheckResponseStatus(context, showHoursWorkedResponse);

                if (status)
                {
                    var showHoursWorkedOrderedList = showHoursWorkedResponse?.Timesheet?.PeriodTotalData?.PeriodTotals?.Totals?.Total?.FindAll(x => filteredResults.Contains(x.PayCodeName) || x.PayCodeName == Constants.AllHours).Select(x => new { x.PayCodeName, x.AmountInTime }).ToList();

                    //Regular Updated
                    var regular = showHoursWorkedOrderedList?.FindAll(x => x.PayCodeName.Contains(Constants.Regular));
                    //var regular1 = showHoursWorkedOrderedList?.FirstOrDefault(x => x.PayCodeName.Contains(Constants.Regular))?.AmountInTime ?? "0:00";
                    int regularTotalHours = 0;
                    int regularTotalMin = 0;
                    string regularTotalHourTime = string.Empty;
                    string regularTotalMinTime = string.Empty;
                    foreach (var item in regular)
                    {
                        regularTotalHours += int.Parse((item?.AmountInTime ?? "0:00")?.Split(':')[0]);
                        regularTotalMin += int.Parse((item?.AmountInTime ?? "0:00")?.Split(':')[1]);
                    }

                    if (regularTotalMin > 60)
                    {
                        regularTotalHours += regularTotalMin / 60;
                        regularTotalMin = regularTotalMin % 60;
                    }

                    regularTotalHourTime = regularTotalHours < 10 ? "0" + regularTotalHours : regularTotalHours.ToString();
                    regularTotalMinTime = regularTotalMin < 10 ? "0" + regularTotalMin : regularTotalMin.ToString();



                    //Overtime Updated
                    var overtime = showHoursWorkedOrderedList?.FindAll(x => x.PayCodeName.Contains(Constants.TeamOvertimes));

                    int overtimeTotalHours = 0;
                    int overtimeTotalMin = 0;
                    string overtimeTotalHourTime = string.Empty;
                    string overtimeTotalMinTime = string.Empty;
                    foreach (var item in overtime)
                    {
                        overtimeTotalHours += int.Parse((item?.AmountInTime ?? "0:00")?.Split(':')[0]);
                        overtimeTotalMin += int.Parse((item?.AmountInTime ?? "0:00")?.Split(':')[1]);
                    }

                    if (overtimeTotalMin > 60)
                    {
                        overtimeTotalHours += overtimeTotalMin / 60;
                        overtimeTotalMin = overtimeTotalMin % 60;
                    }

                    overtimeTotalHourTime = overtimeTotalHours < 10 ? "0" + overtimeTotalHours : overtimeTotalHours.ToString();
                    overtimeTotalMinTime = overtimeTotalMin < 10 ? "0" + overtimeTotalMin : overtimeTotalMin.ToString();

                    //All Hours Updated
                    var allHours = showHoursWorkedOrderedList?.FindAll(x => x.PayCodeName.Contains(Constants.AllHours));

                    int allHoursTotalHours = 0;
                    int allHoursTotalMin = 0;
                    string allHoursTotalHourTime = string.Empty;
                    string allHoursTotalMinTime = string.Empty;
                    foreach (var item in allHours)
                    {
                        allHoursTotalHours += int.Parse((item?.AmountInTime ?? "0:00")?.Split(':')[0]);
                        allHoursTotalMin += int.Parse((item?.AmountInTime ?? "0:00")?.Split(':')[1]);
                    }

                    if (allHoursTotalMin > 60)
                    {
                        allHoursTotalHours += allHoursTotalMin / 60;
                        allHoursTotalMin = allHoursTotalMin % 60;
                    }

                    allHoursTotalHourTime = allHoursTotalHours < 10 ? "0" + allHoursTotalHours : allHoursTotalHours.ToString();
                    allHoursTotalMinTime = allHoursTotalMin < 10 ? "0" + allHoursTotalMin : allHoursTotalMin.ToString();

                    await this.showHoursWorkedCard.ShowHoursWorkedData(context, showHoursWorkedResponse, punchText, startDate, endDate,
                        regularTotalHourTime + ":" + regularTotalMinTime, overtimeTotalHourTime + ":" + overtimeTotalMinTime,
                        allHoursTotalHourTime + ":" + allHoursTotalMinTime);
                }
                else
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }

                context.Done(showHoursWorkedResponse);
            }
            catch (Exception)
            {
                Response showHoursWorkedResponse = default(Response);
                context.Done(showHoursWorkedResponse);
            }
            
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
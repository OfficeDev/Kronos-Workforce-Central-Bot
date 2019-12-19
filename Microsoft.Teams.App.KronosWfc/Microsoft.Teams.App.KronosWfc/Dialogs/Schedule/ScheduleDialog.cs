// <Copyright file="ScheduleDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </Copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Dialogs.Schedule
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
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Core;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Hours;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.Schedule;
    using Microsoft.Teams.App.KronosWfc.Cards.HeroCards;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Schedule;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// schedule dialog.
    /// </summary>
    [Serializable]
    public class ScheduleDialog : IDialog<object>
    {
        private readonly IScheduleActivity scheduleActivity;
        private readonly IAuthenticationService authenticationService;
        private readonly IHoursWorkedActivity hoursWorkedActivity;
        private readonly IAzureTableStorageHelper azureTableStorageHelper;
        private LoginResponse response;
        private HeroShowSchedule heroShowSchedule;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDialog"/> class.
        /// </summary>
        /// <param name="scheduleActivity">schedule activity.</param>
        /// <param name="authenticationService">authentication service.</param>
        /// <param name="hoursWorkedActivity">hours worked activity.</param>
        /// <param name="azureTableStorageHelper">azure table storage helper.</param>
        /// <param name="response">login response.</param>
        /// <param name="adaptiveSchedule">schedule card.</param>
        public ScheduleDialog(IScheduleActivity scheduleActivity, IAuthenticationService authenticationService, IHoursWorkedActivity hoursWorkedActivity, IAzureTableStorageHelper azureTableStorageHelper, LoginResponse response, HeroShowSchedule heroShowSchedule)
        {
            this.scheduleActivity = scheduleActivity;
            this.authenticationService = authenticationService;
            this.hoursWorkedActivity = hoursWorkedActivity;
            this.azureTableStorageHelper = azureTableStorageHelper;
            this.response = response;
            this.heroShowSchedule = heroShowSchedule;
        }

        /// <inheritdoc/>
        public Task StartAsync(IDialogContext context)
        {
            Console.Write("Context: " + context);
            context.Wait<string>(this.ShowSchedule);
            return Task.CompletedTask;
        }

        /// <summary>
        /// show schedule.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="result">awaitable string.</param>
        /// <returns>schedule for provided date or date ranges.</returns>
        private async Task ShowSchedule(IDialogContext context, IAwaitable<string> result)
        {
            Console.Write("inside scheduleDialog result: " + result);
            string resultString = await result;
            string message = JsonConvert.DeserializeObject<Message>(resultString).message;
            var luisResult = JsonConvert.DeserializeObject<Message>(resultString).luisResult;
            string jSession = string.Empty;

            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();

            string startDate = default(string);
            string endDate = default(string);

            // get person number
            string personNumber = string.Empty;

            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                personNumber = this.response.PersonNumber;
                jSession = this.response.JsessionID;
            }

            AppInsightsLogger.CustomEventTrace("ScheduleDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ShowSchedule" }, { "Command", message } });
            if (luisResult.entities.Count == 0)
            {
                if (message.Contains(KronosResourceText.Current))
                {
                    DateTime start = context.Activity.LocalTimestamp.Value.DateTime.StartWeekDate(DayOfWeek.Sunday);
                    startDate = start.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                    endDate = start.EndOfWeek().ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                }
                else if (message.Contains(KronosResourceText.Next))
                {
                    // next schedule period
                    DateTime start = context.Activity.LocalTimestamp.Value.DateTime.StartWeekDate(DayOfWeek.Sunday).AddDays(6);
                    startDate = start.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                    endDate = start.EndOfWeek().ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                }
                else if (message.Equals(Constants.DateRangeSchedule, StringComparison.CurrentCultureIgnoreCase))
                {
                    // date range
                    // send date range card
                    await this.heroShowSchedule.ShowDateRange(context, Constants.SubmitDateRangeSchedule);
                }
                else if (message.Equals(Constants.SubmitDateRangeSchedule, StringComparison.CurrentCultureIgnoreCase))
                {
                    // date range submitted
                    dynamic value = ((Activity)context.Activity).Value;
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
                            context.Done(default(string));
                            return;
                        }

                        startDate = DateTime.Parse(dateRange.StartDate).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                        endDate = DateTime.Parse(dateRange.EndDate).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    // default schedule for today
                    startDate = context.Activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                    endDate = context.Activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
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
            context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);

            Response scheduleResponse = await this.scheduleActivity.ShowSchedule(tenantId, superSession, startDate, endDate, personNumber);

            if (scheduleResponse?.Status == ApiConstants.Failure)
            {
                // check if authentication failure then send sign in card
                if (scheduleResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }
            }
            else
            {
                // send schedule card
                if ((luisResult?.entities?.FirstOrDefault()?.resolution?.values?.FirstOrDefault()) != null)
                {
                    await context.PostAsync(KronosResourceText.DefaultSchedule + KronosResourceText.From + " **" + DateTime.Parse(startDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture) + "** " + KronosResourceText.Till + " **" + DateTime.Parse(endDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture) + "**");
                }

                else
                {
                    if (message.Contains(KronosResourceText.Current))
                    {
                        await context.PostAsync(KronosResourceText.DefaultSchedule + " **" + Constants.CurrentWeek + "**");
                    }
                    else if (message.Contains(KronosResourceText.Next))
                    {
                        await context.PostAsync(KronosResourceText.DefaultSchedule + " **" + Constants.NextWeek + "**");
                    }
                    else if (message.Equals(Constants.SubmitDateRangeSchedule, StringComparison.CurrentCultureIgnoreCase))
                    {
                        await context.PostAsync(KronosResourceText.DefaultSchedule + KronosResourceText.From + " **" + DateTime.Parse(startDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture) + "** " + KronosResourceText.Till + " **" + DateTime.Parse(endDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture) + "**");
                    }
                    else
                        await context.PostAsync(KronosResourceText.DefaultSchedule + " " + KronosResourceText.Today);
                }
                await this.heroShowSchedule.ShowScheduleCard(context, scheduleResponse, message);
            }
            context.Done(default(string));
        }
    }
}
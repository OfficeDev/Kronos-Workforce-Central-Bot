//-----------------------------------------------------------------------
// <copyright file="UpcomingShiftsDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
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
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Shifts;
    using Microsoft.Teams.App.KronosWfc.Cards.CarouselCards;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Shifts.UpcomingShifts;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// upcoming shifts dialog.
    /// </summary>
    [Serializable]
    public class UpcomingShiftsDialog : IDialog<object>
    {
        private readonly IUpcomingShiftsActivity upcomingShiftsActivity;
        private readonly IAuthenticationService authenticationService;
        private LoginResponse response;
        private CarouselUpcomingShifts carouselUpcomingShifts;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpcomingShiftsDialog"/> class.
        /// </summary>
        /// <param name="upcomingShiftsActivity">upcoming shift activity.</param>
        /// <param name="authenticationService">authentication service.</param>
        /// <param name="response">login response object.</param>
        /// <param name="carouselUpcomingShifts">carousel shifts card.</param>
        public UpcomingShiftsDialog(IUpcomingShiftsActivity upcomingShiftsActivity, IAuthenticationService authenticationService, LoginResponse response, CarouselUpcomingShifts carouselUpcomingShifts)
        {
            this.upcomingShiftsActivity = upcomingShiftsActivity;
            this.authenticationService = authenticationService;
            this.response = response;
            this.carouselUpcomingShifts = carouselUpcomingShifts;
        }

        /// <inheritdoc/>
        public Task StartAsync(IDialogContext context)
        {
           context.Wait<string>(this.ShowSchedule);
           return Task.CompletedTask;
        }

        /// <summary>
        /// show shifts.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="result">awaitable string.</param>
        /// <returns>show shifts for date or date ranges.</returns>
        private async Task ShowSchedule(IDialogContext context, IAwaitable<string> result)
        {
            string message = await result;
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

            AppInsightsLogger.CustomEventTrace("UpcomingShiftsDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ShowSchedule" }, { "Command", message } });

            if (message.Contains(KronosResourceText.Current))
            {
                // current week
                DateTime start = context.Activity.LocalTimestamp.Value.DateTime.StartWeekDate(DayOfWeek.Sunday);
                startDate = start.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                endDate = start.EndOfWeek().ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }
            else if (message.Contains(KronosResourceText.Next))
            {
                // next week
                DateTime start = context.Activity.LocalTimestamp.Value.DateTime.StartWeekDate(DayOfWeek.Sunday).AddDays(6);
                startDate = start.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                endDate = start.EndOfWeek().ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }
            else if (message.Equals(Constants.DateRangeShift, StringComparison.CurrentCultureIgnoreCase))
            {
                // send date range card
                await this.carouselUpcomingShifts.ShowDateRange(context, Constants.SubmitDateRangeShift);
                context.Done(default(string));
                return;
            }
            else if (message.Equals(Constants.SubmitDateRangeShift, StringComparison.CurrentCultureIgnoreCase))
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
                        return;
                    }

                    startDate = DateTime.Parse(dateRange.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                    endDate = DateTime.Parse(dateRange.EndDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                }
            }
            else
            {
                // default shifts for today
                startDate = context.Activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                endDate = context.Activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            }

            context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);

            Response scheduleResponse = await this.upcomingShiftsActivity.ShowUpcomingShifts(tenantId, superSession, startDate, endDate, personNumber);

            if (scheduleResponse?.Status == ApiConstants.Failure)
            {
                // check if authentication failure then send sign in card
                // User is not logged in - Send Sign in card
                if (scheduleResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }
                else
                {
                    await context.PostAsync(scheduleResponse.Error.Message);
                }
            }
            else
            {
                // send shift card
                if (message.Contains(KronosResourceText.Current))
                {
                    await context.PostAsync(KronosResourceText.DefaultShiftsText + " **" + Constants.CurrentWeek + "**");
                }
                else if (message.Contains(KronosResourceText.Next))
                {
                    await context.PostAsync(KronosResourceText.DefaultShiftsText + " **" + Constants.NextWeek + "**");
                }
                else if (message.Equals(Constants.SubmitDateRangeShift, StringComparison.CurrentCultureIgnoreCase))
                {
                    await context.PostAsync(KronosResourceText.DefaultShiftsText + KronosResourceText.From + " **" + DateTime.Parse(startDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture) + "** " + KronosResourceText.Till + " **" + DateTime.Parse(endDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture) + "**");
                }
                else
                {
                    await context.PostAsync(KronosResourceText.DefaultShiftsText + " " + KronosResourceText.Today);
                }

                await this.carouselUpcomingShifts.ShowUpcomingShiftsCard(context, scheduleResponse, message);
            }

            context.Done(default(string));
        }
    }
}
//-----------------------------------------------------------------------
// <copyright file="CarouselShowHoursWorked.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.CarouselCards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Hours;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// Show hours worked card.
    /// </summary>
    [Serializable]
    public class CarouselShowHoursWorked
    {
        /// <summary>
        /// Show hours worked card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="showWorkedHoursResponse">List of DateTotals object.</param>
        /// <param name="payPeriod">Pay period.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <param name="regular">Regular hours.</param>
        /// <param name="overtime">Overtime hours.</param>
        /// <param name="allHours">All hours.</param>
        /// <returns>A task.</returns>
        public async Task ShowHoursWorkedData(IDialogContext context, Response showWorkedHoursResponse, string payPeriod, string startDate, string endDate, string regular, string overtime, string allHours)
        {
            var showHoursWorkedOrderedList = showWorkedHoursResponse?.Timesheet?.DailyTotals?.DateTotals?.OrderByDescending(x => x.Date).ToList();
            var reply = ((Activity)context.Activity).CreateReply();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            var pageSize = 3;
            var startDateTime = DateTime.Parse(startDate, CultureInfo.InvariantCulture, DateTimeStyles.None);
            var endDateTime = DateTime.Parse(endDate, CultureInfo.InvariantCulture, DateTimeStyles.None);
            var titles = new StringBuilder();

            // Response has data
            if (showHoursWorkedOrderedList.Any())
            {
                showHoursWorkedOrderedList = showHoursWorkedOrderedList.Where(x => DateTime.Parse(x.Date, CultureInfo.InvariantCulture, DateTimeStyles.None) >= startDateTime && DateTime.Parse(x.Date, CultureInfo.InvariantCulture, DateTimeStyles.None) <= endDateTime && x.GrandTotal != "0:00").ToList();

                if (showHoursWorkedOrderedList.Count == 0)
                {
                    titles.Append(this.NoReportedWorkedHours(context, payPeriod, startDate, endDate));
                    ProcessEmptyResponse(payPeriod, reply, titles);
                }
                else
                {
                    await ProcessResponse(context, payPeriod, startDate, endDate, regular, overtime, allHours, showHoursWorkedOrderedList, reply, pageSize, titles);
                }
            }

            // Response is null
            else
            {
                titles.Append(this.NoReportedWorkedHours(context, payPeriod, startDate, endDate));
                ProcessEmptyResponse(payPeriod, reply, titles);
            }

            await context.PostAsync(reply);
        }

        private static async Task ProcessResponse(IDialogContext context, string payPeriod, string startDate, string endDate, string regular, string overtime, string allHours, List<DateTotals> showHoursWorkedOrderedList, Activity reply, int pageSize, StringBuilder titles)
        {
            var pageCount = Math.Ceiling((double)showHoursWorkedOrderedList.Count / pageSize);
            pageCount = pageCount > 10 ? 10 : pageCount;

            if (!string.IsNullOrWhiteSpace(payPeriod))
            {
                await context.PostAsync(KronosResourceText.HoursWorkedText.Replace("{payPeriod}", payPeriod));
            }
            else
            {
                await context.PostAsync(KronosResourceText.HoursWorkedDateRangeText.Replace("{startDate}", startDate).Replace("{endDate}", endDate));
            }

            for (int i = 0; i < pageCount; i++)
            {
                var heroCard = new HeroCard();

                foreach (var response in showHoursWorkedOrderedList.Skip(pageSize * i).Take(pageSize))
                {
                    var date = DateTime.Parse(response.Date, CultureInfo.InvariantCulture, DateTimeStyles.None);

                    var grandTotal = Convert.ToDouble(response.GrandTotal.Replace(':', '.'));
                    titles.Append($"<br><b><u>{DateTime.Parse(response.Date, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dddd, dd MMMM", CultureInfo.InvariantCulture)}</b></u><br>");
                    titles.Append(KronosResourceText.WorkedShiftsText.Replace("{grandTotal}", Convert.ToString(grandTotal)));
                }

                var buttons = new List<CardAction>();

                if (payPeriod.ToLower() != Constants.PreviousPayPeriodPunches)
                {
                    buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.PreviousWeek, displayText: Constants.PreviousPayPeriodPunchesText, text: Constants.PreviousPayPeriodHoursWorkedText, value: string.Empty));
                }

                if (payPeriod.ToLower() != Constants.CurrentpayPeriodPunches)
                {
                    buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.CurrentWeek, displayText: Constants.CurrentpayPeriodPunchesText, text: Constants.CurrentpayPeriodHoursWorkedText, value: string.Empty));
                }

                buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.DateRange, displayText: Constants.DateRangeText, text: Constants.DateRangeHoursWorked, value: string.Empty));

                heroCard.Buttons = buttons;
                heroCard.Text = titles.ToString();
                reply.Attachments.Add(heroCard.ToAttachment());

                titles.Clear();
                foreach (var item in reply.Attachments)
                {
                    ((HeroCard)item.Content).Title = KronosResourceText.TotalHourdWorked.Replace("{totalWorked}", allHours);
                    ((HeroCard)item.Content).Subtitle = KronosResourceText.RegularOvertimeText.Replace("{regular}", regular).Replace("{overtime}", overtime);
                }
            }
        }

        private static void ProcessEmptyResponse(string payPeriod, Activity reply, StringBuilder titles)
        {
            var buttons = new List<CardAction>();

            if (payPeriod.ToLower() != Constants.PreviousPayPeriodPunches)
            {
                buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.PreviousWeek, displayText: Constants.PreviousPayPeriodPunchesText, text: Constants.PreviousPayPeriodHoursWorkedText, value: string.Empty));
            }

            if (payPeriod.ToLower() != Constants.CurrentpayPeriodPunches)
            {
                buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.CurrentWeek, displayText: Constants.CurrentpayPeriodPunchesText, text: Constants.CurrentpayPeriodHoursWorkedText, value: string.Empty));
            }

            buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.DateRange, displayText: Constants.DateRangeText, text: Constants.DateRangeHoursWorked, value: string.Empty));

            var heroCard = new HeroCard();

            heroCard.Buttons = buttons;
            heroCard.Text = titles.ToString();
            reply.Attachments.Add(heroCard.ToAttachment());
        }

        /// <summary>
        /// Show message if no reported work hours found.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="payPeriod">Pay period.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <returns>String value.</returns>
        private string NoReportedWorkedHours(IDialogContext context, string payPeriod, string startDate, string endDate)
        {
            string noWorkHourMsg = string.Empty;
            if (!string.IsNullOrWhiteSpace(payPeriod))
            {
                noWorkHourMsg = KronosResourceText.NoWorkHoursPeriod.Replace("{payPeriod}", payPeriod);
            }
            else
            {
                noWorkHourMsg = KronosResourceText.NoWorkHoursDateRange.Replace("{startDate}", startDate).Replace("{endDate}", endDate);
            }

            return noWorkHourMsg;
        }
    }
}
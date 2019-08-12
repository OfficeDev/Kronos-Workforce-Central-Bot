//-----------------------------------------------------------------------
// <copyright file="CarouselTeamOvertimes.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.CarouselCards
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// Show all overtimes card.
    /// </summary>
    [Serializable]
    public class CarouselTeamOvertimes
    {
        /// <summary>
        /// Show all overtimes card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="overtimeEmployeesList">List of employees with overtimes.</param>
        /// <param name="payPeriod">Pay period.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <returns>A task.</returns>
        public async Task ShowOvertimeEmployeesData(IDialogContext context, List<string> overtimeEmployeesList, string payPeriod, string startDate, string endDate)
        {
            var reply = ((Activity)context.Activity).CreateReply();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            var pageSize = 10;
            var buttons = new List<CardAction>();

            if (payPeriod != Constants.PreviousPayPeriodPunchesText)
            {
                buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.PreviousWeek, displayText: Constants.PreviousPayPeriodPunchesText, text: Constants.PreviousWeekTeamOvertimes));
            }

            if (payPeriod != Constants.CurrentpayPeriodPunchesText)
            {
                buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.CurrentWeek, displayText: Constants.CurrentpayPeriodPunchesText, text: Constants.CurrentWeekTeamOvertimes));
            }

            buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.DateRange, displayText: Constants.DateRangeText, text: Constants.DateRangeTeamOvertimes));

            if (overtimeEmployeesList.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(payPeriod))
                {
                    await context.PostAsync(KronosResourceText.TeamOvertimeDataFound.Replace("{PayPeriod}", payPeriod));
                }
                else
                {
                    await context.PostAsync(KronosResourceText.TeamOvertimeDataFoundDateRange.Replace("{StartDate}", startDate).Replace("{EndDate}", endDate));
                }

                var pageCount = Math.Ceiling((double)overtimeEmployeesList.Count / pageSize);
                pageCount = pageCount > 10 ? 10 : pageCount;

                for (int i = 0; i < pageCount; i++)
                {
                    var heroCard = new HeroCard();
                    var titles = new StringBuilder();

                    foreach (var item in overtimeEmployeesList.Skip(pageSize * i).Take(pageSize))
                    {
                        titles.Append(item);
                    }

                    heroCard.Text = titles.ToString();
                    heroCard.Buttons = buttons;
                    reply.Attachments.Add(heroCard.ToAttachment());
                }

                foreach (var item in reply.Attachments)
                {
                    ((HeroCard)item.Content).Title = KronosResourceText.TeamOvertimesTitle;
                }

                await context.PostAsync(reply);
            }
            else
            {
                var heroCard = new HeroCard();
                if (!string.IsNullOrWhiteSpace(payPeriod))
                {
                    heroCard.Text = KronosResourceText.TeamOvertimeNoDataFound.Replace("{PayPeriod}", payPeriod);
                }
                else
                {
                    heroCard.Text = KronosResourceText.TeamOvertimeNoDataFoundDateRange.Replace("{StartDate}", startDate).Replace("{EndDate}", endDate);
                }

                heroCard.Buttons = buttons;
                reply.Attachments.Add(heroCard.ToAttachment());
                await context.PostAsync(reply);
            }
        }
    }
}
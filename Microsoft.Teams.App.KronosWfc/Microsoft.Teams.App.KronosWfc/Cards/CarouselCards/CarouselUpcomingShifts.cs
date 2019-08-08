//-----------------------------------------------------------------------
// <copyright file="CarouselUpcomingShifts.cs" company="Microsoft">
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
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Shifts.UpcomingShifts;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// upcoming shift card class.
    /// </summary>
    [Serializable]
    public class CarouselUpcomingShifts
    {
        /// <summary>
        /// get partial list.
        /// </summary>
        /// <typeparam name="T">list object type.</typeparam>
        /// <param name="list">list object.</param>
        /// <param name="page">page number.</param>
        /// <param name="pageSize">page size.</param>
        /// <returns>paginated list.</returns>
        public IList<T> GetPage<T>(IList<T> list, int page, int pageSize)
        {
            return list.Skip(page * pageSize).Take(pageSize).ToList();
        }

        /// <summary>
        /// show upcoming shifts card.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="data">shifts data.</param>
        /// <param name="message">user text.</param>
        /// <returns>upcoming shifts card.</returns>
        public async Task ShowUpcomingShiftsCard(IDialogContext context, Response data, string message)
        {
            var replyMessage = context.MakeMessage();
            string datePeriod = string.Empty;
            string shift = string.Empty;
            string startTime = string.Empty;
            string endTime = string.Empty;

            StringBuilder str = new StringBuilder();

            List<Attachment> attachments = new List<Attachment>();
            List<ScheduleShift> scheduleShifts = data.Schedule?.ScheduleItems?.ScheduleShift?.OrderBy(x => x.StartDate).ToList();
            int count = (scheduleShifts?.Count > 0) ? (int)Math.Ceiling((double)scheduleShifts.Count / 3) : 0;
            count = count > 10 ? 10 : count;
            for (int i = 0; i < count; i++)
            {
                IList<ScheduleShift> specificList = this.GetPage(scheduleShifts, i, 3);
                foreach (ScheduleShift scheduleShift in specificList)
                {
                    datePeriod = DateTime.Parse(scheduleShift.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
                    str.Append($"<u><b>{datePeriod}</b></u>");
                    foreach (ShiftSegment shiftSegment in scheduleShift.ShiftSegments)
                    {
                        shift = shiftSegment.SegmentTypeName;
                        startTime = shiftSegment.StartTime;
                        endTime = shiftSegment.EndTime;
                        str.Append($"<br/>{shift} - {startTime} to {endTime}");
                    }

                    str.Append("<br/>");
                }

                var heroCard = new HeroCard
                {
                    Title = KronosResourceText.YourShiftsTitle,
                    Text = str.ToString(),
                    Buttons = this.GetButtons(message),
                };
                str.Clear();
                attachments.Add(heroCard.ToAttachment());
            }

            if (count == 0)
            {
                var heroCard = new HeroCard
                {
                    Title = KronosResourceText.YourShiftsTitle,
                    Text = KronosResourceText.NoShifts,
                    Buttons = this.GetButtons(message),
                };
                str.Clear();
                attachments.Add(heroCard.ToAttachment());
            }

            replyMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            replyMessage.Attachments = attachments;
            await context.PostAsync(replyMessage);
        }

        /// <summary>
        /// get buttons based on message.
        /// </summary>
        /// <param name="message">user message.</param>
        /// <returns>list of buttons.</returns>
        public List<CardAction> GetButtons(string message)
        {
            if (message.Contains(KronosResourceText.Current))
            {
                return new List<CardAction>
                    {
                        new CardAction(ActionTypes.MessageBack, title: Constants.NextWeek, displayText: Constants.NextWeek, text: Constants.NextWeekShift, value: string.Empty),
                        new CardAction(ActionTypes.MessageBack, title: Constants.DateRangeText, displayText: Constants.DateRangeText, text: Constants.DateRangeShift, value: string.Empty),
                    };
            }
            else if (message.Contains(KronosResourceText.Next))
            {
                return new List<CardAction>
                    {
                        new CardAction(ActionTypes.MessageBack, title: Constants.CurrentWeek, displayText: Constants.CurrentWeek, text: Constants.CurrentWeekShift, value: string.Empty),
                        new CardAction(ActionTypes.MessageBack, title: Constants.DateRangeText, displayText: Constants.DateRangeText, text: Constants.DateRangeShift, value: string.Empty),
                    };
            }
            else
            {
                return new List<CardAction>
                    {
                        new CardAction(ActionTypes.MessageBack, title: Constants.CurrentWeek, displayText: Constants.CurrentWeek, text: Constants.CurrentWeekShift, value: string.Empty),
                        new CardAction(ActionTypes.MessageBack, title: Constants.NextWeek, displayText: Constants.NextWeek, text: Constants.NextWeekShift, value: string.Empty),
                        new CardAction(ActionTypes.MessageBack, title: Constants.DateRangeText, displayText: Constants.DateRangeText, text: Constants.DateRangeShift, value: string.Empty),
                    };
            }
        }

        /// <summary>
        /// show date range card.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="command">user command.</param>
        /// <returns>date range card.</returns>
        public async Task ShowDateRange(IDialogContext context, string command)
        {
            var message = context.MakeMessage();

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
                                        Size = AdaptiveTextSize.Large,
                                        Text = Resources.KronosResourceText.EnterTimeframe,
                                        Weight = AdaptiveTextWeight.Bolder,
                                    },
                                     new AdaptiveTextBlock
                                    {
                                        Text = "Start Date",
                                    },
                                     new AdaptiveDateInput
                                    {
                                        Id = "StartDate",
                                        Placeholder = KronosResourceText.EnterStartDate,
                                    },
                                     new AdaptiveTextBlock
                                    {
                                        Text = "End Date",
                                    },
                                     new AdaptiveDateInput
                                    {
                                        Id = "EndDate",
                                        Placeholder = KronosResourceText.EnterEndDate,
                                    },
                                },
                            },
                        },
            };

            card.Actions.Add(
                            new AdaptiveSubmitAction()
                            {
                                Title = "Submit",
                                Data = new AdaptiveCardAction
                                {
                                    msteams = new Msteams
                                    {
                                        type = "messageBack",
                                        text = command,
                                    },
                                },
                            });

            message.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
            });

            await context.PostAsync(message);
        }
    }
}
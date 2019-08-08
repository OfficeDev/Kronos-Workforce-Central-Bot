//-----------------------------------------------------------------------
// <copyright file="HeroShowSchedule.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Cards.HeroCards
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
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Schedule;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// hero show schedule card class.
    /// </summary>
    [Serializable]
    public class HeroShowSchedule
    {
        /// <summary>
        /// get partial list.
        /// </summary>
        /// <typeparam name="T">list object type.</typeparam>
        /// <param name="list">list object.</param>
        /// <param name="page">page number.</param>
        /// <param name="pageSize">page size.</param>
        /// <returns>paginated list</returns>
        public IList<T> GetPage<T>(IList<T> list, int page, int pageSize)
        {
            return list.Skip(page * pageSize).Take(pageSize).ToList();
        }

        /// <summary>
        /// show schedule card.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="data">schedule data.</param>
        /// <param name="message">user command.</param>
        /// <returns>schedule card.</returns>
        public async Task ShowScheduleCard(IDialogContext context, Response data, string message)
        {
            var replyMessage = context.MakeMessage();
            string datePeriod = string.Empty;
            string shift = string.Empty;
            string startTime = string.Empty;
            string endTime = string.Empty;
            HeroCard card = new HeroCard();
            StringBuilder str = new StringBuilder();
            string dateCounter = string.Empty;
            List<Attachment> attachments = new List<Attachment>();
            List<object> items = data.Schedule?.ScheduleItems?.Items?.ToList();

            int count = (items?.Count > 0) ? (int)Math.Ceiling((double)items.Count / 5) : 0;
            count = count > 10 ? 10 : count;
            for (int i = 0; i < count; i++)
            {
                IList<object> specificList = this.GetPage(items, i, 5);
                foreach (dynamic scheduleShift in specificList)
                {
                    datePeriod = DateTime.Parse(scheduleShift.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
                    if (dateCounter != datePeriod)
                    {
                        str.Append($"<u><b>{datePeriod}</b></u>");
                    }

                    if (scheduleShift as SchedulePayCodeEdit != null)
                    {
                        if (dateCounter != datePeriod)
                        {
                            str.Append($"<br/>{Resources.KronosResourceText.TimeOffRequstText} - {((SchedulePayCodeEdit)scheduleShift).PayCodeName}");
                        }
                        else
                        {
                            str.Append($"{Resources.KronosResourceText.TimeOffRequstText} - {((SchedulePayCodeEdit)scheduleShift).PayCodeName}");
                        }
                    }

                    if (scheduleShift as ScheduleShift != null)
                    {
                        shift = KronosResourceText.Shift;
                        startTime = ((ScheduleShift)scheduleShift).ShiftSegments.FirstOrDefault().StartTime;
                        endTime = ((ScheduleShift)scheduleShift).ShiftSegments.LastOrDefault().EndTime;
                        if (dateCounter != datePeriod)
                        {
                            str.Append($"<br/>{shift} - {startTime} to {endTime}");
                        }
                        else
                        {
                            str.Append($"{shift} - {startTime} to {endTime}");
                        }
                    }

                    dateCounter = datePeriod;
                    if (dateCounter != datePeriod)
                    {
                        continue;
                    }

                    str.Append("<br/>");
                }

                var heroCard = new HeroCard
                {
                    Title = KronosResourceText.YourScheduleTitle,
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
                    Title = KronosResourceText.YourScheduleTitle,
                    Text = KronosResourceText.NoSchedule,
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
        /// get button list based on user command.
        /// </summary>
        /// <param name="message">user command.</param>
        /// <returns>button list.</returns>
        public List<CardAction> GetButtons(string message)
        {
            if (message.Contains(KronosResourceText.Current))
            {
                return new List<CardAction>
                    {
                        new CardAction(ActionTypes.MessageBack, title: Constants.NextWeek, displayText: Constants.NextWeek, text: Constants.NextWeekSchedule, value: string.Empty),
                        new CardAction(ActionTypes.MessageBack, title: Constants.DateRangeText, displayText: Constants.DateRangeText, text: Constants.DateRangeSchedule, value: string.Empty),
                    };
            }
            else if (message.Contains(KronosResourceText.Next))
            {
                return new List<CardAction>
                    {
                        new CardAction(ActionTypes.MessageBack, title: Constants.CurrentWeek, displayText: Constants.CurrentWeek, text: Constants.CurrentWeekSchedule, value: string.Empty),
                        new CardAction(ActionTypes.MessageBack, title: Constants.DateRangeText, displayText: Constants.DateRangeText, text: Constants.DateRangeSchedule, value: string.Empty),
                    };
            }
            else
            {
                return new List<CardAction>
                    {
                        new CardAction(ActionTypes.MessageBack, title: Constants.CurrentWeek, displayText: Constants.CurrentWeek, text: Constants.CurrentWeekSchedule, value: string.Empty),
                        new CardAction(ActionTypes.MessageBack, title: Constants.NextWeek, displayText: Constants.NextWeek, text: Constants.NextWeekSchedule, value: string.Empty),
                        new CardAction(ActionTypes.MessageBack, title: Constants.DateRangeText, displayText: Constants.DateRangeText, text: Constants.DateRangeSchedule, value: string.Empty),
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
            // var data = JsonConvert.SerializeObject(Constants.SubmitDateRangeSchedule);
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
                                        Text = KronosResourceText.EnterTimeframeForSchedule,
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
namespace Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json;
    using HoursAlias = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Hours;
    using ScheduleAlias = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Schedule;

    /// <summary>
    /// show schedule card.
    /// </summary>
    [Serializable]
    public class AdaptiveSchedule
    {
        /// <summary>
        /// show schedule card.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="data">schedule data.</param>
        /// <param name="showWorkedHoursResponse">hours worked data.</param>
        /// <param name="allHours">total worked hours.</param>
        /// <param name="message">user command.</param>
        /// <param name="todayDate">current date.</param>
        /// <param name="startDate">start date.</param>
        /// <param name="endDate">end date.</param>
        /// <returns>schedule card.</returns>
        public async Task ShowScheduleHoursCard(IDialogContext context, ScheduleAlias.Response data, HoursAlias.Response showWorkedHoursResponse, string allHours, string message, string todayDate, string startDate, string endDate)
        {
            var replyMessage = context.MakeMessage();
            string datePeriod = string.Empty;
            string shift = string.Empty;
            string startTime = string.Empty;
            string endTime = string.Empty;
            StringBuilder str = new StringBuilder();
            string dateCounter = string.Empty;
            StringBuilder tempContent = new StringBuilder();
            string workedHours = string.Empty;
            double assignedHours = default(double);
            DateTime stDateTime = default(DateTime);
            DateTime eDateTime = default(DateTime);

            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/Schedule/MainSchedule.json");
            var mainCard = File.ReadAllText(fullPath);
            mainCard = mainCard.Replace("{txt_CardTitle}", KronosResourceText.YourScheduleTitle);

            List<object> items = data.Schedule?.ScheduleItems?.Items?.ToList();

            fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/Schedule/ScheduleContent.json");
            var content = File.ReadAllText(fullPath);

            int count = (items?.Count > 0) ? items.Count : 0;
            if (count == 0)
            {
                var noSchedule = new
                {
                    type = "TextBlock",
                    text = KronosResourceText.NoSchedule,
                };
                string noScheduleJson = JsonConvert.SerializeObject(noSchedule);

                mainCard = mainCard.Replace("{card_Content}", noScheduleJson);
            }
            else
            {
                // hours worked
                var showHoursWorkedOrderedList = showWorkedHoursResponse?.Timesheet?.DailyTotals?.DateTotals.OrderBy(x => x.Date).ToList();
                showHoursWorkedOrderedList = showHoursWorkedOrderedList.Where(x => Convert.ToDateTime(x.Date) >= Convert.ToDateTime(startDate) && Convert.ToDateTime(x.Date) <= Convert.ToDateTime(endDate) && x.GrandTotal != "0:00").ToList();

                foreach (dynamic scheduleShift in items)
                {
                    datePeriod = DateTime.Parse(scheduleShift.StartDate).ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
                    if (dateCounter != datePeriod)
                    {
                        str.Append(tempContent.ToString() + ",");
                        tempContent.Clear();
                        if (datePeriod == DateTime.Parse(todayDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture))
                        {
                            tempContent.Append(content.Replace("{barImg}", ConfigurationManager.AppSettings["BaseUri"] + "/Static/Images/VioletBar.png")
                                                    .Replace("{txt_DatePeriod}", datePeriod)
                                                    .Replace("{wght_DatePeriod}", "Bolder")
                                                    .Replace("{imp_DatePeriod}", "false"));
                        }
                        else if (Convert.ToDateTime(scheduleShift.StartDate) > Convert.ToDateTime(todayDate))
                        {
                            tempContent.Append(content.Replace("{txt_DatePeriod}", datePeriod)
                                                .Replace("{wght_DatePeriod}", "Default")
                                                .Replace("{imp_DatePeriod}", "false"));
                        }
                        else
                        {
                            tempContent.Append(content.Replace("{txt_DatePeriod}", datePeriod)
                                                    .Replace("{wght_DatePeriod}", "Default")
                                                    .Replace("{imp_DatePeriod}", "true"));
                        }
                    }

                    // paycode edit
                    if (scheduleShift as ScheduleAlias.SchedulePayCodeEdit != null)
                    {
                        if (dateCounter != datePeriod)
                        {
                            if (datePeriod == DateTime.Parse(todayDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture))
                            {
                                tempContent.Replace("{txt_TimeOff}", $"{KronosResourceText.TimeOffRequstText} - {((ScheduleAlias.SchedulePayCodeEdit)scheduleShift).PayCodeName}")
                                .Replace("{wght_TimeOff}", "Bolder")
                                .Replace("{imp_TimeOff}", "false");
                            }
                            else if (Convert.ToDateTime(scheduleShift.StartDate) > Convert.ToDateTime(todayDate))
                            {
                                tempContent.Replace("{txt_TimeOff}", $"{KronosResourceText.TimeOffRequstText} - {((ScheduleAlias.SchedulePayCodeEdit)scheduleShift).PayCodeName}")
                               .Replace("{wght_TimeOff}", "Default")
                               .Replace("{imp_TimeOff}", "false");
                            }
                            else
                            {
                                tempContent.Replace("{txt_TimeOff}", $"{KronosResourceText.TimeOffRequstText} - {((ScheduleAlias.SchedulePayCodeEdit)scheduleShift).PayCodeName}")
                                .Replace("{wght_TimeOff}", "Default")
                                .Replace("{imp_TimeOff}", "true");
                            }
                        }
                        else
                        {
                            if (datePeriod == DateTime.Parse(todayDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture))
                            {
                                tempContent.Replace("{txt_TimeOff}", $"{KronosResourceText.TimeOffRequstText} - {((ScheduleAlias.SchedulePayCodeEdit)scheduleShift).PayCodeName}")
                                .Replace("{wght_TimeOff}", "Bolder")
                                .Replace("{imp_TimeOff}", "false");
                            }
                            else if (DateTime.Parse(scheduleShift.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None) > DateTime.Parse(todayDate, CultureInfo.InvariantCulture, DateTimeStyles.None))
                            {
                                tempContent.Replace("{txt_TimeOff}", $"{KronosResourceText.TimeOffRequstText} - {((ScheduleAlias.SchedulePayCodeEdit)scheduleShift).PayCodeName}")
                               .Replace("{wght_TimeOff}", "Default")
                               .Replace("{imp_TimeOff}", "false");
                            }
                            else
                            {
                                tempContent = tempContent.Replace("{txt_TimeOff}", $"{KronosResourceText.TimeOffRequstText} - {((ScheduleAlias.SchedulePayCodeEdit)scheduleShift).PayCodeName}")
                                .Replace("{wght_TimeOff}", "Default")
                                .Replace("{imp_TimeOff}", "true");
                            }
                        }
                    }

                    // schedule shift
                    if (scheduleShift as ScheduleAlias.ScheduleShift != null)
                    {
                        shift = KronosResourceText.Shift;
                        var shiftSegment = ((ScheduleAlias.ScheduleShift)scheduleShift).ShiftSegments.FirstOrDefault();
                        startTime = shiftSegment.StartTime;
                        endTime = shiftSegment.EndTime;
                        stDateTime = Convert.ToDateTime($"{shiftSegment.StartDate} {startTime}");
                        eDateTime = Convert.ToDateTime($"{shiftSegment.EndDate} {endTime}");

                        assignedHours = assignedHours + Math.Abs(Math.Round((stDateTime - eDateTime).TotalHours, 2));

                        dynamic workResponse = showHoursWorkedOrderedList.Where(x => x.Date == scheduleShift.StartDate).FirstOrDefault();

                        if (workResponse != null)
                        {
                            workedHours = " | " + workResponse.GrandTotal.Replace(':', '.') + " " + KronosResourceText.HrsWorkedTxt;
                        }

                        if (dateCounter != datePeriod)
                        {
                            if (datePeriod == DateTime.Parse(todayDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture))
                            {
                                tempContent.Replace("{txt_Shift}", $"{shift} - {startTime} to {endTime} {workedHours}")
                                .Replace("{wght_Shift}", "Bolder")
                                .Replace("{imp_Shift}", "false");
                            }
                            else if (DateTime.Parse(scheduleShift.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None) > DateTime.Parse(todayDate, CultureInfo.InvariantCulture))
                            {
                                tempContent.Replace("{txt_Shift}", $"{shift} - {startTime} to {endTime} {workedHours}")
                                .Replace("{wght_Shift}", "Default")
                                .Replace("{imp_Shift}", "false");
                            }
                            else
                            {
                                tempContent.Replace("{txt_Shift}", $"{shift} - {startTime} to {endTime} {workedHours}")
                                .Replace("{wght_Shift}", "Default")
                                .Replace("{imp_Shift}", "true");
                            }
                        }
                        else
                        {
                            if (datePeriod == DateTime.Parse(todayDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture))
                            {
                                tempContent.Replace("{txt_Shift}", $"{shift} - {startTime} to {endTime} {workedHours}")
                                .Replace("{wght_Shift}", "Bolder")
                                .Replace("{imp_Shift}", "false");
                            }
                            else if (DateTime.Parse(scheduleShift.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None) > DateTime.Parse(todayDate, CultureInfo.InvariantCulture, DateTimeStyles.None))
                            {
                                tempContent.Replace("{txt_Shift}", $"{shift} - {startTime} to {endTime} {workedHours}")
                                .Replace("{wght_Shift}", "Default")
                                .Replace("{imp_Shift}", "false");
                            }
                            else
                            {
                                tempContent.Replace("{txt_Shift}", $"{shift} - {startTime} to {endTime} {workedHours}")
                                .Replace("{wght_Shift}", "Default")
                                .Replace("{imp_Shift}", "true");
                            }
                        }
                    }

                    // storing previous date
                    dateCounter = datePeriod;
                    workedHours = string.Empty;
                }

                str.Append(tempContent.ToString() + ",");
                tempContent.Clear();

                // replace unreplaced strings
                str.Replace("{wght_Shift}", "Default")
                    .Replace("{txt_Shift}", null)
                    .Replace("{imp_Shift}", "true")
                    .Replace("{wght_TimeOff}", "Default")
                    .Replace("{txt_TimeOff}", null)
                    .Replace("{imp_TimeOff}", "true")
                    .Replace("{barImg}", ConfigurationManager.AppSettings["BaseUri"] + "/Static/Images/TransperantBar.png");

                mainCard = mainCard.Replace("{card_Content}", str.ToString().TrimStart(',').TrimEnd(','));
                str.Clear();
            }

            mainCard = mainCard.Replace("{txt_Totalhrs}", KronosResourceText.TotalhrsWorkedTxt + ":").Replace("{val_TotalHrs}", KronosResourceText.TotalhrsWorkedTxtVal.Replace("{allHours}", allHours).Replace("{assignedHours}", string.Format("{0:0.00}", assignedHours)));
            var card = AdaptiveCard.FromJson(mainCard).Card;

            card.Actions.AddRange(this.GetButtons(message));
            replyMessage.Attachments.Add(
                new Attachment
                {
                    Content = card,
                    ContentType = "application/vnd.microsoft.card.adaptive",
                });

            await context.PostAsync(replyMessage);
        }

        /// <summary>
        /// get button list based on user command.
        /// </summary>
        /// <param name="message">user command.</param>
        /// <returns>button list.</returns>
        public List<AdaptiveSubmitAction> GetButtons(string message)
        {
            if (message.Contains(KronosResourceText.Current))
            {
               return new List<AdaptiveSubmitAction>
                {
                       new AdaptiveSubmitAction()
                        {
                            Title = Constants.NextWeek,
                            Data = new AdaptiveCardAction
                            {
                                msteams = new Msteams
                                {
                                    type = "messageBack",
                                    text = Constants.NextWeekSchedule,
                                },
                            },
                        },
                       new AdaptiveSubmitAction()
                        {
                            Title = Constants.DateRangeText,
                            Data = new AdaptiveCardAction
                            {
                                msteams = new Msteams
                                {
                                    type = "messageBack",
                                    text = Constants.DateRangeSchedule,
                                },
                            },
                        },
                };
            }
            else if (message.Contains(KronosResourceText.Next))
            {
               return new List<AdaptiveSubmitAction>
                {
                       new AdaptiveSubmitAction()
                        {
                            Title = Constants.CurrentWeek,
                            Data = new AdaptiveCardAction
                            {
                                msteams = new Msteams
                                {
                                    type = "messageBack",
                                    text = Constants.CurrentWeekSchedule,
                                },
                            },
                        },
                       new AdaptiveSubmitAction()
                        {
                            Title = Constants.DateRangeText,
                            Data = new AdaptiveCardAction
                            {
                                msteams = new Msteams
                                {
                                    type = "messageBack",
                                    text = Constants.DateRangeSchedule,
                                },
                            },
                        },
                };
            }
            else
            {
                return new List<AdaptiveSubmitAction>
                {
                       new AdaptiveSubmitAction()
                        {
                            Title = Constants.CurrentWeek,
                            Data = new AdaptiveCardAction
                            {
                                msteams = new Msteams
                                {
                                    type = "messageBack",
                                    text = Constants.CurrentWeekSchedule,
                                },
                            },
                        },
                       new AdaptiveSubmitAction()
                        {
                            Title = Constants.NextWeek,
                            Data = new AdaptiveCardAction
                            {
                                msteams = new Msteams
                                {
                                    type = "messageBack",
                                    text = Constants.NextWeekSchedule,
                                },
                            },
                        },
                       new AdaptiveSubmitAction()
                        {
                            Title = Constants.DateRangeText,
                            Data = new AdaptiveCardAction
                            {
                                msteams = new Msteams
                                {
                                    type = "messageBack",
                                    text = Constants.DateRangeSchedule,
                                },
                            },
                        },
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
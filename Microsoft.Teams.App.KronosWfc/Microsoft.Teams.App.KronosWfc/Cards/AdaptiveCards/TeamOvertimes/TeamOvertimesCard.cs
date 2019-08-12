//-----------------------------------------------------------------------
// <copyright file="TeamOvertimesCard.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.TeamOvertimesCard
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Show all overtimes card.
    /// </summary>
    [Serializable]
    public class TeamOvertimesCard
    {
        /// <summary>
        /// Show all overtimes card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="payPeriod">Pay period.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <param name="currentPage">Current page number.</param>
        /// <returns>Adaptive card.</returns>
        public AdaptiveCard GetCard(IDialogContext context, string payPeriod, string startDate, string endDate, int currentPage)
        {
            var pagewiseHashtable = context.PrivateConversationData.GetValue<Hashtable>("PagewiseOvertimes");
            List<string> overtimeEmployeesList = new List<string>();
            if (pagewiseHashtable.ContainsKey((currentPage - 1).ToString()))
            {
                overtimeEmployeesList = JsonConvert.DeserializeObject<List<string>>(Convert.ToString(pagewiseHashtable[(currentPage - 1).ToString()]));
            }

            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TeamOvertimes/MainCard.json");
            var mainCard = File.ReadAllText(fullPath);
            var buttons = new List<AdaptiveAction>();

            if (pagewiseHashtable.Count > 0)
            {
                if (currentPage == 1)
                {
                    if (currentPage != pagewiseHashtable.Count)
                    {
                        buttons.Add(new AdaptiveSubmitAction()
                        {
                            Title = KronosResourceText.NextButton,
                            Data = new Data
                            {
                                msteams = new Msteams
                                {
                                    type = "messageBack",
                                    displayText = null,
                                    text = Constants.OvertimeNext,
                                },
                                CurrentPage = currentPage.ToString(),
                                PayPeriod = payPeriod,
                            },
                        });
                    }
                }
                else
                {
                    buttons.Add(new AdaptiveSubmitAction()
                    {
                        Title = KronosResourceText.Previous,
                        Data = new Data
                        {
                            msteams = new Msteams
                            {
                                type = "messageBack",
                                displayText = null,
                                text = Constants.OvertimePrevious,
                            },
                            CurrentPage = currentPage.ToString(),
                            PayPeriod = payPeriod,
                        },
                    });

                    if (currentPage != pagewiseHashtable.Count)
                    {
                        buttons.Add(new AdaptiveSubmitAction()
                        {
                            Title = KronosResourceText.NextButton,
                            Data = new Data
                            {
                                msteams = new Msteams
                                {
                                    type = "messageBack",
                                    displayText = null,
                                    text = Constants.OvertimeNext,
                                },
                                CurrentPage = currentPage.ToString(),
                                PayPeriod = payPeriod,
                            },
                        });
                    }
                }
            }

            if (payPeriod != Constants.PreviousPayPeriodPunchesText)
            {
                buttons.Add(new AdaptiveSubmitAction()
                {
                    Title = KronosResourceText.PreviousWeek,
                    Data = new Data
                    {
                        msteams = new Msteams
                        {
                            type = "messageBack",
                            displayText = null,
                            text = Constants.PreviousWeekTeamOvertimes,
                        },
                    },
                });
            }

            if (payPeriod != Constants.CurrentpayPeriodPunchesText)
            {
                buttons.Add(new AdaptiveSubmitAction()
                {
                    Title = KronosResourceText.CurrentWeek,
                    Data = new Data
                    {
                        msteams = new Msteams
                        {
                            type = "messageBack",
                            displayText = null,
                            text = Constants.CurrentWeekTeamOvertimes,
                        },
                    },
                });
            }

            buttons.Add(new AdaptiveSubmitAction()
            {
                Title = KronosResourceText.DateRange,
                Data = new Data
                {
                    msteams = new Msteams
                    {
                        type = "messageBack",
                        displayText = null,
                        text = Constants.DateRangeTeamOvertimes,
                    },
                },
            });

            if (overtimeEmployeesList.Count > 0)
            {
                var payperiod = !string.IsNullOrWhiteSpace(payPeriod) ? payPeriod.ToLower() : KronosResourceText.DateRange.ToLowerInvariant();
                var total = JsonConvert.DeserializeObject<List<string>>(Convert.ToString(pagewiseHashtable[(pagewiseHashtable.Count - 1).ToString()])).Count + ((pagewiseHashtable.Count - 1) * 5);
                mainCard = mainCard.Replace("{txt_CardTitle}", KronosResourceText.TeamOTCardTitle.Replace("{First}", ((currentPage * 5) - 4).ToString()).Replace("{Last}", (((currentPage * 5) - 4) + (overtimeEmployeesList.Count - 1)).ToString()).Replace("{Total}", total.ToString()).Replace("{Payperiod}", payperiod));
                mainCard = mainCard.Replace("{ShowList}", "true").Replace("{NoOverTime}", "false").Replace("{txt_NoOverTime}", null);
                var row = File.ReadAllText(HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TeamOvertimes/Row.json"));
                StringBuilder rows = new StringBuilder();
                for (int i = 0; i < overtimeEmployeesList.Count; i++)
                {
                    string item = row;
                    var split = overtimeEmployeesList[i].Split('-');
                    item = item.Replace("{Name}", split[0]).Replace("{Overtime}", split[2]).Replace("{Role}", split[1]);
                    if (i == 0)
                    {
                        item = item.Replace("{Separator}", "false").Replace("{Spacing}", "Large");
                        rows.Append(item);
                    }
                    else
                    {
                        item = item.Replace("{Separator}", "true").Replace("{Spacing}", "Medium");
                        rows.Append(", " + item);
                    }
                }

                mainCard = mainCard.Replace("{List}", rows.ToString());
            }
            else
            {
                mainCard = mainCard.Replace("{txt_CardTitle}", string.Empty);
                mainCard = mainCard.Replace("{ShowList}", "false").Replace("{List}", null).Replace("{NoOverTime}", "true");
                mainCard = mainCard.Replace("{txt_NoOverTime}", !string.IsNullOrWhiteSpace(payPeriod) ? KronosResourceText.TeamOvertimeNoDataFound.Replace("{PayPeriod}", payPeriod) : KronosResourceText.TeamOvertimeNoDataFoundDateRange.Replace("{StartDate}", startDate).Replace("{EndDate}", endDate));
                mainCard = mainCard.Replace("<b>", "**").Replace("</b>", "**");
            }

            var card = AdaptiveCard.FromJson(mainCard).Card;
            card.Actions.AddRange(buttons);
            return card;
        }
    }

    #region  model for overtime
    /// <summary>
    /// Msteams class.
    /// </summary>
    public class Msteams
    {
        public string type { get; set; }

        public string text { get; set; }

        public string displayText { get; set; }
    }

    /// <summary>
    /// Data class.
    /// </summary>
    public class Data
    {
        public Msteams msteams { get; set; }

        public string CurrentPage { get; set; }

        public string PayPeriod { get; set; }
    }

    /// <summary>
    /// Root object class.
    /// </summary>
    public class RootObject
    {
        public Data data { get; set; }
    }
    #endregion
}
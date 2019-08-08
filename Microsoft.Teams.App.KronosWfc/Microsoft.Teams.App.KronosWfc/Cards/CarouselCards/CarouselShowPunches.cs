//-----------------------------------------------------------------------
// <copyright file="CarouselShowPunches.cs" company="Microsoft">
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
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.ShowPunches;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// Show all punches card.
    /// </summary>
    [Serializable]
    public class CarouselShowPunches
    {
        /// <summary>
        /// Show all punches card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="showPunchesResponse">List of TotaledSpan object.</param>
        /// <param name="payPeriod">Pay period.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <returns>A task.</returns>
        public async Task ShowPunchesData(IDialogContext context, List<TotaledSpan> showPunchesResponse, string payPeriod, string startDate, string endDate)
        {
            var reply = ((Activity)context.Activity).CreateReply();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            var pageSize = 8;
            var titles = new StringBuilder();
            var buttons = new List<CardAction>();
            string cardTitle = string.Empty;

            // Response has data
            if (showPunchesResponse.Any())
            {
                if (!string.IsNullOrWhiteSpace(payPeriod))
                {
                    await context.PostAsync(KronosResourceText.PunchesForPeriod.Replace("{payPeriod}", payPeriod));
                }
                else
                {
                    await context.PostAsync(KronosResourceText.PunchesForPeriodDR.Replace("{startDate}", startDate).Replace("{endDate}", endDate));
                }

                var punchList = this.CreatePunchList(showPunchesResponse);
                var pageCount = Math.Ceiling((double)punchList.Count / pageSize);
                pageCount = pageCount > 10 ? 10 : pageCount;

                for (int i = 0; i < pageCount; i++)
                {
                    buttons.Clear();

                    foreach (var item in punchList.Skip(pageSize * i).Take(pageSize))
                    {
                        titles.Append(item);
                    }

                    CreateHeroCard(reply, titles, buttons);
                    titles.Clear();
                }
            }

            // Response is null
            else
            {
                if (!string.IsNullOrWhiteSpace(payPeriod))
                {
                    titles.Append(KronosResourceText.NoPunchesFound.Replace("{payPeriod}", payPeriod));
                }
                else
                {
                    titles.Append(KronosResourceText.NoPunchesFoundDR.Replace("{startDate}", startDate).Replace("{endDate}", endDate));
                }

                CreateHeroCard(reply, titles, buttons);
            }

            await context.PostAsync(reply);
        }

        private static void CreateHeroCard(Activity reply, StringBuilder titles, List<CardAction> buttons)
        {
            var heroCard = new HeroCard();
            heroCard.Text = titles.ToString();
            buttons.Add(new CardAction(ActionTypes.ImBack, title: KronosResourceText.AddPunch, value: Constants.AddPunchText));
            buttons.Add(new CardAction(ActionTypes.ImBack, title: KronosResourceText.AnotherTimePeriod, value: Constants.AnotherTimeFrameText));
            heroCard.Buttons = buttons;
            reply.Attachments.Add(heroCard.ToAttachment());
        }

        /// <summary>
        /// Create list of punches.
        /// </summary>
        /// <param name="showPunchesResponse">List of TotaledSpan object.</param>
        /// <returns>List of string.</returns>
        private List<string> CreatePunchList(List<TotaledSpan> showPunchesResponse)
        {
            var punchList = new List<string>();

            foreach (var response in showPunchesResponse)
            {
                var inPunchDate = response.InPunch.Punch.EnteredOnDate;
                var inPunchTime = response.InPunch.Punch.EnteredOnTime;

                var outPunchDate = response.OutPunch.Punch.EnteredOnDate;
                var outPunchTime = response.OutPunch.Punch.EnteredOnTime;

                if (outPunchDate != null && outPunchTime != null && response.OutPunch.Punch.WorkRuleName == null && response.OutPunch.Punch.OrgJobName == null && response.OutPunch.Punch.LaborAccountName == null)
                {
                    var outPunchDateTime = Convert.ToDateTime(outPunchDate + " " + outPunchTime);
                    punchList.Add($"<br><b>{outPunchDateTime.ToString("dddd, dd MMMM yyyy h:mm tt", CultureInfo.InvariantCulture)}</b><br>");
                }

                if (inPunchDate != null && inPunchTime != null)
                {
                    var inPunchDateTime = Convert.ToDateTime(inPunchDate + " " + inPunchTime);
                    punchList.Add($"<br><b>{inPunchDateTime.ToString("dddd, dd MMMM yyyy h:mm tt", CultureInfo.InvariantCulture)}</b><br>");

                    if (response.InPunch.Punch.OrgJobName != null)
                    {
                        punchList.Add(KronosResourceText.OrgJobTxt.Replace("{txt}", response.InPunch.Punch.OrgJobName));
                    }

                    if (response.InPunch.Punch.WorkRuleName != null)
                    {
                        punchList.Add(KronosResourceText.WorkRuleTxt.Replace("{txt}", response.InPunch.Punch.WorkRuleName));
                    }

                    if (response.InPunch.Punch.LaborAccountName != null)
                    {
                        punchList.Add(KronosResourceText.TransferTxt.Replace("{txt}", response.InPunch.Punch.LaborAccountName));
                    }
                }
            }

            return punchList;
        }
    }
}
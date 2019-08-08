//-----------------------------------------------------------------------
// <copyright file="HeroPresentEmployees.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.HeroCards
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// Show list of employees who are either present or absent, in a card.
    /// </summary>
    [Serializable]
    public class HeroPresentEmployees
    {
        /// <summary>
        /// Show list of employees who are either present or absent, in a card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="punchList">List of punches.</param>
        /// <param name="isHere">Boolean to check whether employee is present.</param>
        /// <returns>A task.</returns>
        public async Task ShowPresentEmployeesData(IDialogContext context, List<string> punchList, bool isHere)
        {
            string title = isHere ? KronosResourceText.HereAreEmpWhoAreHere : KronosResourceText.HereAreEmpWhoAreNotHere;
            if (punchList.Count > 0)
            {
                await context.PostAsync(title);
                var reply = ((Activity)context.Activity).CreateReply();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                var pageSize = isHere ? 10 : 9;

                var pageCount = Math.Ceiling((double)punchList.Count / pageSize);
                pageCount = pageCount > 10 ? 10 : pageCount;

                for (int i = 0; i < pageCount; i++)
                {
                    var heroCard = new HeroCard();
                    var titles = new StringBuilder();

                    foreach (var item in punchList.Skip(pageSize * i).Take(pageSize))
                    {
                        titles.Append(item);
                    }

                    heroCard.Text = titles.ToString();
                    reply.Attachments.Add(heroCard.ToAttachment());
                }

                await context.PostAsync(reply);
            }
            else
            {
                if (isHere)
                {
                    await context.PostAsync(KronosResourceText.NoEmployeesClockedToday);
                }
                else
                {
                    await context.PostAsync(KronosResourceText.NoEmployeesNotClockedToday);
                }
            }
        }
    }
}
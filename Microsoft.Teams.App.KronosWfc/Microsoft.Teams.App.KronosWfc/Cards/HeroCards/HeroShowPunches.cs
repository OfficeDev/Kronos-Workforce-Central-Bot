//-----------------------------------------------------------------------
// <copyright file="HeroShowPunches.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// Show card to display various time periods.
    /// </summary>
    [Serializable]
    public class HeroShowPunches
    {
        /// <summary>
        /// Show card to display various time periods.
        /// </summary>
        /// <param name="context">Diaog context.</param>
        /// <param name="payPeriod">Pay period.</param>
        /// <param name="titles">Title of the card.</param>
        /// <returns>A task.</returns>
        public async Task ShowNoPunchesCard(IDialogContext context, string payPeriod, string titles)
        {
            var reply = context.MakeMessage();
            var buttons = new List<CardAction>();

            if (payPeriod == Constants.PreviousPayPeriodPunchesText || string.IsNullOrWhiteSpace(payPeriod))
            {
                buttons.Add(new CardAction(ActionTypes.ImBack, title: KronosResourceText.CurrentWeek, value: Constants.CurrentpayPeriodPunchesText));
            }
            else if (payPeriod == Constants.CurrentpayPeriodPunchesText || string.IsNullOrWhiteSpace(payPeriod))
            {
                buttons.Add(new CardAction(ActionTypes.ImBack, title: KronosResourceText.PreviousWeek, value: Constants.PreviousPayPeriodPunchesText));
            }

            buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.DateRange, displayText: KronosResourceText.DateRange, text: Constants.DateRangePunches));

            var heroCard = new HeroCard
            {
                Text = titles,
            };

            heroCard.Buttons = buttons;

            reply.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(reply);
        }
    }
}
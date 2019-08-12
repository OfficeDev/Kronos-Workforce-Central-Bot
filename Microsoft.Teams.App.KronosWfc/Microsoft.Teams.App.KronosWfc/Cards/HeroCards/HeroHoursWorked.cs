//-----------------------------------------------------------------------
// <copyright file="HeroHoursWorked.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.HeroCards
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// hours worked card class 
    /// </summary>
    [Serializable]
    public class HeroHoursWorked
    {
        /// <summary>
        /// show time periods card
        /// </summary>
        /// <param name="context">dialog context</param>
        /// <param name="payPeriod">pay period</param>
        /// <returns>time period card</returns>
        public async Task ShowTimePeriods(IDialogContext context, string payPeriod = default(string))
        {
            var reply = context.MakeMessage();
            var buttons = new List<CardAction>();

            if (payPeriod != Constants.PreviousPayPeriodPunches)
            { 
                buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.PreviousWeek, displayText: Constants.PreviousPayPeriodPunchesText, text: Constants.PreviousPayPeriodHoursWorkedText));
            }

            if (payPeriod != Constants.CurrentpayPeriodPunches)
            { 
                buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.CurrentWeek, displayText: Constants.CurrentpayPeriodPunchesText, text: Constants.CurrentpayPeriodHoursWorkedText));
            }

            buttons.Add(new CardAction(ActionTypes.MessageBack, title: KronosResourceText.DateRange, displayText: Constants.DateRangeText, text: Constants.DateRangeHoursWorked));

            var heroCard = new HeroCard();

            heroCard.Buttons = buttons;

            reply.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(reply);
        }
    }
}
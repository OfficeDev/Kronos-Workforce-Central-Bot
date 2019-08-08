//-----------------------------------------------------------------------
// <copyright file="HeroAddPunch.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.HeroCards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.WorkRuleTransfer;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// Card to show add punch option.
    /// </summary>
    [Serializable]
    public class HeroAddPunch
    {
        /// <summary>
        /// Add a punch.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="localTimestamp">Local timestamp.</param>
        /// <param name="punchType">Punch type.</param>
        /// <param name="isWorkRuleTransfer">If the punch is for work rule transfer.</param>
        /// <returns>A task.</returns>
        public async Task AddPunches(
            IDialogContext context,
            DateTimeOffset? localTimestamp,
            string punchType,
            bool isWorkRuleTransfer = false)
        {
            var reply = context.MakeMessage();
            var punchTitle = string.Empty;
            var punchText = string.Empty;
            var buttons = new List<CardAction>();

            buttons.Add(new CardAction(ActionTypes.ImBack, title: "Yes", value: "Yes"));
            buttons.Add(new CardAction(ActionTypes.ImBack, title: "No", value: "No"));

            if (punchType == Constants.RecentTransfer)
            {
                punchTitle = KronosResourceText.NoRecentTransfers;
                punchText = KronosResourceText.SubmitPunchWithoutTransfer.Replace("{txt}", localTimestamp.Value.DateTime.ToString("dddd, dd MMMM yyyy h:mm tt", CultureInfo.InvariantCulture));
                context.PrivateConversationData.SetValue($"{context.Activity.From.Id}AddPunch", punchType);
                context.PrivateConversationData.SetValue($"{context.Activity.From.Id}WorkRule", string.Empty);
            }
            else if (punchType == Constants.Punch)
            {
                punchTitle = KronosResourceText.Confirmation;
                punchText = KronosResourceText.PunchConfirmation.Replace("{txt}", localTimestamp.Value.DateTime.ToString("dddd, dd MMMM yyyy h:mm tt", CultureInfo.InvariantCulture));
                buttons.Add(new CardAction(ActionTypes.ImBack, title: KronosResourceText.RecentTransfers, value: "Transfer"));
                context.PrivateConversationData.SetValue($"{context.Activity.From.Id}AddPunch", punchType);
                context.PrivateConversationData.SetValue($"{context.Activity.From.Id}WorkRule", string.Empty);
            }
            else if (isWorkRuleTransfer)
            {
                punchTitle = KronosResourceText.Confirmation;
                punchText = KronosResourceText.ConfirmPunchWithTransfer.Replace("{punchType}", punchType).Replace("{datetime}", localTimestamp.Value.DateTime.ToString("dddd, dd MMMM yyyy h:mm tt", CultureInfo.InvariantCulture));
                context.PrivateConversationData.SetValue($"{context.Activity.From.Id}WorkRule", punchType);
                context.PrivateConversationData.SetValue($"{context.Activity.From.Id}AddPunch", string.Empty);
            }

            if (!isWorkRuleTransfer)
            {
                buttons.Add(new CardAction(ActionTypes.ImBack, title: KronosResourceText.WorkRuleTransfer, value: Constants.WorkRuleTransfer));
            }

            var heroCard = new HeroCard
            {
                Title = punchTitle,
                Text = punchText,
            };

            heroCard.Buttons = buttons;

            reply.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(reply);
        }

        /// <summary>
        /// Show all work rules.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="response">Response object.</param>
        /// <returns>A task.</returns>
        public async Task ShowAllWorkRules(IDialogContext context, Response response)
        {
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            var pageSize = 6;
            var pageCount = Math.Ceiling((double)response.WorkRule.Count / pageSize);

            for (int i = 0; i < pageCount; i++)
            {
                var heroCard = new HeroCard();
                var buttons = new List<CardAction>();

                foreach (var item in response.WorkRule.Skip(pageSize * i).Take(pageSize))
                {
                    buttons.Add(new CardAction(ActionTypes.ImBack, title: item.WorkRuleName, value: item.WorkRuleName));
                }

                heroCard.Buttons = buttons;
                reply.Attachments.Add(heroCard.ToAttachment());
            }

            await context.PostAsync(reply);
        }
    }
}
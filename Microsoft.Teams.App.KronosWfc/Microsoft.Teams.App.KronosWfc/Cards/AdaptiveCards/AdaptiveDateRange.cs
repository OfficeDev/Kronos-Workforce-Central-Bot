//-----------------------------------------------------------------------
// <copyright file="AdaptiveDateRange.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// Adaptive date range class.
    /// </summary>
    [Serializable]
    public class AdaptiveDateRange
    {
        /// <summary>
        /// Show date range card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="command">Command text.</param>
        /// <param name="title">Title.</param>
        /// <returns>A task.</returns>
        public async Task ShowDateRange(IDialogContext context, string command, string title)
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
                                        Text = title,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Wrap = true,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                      // Size = AdaptiveTextSize.Large,
                                        Text = "Start Date",

                                     // Weight = AdaptiveTextWeight.Bolder
                                    },
                                    new AdaptiveDateInput
                                    {
                                        Id = "StartDate",
                                        Placeholder = KronosResourceText.EnterStartDate,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                      // Size = AdaptiveTextSize.Large,
                                        Text = "End Date",

                                     // Weight = AdaptiveTextWeight.Bolder,
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
//-----------------------------------------------------------------------
// <copyright file="CarouselShowPunches.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.Punches
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
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.ShowPunches;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// Show all punches card.
    /// </summary>
    [Serializable]
    public class AdaptiveShowPunches
    {
        /// <summary>
        /// Show all punches card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="showPunchesResponse">List of TotaledSpan object.</param>
        /// <param name="payPeriod">Pay period.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End date.</param>
        /// <param name="response">login response.</param>
        /// <param name="assignedHours">Assigned hours.</param>
        /// <param name="allHours">All hours.</param>
        /// <returns>A task.</returns>
        public async Task ShowPunchesData(IDialogContext context, List<TotaledSpan> showPunchesResponse, string payPeriod, string startDate, string endDate, LoginResponse response, string assignedHours, string allHours)
        {
            var reply = context.MakeMessage();
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

                await CreateAdaptiveCard(context, showPunchesResponse, payPeriod, response, assignedHours, allHours);
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

                await this.CreateBlankAdaptiveCard(context, payPeriod, titles.ToString());
            }
        }

        private static async Task CreateAdaptiveCard(IDialogContext context, List<TotaledSpan> showPunchesResponse, string payPeriod, LoginResponse response, string assignedHours, string allHours)
        {
            var message = context.MakeMessage();
            AdaptiveCard card = new AdaptiveCard("1.0");

            var staticContainer = new AdaptiveContainer();
            var dynamicContainer = new AdaptiveContainer();
            var staticItems = new List<AdaptiveElement>();
            var dynamicItems = new List<AdaptiveElement>();

            // Static items
            var title = new AdaptiveTextBlock
            {
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder,
                Text = KronosResourceText.ShowPunchesCardTitleText,
                Wrap = true,
            };

            var titleColumnSet = new AdaptiveColumnSet
            {
                Columns = new List<AdaptiveColumn>
                     {
                         new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveImage
                                 {
                                     Style = AdaptiveImageStyle.Person,
                                     Url = new Uri("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAASuSURBVGhD7ZpZyFdFGIe/yooWCi1B224iKMMMEyIsBAlaoJWiRXKBjEJaIKKrumhRW4iCsqK6iCIioohWumghKgLLiIQuMrXCNtvLitbnOTUwHM//nJn5L1/K94MHnf85M+c9Z7b3fecbm9B2rANhIdwDL8En8C38Db/BN/A+PAHXw/GwG/wvNAUuhdWgwblsgYfhBNgBRq59YSX8AMGor+Fx8MXmwwGwByiNnAyHw5lwI7wJf0Co/y6cBSORBl0IX4EP/wuegdNgZ8jVfnA1rIfwQg7Lw2Bo2gc0OjzwaZgFg5AfYTFsAtv+BS6CgetQ2AA+5DM4FVK1O+z47387tRfcCfa0z7oPdoKBaA5sBht+BaZBLzn0ToK7YS38CNYTV7EH4Qjo0unwPVjvKdgF+tIM+BJs8FFoa1ADncDB8F64FF8BLhhtOhLsfev47OKecU58DDb0CLQ1ZK99B7HBXfwKy6Gt3UMgfMhb/CFXDpEwsV+Gtp5wLwlfroQHoE1Hg5PfeXOKP+RoKfiQz6FtTih36bpxuRwHbboEvM/ecT9KkmM3TO6U1elDiI0q4SHo0rPgvS4kSboJrOA+0SU3tNigUlzRunQwOMR+B+dOqxzvLpmOx5n+0KFjoMmwXP6EFM/APcb7u+bV2GXgjSm9oY6F2KB+2Bu6dBDYI/ZM6/1vg42mrg6DehGNmwQpCnPFBalRvq03ONFTHUCdu9igUj6CVJ0H1vGFGqXT5g2PVaU0+RXd2GKjSngRUuWq6pwyhGj84PeCjS6rSulaA3XDcrkBcvQeWG92VarpVfDivKqULr9mbFQJF0COdJms5zDbSp+CF/evSmnyXrs5NqqE1FUy6Dqw3jVVqaYQtuYkBI6C2KBS3oEchW3i1qpUkxfcCHM0FWKDSjGzkqMlYL1VVakm13Iv5uoNiI0qwTRSjgyDrXdXVaopxBOGmzkybg8xQwn2Rm7QdCVYd0VVqsnQ1ItGhbnyi8bGpeLH2xVydTtY//KqVJMrhxdzEgtB+j0hq5hD49BIUHBTTq5KNbkpeTF3cwoyPxUb2YVetkm8XBm9hqFsmnYrmbb04utVKV+6KzkT35WnRIYX1l9XlRq0J/wMpjGn+0OBUveVt6BU14JtmCzvKdMu3uSqUKLwtboofRGH1QdgG+aXe+pE8Cbd6tT4IFZqxKjTVyInt/W1rzV76RuHZXiRP2QqZDu6MAmuV5AjbQtz0Ix/p84Fb9aJdN6kyL3gKjAEjQ1uw0MfI8xUnQ/WM2mY5A/65q+Ble7whxaZ3L4NQvqoBA+JLoa2GNxgKmTqfaFkubt7mtSU3fOB+jqD8LFiwumVR3Lx+PfDhqznc/+Vs2SkaGXdCFcjX0DfJj6lGhYOu7NB+Ux/+wK6sp49dT/YiPMlBF6jxDjFf83gz4VimbwOfs144QZ9DvQtX+ZJaHrIsLEnBnpAarxwMzQ9bFg4J/oaTm06AzxqaHrwIHkeTJAPVa7pOmwhNB4kGyE3NdS3TJe6qrn+NxmVg2csRnvj+icdHkX4RwQvwE/QZGgTHnd7aKMXm3p8PTLpMZvGXAAmzwxlTcWKroxf3VA6Jwk4oW1QY2P/ANttdsxdJ2yfAAAAAElFTkSuQmCC"),
                                     Size = AdaptiveImageSize.Small,
                                 },
                             },
                             Width = "auto",
                         },
                         new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     Weight = AdaptiveTextWeight.Bolder,
                                     Text = response.Name,
                                     Wrap = true,
                                 },
                                 new AdaptiveTextBlock
                                 {
                                     Spacing = AdaptiveSpacing.None,
                                     Text = $"{KronosResourceText.Employee} #{response.PersonNumber}",
                                     IsSubtle = true,
                                     Wrap = true,
                                 },
                             },
                         },
                     },
            };

            staticItems.Add(title);
            staticItems.Add(titleColumnSet);
            staticContainer.Items = staticItems;

            var dynamicColumnSetHeading = new AdaptiveColumnSet
            {
                Columns = new List<AdaptiveColumn>
                     {
                        new AdaptiveColumn
                             {
                                 Width = "25",
                             },
                        new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     Text = KronosResourceText.ShowPunchesCardPunchInColumnHeader,
                                     IsSubtle = true,
                                     Wrap = true,
                                 },
                             },
                             Width = "20",
                         },
                        new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     Text = KronosResourceText.ShowPunchesCardPunchOutColumnHeader,
                                     IsSubtle = true,
                                     Wrap = true,
                                 },
                             },
                             Width = "20",
                         },
                        new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                     Text = KronosResourceText.ShowPunchesCardWorkRuleColumnHeader,
                                     IsSubtle = true,
                                 },
                             },
                             Width = "35",
                         },
                     },
            };

            dynamicItems.Add(dynamicColumnSetHeading);

            // Placeholder to hold previous date
            var prevDate = string.Empty;

            for (int i = 0; i < showPunchesResponse.Count(); i++)
            {
                var dynamicColumnData = new AdaptiveColumnSet();
                var punchDate = string.Empty;
                var showSeparator = false;

                if (showPunchesResponse[i]?.InPunch?.Punch?.EnteredOnDate != null && prevDate != showPunchesResponse[i]?.InPunch?.Punch?.Date)
                {
                    showSeparator = true;
                    prevDate = showPunchesResponse[i].InPunch.Punch.Date;
                    punchDate = DateTime.Parse(prevDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dd MMMM, yyyy", CultureInfo.InvariantCulture);
                }

                dynamicColumnData.Columns = new List<AdaptiveColumn>
                     {
                        new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     Text = punchDate ?? string.Empty,
                                     Weight = AdaptiveTextWeight.Bolder,
                                     Wrap = true,
                                 },
                             },
                             Width = "25",
                         },
                        new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     Text = showPunchesResponse[i]?.InPunch?.Punch?.Time,
                                     Wrap = true,
                                 },
                             },
                             Width = "20",
                         },
                        new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     Text = showPunchesResponse[i]?.OutPunch?.Punch?.Time ?? string.Empty,
                                     Wrap = true,
                                 },
                             },
                             Width = "20",
                         },
                        new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     Text = showPunchesResponse[i]?.InPunch?.Punch?.WorkRuleName ?? showPunchesResponse[i]?.OrgJobName.Substring(showPunchesResponse[i].OrgJobName.LastIndexOf('/') + 1),
                                     Wrap = true,
                                 },
                             },
                             Width = "35",
                         },
                     };

                if (showSeparator)
                {
                    dynamicColumnData.Separator = showSeparator;
                }

                dynamicItems.Add(dynamicColumnData);
            }

            var dynamicColumnSetFooter = new AdaptiveColumnSet
            {
                Separator = true,
                Columns = new List<AdaptiveColumn>
                     {
                        new AdaptiveColumn
                             {
                                 Width = "20",
                             },
                        new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                     Spacing = AdaptiveSpacing.None,
                                     Text = KronosResourceText.ShowPunchesCardTotalHoursLabel,
                                 },
                             },
                             Width = "30",
                         },
                        new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     Spacing = AdaptiveSpacing.None,
                                     Weight = AdaptiveTextWeight.Bolder,
                                     Text = $"{allHours} {KronosResourceText.GenericHoursOfText} {assignedHours} {KronosResourceText.GenericHoursText}",
                                     HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                     Wrap = true,
                                 },
                             },
                             Width = "30",
                         },
                     },
            };

            dynamicItems.Add(dynamicColumnSetFooter);
            dynamicContainer.Items = dynamicItems;

            card.Body.Add(staticContainer);
            card.Body.Add(dynamicContainer);

            if (payPeriod == Constants.PreviousPayPeriodPunchesText || string.IsNullOrWhiteSpace(payPeriod))
            {
                card.Actions.Add(
                            new AdaptiveSubmitAction()
                            {
                                Title = Constants.CurrentpayPeriodPunchesText,
                                Data = new AdaptiveCardAction
                                {
                                    msteams = new Msteams
                                    {
                                        type = "messageBack",
                                        displayText = Constants.CurrentpayPeriodPunchesText,
                                        text = Constants.CurrentpayPeriodPunchesText,
                                    },
                                },
                            });
            }
            else if (payPeriod == Constants.CurrentpayPeriodPunchesText || string.IsNullOrWhiteSpace(payPeriod))
            {
                card.Actions.Add(
                            new AdaptiveSubmitAction()
                            {
                                Title = Constants.PreviousPayPeriodPunchesText,
                                Data = new AdaptiveCardAction
                                {
                                    msteams = new Msteams
                                    {
                                        type = "messageBack",
                                        displayText = Constants.PreviousPayPeriodPunchesText,
                                        text = Constants.PreviousPayPeriodPunchesText,
                                    },
                                },
                            });
            }

            card.Actions.Add(
                            new AdaptiveSubmitAction()
                            {
                                Title = Constants.DateRangeText,
                                Data = new AdaptiveCardAction
                                {
                                    msteams = new Msteams
                                    {
                                        type = "messageBack",
                                        displayText = Constants.DateRangeText,
                                        text = Constants.DateRangePunches,
                                    },
                                },
                            });

            if (message.Attachments == null)
            {
                message.Attachments = new List<Attachment>();
            }

            message.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
                Name = "Show Punches",
            });

            await context.PostAsync(message);
        }

        private async Task CreateBlankAdaptiveCard(IDialogContext context, string payPeriod, string titles)
        {
            var message = context.MakeMessage();
            AdaptiveCard card = new AdaptiveCard("1.0");

            var container = new AdaptiveContainer();
            var items = new List<AdaptiveElement>();

            var title = new AdaptiveTextBlock
            {
                Size = AdaptiveTextSize.Medium,
                Text = titles,
                Wrap = true,
            };

            items.Add(title);
            container.Items = items;
            card.Body.Add(container);

            if (payPeriod == Constants.PreviousPayPeriodPunchesText || string.IsNullOrWhiteSpace(payPeriod))
            {
                card.Actions.Add(
                            new AdaptiveSubmitAction()
                            {
                                Title = Constants.CurrentpayPeriodPunchesText,
                                Data = new AdaptiveCardAction
                                {
                                    msteams = new Msteams
                                    {
                                        type = "messageBack",
                                        displayText = Constants.CurrentpayPeriodPunchesText,
                                        text = Constants.CurrentpayPeriodPunchesText,
                                    },
                                },
                            });
            }
            else if (payPeriod == Constants.CurrentpayPeriodPunchesText || string.IsNullOrWhiteSpace(payPeriod))
            {
                card.Actions.Add(
                            new AdaptiveSubmitAction()
                            {
                                Title = Constants.PreviousPayPeriodPunchesText,
                                Data = new AdaptiveCardAction
                                {
                                    msteams = new Msteams
                                    {
                                        type = "messageBack",
                                        displayText = Constants.PreviousPayPeriodPunchesText,
                                        text = Constants.PreviousPayPeriodPunchesText,
                                    },
                                },
                            });
            }

            card.Actions.Add(
                            new AdaptiveSubmitAction()
                            {
                                Title = Constants.DateRangeText,
                                Data = new AdaptiveCardAction
                                {
                                    msteams = new Msteams
                                    {
                                        type = "messageBack",
                                        displayText = Constants.DateRangeText,
                                        text = Constants.DateRangePunches,
                                    },
                                },
                            });

            if (message.Attachments == null)
            {
                message.Attachments = new List<Attachment>();
            }

            message.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
                Name = "Show Punches",
            });

            await context.PostAsync(message);
        }
    }
}
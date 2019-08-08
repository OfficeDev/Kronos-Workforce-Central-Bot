//-----------------------------------------------------------------------
// <copyright file="AdaptiveVacationBalance.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.VacationBalance
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Vacation.ViewBalance;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// Show all punches card.
    /// </summary>
    [Serializable]
    public class AdaptiveVacationBalance
    {
        /// <summary>
        /// Show vacation balance card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="viewBalanceResponse">Response object.</param>
        /// <returns>An activity.</returns>
        public IMessageActivity ShowVacationBalanceCard(IDialogContext context, Response viewBalanceResponse)
        {
            var personalHours = string.Empty;
            var vacationHours = string.Empty;
            var sickHours = string.Empty;

            foreach (var response in viewBalanceResponse?.AccrualData?.AccrualBalances?.AccrualBalanceSummary)
            {
                if (response.AccrualCodeName == KronosResourceText.VacationBalanceCodePersonal)
                {
                    personalHours = $"{response.EncumberedBalanceInTime} {KronosResourceText.GenericHoursText}";
                }
                else if (response.AccrualCodeName == KronosResourceText.VacationBalanceCodeVacation)
                {
                    vacationHours = $"{response.EncumberedBalanceInTime} {KronosResourceText.GenericHoursText}";
                }
                else if (response.AccrualCodeName == KronosResourceText.VacationBalanceCodeSick)
                {
                    sickHours = $"{response.EncumberedBalanceInTime} {KronosResourceText.GenericHoursText}";
                }
            }

            return this.CreateAdaptiveCard(context, viewBalanceResponse, personalHours, vacationHours, sickHours);
        }

        private IMessageActivity CreateAdaptiveCard(IDialogContext context, Response viewBalanceResponse, string personalHours, string vacationHours, string sickHours)
        {
            var message = context.MakeMessage();
            AdaptiveCard card = new AdaptiveCard("1.0");

            var container = new AdaptiveContainer();
            var items = new List<AdaptiveElement>();

            var title = new AdaptiveTextBlock
            {
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder,
                Text = KronosResourceText.VacationBalanceCardTitleText,
            };

            var subTitle = new AdaptiveTextBlock
            {
                Text = $"{KronosResourceText.VacationBalanceCardSubtitleText} {context.Activity.LocalTimestamp.Value.DateTime.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}",
                Wrap = true,
            };

            var balanceColumnSet = new AdaptiveColumnSet
            {
                Columns = new List<AdaptiveColumn>
                {
                    new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                     Text = KronosResourceText.VacationBalanceCardAvailableText,
                                     IsSubtle = true,
                                 },
                             },
                             Width = "stretch",
                         },
                },
            };

            items.Add(title);
            items.Add(subTitle);
            items.Add(balanceColumnSet);
            container.Items = items;

            var personalColumnSet = new AdaptiveColumnSet
            {
                Separator = true,
                Columns = new List<AdaptiveColumn>
                {
                    new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     Text = KronosResourceText.VacationBalanceCardPersonalLabel,
                                 },
                             },
                             Width = "stretch",
                         },
                    new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                     Text = personalHours,
                                 },
                             },
                             Width = "stretch",
                         },
                },
            };

            var vacationColumnSet = new AdaptiveColumnSet
            {
                Separator = true,
                Columns = new List<AdaptiveColumn>
                {
                    new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     Text = KronosResourceText.VacationBalanceCodeVacation,
                                 },
                             },
                             Width = "stretch",
                         },
                    new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                     Text = vacationHours,
                                 },
                             },
                             Width = "stretch",
                         },
                },
            };

            var sickColumnSet = new AdaptiveColumnSet
            {
                Separator = true,
                Columns = new List<AdaptiveColumn>
                {
                    new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     Text = KronosResourceText.VacationBalanceCardSCKLabel,
                                 },
                             },
                             Width = "stretch",
                         },
                    new AdaptiveColumn
                         {
                             Items = new List<AdaptiveElement>
                             {
                                 new AdaptiveTextBlock
                                 {
                                     HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                     Text = sickHours,
                                 },
                             },
                             Width = "stretch",
                         },
                },
            };

            card.Body.Add(container);
            card.Body.Add(personalColumnSet);
            card.Body.Add(vacationColumnSet);
            card.Body.Add(sickColumnSet);

            if (!string.IsNullOrEmpty(personalHours))
            {
                card.Actions.Add(
                            new AdaptiveShowCardAction()
                            {
                                Title = KronosResourceText.VacationBalanceCardPersonalLabel,
                                Card = this.ShowDetailsCard(context, viewBalanceResponse, "Personal"),
                            });
            }

            if (!string.IsNullOrEmpty(vacationHours))
            {
                card.Actions.Add(
                            new AdaptiveShowCardAction()
                            {
                                Title = KronosResourceText.VacationBalanceCodeVacation,
                                Card = this.ShowDetailsCard(context, viewBalanceResponse, "Vacation"),
                            });
            }

            if (!string.IsNullOrEmpty(sickHours))
            {
                card.Actions.Add(
                            new AdaptiveShowCardAction()
                            {
                                Title = KronosResourceText.VacationBalanceCardSCKLabel,
                                Card = this.ShowDetailsCard(context, viewBalanceResponse, "Sick"),
                            });
            }

            if (message.Attachments == null)
            {
                message.Attachments = new List<Attachment>();
            }

            message.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
                Name = "Show Vacation Balance",
            });

            return message;
        }

        private AdaptiveCard ShowDetailsCard(IDialogContext context, Response viewBalanceResponse, string accrualCodeName)
        {
            var message = context.MakeMessage();
            var vacationBalance = viewBalanceResponse?.AccrualData?.AccrualBalances?.AccrualBalanceSummary?.Find(x => x.AccrualCodeName == accrualCodeName);
            AdaptiveCard card = new AdaptiveCard("1.0");

            var container = new AdaptiveContainer();
            var items = new List<AdaptiveElement>();

            var title = new AdaptiveTextBlock
            {
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder,
                Text = accrualCodeName == KronosResourceText.VacationBalanceCodePersonal ? KronosResourceText.VacationBalanceCardPersonalLabel : accrualCodeName,
            };

            var factList = new List<AdaptiveFact>
            {
                new AdaptiveFact
                {
                    Title = KronosResourceText.VacationBalanceCardVestedHoursLabel,
                    Value = $"{vacationBalance.VestedBalanceInTime} hours",
                },
                new AdaptiveFact
                {
                    Title = KronosResourceText.VacationBalanceCardProbationHoursLabel,
                    Value = $"{vacationBalance.ProbationaryBalanceInTime ?? "0:00"} hours",
                },
                new AdaptiveFact
                {
                    Title = KronosResourceText.VacationBalanceCardPlannedTakingsLabel,
                    Value = $"{vacationBalance.ProjectedTakingAmountInTime} hours",
                },
                new AdaptiveFact
                {
                    Title = KronosResourceText.VacationBalanceCardPendingGrantsLabel,
                    Value = $"{vacationBalance.ProjectedGrantAmountInTime} hours",
                },
            };

            var facts = new AdaptiveFactSet
            {
                Facts = factList,
            };

            items.Add(title);
            items.Add(facts);
            container.Items = items;
            card.Body.Add(container);

            card.Actions.Add(
                            new AdaptiveSubmitAction()
                            {
                                Title = KronosResourceText.RequestTimeOff,
                                Data = new AdaptiveCardAction
                                {
                                    msteams = new Msteams
                                    {
                                        type = "messageBack",
                                        displayText = KronosResourceText.TimeOffRequest,
                                        text = Constants.CreateTimeOff,
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
                Name = "Show Vacation Balance Details",
            });

            return card;
        }
    }
}
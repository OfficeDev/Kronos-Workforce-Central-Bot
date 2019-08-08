//-----------------------------------------------------------------------
// <copyright file="CarouselVacationBalance.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.CarouselCards
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Vacation.ViewBalance;

    /// <summary>
    /// Show vacation balance card.
    /// </summary>
    [Serializable]
    public partial class CarouselVacationBalance
    {
        /// <summary>
        /// Show vacation balance card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="viewBalanceResponse">Response object.</param>
        /// <returns>An activity.</returns>
        public Activity ShowVacationBalanceCard(IDialogContext context, Response viewBalanceResponse)
        {
            var reply = ((Activity)context.Activity).CreateReply();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            List<Attachment> attachments = new List<Attachment>();
            var accrualType = string.Empty;

            foreach (var response in viewBalanceResponse?.AccrualData?.AccrualBalances?.AccrualBalanceSummary)
            {
                if (Convert.ToInt32(response.AccrualType) == (int)AccrualType.Hours)
                {
                    accrualType = Constants.AccrualTypeHours;
                }
                else if (Convert.ToInt32(response.AccrualType) == (int)AccrualType.Days)
                {
                    accrualType = Constants.AccrualTypeDays;
                }
                else
                {
                    accrualType = Constants.AccrualTypeCurrency;
                }

                var heroCard = new HeroCard
                {
                    Title = $"{response.AccrualCodeName} {response.EncumberedBalanceInTime} ({accrualType})",
                    Subtitle = $"{Constants.VacationBalanceStartDate}, {DateTime.Now.Year} - {Constants.VacationBalanceEndDate}, {DateTime.Now.Year}",
                    Text = $"{Constants.VacationBalanceVestedHours} - {response.VestedBalanceInTime} <br> {Constants.VacationBalanceProbationHours} - {response.ProbationaryBalanceInTime} <br> {Constants.VacationBalancePlannedTakings} - {response.ProjectedTakingAmountInTime} <br> {Constants.VacationBalancePendingGrants} - {response.ProjectedGrantAmountInTime}",
                };
                attachments.Add(heroCard.ToAttachment());
            }

            reply.Attachments = attachments;
            return reply;
        }
    }
}
// <copyright file="WelcomeCard.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.Welcome
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// Class for creating cards for welcome and take tour.
    /// </summary>
    [Serializable]
    public class WelcomeCard
    {
        /// <summary>
        /// Get welcome card while installing app for the first time.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>return adaptive card for welcome message.</returns>
        public async Task GetWelcomeAdaptiveCard(IDialogContext context)
        {
            try
            {
                // Time Off Tour Card
                var message = context.MakeMessage();
                message.AttachmentLayout = "carousel";
                message.Type = "message";
                var timeOffCard = this.CreateTourCard(KronosResourceText.TimeOffTour, "TimeOff.PNG", KronosResourceText.TimeOffDesc, KronosResourceText.TimeOffRequstText, KronosResourceText.TimeOffRequest);
                message.Attachments.Add(new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = timeOffCard,
                });

                // Upcoming Shift Tour Card
                var upcomingCard = this.CreateTourCard(KronosResourceText.UpcomingSchedTour, "UpcomingSchedule.PNG", KronosResourceText.UpcomingShiftsDesc, KronosResourceText.MySchedule, KronosResourceText.MySchedule);
                message.Attachments.Add(new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = upcomingCard,
                });

                // Swap Shift Tour Card
                var cardSwapShift = this.CreateTourCard(KronosResourceText.UpcomingSchedTour, "SwapShift.PNG", KronosResourceText.SwapShiftDesc, "Swap Shifts", "swap");
                message.Attachments.Add(new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = cardSwapShift,
                });

                await context.PostAsync(message);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private AdaptiveCard CreateTourCard(string message, string imageName, string descriptiveText, string gotoActionText, string gotoAction)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/Welcome/TourCard.json");
            var adaptiveCard = File.ReadAllText(fullPath);
            adaptiveCard = adaptiveCard.Replace("{imageUrl}", ConfigurationManager.AppSettings["BaseUri"] + "/Static/Images/" + imageName);
            adaptiveCard = adaptiveCard.Replace("{text}", message);
            adaptiveCard = adaptiveCard.Replace("{descriptiveText}", descriptiveText);
            adaptiveCard = adaptiveCard.Replace("{actionText}", gotoActionText);
            adaptiveCard = adaptiveCard.Replace("{action}", gotoAction);
            var card = AdaptiveCard.FromJson(adaptiveCard).Card;
            return card;
        }
    }
}
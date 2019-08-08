//-----------------------------------------------------------------------
// <copyright file="CarouselHelp.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Cards.CarouselCards
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// caruosel help card class.
    /// </summary>
    [Serializable]
    public class CarouselHelp
    {
        /// <summary>
        /// get pagination of list card action for carousel card.
        /// </summary>
        /// <param name="list">card action list.</param>
        /// <param name="page">number of pages.</param>
        /// <param name="pageSize">page size.</param>
        /// <returns>returns partial list of card action.</returns>
        public IList<CardAction> GetPage(IList<CardAction> list, int page, int pageSize)
        {
            return list.Skip(page * pageSize).Take(pageSize).ToList();
        }

        /// <summary>
        /// show carousel help card.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="activity">activity object.</param>
        /// <returns>help card.</returns>
        public async Task ShowHelpCard(IDialogContext context, Activity activity)
        {
            var reply = activity.CreateReply();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            List<Attachment> attachments = new List<Attachment>();

            List<CardAction> buttons = new List<CardAction>();
            buttons.AddRange(new List<CardAction>
            {
                new CardAction(ActionTypes.MessageBack, KronosResourceText.HoursWorked, text: Constants.HowManyHoursWorked, displayText: KronosResourceText.HoursWorked, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.ShowMyPunches, text: Constants.ShowMeMyPunches, displayText: KronosResourceText.ShowMyPunches, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.AddPunch, text: Constants.AddPunch, displayText: KronosResourceText.AddPunch, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.VacationHours, text: Constants.VacationHoursIHave, displayText: KronosResourceText.VacationHours, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.MySchedule, text: Constants.MySchedule, displayText: KronosResourceText.MySchedule, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.NextVacation, text: Constants.NextVacation, displayText: KronosResourceText.NextVacation, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.UpcomingShifts, text: Constants.UpcomingShifts, displayText: KronosResourceText.UpcomingShifts, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.WhoIsNotHere, text: Constants.WhoIsNotHere, displayText: KronosResourceText.WhoIsNotHere, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.WhoIsHere, text: Constants.WhoIsHere, displayText: KronosResourceText.WhoIsHere, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.WhoIsApproachingOT, text: Constants.ApproachingOT, displayText: KronosResourceText.WhoIsApproachingOT, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.WhereIsSomeone, text: Constants.WhereIsSomeone, displayText: KronosResourceText.WhereIsSomeone, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.SwapShiftRequest, text: Constants.CreateSwapShift, displayText: KronosResourceText.SwapShiftRequest, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.TimeOffRequest, text: Constants.CreateTimeOff, displayText: KronosResourceText.TimeOffRequest, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.ShowTimeOffRequests, text: Constants.ShowAllTimeOff, displayText: KronosResourceText.ShowTimeOffRequests, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.SupervisorTORListHelpButton, text: Constants.SupervisorTORList, displayText: KronosResourceText.SupervisorTORListHelpButton, value: string.Empty),
                new CardAction(ActionTypes.MessageBack, KronosResourceText.SignOut, text: Constants.SignOut, displayText: KronosResourceText.SignOut, value: string.Empty),
            });

            int count = (int)Math.Ceiling((double)buttons.Count / 6);
            for (int i = 0; i < count; i++)
            {
                    IList<CardAction> specificList = this.GetPage(buttons, i, 6);
                    var heroCard = new ThumbnailCard
                    {
                        Buttons = specificList,
                    };
                    attachments.Add(heroCard.ToAttachment());
            }

            reply.Attachments = attachments;
            try
            {
                await context.PostAsync(KronosResourceText.ChooseOption);
                await context.PostAsync(reply);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
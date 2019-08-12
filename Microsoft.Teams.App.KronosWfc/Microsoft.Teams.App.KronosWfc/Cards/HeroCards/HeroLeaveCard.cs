//-----------------------------------------------------------------------
// <copyright file="HeroLeaveCard.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Cards.HeroCards
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// hero leave card class.
    /// </summary>
    [Serializable]
    public class HeroLeaveCard
    {
        /// <summary>
        /// show employees who are on vacation or sick.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="data">leave data.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task ShowEmployeesonLeaveCard(IDialogContext context, Dictionary<string, string> data)
        {
            var reply = context.MakeMessage();
            var heroCard = new HeroCard();

            StringBuilder strVacation = new StringBuilder();
            StringBuilder strSick = new StringBuilder();
            foreach (var val in data)
            {
                if (val.Value.ToLowerInvariant().Contains(Constants.Vacation))
                {
                    strVacation.Append("<br/>- " + val.Key);
                }
                else
                {
                    strSick.Append("<br/>- " + val.Key);
                }
            }

            if (data.Count == 0)
            {
                heroCard.Text = KronosResourceText.NoEmployeesOnLeave;
            }
            else
            {
                if (strVacation.Length == 0 && strSick.Length > 0)
                {
                    heroCard.Text = $"<b>{KronosResourceText.OnLeaveSick}</b>{strSick.ToString()}";
                }
                else if (strVacation.Length > 0 && strSick.Length == 0)
                {
                    heroCard.Text = $"<b>{KronosResourceText.OnLeaveVacation}</b>{strVacation.ToString()}";
                }
                else
                {
                    heroCard.Text = $"<b>{KronosResourceText.OnLeaveVacation}</b>{strVacation.ToString()}<br/><b>{KronosResourceText.OnLeaveSick}</b>{strSick.ToString()}";
                }
            }

            strSick.Clear();
            strVacation.Clear();
            reply.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(reply);
        }
    }
}
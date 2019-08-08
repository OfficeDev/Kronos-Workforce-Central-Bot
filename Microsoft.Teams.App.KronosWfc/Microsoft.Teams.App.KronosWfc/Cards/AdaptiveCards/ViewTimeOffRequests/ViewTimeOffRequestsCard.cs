//-----------------------------------------------------------------------
// <copyright file="ViewTimeOffRequestsCard.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.ViewTimeOffRequests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOff.AddResponse;
    using TimeOffResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOff.AddResponse;

    /// <summary>
    /// View time off request card class.
    /// </summary>
    [Serializable]
    public class ViewTimeOffRequestsCard
    {
        /// <summary>
        /// return next vacation card.
        /// </summary>
        /// <param name="perPageList">List of requests for current page.</param>
        /// <param name="actionSet">Actions for card.</param>
        /// <param name="currentPage">Current page count.</param>
        /// <param name="type">1. Next vacation 2. All time off requests.</param>
        /// <returns>next vacation card.</returns>
        public AdaptiveCard GetCard(IList<EmployeeGlobalTimeOffRequestItem> perPageList, string actionSet, int currentPage, int type)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/ViewTimeOffRequests/NextVacationCard.json");
            var adaptiveCard = File.ReadAllText(fullPath);
            string row = File.ReadAllText(HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/ViewTimeOffRequests/TimeOffRequestItem.json"));
            string noRows = "{\"type\": \"TextBlock\",\"spacing\": \"Medium\",\"size\": \"Small\",\"text\": \"{NoVacationRequests}\",\"isSubtle\": true,\"wrap\": true}";
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < perPageList.Count; j++)
            {
                EmployeeGlobalTimeOffRequestItem item = perPageList[j];
                var status = item.StatusName.ToLowerInvariant() == Constants.Approved.ToLowerInvariant() ? Resources.KronosResourceText.Approved : item.StatusName.ToLowerInvariant() == Constants.Refused.ToLowerInvariant() ? Resources.KronosResourceText.Refused : Resources.KronosResourceText.Submitted;
                var statusColor = item.StatusName.ToLowerInvariant() == Constants.Approved.ToLowerInvariant() ? Constants.Green : item.StatusName.ToLowerInvariant() == Constants.Refused.ToLowerInvariant() ? Constants.Red : Constants.Purple;
                var duration = item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().Duration;
                duration = duration.ToLowerInvariant() == Constants.full_day.ToLowerInvariant() ? Resources.KronosResourceText.FullDay : duration.ToLowerInvariant() == Constants.half_day.ToLowerInvariant() ? Resources.KronosResourceText.HalfDay : duration.ToLowerInvariant() == Constants.first_half_day.ToLowerInvariant() ? Resources.KronosResourceText.FirstHalfDay : Resources.KronosResourceText.Hours;
                var sdt = item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().StartDate;
                var edt = item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().EndDate;
                var paycode = item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().PayCodeName;

                var days = (int)Math.Round(item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().edt.Subtract(item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().sdt).TotalDays, MidpointRounding.AwayFromZero);

                var datePeriod = sdt == edt ? item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().sdt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) : item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().sdt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) + " - " + item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().edt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

                var str = row;
                str = str.Replace("{DateRange}", datePeriod);
                str = str.Replace("{Duration}", duration);
                str = str.Replace("{Status}", status).Replace("{Duration_txt}", Resources.KronosResourceText.Duration).Replace("{Status_txt}", Resources.KronosResourceText.Status);
                str = str.Replace("{StatusColor}", statusColor);
                if (j != perPageList.Count)
                {
                    str += ",";
                }

                sb.Append(str);
            }

            adaptiveCard = adaptiveCard.Replace("{YourVacationRequest}", Resources.KronosResourceText.YourVacationRequests);
            if (perPageList.Count > 0)
            {
                adaptiveCard = adaptiveCard.Replace("{rows}", sb.ToString());
            }
            else
            {
                noRows = noRows.Replace("{NoVacationRequests}", Resources.KronosResourceText.NoVacationRequests);
                adaptiveCard = adaptiveCard.Replace("{rows}", noRows);
            }

            if (type == 1)
            {
                string actions = File.ReadAllText(HttpContext.Current.Server.MapPath($"/Cards/AdaptiveCards/ViewTimeOffRequests/Actions_{actionSet}.json"));
                actions = actions.Replace("{txt_RequestTimeoff}", Resources.KronosResourceText.RequestTimeOff).Replace("{txt_Next}", Resources.KronosResourceText.NextButton);
                actions = actions.Replace("{NextCommand}", Constants.NextVacation_Next).Replace("{CurrentPage}", currentPage.ToString());
                actions = actions.Replace("{txt_Previous}", Resources.KronosResourceText.Previous).Replace("{PreviousCommand}", Constants.NextVacation_Previous);
                adaptiveCard = adaptiveCard.Replace("{Actions}", actions);
            }
            else
            {
                string actions = File.ReadAllText(HttpContext.Current.Server.MapPath($"/Cards/AdaptiveCards/ViewTimeOffRequests/Actions_{actionSet}.json"));
                actions = actions.Replace("{txt_RequestTimeoff}", Resources.KronosResourceText.RequestTimeOff).Replace("{txt_Next}", Resources.KronosResourceText.NextButton);
                actions = actions.Replace("{NextCommand}", Constants.ShowAllVacation_Next).Replace("{CurrentPage}", currentPage.ToString());
                actions = actions.Replace("{txt_Previous}", Resources.KronosResourceText.Previous).Replace("{PreviousCommand}", Constants.ShowAllVacation_Previous);
                adaptiveCard = adaptiveCard.Replace("{Actions}", actions);
            }

            var card = AdaptiveCard.FromJson(adaptiveCard).Card;

            return card;
        }
    }
}
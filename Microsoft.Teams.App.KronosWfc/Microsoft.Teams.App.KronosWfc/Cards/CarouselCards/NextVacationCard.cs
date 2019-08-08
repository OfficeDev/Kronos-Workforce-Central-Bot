//-----------------------------------------------------------------------
// <copyright file="NextVacationCard.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Cards.CarouselCards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOff.AddResponse;
    using TimeOffResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOff.AddResponse;

    /// <summary>
    /// next vacation card class.
    /// </summary>
    [Serializable]
    public class NextVacationCard
    {
        /// <summary>
        /// get paginated list.
        /// </summary>
        /// <typeparam name="T">list type.</typeparam>
        /// <param name="list">list object.</param>
        /// <param name="page">page number.</param>
        /// <param name="pageSize">page size.</param>
        /// <returns>paginated list.</returns>
        public IList<T> GetPage<T>(IList<T> list, int page, int pageSize)
        {
            return list.Skip(page * pageSize).Take(pageSize).ToList();
        }

        /// <summary>
        /// show next vacation card.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="obj">time off data.</param>
        /// <returns>vacation card.</returns>
        public async Task ShowVacationListCard(IDialogContext context, TimeOffResponse.Response obj)
        {
            var replyMessage = context.MakeMessage();
            string datePeriod = string.Empty;
            HeroCard card = new HeroCard();
            StringBuilder str = new StringBuilder();

            List<Attachment> attachments = new List<Attachment>();
            var items = obj.EmployeeRequestMgm.RequestItem.GlobalTimeOffRequestItms.Where(w => w.StatusName == "APPROVED" || w.StatusName == "SUBMITTED").ToList();

            DateTime startDate;
            foreach (var item in items)
            {
                var sdt = item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().StartDate;
                var edt = item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().EndDate;
                DateTime.TryParse(sdt, out startDate);
                DateTime.TryParse(edt, out DateTime endDate);
                endDate = endDate.AddHours(23);
                endDate = endDate.AddMinutes(59);
                item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().sdt = startDate;
                item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().edt = endDate;
            }

            items = items.OrderBy(w => w.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().sdt).ToList();
            int count = (int)Math.Ceiling((double)items.Count / 10);
            count = count > 10 ? 10 : count;
            for (int i = 0; i < count; i++)
            {
                IList<EmployeeGlobalTimeOffRequestItem> perPageList = this.GetPage(items, i, 10);
                foreach (EmployeeGlobalTimeOffRequestItem item in perPageList)
                {
                    var status = item.StatusName;
                    var duration = item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().Duration;
                    duration = duration == "full_day" ? "Full Day" : duration == "half_day" ? "Half Day" : duration == "first_half_day" ? "First Half Day" : "Hours";
                    var sdt = item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().StartDate;
                    var edt = item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().EndDate;
                    var paycode = item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().PayCodeName;

                    var days = (int)Math.Round(item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().edt.Subtract(item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().sdt).TotalDays, MidpointRounding.AwayFromZero);

                    datePeriod = sdt == edt ? item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().sdt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) : item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().sdt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) + " till " + item.TimeOffPeriodsList.TimeOffPerd.FirstOrDefault().edt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

                    str.Append($"<b>{datePeriod}</b>");
                    str.Append($" - {paycode} ");
                    str.Append($" - {duration} ");
                    str.Append($" - {status}<br/>");
                }

                var heroCard = new HeroCard
                {
                    Title = Resources.KronosResourceText.YourVacationRequests,
                    Text = str.ToString(),
                };
                str.Clear();
                attachments.Add(heroCard.ToAttachment());
            }

            if (count == 0)
            {
                var heroCard = new HeroCard
                {
                    Title = Resources.KronosResourceText.YourVacationRequests,
                    Text = Resources.KronosResourceText.NoVacationRequests,
                };
                str.Clear();
                attachments.Add(heroCard.ToAttachment());
            }

            replyMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            replyMessage.Attachments = attachments;
            await context.PostAsync(replyMessage);
        }
    }
}
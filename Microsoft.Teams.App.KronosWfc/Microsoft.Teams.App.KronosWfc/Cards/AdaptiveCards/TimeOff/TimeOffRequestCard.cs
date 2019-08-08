//-----------------------------------------------------------------------
// <copyright file="TimeOffRequestCard.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOffRequests;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// time off request card class.
    /// </summary>
    [Serializable]
    public class TimeOffRequestCard
    {
        /// <summary>
        /// Initial time off request card which submits request for duration=FullDay by default.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="paycodes">Paycodes from azure table storage.</param>
        /// <returns>Basic time off request card.</returns>
        public IMessageActivity GetBasicTimeOffRequestCard(IDialogContext context, List<string> paycodes)
        {
            var repoMessage = context.MakeMessage();
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/CreateTimeOffRequestCard.json");

            var adaptiveCard = File.ReadAllText(fullPath);

            var now = DateTime.Now.ToString("MMM,d yyyy", CultureInfo.InvariantCulture);
            adaptiveCard = adaptiveCard.Replace("{Title}", KronosResourceText.TimeOffRequstText).Replace("{TypeOfTimeOff}", KronosResourceText.TypeOfTimeoff).Replace("{Advanced}", KronosResourceText.Advanced);
            adaptiveCard = adaptiveCard.Replace("{StartDate}", KronosResourceText.StartDate).Replace("{EndDate}", KronosResourceText.EndDate).Replace("{Cancel}", KronosResourceText.Cancel).Replace("{Submit}", KronosResourceText.Submit);
            adaptiveCard = adaptiveCard.Replace("sdtValue", now).Replace("edtValue", now);

            if (paycodes.Count > 0)
            {
                var row = "{\"title\": \"{Text}\",\"value\": \"{Value}\"}";
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < paycodes.Count; i++)
                {
                    var paycode = row.Replace("{Text}", paycodes[i]).Replace("{Value}", paycodes[i]);
                    if (i == 0)
                    {
                        sb.Append(paycode);
                        adaptiveCard = adaptiveCard.Replace("{SelectedPayCode}", paycodes[i]);
                    }
                    else
                    {
                        sb.Append(", " + paycode);
                    }
                }

                adaptiveCard = adaptiveCard.Replace("{Paycodes}", sb.ToString());
            }
            else
            {
                adaptiveCard = adaptiveCard.Replace("{Paycodes}", string.Empty).Replace("{SelectedPayCode}", string.Empty);
            }

            var card = AdaptiveCard.FromJson(adaptiveCard).Card;

            if (repoMessage.Attachments == null)
            {
                repoMessage.Attachments = new List<Attachment>();
            }

            repoMessage.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
            });
            return repoMessage;
        }

        /// <summary>
        /// For displaying advanced time off card for selection of start and end date after clicking 'Advanced' from initial time off card.
        /// </summary>
        /// <param name="context">Dialog context object.</param>
        /// <param name="obj">Advanced time off object.</param>
        /// <returns>Advanced time off request card.</returns>
        public AdaptiveCard GetAdvancedTimeOffRequestCard(IDialogContext context, AdvancedTimeOff obj)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/AdvanceTimeOff1.json");

            var adaptiveCard = File.ReadAllText(fullPath);
            adaptiveCard = adaptiveCard.Replace("sdtValue", obj.sdt ?? DateTime.Now.ToString("MMM,d yyyy", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("edtValue", obj.edt ?? DateTime.Now.ToString("MMM,d yyyy", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{Title}", KronosResourceText.TimeOffRequstText).Replace("{StartDate}", KronosResourceText.StartDate).Replace("{EndDate}", KronosResourceText.EndDate);
            adaptiveCard = adaptiveCard.Replace("{Back}", KronosResourceText.Back).Replace("{Cancel}", KronosResourceText.Cancel).Replace("{Next}", KronosResourceText.NextButton);
            var card = AdaptiveCard.FromJson(adaptiveCard).Card;

            return card;
        }

        /// <summary>
        ///  For displaying advanced time off card for selection of duration (i.e. Hours,FullDay,HalfDay etc.) after clicking 'Next' action from Date range selection card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="obj">Advanced time off object.</param>
        /// <returns>Duration card.</returns>
        public AdaptiveCard OnNextGetDurationCard(IDialogContext context, AdvancedTimeOff obj)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/AdvanceTimeOff2.json");

            var json = File.ReadAllText(fullPath);
            json = json.Replace("{duration}", obj.duration ?? Constants.full_day.ToUpper()).Replace("{Title}", KronosResourceText.TimeOffRequstText);
            json = json.Replace("{Duration}", KronosResourceText.Duration).Replace("{Back}", KronosResourceText.Back).Replace("{Cancel}", KronosResourceText.Cancel).Replace("{Next}", KronosResourceText.NextButton);
            var card = AdaptiveCard.FromJson(json).Card;

            return card;
        }

        /// <summary>
        /// For displaying advanced time off card for selection of start and end time after clicking 'Next' action from duration selection card and selected duration is 'Hours'.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="obj">Advanced time off object.</param>
        /// <returns>Hours card.</returns>
        public AdaptiveCard OnNextGetHoursCard(IDialogContext context, AdvancedTimeOff obj)
        {
            string fullPath = fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/AdvanceTimeOff3_Hours.json");
            var json = File.ReadAllText(fullPath);

            json = json.Replace("{sTime}", obj.StartTime ?? "09:00");
            json = json.Replace("{eTime}", obj.EndTime ?? "17:00");
            json = json.Replace("{Title}", KronosResourceText.TimeOffRequstText).Replace("{StartTime}", KronosResourceText.StartTime).Replace("{EndTime}", KronosResourceText.EndTime);
            json = json.Replace("{Back}", KronosResourceText.Back).Replace("{Cancel}", KronosResourceText.Cancel).Replace("{Next}", KronosResourceText.NextButton);
            var card = AdaptiveCard.FromJson(json).Card;

            return card;
        }

        /// <summary>
        /// For displaying advanced time off card for selection of DeductFrom.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="obj">Advanced time off object.</param>
        /// <param name="paycodes">Paycodes from azure table storage.</param>
        /// <returns>Deduct from card.</returns>
        public AdaptiveCard OnNextGetDeductFromCard(IDialogContext context, AdvancedTimeOff obj, List<string> paycodes)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/AdvanceTimeOff3.json");
            var json = File.ReadAllText(fullPath);
            if (paycodes.Count > 0)
            {
                var row = "{\"title\": \"{Text}\",\"value\": \"{Value}\"}";
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < paycodes.Count; i++)
                {
                    var paycode = row.Replace("{Text}", paycodes[i]).Replace("{Value}", paycodes[i]);
                    if (i == 0)
                    {
                        sb.Append(paycode);
                        json = json.Replace("{DeductFrom}", obj.DeductFrom ?? paycodes[i]);
                    }
                    else
                    {
                        sb.Append(", " + paycode);
                    }
                }

                json = json.Replace("{Paycodes}", sb.ToString());
            }
            else
            {
                json = json.Replace("{Paycodes}", string.Empty).Replace("{DeductFrom}", obj.DeductFrom ?? string.Empty);
            }

            json = json.Replace("{Title}", KronosResourceText.TimeOffRequstText).Replace("{TimeOffType}", KronosResourceText.TypeOfTimeoff);
            json = json.Replace("{Back}", KronosResourceText.Back).Replace("{Cancel}", KronosResourceText.Cancel).Replace("{Next}", KronosResourceText.NextButton);
            json = json.Replace(Constants.AdvancedBack2, obj.duration.ToLowerInvariant() == Constants.Hours.ToLowerInvariant() ? Constants.AdvancedBack3ToHours : Constants.AdvancedBack2);
            var card = AdaptiveCard.FromJson(json).Card;

            return card;
        }

        /// <summary>
        /// For displaying advanced time off confirmation card which shows details selected by employee through previous steps.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="obj">Advanced timeoff object.</param>
        /// <param name="comments">List of comments from API.</param>
        /// <returns>Confirmation card.</returns>
        public AdaptiveCard OnNextGetConfirmationCard(IDialogContext context, AdvancedTimeOff obj, Models.ResponseEntities.CommentList.Response comments)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/ConfirmAdvanceTimeOff.json");
            var json = File.ReadAllText(fullPath);
            DateTime.TryParse(obj.sdt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sdt);
            DateTime.TryParse(obj.edt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime edt);
            edt = edt.AddHours(23).AddMinutes(59);
            var days = (int)Math.Round(edt.Subtract(sdt).TotalDays, MidpointRounding.AwayFromZero);
            if (obj.duration.ToLowerInvariant() == Constants.Hours.ToLowerInvariant())
            {
                var shr = Convert.ToInt32(obj.StartTime.Split(' ')[0].Split(':')[0]);
                var smin = Convert.ToInt32(obj.StartTime.Split(' ')[0].Split(':')[1]);
                var ehr = Convert.ToInt32(obj.EndTime.Split(' ')[0].Split(':')[0]);
                var emin = Convert.ToInt32(obj.EndTime.Split(' ')[0].Split(':')[1]);
                var stime = new DateTime(2000, 1, 1, shr, smin, 0);
                var etime = new DateTime(2000, 1, 1, ehr, emin, 0);
                json = json.Replace("{DurationHours}", "**" + KronosResourceText.TimePeriod + ":** " + stime.ToString("h:mm tt", CultureInfo.InvariantCulture) + " to " + etime.ToString("h:mm tt", CultureInfo.InvariantCulture));
                json = json.Replace("{Duration}", KronosResourceText.Hours);
            }
            else
            {
                json = json.Replace("{DurationHours}", string.Empty);
                if (obj.duration.ToLowerInvariant() == Constants.first_half_day.ToLowerInvariant())
                {
                    json = json.Replace("{Duration}", KronosResourceText.FirstHalfDay);
                }
                else if (obj.duration.ToLowerInvariant() == Constants.half_day.ToLowerInvariant())
                {
                    json = json.Replace("{Duration}", KronosResourceText.HalfDay);
                }
                else
                {
                    json = json.Replace("{Duration}", KronosResourceText.FullDay);
                }
            }

            json = json.Replace("{Title}", KronosResourceText.TimeOffRequstText).Replace("{ConfirmTimeOffRequest}", KronosResourceText.ConfirmTimeoffRequest).Replace("{PayCode_txt}", KronosResourceText.PayCode);
            json = json.Replace("{Date_txt}", KronosResourceText.Date).Replace("{Duration_txt}", KronosResourceText.Duration).Replace("{Back}", KronosResourceText.Back).Replace("{Cancel}", KronosResourceText.Cancel).Replace("{Confirm}", KronosResourceText.Confirm);
            json = json.Replace("{SelectComment}", KronosResourceText.SelectComment).Replace("{EnterNote}", KronosResourceText.EnterNote).Replace("{Submit}", KronosResourceText.Submit);
            json = json.Replace("{Days}", days.ToString() + (days > 1 ? " " + KronosResourceText.Days : " " + KronosResourceText.Day));
            json = days > 1 ? json.Replace("{DurationDate}", sdt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) + " - " + edt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)) : json.Replace("{DurationDate}", sdt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture));

            json = json.Replace("{DeductFrom}", obj.DeductFrom);

            var row = "{\"title\": \"{Text}\",\"value\": \"{Value}\"}";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < comments.Comments.Count; i++)
            {
                if (i == 0)
                {
                    sb.Append(row.Replace("{Text}", comments.Comments[i].CommentText).Replace("{Value}", comments.Comments[i].CommentText));
                    json.Replace("{CommentValue}", comments.Comments[i].CommentText);
                }
                else
                {
                    sb.Append(", " + row.Replace("{Text}", comments.Comments[i].CommentText).Replace("{Value}", comments.Comments[i].CommentText));
                }
            }

            json = json.Replace("{CommentRows}", sb.ToString());

            var card = AdaptiveCard.FromJson(json).Card;

            return card;
        }

        /// <summary>
        /// Show time selection card if Duration is Hours and 'Back' is clicked from confirmation card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="data">Advanced time off object.</param>
        /// <returns>Hours card.</returns>
        public AdaptiveCard OnBackGetHoursCard(IDialogContext context, AdvancedTimeOff data)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/AdvanceTimeOff3_Hours.json");
            var json = File.ReadAllText(fullPath);

            json = json.Replace("{sTime}", data.StartTime).Replace("{eTime}", data.EndTime);
            json = json.Replace("{Title}", KronosResourceText.TimeOffRequstText).Replace("{StartTime}", KronosResourceText.StartTime).Replace("{EndTime}", KronosResourceText.EndTime);
            json = json.Replace("{Back}", KronosResourceText.Back).Replace("{Cancel}", KronosResourceText.Cancel).Replace("{Next}", KronosResourceText.NextButton);
            var card = AdaptiveCard.FromJson(json).Card;

            return card;
        }

        /// <summary>
        /// Show DeductFrom selection card once 'Back' is clicked either from time selection or confirmation card depending upon duration value.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="data">Advanced time off object.</param>
        /// <param name="paycodes">List of paycodes from azure storage.</param>
        /// <returns>Deduct from card.</returns>
        public AdaptiveCard OnBackGetDeductFromCard(IDialogContext context, AdvancedTimeOff data, List<string> paycodes)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/AdvanceTimeOff3.json");
            var json = File.ReadAllText(fullPath);
            if (paycodes.Count > 0)
            {
                var row = "{\"title\": \"{Text}\",\"value\": \"{Value}\"}";
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < paycodes.Count; i++)
                {
                    var paycode = row.Replace("{Text}", paycodes[i]).Replace("{Value}", paycodes[i]);
                    if (i == 0)
                    {
                        sb.Append(paycode);
                        json = json.Replace("{DeductFrom}", data.DeductFrom ?? paycodes[i]);
                    }
                    else
                    {
                        sb.Append(", " + paycode);
                    }
                }

                json = json.Replace("{Paycodes}", sb.ToString());
            }
            else
            {
                json = json.Replace("{Paycodes}", string.Empty).Replace("{DeductFrom}", data.DeductFrom ?? string.Empty);
            }

            json = json.Replace("{Title}", KronosResourceText.TimeOffRequstText).Replace("{TimeOffType}", KronosResourceText.TypeOfTimeoff);
            json = json.Replace("{Back}", KronosResourceText.Back).Replace("{Cancel}", KronosResourceText.Cancel).Replace("{Next}", KronosResourceText.NextButton);
            json = data.duration.ToLowerInvariant() == Constants.Hours.ToLowerInvariant() ? json.Replace(Constants.AdvancedBack2, Constants.AdvancedBack3ToHours) : json.Replace(Constants.AdvancedBack2, Constants.AdvancedBack2);
            var card = AdaptiveCard.FromJson(json).Card;

            return card;
        }

        /// <summary>
        /// Show duration selection card once 'Back' is clicked from DeductFrom card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="data">Advanced time off object.</param>
        /// <returns>Duration card.</returns>
        public AdaptiveCard OnBackGetDurationCard(IDialogContext context, AdvancedTimeOff data)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/AdvanceTimeOff2.json");
            var json = File.ReadAllText(fullPath);
            json = json.Replace("{duration}", data.duration).Replace("{Title}", KronosResourceText.TimeOffRequstText);
            json = json.Replace("{Duration}", KronosResourceText.Duration).Replace("{Back}", KronosResourceText.Back).Replace("{Cancel}", KronosResourceText.Cancel).Replace("{Next}", KronosResourceText.NextButton);

            var card = AdaptiveCard.FromJson(json).Card;

            return card;
        }

        /// <summary>
        /// Show start and end date selection card once 'Back' is clicked from Duration selection card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="data">Advanced time off object.</param>
        /// <returns>Date range card.</returns>
        public AdaptiveCard OnBackGetDateRangeCard(IDialogContext context, AdvancedTimeOff data)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/AdvanceTimeOff1.json");
            var json = File.ReadAllText(fullPath);
            json = json.Replace("sdtValue", data.sdt).Replace("edtValue", data.edt);
            json = json.Replace("{Title}", KronosResourceText.TimeOffRequstText).Replace("{StartDate}", KronosResourceText.StartDate).Replace("{EndDate}", KronosResourceText.EndDate);
            json = json.Replace("{Back}", KronosResourceText.Back).Replace("{Cancel}", KronosResourceText.Cancel).Replace("{Next}", KronosResourceText.NextButton);
            var card = AdaptiveCard.FromJson(json).Card;

            return card;
        }

        /// <summary>
        /// Create supervisor approval notification card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="reqId">Created request Id.</param>
        /// <param name="personNumber">Person number of request creator.</param>
        /// <param name="paycode">Paycode for request.</param>
        /// <param name="timePeriod">Time period for request.</param>
        /// <param name="details">Request details from API.</param>
        /// <param name="note">Comment by requestor while creating request.</param>
        /// <param name="requestor">Request creator name.</param>
        /// <param name="comments">List of comments to which note will be attached.</param>
        /// <param name="advancedTimeOff">Advanced time off request object.</param>
        /// <returns>Supervisor notification card.</returns>
        public AdaptiveCard GetSupervisorNotificationCard(IDialogContext context, string reqId, string personNumber, string paycode, string timePeriod, GlobalTimeOffRequestItem details, string note, string requestor, Models.ResponseEntities.CommentList.Response comments, AdvancedTimeOff advancedTimeOff = null)
        {
            var repoMessage = context.MakeMessage();
            var adaptiveCard = File.ReadAllText(HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/SupervisorNotificationCard.json"));
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());

            DateTime sdt, edt;
            if (advancedTimeOff == null)
            {
                DateTime.TryParse((string)token.SelectToken("sdt"), CultureInfo.InvariantCulture, DateTimeStyles.None, out sdt);
                DateTime.TryParse((string)token.SelectToken("edt"), CultureInfo.InvariantCulture, DateTimeStyles.None, out edt);
            }
            else
            {
                DateTime.TryParse(advancedTimeOff.sdt, CultureInfo.InvariantCulture, DateTimeStyles.None, out sdt);
                DateTime.TryParse(advancedTimeOff.edt, CultureInfo.InvariantCulture, DateTimeStyles.None, out edt);
            }

            edt = edt.AddHours(23);
            edt = edt.AddMinutes(59);
            var days = (int)Math.Round(edt.Subtract(sdt).TotalDays, MidpointRounding.AwayFromZero);

            adaptiveCard = adaptiveCard.Replace("{Title}", KronosResourceText.TimeOffRequstText).Replace("{Status_txt}", KronosResourceText.Status).Replace("{Employee_txt}", KronosResourceText.Employee);
            adaptiveCard = adaptiveCard.Replace("{Paycode_txt}", KronosResourceText.PayCode).Replace("{Date_txt}", KronosResourceText.Date).Replace("{TimePeriod_txt}", KronosResourceText.TimePeriod);
            adaptiveCard = adaptiveCard.Replace("{Note_txt}", KronosResourceText.Note).Replace("{SelectComment}", KronosResourceText.SelectComment).Replace("{EnterNote}", KronosResourceText.EnterNote);
            adaptiveCard = adaptiveCard.Replace("{Submit}", KronosResourceText.Submit).Replace("{Refuse}", KronosResourceText.Refuse);
            adaptiveCard = adaptiveCard.Replace("{Days}", days.ToString() + (days > 1 ? " Days" : " Day"));
            adaptiveCard = adaptiveCard.Replace("{StartDate}", sdt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{Approve}", KronosResourceText.Approve).Replace("{Submit}", KronosResourceText.Submit).Replace("{Refuse}", KronosResourceText.Refuse).Replace("{ShowHistory}", KronosResourceText.ShowHistory);
            if (timePeriod.ToLowerInvariant() == Constants.FullDay.ToLowerInvariant() && days == 0)
            {
                adaptiveCard = adaptiveCard.Replace("- {EndDate}", string.Empty);
            }
            else
            {
                adaptiveCard = adaptiveCard.Replace("{EndDate}", edt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture));
            }

            adaptiveCard = adaptiveCard.Replace("{RequestId}", reqId).Replace("{PersonNumber}", personNumber).Replace("{Status}", KronosResourceText.Submitted);
            adaptiveCard = adaptiveCard.Replace("{Color}", Constants.Default).Replace("{Employee}", requestor).Replace("{PayCode}", paycode);
            adaptiveCard = adaptiveCard.Replace("{TimePeriod}", timePeriod).Replace("{QueryDateSpan}", sdt.ToString("M/d/yyyy", CultureInfo.InvariantCulture) + "-" + edt.ToString("M/d/yyyy", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{Note}", note ?? KronosResourceText.NoComment);

            var historyItem = File.ReadAllText(HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/HistoryItem.json"));
            var first = true;
            StringBuilder sb = new StringBuilder();
            foreach (var item in details?.RequestStatusChanges?.RequestStatusChange)
            {
                sb.Append("," + historyItem.Replace("{Seperator}", first == true ? "False" : "True").Replace("{Status}", item.ToStatusName).Replace("{Person}", item.User.PersonIdentity.PersonNumber).Replace("{Datetime}", item.ChangeDateTime));
                first = false;
            }

            adaptiveCard = adaptiveCard.Replace("{rows}", details?.RequestStatusChanges.RequestStatusChange.Count == 0 ? null : sb.ToString());

            var row = "{\"title\": \"{Text}\",\"value\": \"{Value}\"}";
            sb = new StringBuilder();
            for (int i = 0; i < comments.Comments.Count; i++)
            {
                if (i == 0)
                {
                    sb.Append(row.Replace("{Text}", comments.Comments[i].CommentText).Replace("{Value}", comments.Comments[i].CommentText));
                    adaptiveCard.Replace("{CommentValue}", comments.Comments[i].CommentText);
                }
                else
                {
                    sb.Append(", " + row.Replace("{Text}", comments.Comments[i].CommentText).Replace("{Value}", comments.Comments[i].CommentText));
                }
            }

            adaptiveCard = adaptiveCard.Replace("{CommentRows}", sb.ToString());
            var card = AdaptiveCard.FromJson(adaptiveCard).Card;
            return card;
        }

        /// <summary>
        /// create employee notification card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="status">Request status.</param>
        /// <param name="reviewer">Reviewer name.</param>
        /// <param name="note">Reviewer note.</param>
        /// <param name="details">Request details from API.</param>
        /// <returns>Employee notification card.</returns>
        public AdaptiveCard GetEmployeeNotificationCard(IDialogContext context, string status, string reviewer, string note, GlobalTimeOffRequestItem details)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            var repoMessage = context.MakeMessage();

            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/EmployeeNotificationCard.json");

            string personNumber = (string)token.SelectToken("PersonNumber");
            string employee = (string)token.SelectToken("Employee");
            string date = (string)token.SelectToken("Date");
            string startDate = (string)token.SelectToken("StartDate");
            string endDate = (string)token.SelectToken("EndDate");
            string payCode = (string)token.SelectToken("PayCode");
            string timePeriod = (string)token.SelectToken("TimePeriod");
            string requestId = (string)token.SelectToken("RequestId");
            var adaptiveCard = File.ReadAllText(fullPath);

            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();

            adaptiveCard = adaptiveCard.Replace("{Title}", KronosResourceText.TimeOffRequstText).Replace("{Info}", KronosResourceText.TimeOffEmployeeNotificationInfoText);
            adaptiveCard = adaptiveCard.Replace("{Status}", status).Replace("{Paycode_Info}", payCode.ToLowerInvariant()).Replace("{Supervisor}", reviewer);
            adaptiveCard = adaptiveCard.Replace("{Type}", KronosResourceText.Type).Replace("{Paycode}", payCode).Replace("{StartDate_txt}", KronosResourceText.StartDate).Replace("{StartDate}", startDate);
            adaptiveCard = adaptiveCard.Replace("{EndDate_txt}", KronosResourceText.EndDate).Replace("{EndDate}", endDate).Replace("{Duration_txt}", KronosResourceText.Duration).Replace("{Duration}", timePeriod);
            adaptiveCard = adaptiveCard.Replace("{Note_txt}", KronosResourceText.Note).Replace("{Note}", string.IsNullOrEmpty(note) ? KronosResourceText.NoComment : note);
            adaptiveCard = adaptiveCard.Replace("{StatusImg}", status.ToLowerInvariant() == Constants.Approved.ToLowerInvariant() ? Constants.ApprovedImg : Constants.RefusedImg);
            var historyItem = File.ReadAllText(HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/HistoryItem.json"));
            var first = true;
            StringBuilder sb = new StringBuilder();
            foreach (var item in details?.RequestStatusChanges?.RequestStatusChange)
            {
                if (first == true)
                {
                    sb.Append(historyItem.Replace("{Seperator}", first == true ? "False" : "True").Replace("{Status}", item.ToStatusName).Replace("{Person}", item.User.PersonIdentity.PersonNumber).Replace("{Datetime}", item.ChangeDateTime));
                }
                else
                {
                    sb.Append("," + historyItem.Replace("{Seperator}", first == true ? "False" : "True").Replace("{Status}", item.ToStatusName).Replace("{Person}", item.User.PersonIdentity.PersonNumber).Replace("{Datetime}", item.ChangeDateTime));
                }

                first = false;
            }

            adaptiveCard = adaptiveCard.Replace("{ShowHistory}", KronosResourceText.ShowHistory);
            adaptiveCard = adaptiveCard.Replace("{rows}", details?.RequestStatusChanges.RequestStatusChange.Count == 0 ? null : sb.ToString());
            var card = AdaptiveCard.FromJson(adaptiveCard).Card;
            return card;
        }

        /// <summary>
        /// create approval confirmation card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="status">Request status.</param>
        /// <param name="reviewer">Reviewer name.</param>
        /// <param name="note">Reviewer note.</param>
        /// <param name="details">Request details from API.</param>
        /// <returns>Employee notification card.</returns>
        public AdaptiveCard GetApprovalConfirmationCard(IDialogContext context, string status, string reviewer, string note, GlobalTimeOffRequestItem details)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            var repoMessage = context.MakeMessage();

            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/ApprovalConfirmationCard.json");

            string personNumber = (string)token.SelectToken("PersonNumber");
            string employee = (string)token.SelectToken("Employee");
            string date = (string)token.SelectToken("Date");
            string payCode = (string)token.SelectToken("PayCode");
            string timePeriod = (string)token.SelectToken("TimePeriod");
            string requestId = (string)token.SelectToken("RequestId");
            var adaptiveCard = File.ReadAllText(fullPath);

            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();

            adaptiveCard = adaptiveCard.Replace("{Title}", KronosResourceText.TimeOffRequstText).Replace("{Info}", KronosResourceText.SupervisorTimeOffPostApprovalInfoText).Replace("{Status_txt}", KronosResourceText.Status);
            adaptiveCard = adaptiveCard.Replace("{Employee_txt}", KronosResourceText.Employee).Replace("{Paycode_txt}", KronosResourceText.PayCode).Replace("{Status_txt}", KronosResourceText.Status).Replace("{Date_txt}", KronosResourceText.Date);
            adaptiveCard = adaptiveCard.Replace("{Status}", status).Replace("{Color}", status.ToLower() == Constants.Approved.ToLower() ? Constants.Green : Constants.Red);
            adaptiveCard = adaptiveCard.Replace("{Note_txt}", KronosResourceText.Note).Replace("{Date}", date).Replace("{Employee}", employee).Replace("{PayCode}", payCode).Replace("{PayCode_Info}", payCode.ToLowerInvariant()).Replace("{TimePeriod}", timePeriod);
            adaptiveCard = adaptiveCard.Replace("{Note}", string.IsNullOrEmpty(note) ? KronosResourceText.NoComment : note).Replace("{ShowHistory}", KronosResourceText.ShowHistory);

            var historyItem = File.ReadAllText(HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/HistoryItem.json"));
            var first = true;
            StringBuilder sb = new StringBuilder();
            foreach (var item in details?.RequestStatusChanges?.RequestStatusChange)
            {
                sb.Append("," + historyItem.Replace("{Seperator}", first == true ? "False" : "True").Replace("{Status}", item.ToStatusName).Replace("{Person}", item.User.PersonIdentity.PersonNumber).Replace("{Datetime}", item.ChangeDateTime));
                first = false;
            }

            adaptiveCard = adaptiveCard.Replace("{rows}", details?.RequestStatusChanges.RequestStatusChange.Count == 0 ? null : sb.ToString());
            var card = AdaptiveCard.FromJson(adaptiveCard).Card;
            return card;
        }

        /// <summary>
        /// TimeOff Success Card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="response">Time off response data.</param>
        /// <param name="obj">Advanced time off object.</param>
        /// <returns>Time off success card.</returns>
        public AdaptiveCard ShowTimeOffSuccessCard(IDialogContext context, Models.ResponseEntities.TimeOff.AddResponse.Response response, AdvancedTimeOff obj)
        {
            var repoMessage = context.MakeMessage();

            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/TimeOffSuccessCard.json");
            var adaptiveCard = File.ReadAllText(fullPath);
            var period = response.EmployeeRequestMgm.RequestItem.GlobalTimeOffRequestItms.FirstOrDefault().TimeOffPeriodsList.TimeOffPerd.FirstOrDefault();

            DateTime.TryParse(response.EmployeeRequestMgm.RequestItem.GlobalTimeOffRequestItms.FirstOrDefault().CreationDateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime creationdate);
            DateTime.TryParse(period.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sdt);
            DateTime.TryParse(period.EndDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime edt);
            edt = edt.AddHours(23);
            edt = edt.AddMinutes(59);
            var days = (int)Math.Round(edt.Subtract(sdt).TotalDays, MidpointRounding.AwayFromZero);
            adaptiveCard = adaptiveCard.Replace("{Title}", KronosResourceText.TimeOffRequstText).Replace("{Info}", KronosResourceText.TimeOffRequestSuccessInfoText);
            adaptiveCard = adaptiveCard.Replace("{Paycode}", period.PayCodeName).Replace("{Paycode_Info}", period.PayCodeName.ToLowerInvariant()).Replace("{StartDate}", sdt.ToString("MMM d, yyyy")).Replace("{EndDate}", edt.ToString("MMM d, yyyy"));
            adaptiveCard = adaptiveCard.Replace("{Type}", KronosResourceText.Type).Replace("{StartDate_Txt}", KronosResourceText.StartDate).Replace("{EndDate_Txt}", KronosResourceText.EndDate).Replace("{Duration_Txt}", KronosResourceText.Duration);
            adaptiveCard = adaptiveCard.Replace("{PendingImg}", Constants.PendingImg);
            var duration = period.Duration == Constants.full_day ? Constants.FullDay : period.Duration == Constants.half_day ? Constants.HalfDay : period.Duration == Constants.first_half_day ? Constants.FirstHalfDay : Constants.Hours;

            if (period.Duration.ToLowerInvariant() == Constants.Hours.ToLowerInvariant())
            {
                var shr = Convert.ToInt32(obj.StartTime.Split(' ')[0].Split(':')[0]);
                var smin = Convert.ToInt32(obj.StartTime.Split(' ')[0].Split(':')[1]);
                var ehr = Convert.ToInt32(obj.EndTime.Split(' ')[0].Split(':')[0]);
                var emin = Convert.ToInt32(obj.EndTime.Split(' ')[0].Split(':')[1]);
                var stime = new DateTime(2000, 1, 1, shr, smin, 0);
                var etime = new DateTime(2000, 1, 1, ehr, emin, 0);
                duration += $" ({stime.ToString("h:mm tt", CultureInfo.InvariantCulture) + " - " + etime.ToString("h:mm tt", CultureInfo.InvariantCulture)})";
            }

            adaptiveCard = adaptiveCard.Replace("{Duration}", duration).Replace("{AdditionalRequest}", KronosResourceText.AdditionalRequest);
            var card = AdaptiveCard.FromJson(adaptiveCard).Card;
            return card;
        }
    }
}
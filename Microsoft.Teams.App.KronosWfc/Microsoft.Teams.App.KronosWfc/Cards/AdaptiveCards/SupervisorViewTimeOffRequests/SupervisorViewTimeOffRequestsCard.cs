//-----------------------------------------------------------------------
// <copyright file="SupervisorViewTimeOffRequestsCard.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.SupervisorViewTimeOffRequests
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
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOffRequests;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Comments = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.CommentList.Comment;
    using KronosResourceText = Microsoft.Teams.App.KronosWfc.Resources.KronosResourceText;
    using TimeOffRequestsResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOffRequests;

    /// <summary>
    /// Supervisor view time off request card class.
    /// </summary>
    [Serializable]
    public class SupervisorViewTimeOffRequestsCard
    {
        /// <summary>
        /// Get string with first uppercase letter.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>String with uppercase letter.</returns>
        public static string FirstCharToUpper(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        /// <summary>
        /// create employee notification card.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="status">request status.</param>
        /// <param name="reviewer">reviewer name.</param>
        /// <param name="note">note while approval.</param>
        /// <param name="details">time off request details.</param>
        /// <returns>employee notification card.</returns>
        public AdaptiveCard GetEmployeeNotificationCard(IDialogContext context, string status, string reviewer, string note, GlobalTimeOffRequestItem details)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            var repoMessage = context.MakeMessage();
            string employee = (string)token.SelectToken("EmpName");
            string personNumber = (string)token.SelectToken("PersonNumber");
            string requestId = (string)token.SelectToken("RequestId");
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/EmployeeNotificationCard.json");
            DateTime.TryParse(details.TimeOffPeriods.TimeOffPeriod.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sdt);
            DateTime.TryParse(details.TimeOffPeriods.TimeOffPeriod.EndDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime edt);
            edt = edt.AddHours(23);
            edt = edt.AddMinutes(59);
            var timePeriod = details.TimeOffPeriods.TimeOffPeriod.Duration.ToLowerInvariant() == Constants.full_day.ToLowerInvariant() ? Constants.FullDay : details.TimeOffPeriods.TimeOffPeriod.Duration.ToLowerInvariant() == Constants.half_day.ToLowerInvariant() ? Constants.HalfDay : details.TimeOffPeriods.TimeOffPeriod.Duration.ToLowerInvariant() == Constants.first_half_day.ToLowerInvariant() ? Constants.FirstHalfDay : Constants.Hours;
            var days = (int)Math.Round(edt.Subtract(sdt).TotalDays, MidpointRounding.AwayFromZero);
            var date = "{StartDate} - {EndDate} ({Days})";
            date = date.Replace("{Days}", days.ToString() + (days > 1 ? " " + KronosResourceText.Days : " " + KronosResourceText.Day));
            date = date.Replace("{StartDate}", sdt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture));
            if (timePeriod.ToLowerInvariant() == Constants.FullDay.ToLowerInvariant() && days == 0)
            {
                date = date.Replace("- {EndDate}", string.Empty);
            }
            else
            {
                date = date.Replace("{EndDate}", edt.ToString("MMM d,yyyy", CultureInfo.InvariantCulture));
            }

            if (timePeriod.ToLowerInvariant() == Constants.Hours)
            {
                var start = "08-15-2019 " + details.TimeOffPeriods.TimeOffPeriod.StartTime.Substring(0, 4) + " " + details.TimeOffPeriods.TimeOffPeriod.StartTime.Substring(4, 2);
                DateTime.TryParse(start, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate);
                var lenHr = Convert.ToDouble(details.TimeOffPeriods.TimeOffPeriod.Length.Split(':')[0]);
                var lenMin = Convert.ToDouble(details.TimeOffPeriods.TimeOffPeriod.Length.Split(':')[1]);
                var endDate = startDate.AddHours(lenHr).AddMinutes(lenMin);

                timePeriod += $" ({startDate.ToString("h:mm tt", CultureInfo.InvariantCulture)} - {endDate.ToString("h:mm tt", CultureInfo.InvariantCulture)})";
            }

            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            var adaptiveCard = File.ReadAllText(fullPath);
            adaptiveCard = adaptiveCard.Replace("{RequestId}", requestId);
            adaptiveCard = adaptiveCard.Replace("{PersonNumber}", personNumber);
            adaptiveCard = adaptiveCard.Replace("{Reviewer}", reviewer);
            adaptiveCard = adaptiveCard.Replace("{Status}", status.ToLowerInvariant() == Constants.Approved.ToLowerInvariant() ? Resources.KronosResourceText.Approved : Resources.KronosResourceText.Refused);
            adaptiveCard = adaptiveCard.Replace("{Color}", status.ToLower() == Constants.Approved.ToLower() ? Constants.Green : Constants.Red);
            adaptiveCard = adaptiveCard.Replace("{Date}", date);
            adaptiveCard = adaptiveCard.Replace("{Employee}", employee);
            adaptiveCard = adaptiveCard.Replace("{PayCode}", details.TimeOffPeriods.TimeOffPeriod.PayCodeName);
            adaptiveCard = adaptiveCard.Replace("{TimePeriod}", timePeriod);
            adaptiveCard = adaptiveCard.Replace("{Comment}", string.IsNullOrEmpty(note) ? Resources.KronosResourceText.NoComment : note);

            var historyItem = File.ReadAllText(HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/TimeOff/HistoryItem.json"));
            var first = true;
            StringBuilder sb = new StringBuilder();
            foreach (var item in details?.RequestStatusChanges?.RequestStatusChange)
            {
                if (first == true)
                {
                    var r = historyItem.Replace("{Seperator}", "False").Replace("{Status}", item.ToStatusName).Replace("{Person}", item.User.PersonIdentity.PersonNumber).Replace("{Datetime}", item.ChangeDateTime);
                    sb.Append("," + r);
                }
                else
                {
                    var r = historyItem.Replace("{Seperator}", "True").Replace("{Status}", item.ToStatusName).Replace("{Person}", item.User.PersonIdentity.PersonNumber).Replace("{Datetime}", item.ChangeDateTime);
                    sb.Append("," + r);
                }

                first = false;
            }

            if (details?.RequestStatusChanges.RequestStatusChange.Count == 0)
            {
                adaptiveCard = adaptiveCard.Replace("{rows}", null);
            }
            else
            {
                adaptiveCard = adaptiveCard.Replace("{rows}", sb.ToString());
            }

            var card = AdaptiveCard.FromJson(adaptiveCard).Card;
            return card;
        }

        /// <summary>
        /// Get card for next, previous and filter action.
        /// </summary>
        /// <param name="requests">List of requests.</param>
        /// <param name="obj">Object containing employees, filters details.</param>
        /// <param name="actionSet">Actionset identifier string.</param>
        /// <param name="comments">List of comments (from API).</param>
        /// <returns>Adaptive Card.</returns>
        public AdaptiveCard GetCard(List<GlobalTimeOffRequestItem> requests, ViewTorListObj obj, string actionSet, List<Comments> comments)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/SupervisorViewTimeOffRequests/Latest/TimeOffRequests.json");
            var mainCard = File.ReadAllText(fullPath);
            mainCard = mainCard.Replace("{txt_CardTitle}", KronosResourceText.ManagerViewTOR_Title);
            if (requests.Count > 0)
            {
                mainCard = mainCard.Replace("{txt_CardInfo}", KronosResourceText.ManagerViewTOR_InfoList).Replace("{ShowRequestList}", "true").Replace("{NoTimeOffRequests}", "false").Replace("{txt_NoTimeOffRequests}", null);
                var requestItem = File.ReadAllText(HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/SupervisorViewTimeOffRequests/Latest/RequestItem.json"));
                var requestItemNoAction = File.ReadAllText(HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/SupervisorViewTimeOffRequests/Latest/RequestItemNoAction.json"));
                StringBuilder requestsSB = new StringBuilder();
                for (int i = 0; i < requests.Count; i++)
                {
                    string item = string.Empty;
                    if (requests[i].StatusName.ToLowerInvariant() == Constants.Submitted.ToLowerInvariant())
                    {
                        item = requestItem;
                    }
                    else
                    {
                        item = requestItemNoAction;
                    }

                    var statusImg = requests[i].StatusName.ToLowerInvariant() == Constants.Approved.ToLowerInvariant() ? Constants.ApprovedImg : requests[i].StatusName.ToLowerInvariant() == Constants.Refused.ToLowerInvariant() ? Constants.RefusedImg : requests[i].StatusName.ToLowerInvariant() == Constants.Submitted.ToLowerInvariant() ? Constants.PendingImg : string.Empty;
                    var employeeName = Convert.ToString(obj.Employees[requests[i].Employee.PersonIdentity.PersonNumber]);
                    var employeeRole = Convert.ToString(obj.EmployeesRoles[requests[i].Employee.PersonIdentity.PersonNumber]);
                    string dateRange = string.Empty;
                    DateTime.TryParse(requests[i].TimeOffPeriods.TimeOffPeriod.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sdt);
                    DateTime.TryParse(requests[i].TimeOffPeriods.TimeOffPeriod.EndDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime edt);
                    DateTime.TryParse(requests[i].CreationDateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime creationDate);
                    edt = edt.AddHours(23);
                    edt = edt.AddMinutes(59);
                    var days = (int)Math.Round(edt.Subtract(sdt).TotalDays, MidpointRounding.AwayFromZero);
                    var timeperiod = requests[i].TimeOffPeriods.TimeOffPeriod.Duration == Constants.full_day ? Constants.FullDay : requests[i].TimeOffPeriods.TimeOffPeriod.Duration == Constants.half_day ? Constants.HalfDay : requests[i].TimeOffPeriods.TimeOffPeriod.Duration == Constants.first_half_day ? Constants.FirstHalfDay : Constants.Hours;
                    var hours = string.Empty;
                    var comment = requests[i].Comments?.Comment?.FirstOrDefault()?.Notes?.Note?.Text;
                    if (requests[i].TimeOffPeriods.TimeOffPeriod.Duration.ToLowerInvariant() == Constants.Hours.ToLowerInvariant())
                    {
                        var start = "08-15-2019 " + requests[i].TimeOffPeriods.TimeOffPeriod.StartTime.Substring(0, 4) + " " + requests[i].TimeOffPeriods.TimeOffPeriod.StartTime.Substring(4, 2);
                        DateTime.TryParse(start, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate);
                        var lenHr = Convert.ToDouble(requests[i].TimeOffPeriods.TimeOffPeriod.Length.Split(':')[0]);
                        var lenMin = Convert.ToDouble(requests[i].TimeOffPeriods.TimeOffPeriod.Length.Split(':')[1]);
                        var endDate = startDate.AddHours(lenHr).AddMinutes(lenMin);

                        hours = $"({startDate.ToString("h:mm tt", CultureInfo.InvariantCulture)} - {endDate.ToString("h:mm tt", CultureInfo.InvariantCulture)})";
                    }

                    if (requests[i].TimeOffPeriods.TimeOffPeriod.StartDate.Equals(requests[i].TimeOffPeriods.TimeOffPeriod.EndDate))
                    {
                        dateRange = sdt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        dateRange = $"{sdt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)} - {edt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}";
                    }

                    // replace values of request details
                    item = item.Replace("{StatusImg}", statusImg).Replace("{Name}", employeeName).Replace("{Role}", employeeRole).Replace("{PayCode}", requests[i].TimeOffPeriods.TimeOffPeriod.PayCodeName);
                    item = item.Replace("{DateRange}", dateRange).Replace("{TimePeriod}", timeperiod).Replace("{Note}", requests[i].Comments?.Comment?.FirstOrDefault()?.Notes?.Note?.Text);

                    // If status is submitted, replace values for approve/refuse actions
                    if (requests[i].StatusName.ToLowerInvariant() == Constants.Submitted.ToLowerInvariant())
                    {
                        var queryDateSpan = sdt.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) + "-" + edt.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                        item = item.Replace("{index}", i.ToString()).Replace("{RequestId}", requests[i].Id).Replace("{QueryDateSpan}", queryDateSpan).Replace("{PersonNumber}", requests[i].CreatedByUser.PersonIdentity.PersonNumber);
                        item = item.Replace("{EmpName}", employeeName).Replace("{conversationId}", obj.ConversationId).Replace("{activityId}", obj.ActivityId);
                    }

                    if (i == 0)
                    {
                        item = item.Replace("{Seperator}", "false");
                        requestsSB.Append(item);
                    }
                    else
                    {
                        item = item.Replace("{Seperator}", "true");
                        requestsSB.Append(", " + item);
                    }
                }

                mainCard = mainCard.Replace("{RequestList}", requestsSB.ToString());
            }
            else
            {
                var daterange = (obj.Filters.ContainsKey("DateRange") && !string.IsNullOrEmpty(Convert.ToString(obj.Filters["DateRange"]))) ? KronosResourceText.DateRange.ToLower() : KronosResourceText.TodayTxt.ToLower();
                mainCard = mainCard.Replace("{txt_CardInfo}", null).Replace("{ShowRequestList}", "false").Replace("{RequestList}", null).Replace("{NoTimeOffRequests}", "true");
                mainCard = mainCard.Replace("{txt_NoTimeOffRequests}", KronosResourceText.NoTimeOffRequests.Replace("{date}", daterange));
            }

            // for action set
            fullPath = HttpContext.Current.Server.MapPath($"/Cards/AdaptiveCards/SupervisorViewTimeOffRequests/Latest/Actions_{actionSet}.json");
            var action = File.ReadAllText(fullPath);
            action = action.Replace("{txt_EmployeeName}", KronosResourceText.EnterEmployeeName).Replace("{txt_EnterEmployeeName}", KronosResourceText.EnterEmpName);
            action = action.Replace("{txt_StartDate}", KronosResourceText.EnterStartDate).Replace("{txt_EndDate}", KronosResourceText.EnterEndDate);
            action = action.Replace("{txt_ShowCompletedRequests}", KronosResourceText.ShowCompletedRequests).Replace("{txt_Apply}", KronosResourceText.Apply);
            if (obj.Filters.Count > 0)
            {
                action = action.Replace("{EmpName}", obj.Filters.ContainsKey("EmpName") ? Convert.ToString(obj.Filters["EmpName"]) : string.Empty);
                action = action.Replace("{StartDate}", obj.Filters.ContainsKey("DateRange") ? Convert.ToString(obj.Filters["DateRange"]).Split(';')[0] : string.Empty);
                action = action.Replace("{EndDate}", obj.Filters.ContainsKey("DateRange") ? Convert.ToString(obj.Filters["DateRange"]).Split(';')[1] : string.Empty);
                action = action.Replace("{ShowCompletedRequests}", obj.Filters.ContainsKey("ShowCompletedRequests") ? Convert.ToString(obj.Filters["ShowCompletedRequests"]) : "false");
            }
            else
            {
                action = action.Replace("{EmpName}", string.Empty).Replace("{txt_StartDate}", string.Empty).Replace("{txt_ShowCompletedRequests}", "false");
            }

            action = action.Replace("{txt_Filter}", KronosResourceText.Filter);
            action = action.Replace("{txt_Apply}", KronosResourceText.Apply).Replace("{FilterApplyCommand}", Constants.TORListApplyFilter);
            action = action.Replace("{txt_Next}", KronosResourceText.CapsNext).Replace("{NextCommand}", Constants.TORListNext);
            action = action.Replace("{txt_Previous}", KronosResourceText.Previous).Replace("{PreviousCommand}", Constants.TORListPrevious);

            mainCard = mainCard.Replace("{Actions}", action).Replace("{CurrentPage}", obj.CurrentPageCount.ToString());

            var serialized = JsonConvert.SerializeObject(comments, Formatting.None, new JsonSerializerSettings() { Formatting = Formatting.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore, StringEscapeHandling = StringEscapeHandling.EscapeHtml });

            mainCard = mainCard.Replace("{Comments}", serialized);

            var card = AdaptiveCard.FromJson(mainCard).Card;
            return card;
        }
    }
}
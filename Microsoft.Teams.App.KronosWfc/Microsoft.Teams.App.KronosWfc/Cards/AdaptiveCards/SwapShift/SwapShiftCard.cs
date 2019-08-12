//-----------------------------------------------------------------------
// <copyright file="SwapShiftCard.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.SwapShift
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.HyperFind;
    using Microsoft.Teams.App.KronosWfc.CommandHandling;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.HyperFind;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.HyperFind;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Shifts.UpcomingShifts;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json;
    using EligibleEmpResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift.LoadEligibleEmployees;
    using JobResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift;
    using ScheduleResponse=Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Schedule;

    /// <summary>
    /// Swap shift process card class.
    /// </summary>
    [Serializable]
    public class SwapShiftCard
    {
        private const string Separator = "-";

        /// <summary>
        /// Card for selecting available shifts for swap.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="workingDaySchedule">Working day schedule of employees.</param>
        /// <param name="hyperFindResults">Person name found using hyperfind query.</param>
        /// <returns>basic shift selection card.</returns>
        public AdaptiveCard GetShiftSelectionCard(IDialogContext context, List<ScheduleShift> workingDaySchedule,  List<ResponseHyperFindResult> hyperFindResults)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/SwapShift/ShiftToSwapSelectionCard.json");
            var adaptiveCard = File.ReadAllText(fullPath);
            var row = "{\"title\": \"{Text}\",\"value\": \"{Value}\"},";

            StringBuilder choices = new StringBuilder();
            bool isFirstElem = true;
            AdaptiveCard card;
            int i = 0;
            if (workingDaySchedule != null)
            {
                foreach (var wDay in workingDaySchedule)
                {
                   var personName = hyperFindResults.Where(c => c.PersonNumber == wDay.Employee[0].PersonNumber).FirstOrDefault().FullName;
                   foreach (var sched in wDay.ShiftSegments)
                    {
                        var newRow = row;

                        newRow = newRow.Replace("{Text}", wDay.StartDate + " " + sched.StartTime + " " + sched.EndTime + Separator + wDay.Employee[0].PersonNumber + Separator + personName);

                        newRow = newRow.Replace("{Value}", wDay.StartDate + Separator + sched.EndDate + Separator + sched.StartTime + Separator + sched.EndTime + Separator + wDay.Employee[0].PersonNumber + Separator + personName);

                        if (isFirstElem)
                        {
                            adaptiveCard = adaptiveCard.Replace("{Choice1}", wDay.StartDate + Separator + sched.EndDate + Separator + sched.StartTime + Separator + sched.EndTime + Separator + wDay.Employee[0].PersonNumber + Separator + personName);
                            isFirstElem = false;
                        }

                        choices.Append(newRow);
                    }

                   i++;
                }

                choices.Remove(choices.Length - 1, 1);

                adaptiveCard = adaptiveCard.Replace("{rows}", choices.ToString());
                adaptiveCard = adaptiveCard.Replace("{noShiftFound}", string.Empty);
                adaptiveCard = adaptiveCard.Replace("{nextAction}", "next swap 2");
                card = AdaptiveCard.FromJson(adaptiveCard).Card;
            }
            else
            {
                adaptiveCard = adaptiveCard.Replace("{noShiftFound}", KronosResourceText.NoShifts);
                adaptiveCard = adaptiveCard.Replace("{nextAction}", string.Empty);
                adaptiveCard = adaptiveCard.Replace("{rows}", string.Empty);
                card = AdaptiveCard.FromJson(adaptiveCard).Card;
            }

            return card;
        }

        /// <summary>
        /// Optins available for swapping.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="workingDaySchedule">Working day schedule of employees.</param>
        /// <param name="hyperFindResults">Person name found using hyperfind query.</param>
        /// <returns>basic available shifts card.</returns>
        public AdaptiveCard GetAvailableShiftsCard(IDialogContext context, List<ScheduleShift> workingDaySchedule, List<ResponseHyperFindResult> hyperFindResults)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/SwapShift/AvailableShiftsToSwap.json");
            var obj = context.PrivateConversationData.GetValue<SwapShiftObj>("SwapShiftObj");
            var adaptiveCard = File.ReadAllText(fullPath);
            var row = "{\"title\": \"{Text}\",\"value\": \"{Value}\"},";
            StringBuilder choices = new StringBuilder();
            bool isFirstElem = true;
            AdaptiveCard card;
            int i = 0;
            if (workingDaySchedule != null)
            {
                foreach (var wDay in workingDaySchedule)
                {
                    var personName = hyperFindResults.Where(c => c.PersonNumber == wDay.Employee[0].PersonNumber).FirstOrDefault().FullName;
                    foreach (var sched in wDay.ShiftSegments)
                    {
                        var newRow = row;
                        newRow = newRow.Replace("{Text}", wDay.StartDate + " " + sched.StartTime + " " + sched.EndTime + Separator + wDay.Employee[0].PersonNumber + Separator + personName);

                        newRow = newRow.Replace("{Value}", wDay.StartDate + Separator + sched.EndDate + Separator + sched.StartTime + Separator + sched.EndTime + Separator + wDay.Employee[0].PersonNumber + Separator + personName);

                        if (isFirstElem)
                        {
                            adaptiveCard = adaptiveCard.Replace("{Choice1}", wDay.StartDate + Separator + sched.EndDate + Separator + sched.StartTime + Separator + sched.EndTime + Separator + wDay.Employee[0].PersonNumber + Separator + personName);
                            isFirstElem = false;
                        }

                        choices.Append(newRow);
                    }

                    i++;
                }

                choices.Remove(choices.Length - 1, 1);
                adaptiveCard = adaptiveCard.Replace("{rows}", choices.ToString());
                adaptiveCard = adaptiveCard.Replace("{noShiftFound}", string.Empty);

                if (obj.SelectedAvailableShift != null)
                {
                    adaptiveCard = adaptiveCard.Replace("{Choice1}", obj.SelectedAvailableShift);
                }

                card = AdaptiveCard.FromJson(adaptiveCard).Card;
            }
            else
            {
                adaptiveCard = adaptiveCard.Replace("{noShiftFound}", KronosResourceText.NoShifts);
                adaptiveCard = adaptiveCard.Replace("{rows}", string.Empty);
                card = AdaptiveCard.FromJson(adaptiveCard).Card;
            }

            return card;
        }

        /// <summary>
        /// Filter shifts option based on employees and job roles.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="allJobs">All the available jobs for swapping.</param>
        /// <param name="empList">All the available employees for swapping jobs.</param>
        /// <returns>basic filter card card.</returns>
        public AdaptiveCard GetFilterCard(IDialogContext context, JobResponse.JobResponse.Response allJobs, EligibleEmpResponse.Response empList)
        {
            var obj = context.PrivateConversationData.GetValue<SwapShiftObj>("SwapShiftObj");
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/SwapShift/SearchFilterCard.json");
            var adaptiveCard = File.ReadAllText(fullPath);
            var row = "{\"title\": \"{Text}\",\"value\": \"{Value}\"}";
            var rows = new StringBuilder();

            AdaptiveCard card;

            // Populate Eligible Employees
            if (empList != null && empList.Person != null)
            {
                for (int i = 0; i < empList.Person.Count(); i++)
                {
                    if (i == 0 && empList.Person.Count() > 1)
                    {
                        rows.Append("{\"title\": \"All Employees\",\"value\":\"" + string.Join(",", empList.Person.Select(c => c.PersonNumber)) + "\" }, ");
                        var item = row.Replace("{Text}", empList.Person[i].FullName);
                        item = item.Replace("{Value}", empList.Person[i].PersonNumber);
                        item += ", ";
                        rows.Append(item);
                    }
                    else if (i == empList.Person.Count() - 1)
                    {
                        var item = row.Replace("{Text}", empList.Person[i].FullName);
                        item = item.Replace("{Value}", empList.Person[i].PersonNumber);
                        rows.Append(item);
                    }
                    else
                    {
                        var item = row.Replace("{Text}", empList.Person[i].FullName);
                        item = item.Replace("{Value}", empList.Person[i].PersonNumber);
                        item += ", ";
                        rows.Append(item);
                    }
                }

                adaptiveCard = adaptiveCard.Replace("{AllEmps}", rows.ToString());
                adaptiveCard = adaptiveCard.Replace("{noEmpsFound}", string.Empty);

                adaptiveCard = adaptiveCard.Replace("{EmployeeValue}", obj.SelectedEmployee ?? string.Join(",", empList.Person.Select(c => c.PersonNumber)));

                rows.Clear();
            }
            else
            {
                adaptiveCard = adaptiveCard.Replace("{noEmpsFound}", KronosResourceText.NoEmpForSwap);
                adaptiveCard = adaptiveCard.Replace("{rows}", string.Empty);
                adaptiveCard = adaptiveCard.Replace("{AllEmps}", string.Empty);
                rows.Clear();
            }

            if (allJobs == null)
            {
                adaptiveCard = adaptiveCard.Replace("{noJobsFound}", KronosResourceText.NoJobForSwap);
                adaptiveCard = adaptiveCard.Replace("{rows}", string.Empty);
                adaptiveCard = adaptiveCard.Replace("{AllJobs}", string.Empty);
                rows.Clear();
            }
            else
            {
                var allJobList = allJobs.OrgJobPath;

                for (int i = 0; i < allJobList.Count(); i++)
                {
                    if (i == 0 && allJobList.Count() > 1)
                    {
                        rows.Append("{\"title\": \"All Jobs\",\"Value\": \"" + string.Join(",", allJobList.Select(c => c.FullPath)) + "\" }, ");
                        var item = row.Replace("{Text}", allJobList[i].FullPath);
                        item = item.Replace("{Value}", allJobList[i].FullPath);
                        item = item.Replace("{Value}", allJobList[i].FullPath);
                        item += ", ";
                        rows.Append(item);
                    }
                    else if (i == allJobList.Count() - 1)
                    {
                        var item = row.Replace("{Text}", allJobList[i].FullPath);
                        item = item.Replace("{Value}", allJobList[i].FullPath);
                        rows.Append(item);
                    }
                    else
                    {
                        var item = row.Replace("{Text}", allJobList[i].FullPath);
                        item = item.Replace("{Value}", allJobList[i].FullPath);
                        item += ", ";
                        rows.Append(item);
                    }
                }

                adaptiveCard = adaptiveCard.Replace("{AllJobs}", rows.ToString());
                adaptiveCard = adaptiveCard.Replace("{noJobsFound}", string.Empty);
                adaptiveCard = adaptiveCard.Replace("{JobsValue}", obj.SelectedJob ?? string.Join(",", allJobList.Select(c => c.FullPath)));
            }

            card = AdaptiveCard.FromJson(adaptiveCard).Card;
            return card;
        }

        /// <summary>
        /// Filter shifts option based on employees and job roles.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="obj">selected values from previous cards.</param>
        /// <param name="comment">passed note while approving.</param>
        /// <param name="comments">selected comment while approving.</param>
        /// <param name="approvalType">approvaltype = Manager/Employee.</param>
        /// <returns>basic filter card card.</returns>
        public AdaptiveCard GetNotificationCard(IDialogContext context, SwapShiftObj obj, string comment, Models.ResponseEntities.CommentList.Response comments, int approvalType)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/SwapShift/ApprovalNotification.json");
            var adaptiveCard = File.ReadAllText(fullPath);
            if (approvalType == 1)
            {
                adaptiveCard = adaptiveCard.Replace("{Title}", KronosResourceText.SwapShiftConfirmTitle);
                adaptiveCard = adaptiveCard.Replace("{Info}", obj.RequestorName + " " + KronosResourceText.RequestedShiftSwapWith);
                adaptiveCard = adaptiveCard.Replace("{LeftBar}", Constants.WhiteBar);
                adaptiveCard = adaptiveCard.Replace("{LeftDate}", obj.Emp2FromDateTime.ToString("ddd, MMM d", CultureInfo.InvariantCulture));
                adaptiveCard = adaptiveCard.Replace("{LeftTimeSpan}", obj.Emp2FromDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture) + Separator + obj.Emp2ToDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture));
                adaptiveCard = adaptiveCard.Replace("{LeftHours}", obj.Emp2ToDateTime.Subtract(obj.Emp2FromDateTime).Hours.ToString());
                adaptiveCard = adaptiveCard.Replace("{LeftMin}", obj.Emp2ToDateTime.Subtract(obj.Emp2FromDateTime).Minutes.ToString());
                adaptiveCard = adaptiveCard.Replace("{LeftText}", KronosResourceText.AssignedToYou);
                adaptiveCard = adaptiveCard.Replace("{SwapIcon}", Constants.SwapIcon);
                adaptiveCard = adaptiveCard.Replace("{RightBar}", Constants.WhiteBar);
                adaptiveCard = adaptiveCard.Replace("{RightDate}", obj.Emp1FromDateTime.ToString("ddd, MMM d", CultureInfo.InvariantCulture));
                adaptiveCard = adaptiveCard.Replace("{RightTimeSpan}", obj.Emp1FromDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture) + Separator + obj.Emp1ToDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture));
                adaptiveCard = adaptiveCard.Replace("{RightHours}", obj.Emp1ToDateTime.Subtract(obj.Emp1FromDateTime).Hours.ToString());
                adaptiveCard = adaptiveCard.Replace("{RightMin}", obj.Emp1ToDateTime.Subtract(obj.Emp1FromDateTime).Minutes.ToString());
                adaptiveCard = adaptiveCard.Replace("{RightText}", KronosResourceText.AssignedTo + obj.RequestorName);
                adaptiveCard = adaptiveCard.Replace("{txt_Updated}", KronosResourceText.Updated);
                adaptiveCard = adaptiveCard.Replace("{StatusImage}", Constants.PendingImg);
                adaptiveCard = adaptiveCard.Replace("{UpdatedOn}", context.Activity.LocalTimestamp.Value.DateTime.ToString("MMM dd, h:mm tt", CultureInfo.InvariantCulture));
                adaptiveCard = adaptiveCard.Replace("{txt_Note}", KronosResourceText.Note);
                adaptiveCard = adaptiveCard.Replace("{Comment}", string.IsNullOrEmpty(comment) ? $"({KronosResourceText.None})" : comment);
                adaptiveCard = adaptiveCard.Replace("{SwapShiftObj}", JsonConvert.SerializeObject(obj, Formatting.None));
                adaptiveCard = adaptiveCard.Replace("{ApproveCmd}", KronosResourceText.ApproveByEmp);
                adaptiveCard = adaptiveCard.Replace("{RefuseCmd}", KronosResourceText.RefuseByEmp);
            }
            else
            {
                adaptiveCard = adaptiveCard.Replace("{Title}", KronosResourceText.SwapShiftConfirmTitle);
                adaptiveCard = adaptiveCard.Replace("{Info}", obj.RequestorName + " " + KronosResourceText.RequestSwapShiftWith + " " + obj.RequestedToName + ".");
                adaptiveCard = adaptiveCard.Replace("{LeftBar}", Constants.WhiteBar);
                adaptiveCard = adaptiveCard.Replace("{LeftDate}", obj.Emp1FromDateTime.ToString("ddd, MMM d", CultureInfo.InvariantCulture));
                adaptiveCard = adaptiveCard.Replace("{LeftTimeSpan}", obj.Emp1FromDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture) + Separator + obj.Emp1ToDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture));
                adaptiveCard = adaptiveCard.Replace("{LeftHours}", obj.Emp1ToDateTime.Subtract(obj.Emp1FromDateTime).Hours.ToString());
                adaptiveCard = adaptiveCard.Replace("{LeftMin}", obj.Emp1ToDateTime.Subtract(obj.Emp1FromDateTime).Minutes.ToString());
                adaptiveCard = adaptiveCard.Replace("{LeftText}", KronosResourceText.AssignedTo + " " + obj.RequestorName);
                adaptiveCard = adaptiveCard.Replace("{SwapIcon}", Constants.SwapIcon);
                adaptiveCard = adaptiveCard.Replace("{RightBar}", Constants.WhiteBar);
                adaptiveCard = adaptiveCard.Replace("{RightDate}", obj.Emp2FromDateTime.ToString("ddd, MMM d", CultureInfo.InvariantCulture));
                adaptiveCard = adaptiveCard.Replace("{RightTimeSpan}", obj.Emp2FromDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture) + Separator + obj.Emp2ToDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture));
                adaptiveCard = adaptiveCard.Replace("{RightHours}", obj.Emp2ToDateTime.Subtract(obj.Emp2FromDateTime).Hours.ToString());
                adaptiveCard = adaptiveCard.Replace("{RightMin}", obj.Emp2ToDateTime.Subtract(obj.Emp2FromDateTime).Minutes.ToString());
                adaptiveCard = adaptiveCard.Replace("{RightText}", KronosResourceText.AssignedTo + " " + obj.RequestedToName);
                adaptiveCard = adaptiveCard.Replace("{txt_Updated}", KronosResourceText.Updated);
                adaptiveCard = adaptiveCard.Replace("{StatusImage}", Constants.PendingImg);
                adaptiveCard = adaptiveCard.Replace("{txt_Note}", KronosResourceText.Note);
                adaptiveCard = adaptiveCard.Replace("{UpdatedOn}", context.Activity.LocalTimestamp.Value.DateTime.ToString("MMM dd, h:mm tt", CultureInfo.InvariantCulture));
                adaptiveCard = adaptiveCard.Replace("{Comment}", string.IsNullOrEmpty(comment) ? $"({KronosResourceText.None})" : comment);
                adaptiveCard = adaptiveCard.Replace("{ApproveCmd}", KronosResourceText.ApproveBySupervisor);
                adaptiveCard = adaptiveCard.Replace("{RefuseCmd}", KronosResourceText.RefuseBySupervisor);
                adaptiveCard = adaptiveCard.Replace("{SwapShiftObj}", JsonConvert.SerializeObject(obj, Formatting.None));
            }

            var row = "{\"title\": \"{Text}\",\"value\": \"{Value}\"}";
            var sb = new StringBuilder();
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
        /// Notification card to employee after approval/refusal.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="obj">selected values from previous cards.</param>
        /// <param name="comment">passed note while approving.</param>
        /// <param name="status">swap shift request status.</param>
        /// <returns>basic notification card.</returns>
        public AdaptiveCard GetPostApprovalCard(IDialogContext context, SwapShiftObj obj, string comment, string status)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/SwapShift/Notification.json");
            var adaptiveCard = File.ReadAllText(fullPath);
            adaptiveCard = adaptiveCard.Replace("{Title}", KronosResourceText.SwapShiftConfirmTitle);
            adaptiveCard = adaptiveCard.Replace("{Info}", obj.RequestedToName + " " + status.ToLower() + " " + KronosResourceText.ToSwapWithYou);
            adaptiveCard = adaptiveCard.Replace("{LeftBar}", status == Constants.Accepted ? Constants.GreenBar : Constants.RedBar);
            adaptiveCard = adaptiveCard.Replace("{LeftDate}", obj.Emp1FromDateTime.ToString("ddd, MMM d", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{LeftTimeSpan}", obj.Emp1FromDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture) + Separator + obj.Emp1ToDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{LeftHours}", obj.Emp1ToDateTime.Subtract(obj.Emp1FromDateTime).Hours.ToString());
            adaptiveCard = adaptiveCard.Replace("{LeftMin}", obj.Emp1ToDateTime.Subtract(obj.Emp1FromDateTime).Minutes.ToString());
            adaptiveCard = adaptiveCard.Replace("{LeftText}", KronosResourceText.AssignedToYou);
            adaptiveCard = adaptiveCard.Replace("{SwapIcon}", Constants.SwapIcon);
            adaptiveCard = adaptiveCard.Replace("{RightBar}", Constants.WhiteBar);
            adaptiveCard = adaptiveCard.Replace("{RightDate}", obj.Emp2FromDateTime.ToString("ddd, MMM d", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{RightTimeSpan}", obj.Emp2FromDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture) + Separator + obj.Emp2ToDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{RightHours}", obj.Emp2ToDateTime.Subtract(obj.Emp2FromDateTime).Hours.ToString());
            adaptiveCard = adaptiveCard.Replace("{RightMin}", obj.Emp2ToDateTime.Subtract(obj.Emp2FromDateTime).Minutes.ToString());
            adaptiveCard = adaptiveCard.Replace("{RightText}", KronosResourceText.AssignedTo + " " + obj.RequestedToName);
            adaptiveCard = adaptiveCard.Replace("{txt_Note}", KronosResourceText.Note).Replace("{txt_Updated}", KronosResourceText.Updated);
            adaptiveCard = adaptiveCard.Replace("{StatusImage}", status == Constants.Accepted ? Constants.ApprovedImg : Constants.RefusedImg);
            adaptiveCard = adaptiveCard.Replace("{Updated}", context.Activity.LocalTimestamp.Value.DateTime.ToString("MMM dd, h:mm tt", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{Note}", comment ?? "(none)");

            var card = AdaptiveCard.FromJson(adaptiveCard).Card;

            return card;
        }

        /// <summary>
        /// Swap submit card.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="obj">selected values from previous cards.</param>
        /// <param name="comment">passed note while approving.</param>
        /// <param name="status">swap shift request status.</param>
        /// <returns>basic swap request card.</returns>
        public AdaptiveCard GetSwapSubmitCard(IDialogContext context, SwapShiftObj obj, string comment, string status)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/SwapShift/Notification.json");
            var adaptiveCard = File.ReadAllText(fullPath);
            adaptiveCard = adaptiveCard.Replace("{Info}", KronosResourceText.RequestedSwapShiftByYou + " " + obj.RequestedToName).Replace("{Title}", KronosResourceText.SwapShiftConfirmTitle);
            adaptiveCard = adaptiveCard.Replace("{LeftBar}", Constants.WhiteBar);
            adaptiveCard = adaptiveCard.Replace("{LeftDate}", obj.Emp1FromDateTime.ToString("ddd, MMM d", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{LeftTimeSpan}", obj.Emp1FromDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture) + Separator + obj.Emp1ToDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{LeftHours}", obj.Emp1ToDateTime.Subtract(obj.Emp1FromDateTime).Hours.ToString());
            adaptiveCard = adaptiveCard.Replace("{LeftMin}", obj.Emp1ToDateTime.Subtract(obj.Emp1FromDateTime).Minutes.ToString());
            adaptiveCard = adaptiveCard.Replace("{LeftText}", KronosResourceText.AssignedToYou);
            adaptiveCard = adaptiveCard.Replace("{SwapIcon}", Constants.SwapIcon);
            adaptiveCard = adaptiveCard.Replace("{RightBar}", Constants.WhiteBar);
            adaptiveCard = adaptiveCard.Replace("{RightDate}", obj.Emp2FromDateTime.ToString("ddd, MMM d", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{RightTimeSpan}", obj.Emp2FromDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture) + Separator + obj.Emp2ToDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{RightHours}", obj.Emp2ToDateTime.Subtract(obj.Emp2FromDateTime).Hours.ToString());
            adaptiveCard = adaptiveCard.Replace("{RightMin}", obj.Emp2ToDateTime.Subtract(obj.Emp2FromDateTime).Minutes.ToString());
            adaptiveCard = adaptiveCard.Replace("{RightText}", KronosResourceText.AssignedTo + " " + obj.RequestedToName);
            adaptiveCard = adaptiveCard.Replace("{StatusImage}", status.ToLowerInvariant() == Constants.Submitted.ToLowerInvariant() ? Constants.PendingImg : string.Empty);
            adaptiveCard = adaptiveCard.Replace("{txt_Note}", KronosResourceText.Note).Replace("{txt_Updated}", KronosResourceText.Updated);
            adaptiveCard = adaptiveCard.Replace("{Updated}", context.Activity.LocalTimestamp.Value.DateTime.ToString("MMM dd, h:mm tt", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{Note}", comment ?? "(none)");

            var card = AdaptiveCard.FromJson(adaptiveCard).Card;

            return card;
        }

        /// <summary>
        /// Swap confirmation card.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="obj">selected values from previous cards.</param>
        /// <returns>basic swap confirmation request card.</returns>
        public AdaptiveCard GetSwapShiftCnfCard(IDialogContext context, SwapShiftObj obj)
        {
            string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/SwapShift/Confirmation.json");
            var adaptiveCard = File.ReadAllText(fullPath);
            adaptiveCard = adaptiveCard.Replace("{Info}", KronosResourceText.RequestedSwapShiftByYou + obj.RequestedToName).Replace("{Title}", KronosResourceText.SwapShiftConfirmTitle);
            adaptiveCard = adaptiveCard.Replace("{LeftBar}", Constants.WhiteBar);
            adaptiveCard = adaptiveCard.Replace("{LeftDate}", obj.Emp1FromDateTime.ToString("ddd, MMM d", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{LeftTimeSpan}", obj.Emp1FromDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture) + Separator + obj.Emp1ToDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{LeftHours}", obj.Emp1ToDateTime.Subtract(obj.Emp1FromDateTime).Hours.ToString());
            adaptiveCard = adaptiveCard.Replace("{LeftMin}", obj.Emp1ToDateTime.Subtract(obj.Emp1FromDateTime).Minutes.ToString());
            adaptiveCard = adaptiveCard.Replace("{LeftText}", obj.RequestorName);
            adaptiveCard = adaptiveCard.Replace("{SwapIcon}", Constants.SwapIcon);
            adaptiveCard = adaptiveCard.Replace("{RightBar}", Constants.WhiteBar);
            adaptiveCard = adaptiveCard.Replace("{RightDate}", obj.Emp2FromDateTime.ToString("ddd, MMM d", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{RightTimeSpan}", obj.Emp2FromDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture) + Separator + obj.Emp2ToDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture));
            adaptiveCard = adaptiveCard.Replace("{RightHours}", obj.Emp2ToDateTime.Subtract(obj.Emp2FromDateTime).Hours.ToString());
            adaptiveCard = adaptiveCard.Replace("{RightMin}", obj.Emp2ToDateTime.Subtract(obj.Emp2FromDateTime).Minutes.ToString());
            adaptiveCard = adaptiveCard.Replace("{RightText}",  obj.RequestedToName);
            adaptiveCard = adaptiveCard.Replace("{StatusImage}", Constants.PendingImg);

            var card = AdaptiveCard.FromJson(adaptiveCard).Card;

            return card;
        }
    }
}
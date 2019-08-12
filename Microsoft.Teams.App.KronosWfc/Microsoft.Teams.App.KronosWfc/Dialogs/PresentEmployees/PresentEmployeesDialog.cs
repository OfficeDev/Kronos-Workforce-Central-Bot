//-----------------------------------------------------------------------
// <copyright file="PresentEmployeesDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Dialogs.PresentEmployees
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.HyperFind;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.JobAssignment;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.PresentEmployees;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Punch;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Shifts;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.PresentEmployees;
    using Microsoft.Teams.App.KronosWfc.CommandHandling;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.ShowPunches;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using JobAssignmentAlias = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.JobAssignment;

    /// <summary>
    /// This dialog is used to display list of employees who have clocked in or not.
    /// </summary>
    [Serializable]
    public class PresentEmployeesDialog : IDialog<object>
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IJobAssignmentActivity jobAssignmentActivity;
        private readonly IUpcomingShiftsActivity upcomingShiftsActivity;
        private readonly IShowPunchesActivity showPunchesActivity;
        private readonly IPresentEmployeesActivity presentEmployeesActivity;
        private readonly IHyperFindActivity hyperFindActivity;
        private readonly AuthenticateUser authenticateUser;
        private readonly LoginResponse response;
        private PresentEmployeeCard presentEmployeesCard;

        /// <summary>
        /// Initializes a new instance of the <see cref="PresentEmployeesDialog" /> class.
        /// </summary>
        /// <param name="response">LoginResponse object.</param>
        /// <param name="authenticationService">AuthenticationService object.</param>
        /// <param name="jobAssignmentActivity">jobAssignmentActivity object.</param>
        /// <param name="upcomingShiftsActivity">UpcomingShiftsActivity object.</param>
        /// <param name="showPunchesActivity">ShowPunchesActivity object.</param>
        /// <param name="presentEmployeesActivity">PresentEmployeesActivity object.</param>
        /// <param name="presentEmployeesCard">HeroPresentEmployees object.</param>
        /// <param name="hyperFindActivity">HyperFindActivity object.</param>
        /// <param name="authenticateUser">AuthenticateUser object.</param>
        public PresentEmployeesDialog(
            LoginResponse response,
            IAuthenticationService authenticationService,
            IJobAssignmentActivity jobAssignmentActivity,
            IUpcomingShiftsActivity upcomingShiftsActivity,
            IShowPunchesActivity showPunchesActivity,
            IPresentEmployeesActivity presentEmployeesActivity,
            PresentEmployeeCard presentEmployeesCard,
            IHyperFindActivity hyperFindActivity,
            AuthenticateUser authenticateUser)
        {
            this.response = response;
            this.authenticationService = authenticationService;
            this.jobAssignmentActivity = jobAssignmentActivity;
            this.upcomingShiftsActivity = upcomingShiftsActivity;
            this.showPunchesActivity = showPunchesActivity;
            this.presentEmployeesActivity = presentEmployeesActivity;
            this.presentEmployeesCard = presentEmployeesCard;
            this.hyperFindActivity = hyperFindActivity;
            this.authenticateUser = authenticateUser;
        }

        /// <summary>
        /// The StartAsync method calls IDialogContext.Wait with the continuation delegate to specify
        /// the method that should be called when a new message is received.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>A task.</returns>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.ShowPresentEmployees);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method is used to show present or absent employees.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">Awaitable string.</param>
        /// <returns>A task.</returns>
        private async Task ShowPresentEmployees(IDialogContext context, IAwaitable<string> result)
        {
            var activity = context.Activity as Activity;

            JObject tenant = activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();

            var personNumber = string.Empty;
            var jSession = string.Empty;

            if (!context.UserData.TryGetValue(context.Activity.From.Id, out LoginResponse response))
            {
                response = this.response;
            }
            else
            {
                personNumber = response.PersonNumber;
                jSession = response.JsessionID;
            }

            AppInsightsLogger.CustomEventTrace("PresentEmployeesDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ShowPresentEmployees" }, { "Command", activity.Text } });

            List<TotaledSpan> totaledSpan = new List<TotaledSpan>();
            var presentEmployeeList = new SortedList<string, string>();
            var personName = string.Empty;
            string message = activity.Text.ToLowerInvariant().Trim();

            var startDate = activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            var endDate = activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            var isHere = activity.Text.ToLowerInvariant().Contains(Constants.WhoIsHere);

            switch (message)
            {
                case string command when command.Contains(Constants.presentEmpNextpage) || command.Contains(Constants.presentEmpPrevpage) || command.Contains(Constants.absentEmpPrevpage) || command.Contains(Constants.absentEmpNextpage):
                    var item = command.Split('/');
                    await this.PrevNext(context, Convert.ToInt32(item[2]), message);
                    break;
                default:
                    string isManager = await this.authenticateUser.IsUserManager(context);

                    if (isManager.Equals(Constants.Yes))
                    {
                        Models.ResponseEntities.HyperFind.Response hyperFindResponse = await this.hyperFindActivity.GetHyperFindQueryValues(tenantId, jSession, startDate, endDate, ApiConstants.ReportsToHyperFindQuery, ApiConstants.PersonalVisibilityCode);

                        if (hyperFindResponse?.Status == ApiConstants.Failure)
                        {
                            // User is not logged in - Send Sign in card
                            if (hyperFindResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                            {
                                await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                            }
                            else
                            {
                                await context.PostAsync(hyperFindResponse.Error?.Message);
                            }
                        }
                        else
                        {
                            var scheduleResponse = await this.upcomingShiftsActivity.ShowUpcomingShifts(tenantId, jSession, startDate, endDate, personNumber, hyperFindResponse.HyperFindResult);

                            if (scheduleResponse?.Status == ApiConstants.Failure)
                            {
                                if (scheduleResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                                {
                                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                                }
                                else
                                {
                                    await context.PostAsync(scheduleResponse.Error?.Message);
                                }
                            }
                            else
                            {
                                var scheduleShift = scheduleResponse?.Schedule?.ScheduleItems?.ScheduleShift?.GroupBy(x => x.Employee[0].PersonNumber);

                                // Employee has a shift
                                if (scheduleShift != null && scheduleShift.Any() && scheduleShift?.Count() > 0)
                                {
                                    var pNumber = string.Empty;
                                    var currentDay = activity.LocalTimestamp.Value.Date;
                                    endDate = currentDay.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
                                    startDate = currentDay.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);

                                    foreach (var items in scheduleShift)
                                    {
                                        var startDates = items.Select(x => x.ShiftSegments[0].StartDate).FirstOrDefault();
                                        var endDates = items.Select(x => x.ShiftSegments.LastOrDefault().EndDate).FirstOrDefault();
                                        var startTime = items.Select(x => x.ShiftSegments.FirstOrDefault().StartTime).FirstOrDefault();
                                        var endTime = items.Select(x => x.ShiftSegments.LastOrDefault().EndTime).LastOrDefault();
                                        var shiftStartDate = DateTime.Parse($"{startDates} {startTime}", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture);
                                        var shiftEndDate = DateTime.Parse($"{endDates} {endTime}", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture);

                                        pNumber = items.Key;
                                        var showPunchesResponse = await this.showPunchesActivity.ShowPunches(tenantId, jSession, pNumber, startDate, endDate);
                                        totaledSpan = showPunchesResponse?.Timesheet?.TotaledSpans?.TotaledSpan;
                                        personName = hyperFindResponse.HyperFindResult.Find(x => x.PersonNumber == pNumber).FullName;
                                        JobAssignmentAlias.Response jobAssignmentResponse = await this.jobAssignmentActivity.getJobAssignment(pNumber, tenantId, jSession);
                                        int lastIndex = (jobAssignmentResponse != null) ? (jobAssignmentResponse?.JobAssign?.PrimaryLaborAccList?.PrimaryLaborAcc?.OrganizationPath).LastIndexOf('/') : 0;
                                        if (totaledSpan.Count > 0)
                                        {
                                            if (activity.Text.ToLowerInvariant().Contains(Constants.WhoIsHere) && totaledSpan.Any(x => x.InPunch.Punch.EnteredOnDate != null))
                                            {
                                                presentEmployeeList.Add(personName, jobAssignmentResponse?.JobAssign?.PrimaryLaborAccList?.PrimaryLaborAcc?.OrganizationPath?.Substring(lastIndex + 1));
                                            }
                                            else if (activity.Text.ToLowerInvariant().Contains(Constants.WhoIsNotHere) && totaledSpan.Any(x => x.InPunch.Punch.EnteredOnDate == null))
                                            {
                                                // absent employees
                                                presentEmployeeList.Add(personName, jobAssignmentResponse?.JobAssign?.PrimaryLaborAccList?.PrimaryLaborAcc?.OrganizationPath?.Substring(lastIndex + 1));
                                            }
                                        }
                                    }
                                }

                                var pagewiseAttendance = this.GetPagewiseList(presentEmployeeList);

                                // save data for pagination
                                if (isHere)
                                {
                                    context.PrivateConversationData.SetValue("PagewisePresentAttendance", pagewiseAttendance);
                                }
                                else
                                {
                                    context.PrivateConversationData.SetValue("PagewiseAbsentAttendance", pagewiseAttendance);
                                }

                                await this.presentEmployeesCard.ShowPresentEmployeesData(context, presentEmployeeList.Take(5), isHere, message, 1, presentEmployeeList.Count);
                            }
                        }
                    }
                    else if (isManager.Equals(Constants.No))
                    {
                        await context.PostAsync(KronosResourceText.NoPermission);
                    }

                    break;
            }

            context.Done(string.Empty);
        }

        private Hashtable GetPagewiseList(SortedList<string, string> presentEmployeeList)
        {
            var pageSize = 5;
            var pageCount = Math.Ceiling((double)presentEmployeeList.Count / pageSize);
            pageCount = pageCount > pageSize ? pageSize : pageCount;
            Hashtable pages = new Hashtable();

            for (int i = 1; i <= pageCount; i++)
            {
                var list = presentEmployeeList.Skip(pageSize * (i - 1)).Take(pageSize).ToList();
                pages.Add(i.ToString(), JsonConvert.SerializeObject(list).ToString());
            }

            return pages;
        }

        private async Task PrevNext(IDialogContext context, int currentPage, string message)
        {
            var presentEmployeeList = new List<KeyValuePair<string, string>>();
            bool isHere = false;
            if (message.Contains(Constants.presentEmpPrevpage) || message.Contains(Constants.presentEmpNextpage))
            {
                isHere = true;
            }

            var pagewiseHashtable = isHere ? context.PrivateConversationData.GetValue<Hashtable>("PagewisePresentAttendance") : context.PrivateConversationData.GetValue<Hashtable>("PagewiseAbsentAttendance");

            if (message.Contains(Constants.presentEmpPrevpage) || message.Contains(Constants.absentEmpPrevpage))
            {
                currentPage -= 1;
                if (pagewiseHashtable.ContainsKey(currentPage.ToString()))
                {
                    presentEmployeeList = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(Convert.ToString(pagewiseHashtable[currentPage.ToString()]));
                }
            }
            else
            {
                currentPage += 1;
                if (pagewiseHashtable.ContainsKey(currentPage.ToString()))
                {
                    presentEmployeeList = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(Convert.ToString(pagewiseHashtable[currentPage.ToString()]));
                }
            }

            var total = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(Convert.ToString(pagewiseHashtable[pagewiseHashtable.Count.ToString()])).Count + ((pagewiseHashtable.Count - 1) * 5);
            await this.presentEmployeesCard.ShowPresentEmployeesData(context, presentEmployeeList, isHere, message, currentPage, total);
        }
    }
}
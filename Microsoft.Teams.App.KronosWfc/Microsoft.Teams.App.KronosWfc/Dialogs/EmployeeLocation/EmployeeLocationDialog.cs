//-----------------------------------------------------------------------
// <copyright file="EmployeeLocationDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Dialogs.EmployeeLocation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.HyperFind;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.JobAssignment;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Punch;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Shifts;
    using Microsoft.Teams.App.KronosWfc.Cards.HeroCards;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.HyperFind;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Newtonsoft.Json.Linq;
    using JobAssignmentAlias = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.JobAssignment;
    using ShowPunchesAlias = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.ShowPunches;
    using UpcomingShiftAlias = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Shifts.UpcomingShifts;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// employee location dialog.
    /// </summary>
    [Serializable]
    public class EmployeeLocationDialog : IDialog<object>
    {
        private readonly IUpcomingShiftsActivity upcomingShiftsActivity;
        private readonly IShowPunchesActivity showPunchesActivity;
        private readonly IAuthenticationService authenticationService;
        private readonly IHyperFindActivity hyperFindActivity;
        private readonly IJobAssignmentActivity jobAssignmentActivity;
        private LoginResponse response;
        private HeroEmployeeLocation heroEmployeeLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeLocationDialog"/> class.
        /// </summary>
        /// <param name="response">user login info.</param>
        /// <param name="upcomingShiftsActivity">upcomingshift activity.</param>
        /// <param name="showPunchesActivity">show punch activity.</param>
        /// <param name="authenticationService">authentication service.</param>
        /// <param name="hyperFindActivity">hyperfind activity.</param>
        /// <param name="heroEmployeeLocation">employee location card.</param>
        /// <param name="jobAssignmentActivity">job assignment activity.</param>
        public EmployeeLocationDialog(
                LoginResponse response,
                IUpcomingShiftsActivity upcomingShiftsActivity,
                IShowPunchesActivity showPunchesActivity,
                IAuthenticationService authenticationService,
                IHyperFindActivity hyperFindActivity,
                HeroEmployeeLocation heroEmployeeLocation,
                IJobAssignmentActivity jobAssignmentActivity)
        {
            this.response = response;
            this.upcomingShiftsActivity = upcomingShiftsActivity;
            this.showPunchesActivity = showPunchesActivity;
            this.authenticationService = authenticationService;
            this.heroEmployeeLocation = heroEmployeeLocation;
            this.hyperFindActivity = hyperFindActivity;
            this.jobAssignmentActivity = jobAssignmentActivity;
        }

        /// <summary>
        /// start async dialog.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <returns>emp details task.</returns>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.ShowEmployeeDetails);
            return Task.CompletedTask;
        }

        /// <summary>
        /// show employee details.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="result">awaitable string.</param>
        /// <returns>employee details.</returns>
        private async Task ShowEmployeeDetails(IDialogContext context, IAwaitable<string> result)
        {
            string resultString = await result;
            string jSession = string.Empty;
            string personNumber = string.Empty;
            string startDate = default(string);
            string endDate = default(string);
            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            string message = JsonConvert.DeserializeObject<Message>(resultString).message;
            var luisResult = JsonConvert.DeserializeObject<Message>(resultString).luisResult;

            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                personNumber = this.response.PersonNumber;
                jSession = this.response.JsessionID;
            }

            AppInsightsLogger.CustomEventTrace("EmployeeLocationDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ShowEmployeeDetails" }, { "Command", message } });
            // todays date
            startDate = context.Activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);
            endDate = context.Activity.LocalTimestamp.Value.DateTime.Date.ToString(ApiConstants.DateFormat, CultureInfo.InvariantCulture);

            // get person number from employee name
            Response hyperFindResponse = await this.hyperFindActivity.GetHyperFindQueryValues(tenantId, jSession, startDate, endDate, ApiConstants.ReportsToHyperFindQuery, ApiConstants.PersonalVisibilityCode);
            if (hyperFindResponse?.Status == ApiConstants.Failure)
            {
                // User is not logged in - Send Sign in card
                if (hyperFindResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }
            }
            else
            {
                var employeeName = luisResult?.entities?.FirstOrDefault()?.entity;
                var employee = hyperFindResponse.HyperFindResult.Where(x => x.FullName.ToLowerInvariant().Contains(employeeName)).FirstOrDefault();
                if (employee == null)
                {
                    await context.PostAsync(Resources.KronosResourceText.NoEmpFoundByName.Replace("{txt}", message));
                }
                else
                {
                    JobAssignmentAlias.Response jobAssignmentResponse = await this.jobAssignmentActivity.getJobAssignment(employee?.PersonNumber, tenantId, jSession);
                    if (jobAssignmentResponse?.Status == ApiConstants.Failure)
                    {
                        if (jobAssignmentResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                        {
                            await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                        }
                    }
                    else
                    {
                        UpcomingShiftAlias.Response scheduleResponse = await this.upcomingShiftsActivity.ShowUpcomingShifts(tenantId, jSession, startDate, endDate, employee?.PersonNumber);
                        ShowPunchesAlias.Response showPunchesResponse = await this.showPunchesActivity.ShowPunches(tenantId, jSession, employee?.PersonNumber, startDate, endDate);
                        if (showPunchesResponse?.Status != ApiConstants.Failure && scheduleResponse?.Status != ApiConstants.Failure)
                        {
                            await this.heroEmployeeLocation.ShowEmployeeDetailCard(context, scheduleResponse, showPunchesResponse, employee.FullName, jobAssignmentResponse);
                        }
                    }
                }
            }

            context.Done(default(string));
        }
    }
}
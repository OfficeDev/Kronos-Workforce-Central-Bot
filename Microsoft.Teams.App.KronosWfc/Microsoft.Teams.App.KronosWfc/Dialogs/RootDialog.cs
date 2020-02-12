//-----------------------------------------------------------------------
// <copyright file="RootDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Builder.Scorables;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.Welcome;
    using Microsoft.Teams.App.KronosWfc.Cards.CarouselCards;
    using Microsoft.Teams.App.KronosWfc.Cards.HeroCards;
    using Microsoft.Teams.App.KronosWfc.CommandHandling;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Dialogs.EmployeeLocation;
    using Microsoft.Teams.App.KronosWfc.Dialogs.Hours;
    using Microsoft.Teams.App.KronosWfc.Dialogs.OnLeave;
    using Microsoft.Teams.App.KronosWfc.Dialogs.PresentEmployees;
    using Microsoft.Teams.App.KronosWfc.Dialogs.Punch;
    using Microsoft.Teams.App.KronosWfc.Dialogs.Schedule;
    using Microsoft.Teams.App.KronosWfc.Dialogs.SwapShift;
    using Microsoft.Teams.App.KronosWfc.Dialogs.TeamOvertimes;
    using Microsoft.Teams.App.KronosWfc.Dialogs.TimeOff;
    using Microsoft.Teams.App.KronosWfc.Dialogs.Vacation;
    using Microsoft.Teams.App.KronosWfc.Filters;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is the main entry point in the Dialog cycle.
    /// </summary>
    [CustomLuisModelAttribute]
    [Serializable]
    public class RootDialog : DispatchDialog
    {
        private readonly IDialogFactory dialogFactory;
        private readonly AuthenticateUser authenticateUser;
        private readonly CarouselHelp carousel;
        private readonly WelcomeCard welcomeCard;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootDialog" /> class.
        /// </summary>
        /// <param name="dialogFactory">DialogFactory object.</param>
        /// <param name="authenticateUser">AuthenticateUser object.</param>
        /// <param name="carousel">CarouselHelp object.</param>
        /// <param name="welcomeCard">WelcomeCard object.</param>
        public RootDialog(
            IDialogFactory dialogFactory,
            AuthenticateUser authenticateUser,
            CarouselHelp carousel,
            WelcomeCard welcomeCard)
        {
            this.dialogFactory = dialogFactory;
            this.authenticateUser = authenticateUser;
            this.carousel = carousel;
            this.welcomeCard = welcomeCard;
        }

        /// <summary>
        /// Since none of the scorables in previous group won, this method sends a message.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [MethodBind]
        [ScorableGroup(2)]
        public async Task Default(IDialogContext context, IActivity activity)
        {
            var message = ((Activity)activity).Text;
            if (!string.IsNullOrWhiteSpace(message) && message != Constants.SignInCompleteText) 
            {
                await context.PostAsync(Constants.CommandNotRecognized);
            }
        }

        /// <summary>
        /// Validates incoming activity and forwards it to SupervisorViewTimeOffDialog.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">Luis result.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [LuisIntent("EmployeeTimeOffRequestList")]
        [ScorableGroup(1)]
        public async Task SupervisorViewTimeOffRequests(IDialogContext context, LuisResult result)
        {

            var response = await this.authenticateUser.GetUserInfo(context);
            var serialized = JsonConvert.SerializeObject(result);
            var luisResult = JsonConvert.DeserializeObject<LuisResultModel>(serialized);
            string message = ((Activity)context.Activity).Text?.ToLowerInvariant().Trim();
            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                var luisMessage = JsonConvert.SerializeObject(new Message { message = response.JsessionID, luisResult = luisResult });
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<SupervisorViewTimeoff.SupervisorViewTimeOffDialog>(response), this.Complete, luisMessage, default(CancellationToken));
            }
        }

        /// <summary>
        /// Validates incoming activity and forwards it to AddPunchDialog.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [RegexPattern("(?i)" + Constants.AddPunch)]
        [RegexPattern("(?i)" + Constants.Punch)]
        [RegexPattern("(?i)" + Constants.WorkRuleTransfer)]
        [RegexPattern("(?i)" + Constants.RecentTransfer)]
        [RegexPattern("(?i)" + Constants.WRTAdminAssistant)]
        [RegexPattern("(?i)" + Constants.WRTAdministration)]
        [RegexPattern("(?i)" + Constants.WRTCA8hrDay)]
        [RegexPattern("(?i)" + Constants.WRTCA9HrDay)]
        [RegexPattern("(?i)" + Constants.WRTCAFullTime)]
        [RegexPattern("(?i)" + Constants.WRTCallback)]
        [RegexPattern("(?i)" + Constants.WRTExecutive)]
        [RegexPattern("(?i)" + Constants.WRTFullTime30Min)]
        [RegexPattern("(?i)" + Constants.WRTFullTime60Min)]
        [RegexPattern("(?i)" + Constants.WRTFullTime60MinNoZone)]
        [RegexPattern("(?i)" + Constants.WRTFullTimeExecutive)]
        [RegexPattern("(?i)" + Constants.WRTOnCall)]
        [RegexPattern("(?i)" + Constants.WRTPartTime)]
        [RegexPattern("(?i)" + Constants.WRTProfessionalHourly)]
        [RegexPattern("(?i)" + Constants.WRTProfessionalSalaried)]
        [RegexPattern("(?i)" + Constants.WRTProfessionalSalariedWFS)]
        [RegexPattern("(?i)" + Constants.WRTSalaried)]
        [RegexPattern("(?i)" + Constants.WRTSupport)]
        [RegexPattern("(?i)" + Constants.WRTTechnicalService)]
        [RegexPattern("(?i)" + Constants.WRTTraining)]
        [ScorableGroup(0)]
        public async Task AddPunch(IDialogContext context, IActivity activity)
        {
            var response = await this.authenticateUser.GetUserInfo(context);
            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<AddPunchDialog>(response), this.Complete, response.JsessionID, default(CancellationToken));
            }
        }

        /// <summary>
        /// Checks if the activity is in signin/verifyState.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [RegexPattern("(?i)" + Constants.SignInCompleteText)]
        [ScorableGroup(0)]
        public async Task VerifyState(IDialogContext context, IActivity activity)
        {
            await this.authenticateUser.GetUserInfo(context);
        }

        /// <summary>
        /// Displays list of help commands.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [RegexPattern("(?i)" + Constants.Help)]
        [ScorableGroup(0)]
        public async Task Help(IDialogContext context, IActivity activity)
        {
            await this.carousel.ShowHelpCard(context, (Activity)activity);
        }

        /// <summary>
        /// Sign out from current session.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [RegexPattern("(?i)" + Constants.SignOut)]
        [RegexPattern("(?i)" + Constants.SignOutVal)]
        [ScorableGroup(0)]
        public async Task SignOut(IDialogContext context, IActivity activity)
        {
            context.UserData.TryGetValue(activity.From.Id, out LoginResponse response);
            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                await context.PostAsync($"Bye, {response.Name}! ☺");

                response.JsessionID = string.Empty;
                response.PersonNumber = string.Empty;
                response.Name = string.Empty;

                context.UserData.SetValue(activity.From.Id, response);
            }
            else
            {
                await this.authenticateUser.GetUserInfo(context);
            }
        }

        /// <summary>
        /// Validates incoming activity and forwards it to VacationBalanceDialog.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [RegexPattern("(?i)" + Constants.VacationBalance)]
        [RegexPattern("(?i)" + Constants.Vacation)]
        [RegexPattern("(?i)" + Constants.Accrual)]
        [LuisIntent("VacationBalance")]
        [ScorableGroup(0)]
        public async Task Vacation(IDialogContext context, IActivity activity)
        {
            var response = await this.authenticateUser.GetUserInfo(context);
            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<VacationBalanceDialog>(response), this.Complete, response.JsessionID, default(CancellationToken));
            }
        }

        /// <summary>
        /// Validates incoming activity and forwards it to ShowPunchesDialog.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [RegexPattern("(?i)" + Constants.Punches)]
        [RegexPattern("(?i)" + Constants.ShowMyPunches)]
        [RegexPattern("(?i)" + Constants.ShowMeMyPunches)]
        [RegexPattern("(?i)" + Constants.AnotherTimePeriod)]
        [RegexPattern("(?i)" + Constants.PreviousPayPeriodPunches)]
        [RegexPattern("(?i)" + Constants.CurrentpayPeriodPunches)]
        [RegexPattern("(?i)" + Constants.DateRangePunches)]
        [RegexPattern("(?i)" + Constants.TodayPunches)]
        [RegexPattern("(?i)" + Constants.SubmitDateRangePunches)]
        [ScorableGroup(0)]
        [LuisIntent("ShowPunches")]
        public async Task Punch(IDialogContext context, LuisResult result)
        {
            var response = await this.authenticateUser.GetUserInfo(context);

            var serialized = JsonConvert.SerializeObject(result);
            var luisResult = JsonConvert.DeserializeObject<LuisResultModel>(serialized);
            string message = ((Activity)context.Activity).Text?.ToLowerInvariant().Trim();
            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {


                var luisMessage = JsonConvert.SerializeObject(new Message { message = message, luisResult = luisResult });
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<ShowPunchesDialog>(response), this.Complete, luisMessage, default(CancellationToken));
                // await context.Forward(this.dialogFactory.CreateLogonResponseDialog<ShowPunchesDialog>(response), this.Complete, response.JsessionID, default(CancellationToken));
            }
        }

        /// <summary>
        /// Validates incoming activity and forwards it to SavePunchDialog.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [RegexPattern("(?i)" + Constants.SavePunch)]
        [ScorableGroup(0)]
        public async Task SavePunch(IDialogContext context, IActivity activity)
        {
            var response = await this.authenticateUser.GetUserInfo(context);
            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<SavePunchDialog>(response), this.Complete, ((Activity)activity).Name, default(CancellationToken));
            }
        }

        /// <summary>
        /// Validates incoming activity and forwards it to SaveWorkRuleTransferDialog.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [RegexPattern("(?i)" + Constants.SaveWorkRuleTransfer)]
        [ScorableGroup(0)]
        public async Task SaveWorkRuleTransfer(IDialogContext context, IActivity activity)
        {
            var response = await this.authenticateUser.GetUserInfo(context);
            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<SaveWorkRuleTransferDialog>(response), this.Complete, ((Activity)activity).Name, default(CancellationToken));
            }
        }

        /// <summary>
        /// handle schedule command.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="result">luis result.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [LuisIntent("Schedule")]
        [ScorableGroup(1)]
        public async Task Schedule(IDialogContext context, LuisResult result)
        {
            var response = await this.authenticateUser.GetUserInfo(context);
            //// Start
            var serialized = JsonConvert.SerializeObject(result);
            var luisResult = JsonConvert.DeserializeObject<LuisResultModel>(serialized);

            //// End
            string message = ((Activity)context.Activity).Text?.ToLowerInvariant().Trim();
            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                var luisMessage = JsonConvert.SerializeObject(new Message { message = message, luisResult = luisResult });
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<ScheduleDialog>(response), this.Complete, luisMessage, default(CancellationToken));
            }
        }

        ///// <summary>
        ///// Luis returned with "None" as the winning intent, so drop down to next level of ScorableGroups.
        ///// </summary>
        ///// <param name="context">DialogContext object.</param>
        ///// <param name="result">LuisResult object.</param>
        ///// <returns>A task.</returns>
        //[LuisIntent("")]
        //[LuisIntent("None")]
        //[ScorableGroup(1)]
        //public Task None(IDialogContext context, LuisResult result)
        //{
        //    this.ContinueWithNextGroup();
        //    return Task.CompletedTask;
        //}

        /// <summary>
        /// Validates incoming activity and forwards it to ShowHoursWorkedDialog.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [RegexPattern("(?i)" + Constants.HoursWorked)]
        [RegexPattern("(?i)" + Constants.Hour)]
        [RegexPattern("(?i)" + Constants.SubmitDateRangeHoursWorked)]
        [RegexPattern("(?i)" + Constants.DateRangeHoursWorked)]
        [RegexPattern("(?i)" + Constants.PreviousPayPeriodHoursWorkedText)]
        [RegexPattern("(?i)" + Constants.CurrentpayPeriodHoursWorkedText)]
        [RegexPattern("(?i)" + Constants.HowManyHoursWorked)]
        [LuisIntent("HoursWorked")]
        [ScorableGroup(0)]
        public async Task HoursWorked(IDialogContext context, LuisResult result)
        {

            var response = await this.authenticateUser.GetUserInfo(context);
            var serialized = JsonConvert.SerializeObject(result);
            var luisResult = JsonConvert.DeserializeObject<LuisResultModel>(serialized);
            string message = ((Activity)context.Activity).Text?.ToLowerInvariant().Trim();

            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                var luisMessage = JsonConvert.SerializeObject(new Message { message = message, luisResult = luisResult });
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<ShowHoursWorkedDialog>(response), this.Complete, luisMessage, default(CancellationToken));
            }
        }

        /// <summary>
        /// Validates incoming activity and forwards it to TimeOffDialog.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [LuisIntent("TimeOffRequest")]
        [ScorableGroup(1)]
        public async Task TimeOff(IDialogContext context, LuisResult result)
        {
            var response = await this.authenticateUser.GetUserInfo(context);
            var serialized = JsonConvert.SerializeObject(result);
            string message = ((Activity)context.Activity).Text?.ToLowerInvariant().Trim();
            var luisResult = JsonConvert.DeserializeObject<LuisResultModel>(serialized);
            if (!string.IsNullOrWhiteSpace(response.JsessionID) && !string.IsNullOrWhiteSpace(response.PersonNumber))
            {
                var luisMessage = JsonConvert.SerializeObject(new Message { message = message, luisResult = luisResult });
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<TimeOffDialog>(response), this.Complete, luisMessage, default(CancellationToken));
            }
        }

        /// <summary>
        /// handle upcoming shifts command.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="activity">activity object.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [RegexPattern("(?i)" + Constants.Shift)]
        [RegexPattern("(?i)" + Constants.DateRangeShift)]
        [RegexPattern("(?i)" + Constants.SubmitDateRangeShift)]
        [RegexPattern("(?i)" + Constants.CurrentWeekShift)]
        [RegexPattern("(?i)" + Constants.NextWeekShift)]
        [LuisIntent("UpcomingShifts")]
        [ScorableGroup(0)]
        public async Task UpcomingShifts(IDialogContext context, LuisResult result)
        {
            var response = await this.authenticateUser.GetUserInfo(context);

            var serialized = JsonConvert.SerializeObject(result);
            var luisResult = JsonConvert.DeserializeObject<LuisResultModel>(serialized);

            string message = ((Activity)context.Activity).Text?.ToLowerInvariant().Trim();
            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                var luisMessage = JsonConvert.SerializeObject(new Message { message = message, luisResult = luisResult });
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<UpcomingShiftsDialog>(response), this.Complete, luisMessage, default(CancellationToken));
            }
        }


        /// <summary>
        /// Validates incoming activity and forwards it to ViewTimeOffRequestsDialog.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="activity">Activty object.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [LuisIntent("MyTimeOffRequestList")]
        [ScorableGroup(1)]
        public async Task NextVacation(IDialogContext context, LuisResult result)
        {
            var response = await this.authenticateUser.GetUserInfo(context);
            var serialized = JsonConvert.SerializeObject(result);
            var luisResult = JsonConvert.DeserializeObject<LuisResultModel>(serialized);

            //// End
            string message = ((Activity)context.Activity).Text?.ToLowerInvariant().Trim();
            if (!string.IsNullOrWhiteSpace(response.JsessionID) && !string.IsNullOrWhiteSpace(response.PersonNumber))
            {
                var luisMessage = JsonConvert.SerializeObject(new Message { message = message, luisResult = luisResult, jID = response.JsessionID });
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<ViewTimeOffRequestsDialog>(response), this.Complete, luisMessage, default(CancellationToken));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [RegexPattern("(?i)" + Constants.Welcome)]
        [ScorableGroup(0)]
        public async Task Welcome(IDialogContext context)
        {
            await this.authenticateUser.GetTenantInfo(context);
        }

        /// <summary>
        /// Take tour to app.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>representing tour to app.</returns>
        [RegexPattern("(?i)" + Constants.TakeTour)]
        [ScorableGroup(0)]
        public async Task TakeTour(IDialogContext context)
        {
            await this.welcomeCard.GetWelcomeAdaptiveCard(context);
        }

        /// <summary>
        /// Validates incoming activity and forwards it to PresentEmployeesDialog.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [RegexPattern("(?i)" + Constants.WhoIsHere)]
        [RegexPattern("(?i)" + Constants.WhoIsNotHere)]
        [RegexPattern("(?i)" + Constants.NotHereClockIn)]
        [RegexPattern("(?i)" + Constants.NotHereAddInPunches)]
        [RegexPattern("(?i)" + Constants.NotHereAddInPunchesToday)]
        [RegexPattern("(?i)" + Constants.NotHereFromMyTeam)]
        [RegexPattern("(?i)" + Constants.AbsentEmployees)]
        [RegexPattern("(?i)" + Constants.AbsenceList)]
        [RegexPattern("(?i)" + Constants.IsHereClockedIn)]
        [RegexPattern("(?i)" + Constants.IsHerePunchedIn)]
        [RegexPattern("(?i)" + Constants.IsHereFromMyTeam)]
        [RegexPattern("(?i)" + Constants.PresentEmployees)]
        [RegexPattern("(?i)" + Constants.PresenceList)]
        [RegexPattern("(?i)" + Constants.presentEmpNextpage)]
        [RegexPattern("(?i)" + Constants.presentEmpPrevpage)]
        [RegexPattern("(?i)" + Constants.absentEmpNextpage)]
        [RegexPattern("(?i)" + Constants.absentEmpPrevpage)]
        [LuisIntent("EmployeePresent")]
        [LuisIntent("EmployeeAbsent")]
        [ScorableGroup(0)]
        public async Task EmployeeAttendance(IDialogContext context, LuisResult result)
        {
            var message = ((Activity)context.Activity).Text.ToLowerInvariant();
            var serialized = JsonConvert.SerializeObject(result);
            var luisResult = JsonConvert.DeserializeObject<LuisResultModel>(serialized);
            if (luisResult?.intents[0]?.intent == "EmployeeAbsent")
            {
                ((Activity)context.Activity).Text = Constants.WhoIsNotHere;
            }
            else if (luisResult?.intents[0]?.intent == "EmployeePresent")
            {
                ((Activity)context.Activity).Text = Constants.WhoIsHere;
            }

            var response = await this.authenticateUser.GetUserInfo(context);
            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                var luisMessage = JsonConvert.SerializeObject(new Message { message = message, luisResult = luisResult });
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<PresentEmployeesDialog>(response), this.Complete, luisMessage, default(CancellationToken));
            }
        }

        /// <summary>
        /// Validates incoming activity and forwards it to SwapShiftDialog.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [RegexPattern("(?i)" + Constants.SwapShift)]
        [LuisIntent("SwapShift")]
        [ScorableGroup(0)]
        public async Task SwapShift(IDialogContext context, LuisResult result)
        {
            var response = await this.authenticateUser.GetUserInfo(context);

            var serialized = JsonConvert.SerializeObject(result);
            string message = ((Activity)context.Activity).Text?.ToLowerInvariant().Trim();
            var luisResult = JsonConvert.DeserializeObject<LuisResultModel>(serialized);
            if (!string.IsNullOrWhiteSpace(response.JsessionID) && !string.IsNullOrWhiteSpace(response.PersonNumber))
            {
                var luisMessage = JsonConvert.SerializeObject(new Message { message = message, luisResult = luisResult });
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<SwapShiftDialog>(response), this.Complete, luisMessage, default(CancellationToken));
            }
        }

        /// <summary>
        /// handle leave command.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="activity">activity object.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [RegexPattern("(?i)" + Constants.Leave)]
        [ScorableGroup(0)]
        public async Task OnLeaveToday(IDialogContext context, IActivity activity)
        {
            var response = await this.authenticateUser.GetUserInfo(context);

            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                string isManager = await this.authenticateUser.IsUserManager(context);
                if (isManager.Equals(Constants.Yes))
                {
                    string message = ((Activity)context.Activity).Text.Trim().ToLowerInvariant();
                    await context.Forward(this.dialogFactory.CreateLogonResponseDialog<OnLeaveDialog>(response), this.Complete, message, default(CancellationToken));
                }
                else if (isManager.Equals(Constants.No))
                {
                    await context.PostAsync(KronosResourceText.NoPermission);
                }
            }
        }

        /// <summary>
        /// handle employee location command.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="activity">activity object.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [RegexPattern("(?i)" + Constants.EmployeeLocation)]
        [RegexPattern("(?i)" + Constants.WhereIsSomeone)]
        [ScorableGroup(0)]
        [LuisIntent("EmployeeLocation")]
        public async Task EmployeeLocation(IDialogContext context, LuisResult result)
        {
            var response = await this.authenticateUser.GetUserInfo(context);

            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                string isManager = await this.authenticateUser.IsUserManager(context);
                if (isManager.Equals(Constants.Yes))
                {
                    string message = ((Activity)context.Activity).Text?.ToLowerInvariant().Trim();
                    var serialized = JsonConvert.SerializeObject(result);
                    var luisResult = JsonConvert.DeserializeObject<LuisResultModel>(serialized);
                    if (luisResult?.entities?.FirstOrDefault()?.type == "name" || luisResult?.entities?.FirstOrDefault()?.type == "builtin.datetimeV2.date")
                    {
                        var luisMessage = JsonConvert.SerializeObject(new Message { message = message, luisResult = luisResult });
                        await context.Forward(this.dialogFactory.CreateLogonResponseDialog<EmployeeLocationDialog>(response), this.Complete, luisMessage, default(CancellationToken));
                    }
                }
                else if (isManager.Equals(Constants.No))
                {
                    await context.PostAsync(KronosResourceText.NoPermission);
                }
            }
        }

        /// <summary>
        /// Validates incoming activity and forwards it to TeamOvertimesDialog.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [RegexPattern("(?i)" + Constants.TeamOvertimes)]
        [RegexPattern("(?i)" + Constants.PreviousWeekTeamOvertimes)]
        [LuisIntent("TeamOverTimes")]
        [ScorableGroup(1)]
        public async Task TeamOverTimes(IDialogContext context, LuisResult result)
        {
            var response = await this.authenticateUser.GetUserInfo(context);
            var serialized = JsonConvert.SerializeObject(result);
            var luisResult = JsonConvert.DeserializeObject<LuisResultModel>(serialized);
            string message = ((Activity)context.Activity).Text?.ToLowerInvariant().Trim();
            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                var luisMessage = JsonConvert.SerializeObject(new Message { message = message, luisResult = luisResult });
                await context.Forward(this.dialogFactory.CreateLogonResponseDialog<TeamOvertimesDialog>(response), this.Complete, luisMessage, default(CancellationToken));
            }
        }

        /// <summary>
        /// Sends sign in command.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>A task.</returns>
        [RegexPattern("(?i)" + Constants.SignIn)]
        [RegexPattern("(?i)" + Constants.SignInVal)]
        [ScorableGroup(0)]
        public async Task SignIn(IDialogContext context, IActivity activity)
        {
            var response = await this.authenticateUser.GetUserInfo(context);
            if (!string.IsNullOrWhiteSpace(response?.JsessionID) && !string.IsNullOrWhiteSpace(response?.PersonNumber))
            {
                await this.authenticateUser.IsSessionValid(context);
            }
        }

        /// <inheritdoc/>
        protected override async Task ActivityReceivedAsync(IDialogContext context, IAwaitable<IActivity> item)
        {
            var activity = (Activity)context.Activity;
            var message = activity.Text;

            if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                await this.authenticateUser.GetTenantInfo(context);
            }

            if (!string.IsNullOrWhiteSpace(message) && message == Constants.SignInCompleteText)
            {
                await this.authenticateUser.GetUserInfo(context);
                context.PrivateConversationData.SetValue(activity.From.Id + "SignIn", string.Empty);
                return;
            }

            if (activity.Text != null && (activity.Text.ToLowerInvariant() == Constants.Yes || activity.Text.ToLowerInvariant() == Constants.No))
            {
                context.PrivateConversationData.TryGetValue($"{context.Activity.From.Id}WorkRule", out string workRuleName);
                context.PrivateConversationData.TryGetValue($"{context.Activity.From.Id}AddPunch", out string addPunch);
                var command = string.Empty;

                if (!string.IsNullOrWhiteSpace(workRuleName))
                {
                    command = activity.Text;
                    activity.Name = command;
                    activity.Text = Constants.SaveWorkRuleTransfer;
                }
                else if (!string.IsNullOrWhiteSpace(addPunch))
                {
                    command = activity.Text;
                    activity.Name = command;
                    activity.Text = Constants.SavePunch;
                }
            }

            await base.ActivityReceivedAsync(context, item);
        }

        /// <summary>
        /// Called after each child dialogs are completed and returned back to parent with result.
        /// </summary>
        /// <param name="context">DialogContext object.</param>
        /// <param name="result">Awaitable object.</param>
        /// <returns>A task.</returns>
        private async Task Complete(IDialogContext context, IAwaitable<object> result)
        {
            await result;
            context.Done(string.Empty);
        }

        //private async Task ShowEmployeeDetails(IDialogContext context, IAwaitable<LuisResult> luisResult)
        //{
        //    string message = ((Activity)context.Activity).Text.Trim().ToLowerInvariant();

        //    if (CommandValidCheck.IsValidCommand(message))
        //    {
        //        // valid command
        //        await base.ActivityReceivedAsync(context, result);
        //        context.Done(default(object));
        //    }
        //    else
        //    {
        //        var response = await this.authenticateUser.GetUserInfo(context);

        //        // if not valid command
        //        await context.Forward(this.dialogFactory.CreateLogonResponseDialog<EmployeeLocationDialog>(response), this.Complete, message, default(CancellationToken));
        //    }
        //}
    }
}

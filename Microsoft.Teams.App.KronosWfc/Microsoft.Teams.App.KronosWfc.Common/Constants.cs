//-----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Configuration;

namespace Microsoft.Teams.App.KronosWfc.Common
{
    /// <summary>
    /// Constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// MS Teams list card item type
        /// 
        /// </summary>
        public const string ItemType = "resultItem";

        /// <summary>
        /// MS Teams card actions
        /// </summary>
        public const string TapType = "imBack";

        /// <summary>
        /// Channel id to be used in GitHub webhook
        /// </summary>
        public const string ActivityChannelId = "msteams";

        /// <summary>
        /// Name sent by MS Teams to indicate sign in event
        /// </summary>
        public const string VerifyState = "signin/verifyState";

        public const string AccrualTypeHours = "hours";

        public const string AccrualTypeDays = "days";

        public const string AccrualTypeCurrency = "currency"; ////VacationBalance

        public const string Hours = "Hours";

        public const string DateFormat = "M/d/yyyy";

        public const string Accepted = "Accepted";
        public const string Approved = "Approved";
        public const string Refused = "Refused";
        public const string Submitted = "Submitted";
        public const string Offered = "Offered";

        
        public const string Green = "Good";
        public const string Red = "Attention";
        public const string Purple = "Accent";
        public const string Default = "Default";
        public const string Requested = "Requested";

        public const string VacationBalanceStartDate = "January 1";
        public const string VacationBalanceEndDate = "December 31";
        public const string VacationBalanceVestedHours = "Vested Hours";
        public const string VacationBalanceProbationHours = "Probation Hours";
        public const string VacationBalancePlannedTakings = "Planned Takings";
        public const string VacationBalancePendingGrants = "Pending Grants";

        public const string AddPunch = "add punch";
        public const string WorkRuleTransfer = "work rule transfer";
        public const string RecentTransfer = "transfer";

        public const string WRTAdminAssistant = "Admin Assistant";
        public const string WRTAdministration = "Administration";
        public const string WRTCA8hrDay = "CA 9-80 8hr Day FT Bi-Weekly";
        public const string WRTCA9HrDay = "CA 9-80 9Hr Day FT Bi-Weekly";
        public const string WRTCAFullTime = "CA Full Time";
        public const string WRTCallback = "Callback";
        public const string WRTExecutive = "Executive";
        public const string WRTFullTime30Min = "Full Time 30 Min";
        public const string WRTFullTime60Min = "Full Time 60 Min";
        public const string WRTFullTime60MinNoZone = "Full Time 60 Min No Zone";
        public const string WRTFullTimeExecutive = "Full Time Executive";
        public const string WRTOnCall = "On-Call";
        public const string WRTPartTime = "Part Time";
        public const string WRTProfessionalHourly = "Professional Hourly";
        public const string WRTProfessionalSalaried = "Professional Salaried";
        public const string WRTProfessionalSalariedWFS = "Professional Salaried -WFS";
        public const string WRTSalaried = "Salaried";
        public const string WRTSupport = "Support";
        public const string WRTTechnicalService = "Technical Service";
        public const string WRTTraining = "Training";

        public const string VacationBalance = "how many vacation hours do i have?";

        public const string full_day = "full_day";
        public const string FullDay = "Full Day";
        public const string half_day = "half_day";
        public const string HalfDay = "Half Day";
        public const string TimeOff = "Time off";
        public const string first_half_day = "first_half_day";
        public const string FirstHalfDay = "First Half Day";
        public const string CreateTimeOff = "create time off request";
        public const string SubmitTimeOff = "submit time off request";
        public const string CancelTimeOff = "cancel time off";
        public const string CreateAdvancedTimeOff = "create advanced time off request";
        public const string AdvancedNext2 = "next advanced time off 2";
        public const string AdvancedNext3 = "next advanced time off 3";
        public const string AdvancedNext3FromHours = "next advanced time off 3_Hrs";
        public const string AdvancedNext4 = "next advanced time off 4";
        public const string AdvancedBack3ToHours = "back advanced time off 3_Hrs";        
        public const string AdvancedBack3 = "back advanced time off 3";
        public const string AdvancedBack2 = "back advanced time off 2";
        public const string AdvancedBack1 = "back advanced time off 1";
        public const string SubmitAdvancedTimeOff = "submit advanced time off request";
        public const string ApproveTimeoff = "approve time off request";
        public const string RefuseTimeoff = "refuse time off request";

        public const string NextVacation = "next vacation";
        public const string NextVacation_Next = "next vacation_next";
        public const string NextVacation_Previous = "next vacation_previous";
        public const string ShowAllVacation_Next = "show all time off requests_next";
        public const string ShowAllVacation_Previous = "show all time off requests_previous";
        public const string AllTimeOffRequests = "time off requests";
        public const string AllTimeOffRequest = "time off request";

        public const string SignInCompleteText = "signincomplete";
        public const string Help = "help";
        public const string SignOut = "sign out";
        public const string CommandNotRecognized = "Sorry, I cannot understand you or I lost track of our conversation. Type \"help\" and discover available actions.";

        public const string Vacation = "vacation";
        public const string Punches = "punches";
        public const string SavePunch = "save punch";
        public const string SaveWorkRuleTransfer = "save transfer";

        public const string Yes = "yes";
        public const string No = "no";

        public const string InvalidCred = "Invalid Credentials, please try again.";

        public const string AnotherTimePeriod = "another time period";
        public const string ShowMyPunches = "show me my punch list";
        public const string PreviousPayPeriodPunches = "previous week";
        public const string CurrentpayPeriodPunches = "current week";
        public const string DateRangePunches = "punch/daterange";
        public const string TodayPunches = "today";
        public const string SubmitDateRangePunches = "punch/submit/daterange";
        public const string SubmitDateRangeHoursWorked = "hoursworked/submit/daterange";
        public const string DateRangeHoursWorked = "hoursworked/daterange";

        public const string PreviousPayPeriodPunchesText = "Previous week";
        public const string CurrentpayPeriodPunchesText = "Current week";
        public const string TodayPunchesText = "Today";
        public const string DateRangeText = "Date Range";
        public const string ChooseTimeFrameText = "Please, choose a timeframe.";
        public const string AddPunchText = "Add Punch";
        public const string AnotherTimeFrameText = "Another Time Period";
        public const string DateRangeParseError = "Error parsing data, check the following errors: \n";
        public const string PreviousPayPeriodHoursWorkedText = "hoursworked/previouspayperiod";
        public const string CurrentpayPeriodHoursWorkedText = "hoursworked/currentpayperiod";
        public const string HowManyHoursWorked = "how many hours did i work";

        public const string HoursWorked = "hours worked";
        public const string HoursWorkedDateRangeText = "Enter the timeframe for hours worked";
        public const string PunchesDateRangeText = "Enter the timeframe for punches";
        public const string NoAccrualBalanceText = "There are no accrual balances for you.";

        public const string Shift = "shift";
        public const string DateRangeShift = "shift/daterange";
        public const string SubmitDateRangeShift = "shift/submit/daterange";
        public const string CurrentWeekShift = "shift/currentweek";
        public const string NextWeekShift = "shift/nextweek";

        public const string Schedule = "schedule";
        public const string DateRangeSchedule = "schedule/daterange";
        public const string SubmitDateRangeSchedule = "schedule/submit/daterange";
        public const string CurrentWeekSchedule = "schedule/currentweek";
        public const string NextWeekSchedule = "schedule/nextweek";

        public const string CurrentWeek = "Current week";
        public const string NextWeek = "Next week";

        public const string Welcome = "welcome";

        public const string TakeTour = "Take Tour";

        public const string WhoIsHere = "who is here";
        public const string WhoIsNotHere = "who is not here";
        public const string EmployeeLocation = "employee location";
        public const string WhereIsSomeone = "where is someone today";

        public const string WorkforceManager = "workforce manager";

        public const string VacationHoursIHave = "How many vacation hours do I have?";
        public const string MySchedule = "Show my schedule";
        public const string MyNextVacation = "When is my next vacation?";
        public const string TakeSickDay = "I need to take a sick day";
        public const string UpcomingShifts = "My upcoming shifts";
        public const string ApproachingOT = "Who is approaching overtime?";
        public const string CreateSwapShift = "swap";
        public const string ShowAllTimeOff = "Show all time off requests";

        public const string SwapShift = "swap";
        public const string SwapShiftNext2 = "next swap 2";
        public const string SwapShiftNext3 = "next swap 3";
        public const string SwapShiftNext4 = "next swap 4";
        public const string SwapShiftBack2 = "back swap 2";
        public const string SwapShiftBack3 = "back swap 3";
        public const string SwapShiftBack1 = "back swap 1";
        public const string CancelSwapShift = "cancel swap";
        public const string SubmitSwapShift = "submit swap";
        public const string ApproveSwapShiftByEmp = "approve swap request by employee";
        public const string RefuseSwapShiftByEmp = "refuse request by employee";
        public const string ApproveSwapShiftBySupervisor = "approve swap request by supervisor";
        public const string RefuseSwapShiftBySupervisor = "refuse swap request by employee";
        public const string WhiteBar = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAQAAABRCAYAAAD1sgc6AAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABl0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC4yMfEgaZUAAAAnSURBVDhPY/z04d1/BiTABKXhYFQAFYwKoIJRAVQwKoAKRrQAAwMALTMEcZENCRcAAAAASUVORK5CYII=";
        public const string GreenBar = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAABhCAIAAACRaPz+AAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAABl0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC4yMfEgaZUAAAAcSURBVDhPY5h4KBAZjfJH+aP8Uf4ofyTyDwUCAAZTG+Jp0gBvAAAAAElFTkSuQmCC";
        public const string SwapIcon = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAAZdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuMjHxIGmVAAACRElEQVRYR+2XTWsaQRjH/ypaG0UaP0JysAaaU3MtUfM5jNVrA9X4BdLmFjX1kt7TNEJ6iS/pPcFDepH05EFSEBFfKmoFsb628wyTgnVXl26CFPzB4uzj7j7/mWd2/zOaXwzMEa34nRsLAYoFDIdD0bpfFAkolUqIRCJIpVIiIk+v10Or1RJns5kpoFKpIBqN4sv1NWKnp0gkEuIfaa4uL/Fmbw/dbldEpjNVQLlcxrvDQ3xnIsxmMz8+nZ0hfn4Oube32WyiVqthpLBksgIoeSQcRqPRwKudHej1ejgcDjidTsRiMdmR0Gg00GoVTy1pATTs4VCIJ/cHAni2vs4nodFohHt7e0zEaDQSd/0bkgK+3d7yhAGW3Gazoc8mFkExnU6Hl14vXC4Xvt7cqH47JAU839jA2/192NfWRGQcKofP5+OjQ201SAqgh5pMJnEmDdWZJqValM+WB2IhYCHg/xcwGAxESx76WMl5hyoBlPz90RGSyaSITEKeEjo4QCaTEZFxVAmgj9HjpSV8PDlBIh7/EyP0BgOq1SpfR+RyOTxZXubxv1EtwO12w7W1xc3p88UFH2ryi0KhwK38B7PnYDCI1ZUVcdc4igXQg6XqaGA99Xg8cDJzogVLOp3mpaFFTL1eR2B3F0/tdnH1JIoEkMdTokfMjqUg7yARLzY3USwW0W638bPTwWu/n7vpNBRtTOiSfD4Pq9UKi8UiopP0+318OD5GNpuFl7mlfUrP77j3nREtUDqs97Pc9I7F1mwhYM4CgN/WQu/iCjztxwAAAABJRU5ErkJggg==";
        public const string RedBar = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAABhCAIAAACRaPz+AAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAABl0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC4yMfEgaZUAAAAbSURBVDhPYzhi6I2MRvmj/FH+KH+UPxL5ht4ACQ5eX5Tqr7oAAAAASUVORK5CYII=";

        public const string SuperUser = "superuser";
        public const string Leave = "who is on leave";
        public const string Sick = "sick";

        public const string TeamOvertimes = "Overtime";
        public const string TeamOvertimesMessage = "who is approaching overtime?";
        public const string PreviousWeekTeamOvertimes = "teamovertimes/previousweek";
        public const string CurrentWeekTeamOvertimes = "teamovertimes/currentweek";
        public const string DateRangeTeamOvertimes = "teamovertimes/daterange";
        public const string SubmitDateRangeTeamOvertimes = "teamovertimes/submit/daterange";
        public const string TeamOvertimesDateRangeText = "Enter the timeframe for team overtimes";
        public const string OvertimeNext = "Overtime next";
        public const string OvertimePrevious = "Overtime previous";

        public const string EmployeeTimeOffRequestList = "EmployeeTimeOffRequestList";
        public const string TOR = "TOR";
        public const string SupervisorTORList = "Supervisor TOR list";
        public const string TORListApplyFilter = "TOR list apply filter";
        public const string TORListNext = "TOR list Next";
        public const string TORListPrevious = "TOR list previous";
        public const string TORListRefresh = "TOR list refresh";
        public const string TORApproveRequest = "TOR approve request";
        public const string TORRefuseRequest = "TOR refuse request";
        public const string TORRemoveDateRange = "TOR_Remove DateRange filter";
        public const string TORRemoveEmpName = "TOR_Remove EmpName filter";
        public const string TORRemoveShowCompletedRequests = "TOR_Remove ShowCompletedRequests filter";
        public const string FilterRemoveIco = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABHNCSVQICAgIfAhkiAAAAIlJREFUKJGtkrsRhDAMRN+6IddCRorIKYYcpzijFhpCl8J9bI/nNpT27ciymOZ9M8uRRpnlOM37BhDkWgl+tASY5UjwQ64VQPcil4aUxrME3j0qNWs91Uyl0Af8bgYoPecDfgQApT2Eb8VW/W/s7oV1f1X3kbSAvwKCy5cWECCl8eTS4PKl5q3qBZjUmlftktZwAAAAAElFTkSuQmCC";        
        public const string AcceptIco = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABkAAAAaCAYAAABCfffNAAAABHNCSVQICAgIfAhkiAAAAMdJREFUSInt0i8OwjAUx/Hfm8WTYBYuAgqLYVmC4QqgCXYH6DgCCjNFgkOQJzgCp9kegpC0sH+0nevXtU36yUsLhEK+UpzG+jryDeScZBHK55GT5SBIzklGwEGEHttZcfnsk38At928WOhnXpA2wAvSBTgjfQADUZzGEaorgL3+aK4AYPyuaiIiI0DO+vdzBYxJgPc0JOWdCGOA1nUT/Qv8IF2QDVCLNEG2QCPyDQlQEGhjA7QiJkRTW6BXitM459VpkMtDIedeRZOJDhW/ghkAAAAASUVORK5CYII=";
        public const string RefuseIco = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABkAAAAZCAYAAADE6YVjAAAABHNCSVQICAgIfAhkiAAAANJJREFUSIm9lsENgzAMRb+j7sKNqANwxgswSTlyo0c6CRNwThbIJp0A91BVKmpFE+LUpyiK/7Mcxw58zZWzPEDZnOXB11wBgBFCRwajs+2kB2gnMhiF0G02/ZlFA7SrpQGK0sgBJfkeAR0KLsUpK80xzioFsyeiWZFfxVIBFAsiQxdZ5QYAr3UTll4N8g4CgBQAAJjYgzn2l3RFAXIvPhmgCir+GIu3leINsnirLz60io9fZ3lQq/kP0PMXdCLBLCvQhOWqBWnC0jvLdxLMWpo/7QGqEP2rqzaIAQAAAABJRU5ErkJggg==";

        public const string StartDateGreaterThanEndDateError = "The start date cannot be after the end date. Please, correct date range.";
        public const string DateDifferenceError = "Entered range cannot exceed 14 days. Please update and resubmit";
        public const string Punch = "punch";
        public const string Hour = "hour";
        public const string Accrual = "accrual";
        public const string NotHereClockIn = "who didn't clock in today";
        public const string NotHereAddInPunches = "who didn't add in punches";
        public const string NotHereAddInPunchesToday = "who didn't punch in today yet";
        public const string NotHereFromMyTeam = "who is not here from my team";
        public const string AbsentEmployees  = "absent employees";
        public const string AbsenceList = "absence list";
        public const string IsHereClockedIn = "who clocked in today";
        public const string IsHerePunchedIn = "who punched in today";
        public const string IsHereFromMyTeam = "who is here from my team";
        public const string PresentEmployees = "present employees";
        public const string PresenceList = "presence list";
        public const string SignIn = "sign in";
        public const string SignInVal = "signin";
        public const string SignOutVal = "signout";
        public const string SignInMessage = "it looks like you are already logged in.";
        public const string ShowMeMyPunches = "show me my punches";
        public const string SickAZTS = "Sick";
        public const string VacationAZTS = "Vacation";
        public const string PersonalAZTS = "Personal";

        public const string PendingImg = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAFAAAAARCAYAAABKFStkAAAABHNCSVQICAgIfAhkiAAAAo5JREFUWIXtlz9IG1Ecxz8XVJq0FVIFBQkGQgtxSJ0MJAHBKU4VxcUlwyUZ29qpHdTSDu1U0dE/Q4becEWhUzMVhETIEuyiQw0oB6FajyCaHDXgdZA8tBhr46XW6me6+9173997P36/93snybIyINnMCaCbG86NCRscSqMN2MxJwH3ZC7pqSODGZk7YpJvgXYRu22Wv4KrTYLXg9NQwdnujeE8mV1lYXLFEe3ysn3x+l7n5ZcbH+nE4mnj+4qMl2rVieQABEokMqXSOocFuwuEuywJ4nFevP1muWQt1LeGFxRV0vcjQ4P/b4OuSgb+ytbUHwOzMiLBVSvvZaB8AbncLdnsjhlHm8ZMPYlxljmGU2dnZF/bKvHeTn3n75hHr69/x+4/6oaYVRIZG5cAJe2vrHVQ1Syqds2Rvdc3AqBzA4Wgilc4xPTVMIpEhFleIxRV6e++LcV5vO6qaJRZXKJUOiMoB4Og8zWQ2iMUVVDWLy+Ws6svn6xDaLpeTUNBDKOjB73cLv/n87onz2QrqkoGRiJ9IxA9ALK4QCnqw2xtP2AFCQQ8Aa2vfREZsb+/R3HxLfJubXwYglc7R1/egqs+lpa/iWdeLtLXdxel0oGkFoT03v4zP12HhTuvcRI7za2lW6OnprKpTKh1ceC37+z8urHEWf+UeWAlmpTTPO6el5bZoQKGg58wSPo1CoYTX2y7eo3LgapTwaahqlkjELw50XS/+9g6XTK4SDncRDndhGGU0rfBHPhcWV+jsvCcakaYVMIxybRuoghSNvTctVfzHmZ0ZIRZXLNO7Vr9y42P96HrRUs0G02RTkqh+kl9xjt89qzWyWjExv0iyrAxgO3wpIT20TPkaYJpsYkpPfwLqmO7fiWjvUAAAAABJRU5ErkJggg==";
        public const string ApprovedImg = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAFAAAAARCAYAAABKFStkAAAABHNCSVQICAgIfAhkiAAAAwpJREFUWIXtl19IU1Ecxz/HejFtiEIJzSFt7E8N0mXRSMIiUqKYoUH0sodRL01IuLTAB3sRm1zoQXpJBAcRgUpZYApCIYUSsgqqpbioNUMrRJbl4+2h7mkm2fJKY9AHLtxzf+d8z+/+zu93z7nCHjxSr2m0CiEq+E/maLxBaM0bSvbY7gkhnNn2J+cQFGkazjwE5dn2JVcRQlTkZduJXGddAjjR0U9PsH09pP45Ex39NHpr1zzecAAbvbWklhZxW+xGpXISwwE8truG6OuXpJYWUXyB9fApp9hoVMBtsXP51jUAqp0e1IFuABRfgKOeA6S+LuIyWwG4O3EfJRLOyAZgyi+k6kIDii/AmcMn5Zx634mOfm4+GpRz9gTbKSowUR8+R0+wHa+jEoCZ+TkOtfqlX7pWLBk3+vrGMlDxBUgtLdI3NowSCctg6Gwr3sr0bAJHUx1dI70crzqYsW0wOkrVhQYavbWcOXySlhtXcDTV4Wiq43jVQRRfgAcvHlPt9Mhxboud66N3UP0higpMsn/i43tUf2iF1vRsgs35BUZCYCyA1U4P0dcvZTuWjKP6Q7I9Mz+HEgkDoA50f2//KPM/2fSs2mevIJaM0zc2LHXHJp/gLrMtW7T0xbSVWnCZrUx2DjHZOYTXUYmt1LJCS4mE+bz0xUgIjJWwy2zFZbYuyx7TpsJVx7z5kKR8i/mvbb/y6fMC8HPRbKUWBqOj0q6XeTqqP8TCl1RG+pmy5gxU/SFm5udkmejXtuKt8liQfq/6Q5jyC+Xqr2ZLZ3zqKS6zddlRw+uoZHzqKQAPX0XxbN+BuaRUZu30bIKanXtXaM0ufJLfRX1eoyW85gz0bN+xrHx1Ysk4x3bX8PzdNDPzc1w8cZa2080AtNy4IvutZkunb2yY8i1m2k43y75dI70y2OpAN6f2H+V5YkqOUSJhboeuMtk5JJ91jfSiDnTjLrPJ57Fk3HAJC3uwVjOk8Bv03VTf/TK15Rr/f+UMkofG22w7kbtoz4Q9eKQeuARiV7bdySk03iK0898Avo5LB4YGUpkAAAAASUVORK5CYII=";
        public const string RefusedImg = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAFAAAAARCAYAAABKFStkAAAABHNCSVQICAgIfAhkiAAAAn1JREFUWIXtl89Lk3Ecx1/fh+3QelDCCn8MXEP0YKIUxHDEHsECxw7VaaPRKRM8zYsQCXaYFyHyNDA9Gu4PkGeHPGyHpGMPBmGIbLCJNVkkQwfFng6yB0dtqT0PMvR1+8L3eT+fz4f3+/t9HqHeDT3QJTEtEANccAL0NGV9woYkXguE66zLaTyES5eYlrgY3qkRiAHprItodGxmC95beYNNdhjr7dU1tGis7jNOv4++yVEAEkrY7JKqavsce0tWTZmmaYkD12cXSChh1mcXaB8exOn31d3f+eg+26trlg7PKiyNcFZNcbCz+899dvkypXzBylIsw/QIH6VnLIhddhiRufPqOS23ewE42NklGYygxOe41HoVdyhA25CH75++ILs6eP/0BQD9U+NV65HkkqG/PrtAVk39VbfyfncoAMDeZsaSHi1xYN/kKCPJJdqGPLwLPAMOB2FvlkkoYRJKmP3cV/qnxkkGIxzs7LK1vGI0Xgvv4owR9YQSJqumauo6/T7coYBxnBTTuaqz2SwscWDFGSPJJZx+H1k1hezqoKmrs8pBJ3VFMZ2jfXiQUr7AxnwcoK7u3mbGcL8WjXHdY/6/gqUR3l5do+vJQ6OJ49zI9dCiMbRoDCU+Z7irlm7/1Dg/fxRPX/wxsfQS0aIx7LKDnrEgxXTuWA4o5Qs4Wq8Z6ys3u//YkwxG2NvM0HKrt6ZuKV8wzkU4HGjDRPgo3z58xB0KkFDCeBdnqqK2tbxiRLHCxnyctiGPse9ozL2LMzR1dQKHl0XFdbV0m7tvVOn8Ku6b3p9QfY9101XPERe/cv+JpKNb84F0DtBBk0RZj+ignXUxjYaOnhHl8svf7nII66XbJigAAAAASUVORK5CYII=";

        public const string Regular = "Regular";
        public const string AllHours = "All Hours";

        public const string TorListOpenModal = "TOR list open modal";
        public const string presentEmpNextpage = "present/nextpage";
        public const string presentEmpPrevpage = "present/prevpage";
        public const string absentEmpNextpage = "absent/nextpage";
        public const string absentEmpPrevpage = "absent/prevpage";
    }
}

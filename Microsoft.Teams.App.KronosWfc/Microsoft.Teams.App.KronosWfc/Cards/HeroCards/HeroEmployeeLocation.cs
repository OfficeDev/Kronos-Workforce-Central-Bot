//-----------------------------------------------------------------------
// <copyright file="HeroEmployeeLocation.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Cards.HeroCards
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using JobAssignmentAlias = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.JobAssignment;
    using ShowPunchesAlias = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.ShowPunches;
    using UpcomingShiftAlias = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Shifts.UpcomingShifts;

    /// <summary>
    /// hero employee location card
    /// </summary>
    [Serializable]
    public class HeroEmployeeLocation
    {
        /// <summary>
        /// show employee details card.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="shiftData">shifts data.</param>
        /// <param name="punchData">punch data.</param>
        /// <param name="employeeName">emp name.</param>
        /// <param name="jobAssignmentData">job assignment data.</param>
        /// <returns>emp details card.</returns>
        public async Task ShowEmployeeDetailCard(IDialogContext context, UpcomingShiftAlias.Response shiftData, ShowPunchesAlias.Response punchData, string employeeName, JobAssignmentAlias.Response jobAssignmentData)
        {
            var reply = context.MakeMessage();
            var heroCard = new HeroCard();
            heroCard.Title = employeeName;
            var showPunchesDataOrderedList = punchData?.Timesheet?.TotaledSpans?.TotaledSpan?.OrderByDescending(x => x.InPunch.Punch.Date ?? x.OutPunch.Punch.Date).ThenByDescending(x => x.InPunch.Punch.Time ?? x.OutPunch.Punch.Time).FirstOrDefault();
            var shiftsToday = shiftData.Schedule?.ScheduleItems?.ScheduleShift?.OrderBy(x => x.StartDate).FirstOrDefault();
            int lastIndex = (jobAssignmentData != null) ? (jobAssignmentData?.JobAssign?.PrimaryLaborAccList?.PrimaryLaborAcc?.OrganizationPath).LastIndexOf('/') : 0;
            StringBuilder str = new StringBuilder();
            str.Append("<br/><u><b>" + Resources.KronosResourceText.Location + "</b></u>");
            str.Append($"<br/><b>{Resources.KronosResourceText.PrimaryOrg}</b> - {jobAssignmentData?.JobAssign?.PrimaryLaborAccList?.PrimaryLaborAcc?.OrganizationPath?.Substring(0, lastIndex)}");
            str.Append($"<br/><b>{Resources.KronosResourceText.PrimaryJob}</b> - {jobAssignmentData?.JobAssign?.PrimaryLaborAccList?.PrimaryLaborAcc?.OrganizationPath?.Substring(lastIndex + 1)}");
            str.Append($"<br/><br/><u><b>{Resources.KronosResourceText.Shifts}</b></u>");

            if (shiftsToday?.ShiftSegments?.Count > 0)
            {
                foreach (UpcomingShiftAlias.ShiftSegment shiftSegment in shiftsToday?.ShiftSegments)
                {
                    str.Append($"<br/><b>{shiftSegment.StartTime} - {shiftSegment.EndTime}</b> {shiftSegment.SegmentTypeName}");
                }
            }
            else
            {
                str.Append("<br/>" + Resources.KronosResourceText.NoShiftsForToday);
            }

            str.Append("<br/><br/><u><b>" + Resources.KronosResourceText.LastPunch + "</b></u><br/>");

            if (showPunchesDataOrderedList == null || string.IsNullOrEmpty(showPunchesDataOrderedList.InPunch.Punch?.EnteredOnDate))
            {
                str.Append(Resources.KronosResourceText.NoPunchesForToday);
            }
            else
            {
                if (showPunchesDataOrderedList?.OutPunch?.Punch?.Time != null)
                {
                    // if outpunch available then show out punch
                    str.Append($"<b>{Resources.KronosResourceText.PunchTime}</b> - {DateTime.Parse(showPunchesDataOrderedList.OutPunch.Punch?.EnteredOnDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture)} {showPunchesDataOrderedList.OutPunch.Punch.Time}");
                    str.Append($"<br/><b>{Resources.KronosResourceText.EnteredOn}</b> - {DateTime.Parse(showPunchesDataOrderedList.OutPunch.Punch?.EnteredOnDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture)} {showPunchesDataOrderedList.OutPunch.Punch.EnteredOnTime}");
                }
                else
                {
                    str.Append($"<b>{Resources.KronosResourceText.PunchTime}</b> - {DateTime.Parse(showPunchesDataOrderedList.InPunch.Punch?.EnteredOnDate, CultureInfo.InvariantCulture , DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture)} {showPunchesDataOrderedList.InPunch.Punch.Time}");
                    str.Append($"<br/><b>{Resources.KronosResourceText.EnteredOn}</b> - {DateTime.Parse(showPunchesDataOrderedList.InPunch.Punch?.EnteredOnDate, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MMM d, yyyy", CultureInfo.InvariantCulture)} {showPunchesDataOrderedList.InPunch.Punch.EnteredOnTime}");
                }
            }

            heroCard.Text = str.ToString();
            str.Clear();
            reply.Attachments.Add(heroCard.ToAttachment());
            await context.PostAsync(reply);
        }
    }
}
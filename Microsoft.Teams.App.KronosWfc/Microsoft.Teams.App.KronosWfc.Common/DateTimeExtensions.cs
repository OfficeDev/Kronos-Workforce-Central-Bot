//-----------------------------------------------------------------------
// <copyright file="DateTimeExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Common
{
    using System;

    /// <summary>
    /// Date Time Extensions
    /// </summary>
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime StartWeekDate(this DateTime dt, DayOfWeek startOfWeek)
        {
            DayOfWeek day = dt.DayOfWeek;
            int days = day - startOfWeek;
            return dt.AddDays(-days);
        }

        public static DateTime EndOfWeek(this DateTime start)
        {
           return start.AddDays(6);
        }
    }
}

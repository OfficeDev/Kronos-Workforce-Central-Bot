//-----------------------------------------------------------------------
// <copyright file="Timezone.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Common
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Timezone
    /// </summary>
    public class Timezone
    {
        public static HashSet<string> GetKronosTimeZone()
        {
            HashSet<string> timeZone = new HashSet<string>();
            timeZone.Add("(GMT -12:00) Dateline");
            timeZone.Add("(GMT -11:00) Midway; Samoa");
            timeZone.Add("(GMT -10:00) Hawaii");
            timeZone.Add("(GMT -09:00) Alaska");
            timeZone.Add("(GMT -08:00) Pacific Time");
            timeZone.Add("(GMT -07:00) Arizona");
            timeZone.Add("(GMT -07:00) Chihuahua");
            timeZone.Add("(GMT -07:00) Mountain Time");
            timeZone.Add("(GMT -06:00) Mexico City");
            timeZone.Add("(GMT -06:00) Saskatchewan");
            timeZone.Add("(GMT -06:00) Central America");
            timeZone.Add("(GMT -06:00) Central Time");
            timeZone.Add("(GMT -05:00) SA Pacific");
            timeZone.Add("(GMT -05:00) Indiana (East)");
            timeZone.Add("(GMT -05:00) Eastern Time");
            timeZone.Add("(GMT -04:00) SA Western");
            timeZone.Add("(GMT -04:00) Santiago");
            timeZone.Add("(GMT -04:00) Atlantic Time");
            timeZone.Add("(GMT -03:30) Newfoundland");
            timeZone.Add("(GMT -03:00) Brasilia");
            timeZone.Add("(GMT -03:00) Buenos Aires");
            timeZone.Add("(GMT -03:00) Greenland");
            timeZone.Add("(GMT -02:00) Mid-Atlantic");
            timeZone.Add("(GMT -01:00) Azores");
            timeZone.Add("(GMT -01:00) Cape Verde Is.");
            timeZone.Add("(GMT) Greenwich Mean Time");
            timeZone.Add("(GMT) Casablanca");
            timeZone.Add("(GMT +01:00) Amsterdam; Berlin");
            timeZone.Add("(GMT +01:00) Warsaw");
            timeZone.Add("(GMT +01:00) Paris");
            timeZone.Add("(GMT +01:00) Belgrade");
            timeZone.Add("(GMT +01:00) West Central Africa");
            timeZone.Add("(GMT +02:00) Athens");
            timeZone.Add("(GMT +02:00) Egypt");
            timeZone.Add("(GMT +02:00) Bucharest");
            timeZone.Add("(GMT +02:00) South Africa");
            timeZone.Add("(GMT +02:00) Jerusalem");
            timeZone.Add("(GMT +02:00) Helsinki");
            timeZone.Add("(GMT +03:00) Kuwait; Riyadh");
            timeZone.Add("(GMT +03:00) Moscow");
            timeZone.Add("(GMT +03:00) Baghdad");
            timeZone.Add("(GMT +03:00) Nairobi");
            timeZone.Add("(GMT +03:30) Tehran");
            timeZone.Add("(GMT +04:00) Abu Dhabi; Muscat");
            timeZone.Add("(GMT +04:00) Tblisi");
            timeZone.Add("(GMT +04:30) Kabul");
            timeZone.Add("(GMT +05:00) Islamabad");
            timeZone.Add("(GMT +05:00) Ekaterinburg");
            timeZone.Add("(GMT +05:30) Calcutta");
            timeZone.Add("(GMT +05:45) Kathmandu");
            timeZone.Add("(GMT +06:00) Central Asia");
            timeZone.Add("(GMT +06:00) Almaty; Novosibirsk");
            timeZone.Add("(GMT +06:00) Sri Jayawardenepura");
            timeZone.Add("(GMT +06:30) Rangoon");
            timeZone.Add("(GMT +07:00) Bangkok; Hanoi; Jakarta");
            timeZone.Add("(GMT +07:00) Krasnoyarsk");
            timeZone.Add("(GMT +08:00) Hong Kong");
            timeZone.Add("(GMT +08:00) Taipei");
            timeZone.Add("(GMT +08:00) Irkutsk");
            timeZone.Add("(GMT +08:00) Singapore");
            timeZone.Add("(GMT +08:00) Perth");
            timeZone.Add("(GMT +09:00) Tokyo");
            timeZone.Add("(GMT +09:00) Seoul");
            timeZone.Add("(GMT +09:00) Yakutsk");
            timeZone.Add("20072007CENTRAL_AUSTRALIA");
            timeZone.Add("(GMT +09:30) Darwin");
            timeZone.Add("(GMT +09:30) Adelaide");
            timeZone.Add("20072007SYDNEY");
            timeZone.Add("(GMT +10:00) Guam");
            timeZone.Add("20072007TASMANIA");
            timeZone.Add("(GMT +10:00) Brisbane");
            timeZone.Add("(GMT +10:00) Vladivostok");
            timeZone.Add("(GMT +10:00) Sydney");
            timeZone.Add("(GMT +10:00) Hobart");
            timeZone.Add("(GMT +10:30) Lord Howe Island");
            timeZone.Add("(GMT +11:00) Solomon Is.");
            timeZone.Add("(GMT +12:00) Marshall Is.");
            timeZone.Add("20072006NEW_ZEALAND");
            timeZone.Add("(GMT +12:00) Auckland; Wellington");
            timeZone.Add("(GMT +13:00) Nuku&apos;alofa");

            return timeZone;
        }

        public static string GetLocalTimeZoneOffset(DateTimeOffset? localTimestamp)
        {
            var tzOffsetHours = string.Empty;

            if (localTimestamp.Value.Offset.Hours > 0)
            {
                tzOffsetHours = localTimestamp.Value.Offset.Hours < 10 ? $"+0{localTimestamp.Value.Offset.Hours}" : $"+{localTimestamp.Value.Offset.Hours}";
            }

            else if (localTimestamp.Value.Offset.Hours < 0)
            {
                tzOffsetHours = localTimestamp.Value.Offset.Hours < -10 ? $"-0{localTimestamp.Value.Offset.Hours}" : $"-{localTimestamp.Value.Offset.Hours}";
            }

            else if (localTimestamp.Value.Offset.Hours == 0)
            {
                tzOffsetHours = $"{localTimestamp.Value.Offset.Hours}";
            }

            return $"{tzOffsetHours}:{localTimestamp.Value.Offset.Minutes}";
        }
    }
}

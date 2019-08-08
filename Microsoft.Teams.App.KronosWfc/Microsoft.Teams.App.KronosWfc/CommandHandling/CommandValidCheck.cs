//-----------------------------------------------------------------------
// <copyright file="CommandValidCheck.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.CommandHandling
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Resources;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// command valid check class.
    /// </summary>
    public class CommandValidCheck
    {
        /// <summary>
        /// Method to check whether the command typed by user is recognized.
        /// </summary>
        /// <param name="message">the incoming command to check.</param>
        /// <returns>A boolean.</returns>
        public static bool IsValidCommand(string message)
        {
            var commands = new ResourceManager(typeof(KronosCommand));

            if (!string.IsNullOrEmpty(message))
            {
                var resourceSet = commands.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
                foreach (DictionaryEntry entry in resourceSet)
                {
                    if (Convert.ToString(entry.Value).ToLowerInvariant().Contains(message))
                    {
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }

            return false;
        }
    }
}
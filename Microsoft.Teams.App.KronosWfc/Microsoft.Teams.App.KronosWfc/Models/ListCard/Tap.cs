//-----------------------------------------------------------------------
// <copyright file="Tap.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Models
{
    /// <summary>
    /// Helper class to create the MS Teams List card for help command.
    /// </summary>
    public class Tap
    {
        /// <summary>
        /// Gets or sets type of the tap
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Gets or sets title of the tap
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Gets or sets value of the tap
        /// </summary>
        public string value { get; set; }
    }
}
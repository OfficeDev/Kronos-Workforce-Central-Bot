//-----------------------------------------------------------------------
// <copyright file="Button.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Models
{
    /// <summary>
    /// Helper class to create the MS Teams List card actions.
    /// </summary>
    public class Button
    {
        /// <summary>
        /// Gets or sets type of the card actions like imBack, openUrl etc.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Gets or sets title of the button.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Gets or sets value of the button.
        /// </summary>
        public string value { get; set; }

        public string text { get; set; }

        public string displayText { get; set; }
    }
}
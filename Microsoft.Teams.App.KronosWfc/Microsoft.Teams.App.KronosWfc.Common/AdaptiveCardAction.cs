//-----------------------------------------------------------------------
// <copyright file="AdaptiveCardAction.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Common
{
    /// <summary>
    /// Adaptive Card Action class
    /// </summary>
    public class AdaptiveCardAction
    {
        /// <summary>
        /// Gets or sets Msteams object
        /// </summary>
        public Msteams msteams { get; set; }
    }

    /// <summary>
    /// MSTeams class
    /// </summary>
    public class Msteams
    {
        /// <summary>
        /// Gets or sets Type
        /// </summary>
        public string type
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets Display text
        /// </summary>
        public string displayText
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets Text
        /// </summary>
        public string text
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets Value
        /// </summary>
        public string value
        {
            get; set;
        }
    }
}

//-----------------------------------------------------------------------
// <copyright file="Content.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Models
{
    /// <summary>
    /// Helper class to create the MS Teams List content.
    /// </summary>
    public class Content
    {
        /// <summary>
        /// Gets or sets title of the content.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Gets or sets array of items to be added on to the content.
        /// </summary>
        public Item[] items { get; set; }

        /// <summary>
        /// Gets or sets array of buttons to be added to the content.
        /// </summary>
        public Button[] buttons { get; set; }
    }
}
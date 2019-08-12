//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Models
{
    /// <summary>
    /// Helper class to create the MS Teams List card for help command.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Gets or sets type of the individual item
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Gets or sets title of the individual item
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Gets or sets id of the individual item
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Gets or sets subtitle of the individual item
        /// </summary>
        public string subtitle { get; set; }

        /// <summary>
        /// Gets or sets tap object
        /// </summary>
        public Tap tap { get; set; }

        /// <summary>
        /// Gets or sets icon of the individual item
        /// </summary>
        public string icon { get; set; }
    }
}
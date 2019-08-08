//-----------------------------------------------------------------------
// <copyright file="ListCard.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Models
{
    /// <summary>
    /// Helper class to create the MS Teams List card for help command.
    /// </summary>
    public class ListCard
    {
        /// <summary>
        /// Gets or sets content object
        /// </summary>
        public Content content { get; set; }

        /// <summary>
        /// Gets or sets content type of the list card
        /// </summary>
        public string contentType { get; set; } = "application/vnd.microsoft.teams.card.list";
    }
}
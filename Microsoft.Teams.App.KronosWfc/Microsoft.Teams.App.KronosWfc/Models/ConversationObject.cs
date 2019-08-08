namespace Microsoft.Teams.App.KronosWfc.Models
{
    using System;

    /// <summary>
    /// User conversation model.
    /// </summary>
    public class ConversationObject
    {
        /// <summary>
        /// Gets or Sets Start Date as user input.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or Sets Teams user id.
        /// </summary>
        // public string TeamsUserId { get; set; }
        /// <summary>
        /// Gets or Sets Jsession
        /// </summary>
        public string PreviousCommand { get; set; }

        /// <summary>
        /// Gets or Sets End Date.
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
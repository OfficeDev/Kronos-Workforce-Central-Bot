namespace Microsoft.Teams.App.KronosWfc.Models
{
    /// <summary>
    /// Root class with properties needed to pass along to various dialog handlers for processing.
    /// </summary>
    public class RootContextObject
    {
        /// <summary>
        /// Gets or sets The message or command.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets OAuth token.
        /// </summary>
        public string Jsession { get; set; }
    }
}
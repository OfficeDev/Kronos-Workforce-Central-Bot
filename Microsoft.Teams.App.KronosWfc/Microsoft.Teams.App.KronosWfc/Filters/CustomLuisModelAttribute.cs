namespace Microsoft.Teams.App.KronosWfc.Filters
{
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Teams.App.KronosWfc.Common;

    /// <summary>
    /// Luis model attribute to set values from app config.
    /// </summary>
    public sealed class CustomLuisModelAttribute : LuisModelAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomLuisModelAttribute"/> class.
        /// </summary>
        public CustomLuisModelAttribute()
            : base(modelID: AppSettings.Instance.LuisModelId, subscriptionKey: AppSettings.Instance.LuisSubscriptionKey)
        {
        }
    }
}
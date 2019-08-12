namespace Microsoft.Teams.App.KronosWfc.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;

    public class CredentialProvider : ICredentialProvider
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialProvider"/> class.
        /// </summary>
        public CredentialProvider()
        {
            try
            {
                if (string.IsNullOrEmpty(MicrosoftAppId) || string.IsNullOrEmpty(MicrosoftAppPassword))
                {
                    MicrosoftAppId = AppSettings.Instance.MicrosoftAppId;
                    MicrosoftAppPassword = AppSettings.Instance.MicrosoftAppPassword;
                    this.Credentials.Add(MicrosoftAppId, MicrosoftAppPassword);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets or sets microsoft appid string.
        /// </summary>
        public static string MicrosoftAppId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets microsoft app password string.
        /// </summary>
        public static string MicrosoftAppPassword { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets credentials key value.
        /// </summary>
        public Dictionary<string, string> Credentials { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// checks if the AppId is valid.
        /// </summary>
        /// <param name="appId">app id.</param>
        /// <returns>boolean result for appId verification.</returns>
        public Task<bool> IsValidAppIdAsync(string appId)
        {
            return Task.FromResult(this.Credentials.ContainsKey(appId));
        }

        /// <summary>
        /// gets the App password.
        /// </summary>
        /// <param name="appId">app id.</param>
        /// <returns>returns the password string.</returns>
        public Task<string> GetAppPasswordAsync(string appId)
        {
            return Task.FromResult(this.Credentials.ContainsKey(appId) ? this.Credentials[appId] : null);
        }

        /// <summary>
        /// check if authentication is disabled.
        /// </summary>
        /// <returns>returns boolean value for authentication.</returns>
        public Task<bool> IsAuthenticationDisabledAsync()
        {
            return Task.FromResult(!this.Credentials.Any());
        }
    }
}
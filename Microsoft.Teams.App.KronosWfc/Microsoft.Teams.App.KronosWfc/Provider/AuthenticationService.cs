//-----------------------------------------------------------------------
// <copyright file="ILogonActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Core;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Logon;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Authentication service class.
    /// </summary>
    [Serializable]
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogonActivity logonActivity;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// </summary>
        /// <param name="logonActivity">login activity object.</param>
        public AuthenticationService(ILogonActivity logonActivity)
        {
            this.logonActivity = logonActivity;
        }

        /// <summary>
        /// login user method.
        /// </summary>
        /// <param name="activity">activity object.</param>
        /// <returns>login response.</returns>
        public async Task<Response> LoginUser(Activity activity)
        {
            Response response = default(Response);
            try
            {
                JObject data = activity.Value as JObject;
                if (data != null && data["state"] != null)
                {
                    JObject tenant = activity.ChannelData as JObject;
                    string tenantId = tenant["tenant"].SelectToken("id").ToString();
                    User user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(data["state"].ToString());
                    user.TenantId = tenantId;
                    response = await this.logonActivity.Logon(user);
                    return response;
                }

                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Send OAuth card to user for sign in.
        /// </summary>
        /// <param name="context">Dialog Context.</param>
        /// <param name="activity">Activity object.</param>
        /// <returns>sign in card.</returns>
        public async Task SendAuthCardAsync(IDialogContext context, Activity activity)
        {
            var buttonLabel = $"{KronosResourceText.ClickLogin}";
            var reply = ((Activity)context.Activity).CreateReply();
            JObject tenant = activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            string teamsUserId = activity.From.Id;
            string convId = activity.Conversation.Id;
            var loginUrl = $"{ConfigurationManager.AppSettings["BaseUri"].ToString()}/Login?tid={tenantId}";

            reply.Attachments = new List<Attachment>()
                    {
                        new Attachment()
                        {
                            ContentType = HeroCard.ContentType,
                            Content = new HeroCard()
                            {
                                Title = KronosResourceText.SignInLabel,
                                Subtitle = KronosResourceText.LoginToContinue,
                                Buttons = new CardAction[]
                                {
                                    new CardAction() { Title = buttonLabel, Value = loginUrl, Type = ActionTypes.Signin },
                                },
                            },
                        },
                    };
            // Create the ConnectorClientFactory
            IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(reply), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
            var msgToUpdate = await factory.MakeConnectorClient().Conversations.SendToConversationAsync(reply.Conversation.Id, reply);

            // saving the activity Id to update the card after successful authentication
            context.PrivateConversationData.SetValue(activity.From.Id + "SignIn", msgToUpdate.Id);
        }

        /// <summary>
        /// message on installing bot.
        /// </summary>
        /// <param name="activity">activity object.</param>
        /// <returns>welcome message.</returns>
        public async Task<Response> InstallBot(Activity activity)
        {
            try
            {
                JObject tenant = activity.ChannelData as JObject;
                string tenantId = tenant["tenant"].SelectToken("id").ToString();
                Response response = default(Response);
                response = await this.logonActivity.Install(tenantId);
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// login super user method.
        /// </summary>
        /// <param name="activity">activity object.</param>
        /// <returns>login response.</returns>
        public async Task<Response> LoginSuperUser(Activity activity)
        {
            Response response = default(Response);
            try
            {
                JObject data = activity.Value as JObject;

                JObject tenant = activity.ChannelData as JObject;
                string tenantId = tenant["tenant"].SelectToken("id").ToString();
                response = await this.logonActivity.LogonSuperUser(tenantId);
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

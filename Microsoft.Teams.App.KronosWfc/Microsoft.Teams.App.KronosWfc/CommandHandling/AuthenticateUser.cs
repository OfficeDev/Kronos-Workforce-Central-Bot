//-----------------------------------------------------------------------
// <copyright file="AuthenticateUser.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.CommandHandling
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Role;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Vacation;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.PersonInfo;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// authenticate user class.
    /// </summary>
    [Serializable]
    public class AuthenticateUser
    {
        /// <summary>
        /// authentication service interface instance variable.
        /// </summary>
        private readonly IAuthenticationService authenticationService;

        /// <summary>
        /// role activity interface instance variable.
        /// </summary>
        private readonly IRoleActivity roleActivity;

        /// <summary>
        /// azure storage helper interface instance.
        /// </summary>
        private readonly IAzureTableStorageHelper azureTableStorageHelper;

        /// <summary>
        /// vacation balance activity interface instance variable.
        /// </summary>
        private readonly IViewVacationBalanceActivity viewBalanceActivity;

        /// <summary>
        /// bot user entity instance variable.
        /// </summary>
        private BotUserEntity botUserEntity;

        /// <summary>
        /// login user info object.
        /// </summary>
        private LoginResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticateUser"/> class.
        /// </summary>
        /// <param name="response">login response.</param>
        /// <param name="authenticationService">authentication service.</param>
        /// <param name="roleActivity">role activity.</param>
        /// <param name="azureTableStorageHelper">table storage helper.</param>
        /// <param name="viewBalanceActivity">ViewBalance activity.</param>
        /// <param name="botUserEntity">bot user entity.</param>
        public AuthenticateUser(LoginResponse response, IAuthenticationService authenticationService, IRoleActivity roleActivity, IAzureTableStorageHelper azureTableStorageHelper, IViewVacationBalanceActivity viewBalanceActivity, BotUserEntity botUserEntity)
        {
            this.response = response;
            this.authenticationService = authenticationService;
            this.roleActivity = roleActivity;
            this.azureTableStorageHelper = azureTableStorageHelper;
            this.viewBalanceActivity = viewBalanceActivity;
            this.botUserEntity = botUserEntity;
        }

        /// <summary>
        /// get user info.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <returns>user login info.</returns>
        public async Task<LoginResponse> GetUserInfo(IDialogContext context)
        {
            var activity = (Activity)context.Activity;
            if (!context.UserData.TryGetValue(activity.From.Id, out LoginResponse response))
            {
                this.response = response;
            }

            if (string.IsNullOrEmpty(response?.JsessionID) && activity.Name != Constants.VerifyState)
            {
                // User is not logged in - Send Sign in card
                await this.authenticationService.SendAuthCardAsync(context, activity);
            }
            else if (activity.Name == Constants.VerifyState)
            {
                JObject data = activity.Value as JObject;
                if (data != null && data["state"] != null)
                {
                    // set the key and value
                    response = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponse>(data["state"].ToString());
                    context.UserData.SetValue(activity.From.Id, response);
                    JObject tenant = activity.ChannelData as JObject;

                    string tenantId = tenant["tenant"].SelectToken("id").ToString();

                    // update sign in card
                    var reply = ((Activity)context.Activity).CreateReply();
                    var conversationId = context.Activity.Conversation.Id;

                    // get the reply to id
                    context.PrivateConversationData.TryGetValue(activity.From.Id + "SignIn", out string activityId);

                    // Create the ConnectorClientFactory
                    IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = KronosResourceText.SignInLabel,
                        Subtitle = KronosResourceText.SignInSuccess,
                    };
                    reply.Attachments.Add(heroCard.ToAttachment());

                    await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, reply);
                    await context.PostAsync(KronosResourceText.NiceToSeeYou.Replace("{response.Name}", response.Name));

                    // store user details in azure table
                    this.botUserEntity.TenantId = tenantId;
                    this.botUserEntity.PersonNumber = response.PersonNumber;
                    this.botUserEntity.TeamsUserId = activity.From.Id;
                    this.botUserEntity.ConversationId = activity.Conversation.Id;
                    this.botUserEntity.PartitionKey = Constants.ActivityChannelId;
                    this.botUserEntity.RowKey = response.PersonNumber + "===" + activity.From.Id;

                    await this.azureTableStorageHelper.InsertOrMergeTableEntityAsync(this.botUserEntity, AppSettings.Instance.KronosUserTableName);

                    // login super user
                    var superUserLogonRes = await this.authenticationService.LoginSuperUser((Activity)context.Activity);
                    if (superUserLogonRes?.Status == ApiConstants.Success)
                    {
                        context.UserData.SetValue(activity.From.Id + Constants.SuperUser, superUserLogonRes.Jsession);
                    }
                    else
                    {
                        await context.PostAsync(KronosResourceText.SuperUserLoginError);
                    }
                }
                else
                {
                    // handle error response
                }
            }

            return response;
        }

        /// <summary>
        /// get tenant info.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <returns>user login info.</returns>
        public async Task<LoginResponse> GetTenantInfo(IDialogContext context)
        {
            var activity = (Activity)context.Activity;

            var result = await this.authenticationService.InstallBot(activity);

            if (string.IsNullOrEmpty(result.ErrorCode))
            {
                var message = context.MakeMessage();
                string fullPath = HttpContext.Current.Server.MapPath("/Cards/AdaptiveCards/Welcome/WelcomeCard.json");
                var adaptiveCard = File.ReadAllText(fullPath);
                adaptiveCard = adaptiveCard.Replace("{logo}", ConfigurationManager.AppSettings["BaseUri"] + "/Static/Images/logo.png");
                var card = AdaptiveCard.FromJson(adaptiveCard).Card;
                message.Attachments.Add(new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = card,
                });
                await context.PostAsync(message);
            }
            else
            {
                await context.PostAsync(KronosResourceText.ContactAdmin);
            }

            return this.response;
        }

        /// <summary>
        /// check if user is manager.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <returns>is manager confirmation text.</returns>
        public async Task<string> IsUserManager(IDialogContext context)
        {
            string jSession = string.Empty;
            string personNumber = string.Empty;
            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                personNumber = this.response.PersonNumber;
                jSession = this.response.JsessionID;
            }

            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            Response personInfoResponse = await this.roleActivity.GetPersonInfo(tenantId, personNumber, jSession);

            if (personInfoResponse?.Status == ApiConstants.Failure)
            {
                // User is not logged in - Send Sign in card
                if (personInfoResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }
                else if (personInfoResponse.Error?.ErrorCode == ApiConstants.UserUnauthorizedError)
                {
                    await context.PostAsync(KronosResourceText.NoPermission);
                }
            }
            else
            {
                // check if user is manager.
                var result = personInfoResponse?.PersonInformation?.PersonLicenseTypes?.PersonLicenseType.FirstOrDefault(x => x.LicenseTypeName.ToLowerInvariant().Contains(Constants.WorkforceManager));
                return (result != null) ? Constants.Yes : Constants.No;
            }

            return string.Empty;
        }

        /// <summary>
        /// Check if the session is valid by vacation bal api call.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <returns>A <see cref="Task"/> send sign in card incase user is not logged in.</returns>
        public async Task IsSessionValid(IDialogContext context)
        {
            string jSession = string.Empty;
            string personNumber = string.Empty;
            string personName = string.Empty;
            if (context.UserData.TryGetValue(context.Activity.From.Id, out this.response))
            {
                personNumber = this.response.PersonNumber;
                jSession = this.response.JsessionID;
                personName = this.response.Name;
            }

            JObject tenant = context.Activity.ChannelData as JObject;
            string tenantId = tenant["tenant"].SelectToken("id").ToString();
            var viewBalanceResponse = await this.viewBalanceActivity.ViewBalance(tenantId, jSession, personNumber);
            if (viewBalanceResponse?.Status == ApiConstants.Failure)
            {
                if (viewBalanceResponse.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }
            }
            else
            {
                await context.PostAsync($"{this.response?.Name}, {Constants.SignInMessage}");
            }
        }
    }
}
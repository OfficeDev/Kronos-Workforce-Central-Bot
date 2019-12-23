
  

# Prerequisites

  

  

  

To begin, you will need:

  

  

  

* An Azure subscription where you can create the following kinds of resources:

  

  

  

* App service

  

  

  

* App service plan

  

  

  

* Bot channels registration

  

  

  

* Azure storage account




  

  

  

* LUIS cognitive service

  

  

  

* Application Insights



  

  

  
* Azure Keyvault
* This is for one to one (end-user) to install personally


  

  

  

* A copy of the Kronos Workforce Central GitHub repo (https://github.com/OfficeDev/Kronos-Workforce-Central-Bot)

# Step 1: Register Azure AD applications

  

  

  

  

Register two Azure AD applications in your tenant's directory: one for the bot, and another for the configuration app.

  

  

  

  

1. Log in to the Azure Portal for your subscription, and go to the "App registrations" blade [here](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps).

  

  

  

  

2. Click on "New registration", and create an Azure AD application.

  

  

  

	1. **Name**: The name of your Teams app - if you are following the template for a default deployment, we recommend "Kronos workforce central".

  

  

  

	2. **Supported account types**: Select "Accounts in any organizational directory"

  

  

  

3. Leave the "Redirect URI" field blank.

  

  

  

  

	![singletenant-app-creation](\wiki\images\singletenant-app-creation.png)

  

  

  

  

4. Click on the "Register" button.

  

  

  

  

5. When the app is registered, you'll be taken to the app's "Overview" page. Copy the **Application (client) ID**; we will need it later. Verify that the "Supported account types" is set to **Multiple organizations**.

  

  

  

  

	![singletenant overview](\wiki\images\singletenant overview.png)

  

  

  

  

5. On the side rail in the Manage section, navigate to the "Certificates & secrets" section. In the Client secrets section, click on "+ New client secret". Add a description for the secret and select an expiry time. Click "Add".

  

  

  

  

	![singletenant-app-secret](/wiki/images/singletenant-app-secret.png)

  

  

  

  

6. Once the client secret is created, copy its **Value**; we will need it later.

  

  

  

  

7. Go back to “App registrations”, then repeat steps 2-3 to create another Azure AD application for the configuration app.

  

  

  

1. **Name**: The name of your configuration app. We advise appending “Configuration” to the name of this app; for example, “Kronos Configuration”.

  

  

  

2. **Supported account types**: Select "Account in this organizational directory only"

  

  

  

3. Leave the "Redirect URI" field blank for now.

  

  

  

  

At this point you have 4 unique values:

  

  

  

* Application (client) ID for the bot

  

  

  

* Client secret for the bot

  

  

  

* Application (client) ID for the configuration app

  

  

  

* Directory (tenant) ID, which is the same for both apps

  

  

  

  

We recommend that you copy these values into a text file, using an application like Notepad. We will need these values later.

  

  

  

  

![azure-config-app](/wiki/images/azure-config-app.png)

  

  

  

  

# Step 2: Deploy to your Azure subscription

  

  

  

1. Click on the "Deploy to Azure" button below.
*TODO : this path is need to update


  

  

  

[![Deploy to Azure](https://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FOfficeDev%2Fmicrosoft-teams-faqplusplus-app%2Fmaster%2FDeployment%2Fazuredeploy.json)

  

  

  

  

2. When prompted, log in to your Azure subscription.

  

  

  

  

3. Azure will create a "Custom deployment" based on the ARM template and ask you to fill in the template parameters.

  

  

  

  

4. Select a subscription and resource group.

  

  

  

* We recommend creating a new resource group.

  

  

  

* The resource group location MUST be in a datacenter that supports: Application Insights; and LUIS. For an up-to-date list, click [here](https://azure.microsoft.com/en-us/global-infrastructure/services/?products=logic-apps,cognitive-services,search,monitor), and select a region where the following services are available:

  

  

  

* Application Insights

  

  

  

* LUIS services

  

  

  



  

  

  

  

5. Enter a "Base Resource Name", which the template uses to generate names for the other resources.

  

  

  

* The app service name `[Base Resource Name]`, `[Base Resource Name]-config`, and `[Base Resource Name]-luis` must be available. For example, if you select `kronos` as the base name, the names `kronos`, `kronos-config`, and `kronos-luis-qnamaker` must be available (not taken); otherwise, the deployment will fail with a Conflict error.

  

  

  

* Remember the base resource name that you selected. We will need it later.

  

  

  

  

6. Fill in the various IDs in the template:

  

  

  

1. **Bot Client ID**: The application (client) ID of the Microsoft Teams Bot app

  

  

  

2. **Bot Client Secret**: The client secret of the Microsoft Teams Bot app

  

  

  

3. **Config App Client Id**: The application (client) ID of the configuration app

  

  

  

4. **Tenant Id**: The tenant ID above

  

  

  

  

Make sure that the values are copied as-is, with no extra spaces. The template checks that GUIDs are exactly 36 characters.

  

  

  

  

7. Fill in the "Config Admin UPN List", which is a semicolon-delimited list of users who will be allowed to access the configuration app.

  

  

  

* For example, to allow Megan Bowen (meganb@contoso.com) and Adele Vance (adelev@contoso.com) to access the configuration app, set this parameter to `meganb@contoso.com;adelv@contoso.com`.

  

  

  

* You can change this list later by going to the configuration app service's "Configuration" blade.

  

  

  

  

8. If you wish to change the app name, description, and icon from the defaults, modify the corresponding template parameters.

  

  

  

  

8. Agree to the Azure terms and conditions by clicking on the check box "I agree to the terms and conditions stated above" located at the bottom of the page.

  

  

  

  

9. Click on "Purchase" to start the deployment.

  

  

  

  

10. Wait for the deployment to finish. You can check the progress of the deployment from the "Notifications" pane of the Azure Portal. It can take more than 10 minutes for the deployment to finish.

  

  

  

  

11. Once the deployment has finished, you would be directed to a page that has the following fields:

  

  

  

* botId - This is the Microsoft Application ID for the Kronos workforce central bot.

  

  

  

* appDomain - This is the base domain for the Kronos workforce central Bot.

  

  

  

* configurationAppUrl - This is the URL for the configuration web application.

  

  

  

  

# Step 3: Set up authentication for the configuration app

  

  

  

  

1. Note location for configurator app that you deployed, which is `https://[BaseResourceName]-config.azurewebsites.net`. For example, if you chose "kronos" as the base name, the configuration app will be at `https://kronos-config.azurewebsites.net`

  

  

  

  

2. Go back to the "App Registrations" page [here](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredAppsPreview).

  

  

  

  

3. Click on the configuration app in the application list. Under "Manage", click on "Authentication" to bring up authentication settings.

  

  

  

  

4. Add a new entry to "Redirect URIs":

  

  

  

* **Type**: Web

  

  

  

* **Redirect URI**: Location (URL) of your configuration app. This is the URL from Step 3.1.

  

  

  

  

5. Under "Implicit grant", check "ID tokens".

  

  

  

  

6. Click "Save" to commit your changes.

  

  

	![azure-config-app-step](/wiki/images/azure-config-app-step.png)

  

# Step 4 : Create the QnA Maker knowledge base

*TODO : we need to do it for LUIS instead of QnA Maker

  

  

  

  

Create a knowledge base, following the instructions in the [LUIS documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/).

  

  

  

Tip: If you don't have the complete knowledge base ready, here is a [Teams FAQ URL](https://support.office.com/en-us/article/faq-f4644010-d5fa-4055-b42a-6a5317316e18) that works well with QnA Maker.

  

  

  

  

Skip the first step, "Create a QnA service in Microsoft Azure", because the ARM template that you deployed in Step 2 already created the QnA service. Proceed directly to the next step, "Connect your QnA service to your KB".

  

  

  

  

Use the following values when connecting to the QnA service:

  

  

  

* **Microsoft Azure Directory ID**: The tenant associated with the Azure subscription selected in Step 2.1.

  

  

  

* **Azure subscription name**: The Azure subscription to which the ARM template was deployed.

  

  

  

* **Azure QnA service**: The QnA service created during the deployment. This is the same as the "Base resource name"; for example, if you chose "contosofaqplus" as the base name, the QnA Maker service will be named `contosofaqplus`.

  

  

  

  

![Screenshot of settings](https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/media/qnamaker-tutorial-create-publish-query-in-portal/create-kb-step-2.png)

  

  

  

  

After [publishing the knowledge base](https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/tutorials/create-publish-query-in-portal#publish-to-get-knowledge-base-endpoints), note the knowledge base ID (see screenshot).

  

  

  

  

![kb-publishing](/wiki/images/kb-publishing.png)

  

  

  

  

Remember the knowledge base ID: we will need it in the next step.

  

  

  

  

# Step 5: Finish configuring the Kronos app

  

  

  

  

1. Go to the configuration app, which is at `https://[BaseResourceName]-config.azurewebsites.net`. for example, if you choose “Kronos” as the base name, the configuration app will be at `https://Kronos-config.azurewebsites.net`.

  

  

  

  

2. You will be prompted to login with your credentials. Make sure that you log in with an account that is in the list of users allowed to access the configuration app.

  

  

  

	*TODO : Need Screenshot for configurator app![config-web-app-login](/wiki/images/config-web-app-login.png)

  

  

  

  
*I guess we do not want these 'teams' steps as we are heaving single tenant. 
3. Get the link to the team with your experts from the Teams client. To do so, open Microsoft Teams, and navigate to the team. Click on the "..." next to the team name, then select "Get link to team".

  

  

  

	![get-link-to-Team](/wiki/images/get-link-to-Team.png)

  

  

  

  

	Cick on "Copy" to copy the link to the clipboard.

  

  

  

	![link-to-team](/wiki/images/link-to-team.png)

  

  

  

  

4. Paste the copied link into the "Team Id" field, then press "OK".

  

  

  

	![fill-in-team-link](/wiki/images/fill-in-team-link.png)

  

  

  

  

5. Enter the QnA Maker knowledge base ID into the "Knowledge base ID" field, then press "OK".

  

  

  

  

6. Customize the "Welcome message" that's sent to your end-users when they install the app. This message supports basic markdown, such as bold, italics, bulleted lists, numbered lists, and hyperlinks. See [here](https://docs.microsoft.com/en-us/adaptive-cards/authoring-cards/text-features#markdown) for complete details on what Markdown features are supported.
7. Customize the "Help tab text", which is displayed in the app's "Help" tab. This message also supports basic markdown, such as bold, italics, bulleted lists, numbered lists, and hyperlinks. See [here](https://docs.microsoft.com/en-us/adaptive-cards/authoring-cards/text-features#markdown) for complete details on what Markdown features are supported.

  

  

  

  

### Notes

  

  

  

Remember to click on "OK" after changing a setting. To edit the setting later, click on "Edit" to make the text box editable.

  

  

  

  

# Step 6: Create the Teams app packages

  

  

  

  

Create two Teams app packages: one for end-users to install personally, and one to be installed to the experts team.

  

  

  

  

1. Open the `Manifest\manifest.json` file in a text editor.

  

  

  

  

2. Change the placeholder fields in the manifest to values appropriate for your organization.

  

  

  

* `developer.name` ([What's this?](https://docs.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema#developer))

  

  

  

* `developer.websiteUrl`

  

  

  

* `developer.privacyUrl`

  

  

  

* `developer.termsOfUseUrl`

  

  

  

  

3. Change the `<<botId>>` placeholder to your Azure AD application's ID from above. This is the same GUID that you entered in the template under "Bot Client ID".

  

  

  

  

4. In the "validDomains" section, replace the `<<appDomain>>` with your Bot App Service's domain. This will be `[BaseResourceName].azurewebsites.net`. For example if you chose "Kronos" as the base name, change the placeholder to `Kronos.azurewebsites.net`.

  

  

  

  

5. Copy the `manifest.json` file to a file named `manifest_user.json`.

  

  

  

  

6. Create a ZIP package with the `manifest_user.json`,`color.png`, and `outline.png`. The two image files are the icons for your app in Teams.

  

  

  

* Name this package `menifest-user.zip`, so you know that this is the app for end-users.

  

  

  

* Make sure that the 3 files are the _top level_ of the ZIP package, with no nested folders.

  

  

  

	![file-explorer](/wiki/images/file-explorer.png)

  

  

  

  

7. Delete the `manifest_user.json` file.

  

  

  

  

Repeat the steps above but with the file `Manifest\manifest_sme.json`. Name the resulting package `manifest.zip`, so you know that this is the app for experts.

  

  

  

  

# Step 7: Run the apps in Microsoft Teams

  

  

  

  

1. If your tenant has sideloading apps enabled, you can install your app by following the instructions [here](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/apps/apps-upload#load-your-package-into-teams)

  

  

  

  

2. You can also upload it to your tenant's app catalog, so that it can be available for everyone in your tenant to install. See [here](https://docs.microsoft.com/en-us/microsoftteams/tenant-apps-catalog-teams)

  

  

  

  

3. Install the experts app (the `manifest.zip` package) to your team of subject-matter experts. 

  

  

  

* We recommend using [app permission policies](https://docs.microsoft.com/en-us/microsoftteams/teams-app-permission-policies) to restrict access to this app to the members of the experts team.

  

  

  

  

4. Install the end-user app (the `manifest.zip` package) to your users.

  

  

  

# Troubleshooting

  

  

  

Please see our [Troubleshooting](/wiki/Troubleshooting.md) page.

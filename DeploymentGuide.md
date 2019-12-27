# Prerequisites
To begin, you will need:
* An Azure subscription where you can create the following kinds of resources
* App service
* App service plan
* Bot channels registration
* Azure storage account
* LUIS cognitive service
* Application Insights
* Azure KeyVault

* This is for one to one user (end-user) need to install personally, in order to run this app.
* A copy of the Kronos Workforce Central GitHub repo (https://github.com/OfficeDev/Kronos-Workforce-Central-Bot)

# Step 1: Register Azure AD applications
Register Azure AD applications in your tenant's directory.
1. Log in to the Azure Portal for your subscription, and go to the "App registrations" blade [here](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps).
2. Click on "New registration", and create an Azure AD application.
	1. **Name**: The name of your Teams app - if you are following the template for a default deployment, we recommend "Kronos workforce central".
	2. **Supported account types**: Select "Accounts in any organizational directory"
3. Leave the "Redirect URI" field blank.
	![singletenant-app-creation](https://github.com/OfficeDev/Kronos-Workforce-Central-Bot/wiki/Images/singletenant-app-creation.png)

 
4. Click on the "Register" button.
5. When the app is registered, you'll be taken to the app's "Overview" page. Copy the **Application (client) ID**; we will need it later. Verify that the "Supported account types" is set to **My organizations only**.
	![singletenant overview](https://github.com/OfficeDev/Kronos-Workforce-Central-Bot/wiki/Images/singletenant%20overview.png)

 6. On the side rail in the Manage section, navigate to the "Certificates & secrets" section. In the Client secrets section, click on "+ New client secret". Add a description for the secret and select an expiry time. Click "Add".
	![singletenant-app-secret](https://github.com/OfficeDev/Kronos-Workforce-Central-Bot/wiki/Images/singletenant-app-secret.png)
7. Once the client secret is created, copy its **Value**; we will need it later.
8. Leave the "Redirect URI" field blank for now.
At this point you have 3 unique values:
* Application (client) ID for the bot
* Client secret for the bot
* Directory (tenant) ID

We recommend that you copy these values into a text file, using an application like Notepad. We will need these values later during deployment.

![azure-config-app](https://github.com/OfficeDev/Kronos-Workforce-Central-Bot/wiki/Images/azure-config-app.png)

# Step 2: Deploy to your Azure subscription
1. Click on the "Deploy to Azure" button below.
[![Deploy to Azure](https://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FOfficeDev%2FKronos-Workforce-Central-Bot%2Fkronosext%2FDeployment%2Fazuredeploy.json)
2. When prompted, log in to your Azure subscription.
3. Azure will create a "Custom deployment" based on the ARM template and ask you to fill in the template parameters.

4. Select a subscription and resource group.

* We recommend creating a new resource group.
* The resource group location MUST be in a datacenter that supports: Application Insights; and LUIS. For an up-to-date list, click [here](https://azure.microsoft.com/en-us/global-infrastructure/services/?products=logic-apps,cognitive-services,search,monitor), and select a region where the following services are available:
* Application Insights

* LUIS services
5. Enter a "Base Resource Name", which the template uses to generate names for the other resources.
* The app service name `[Base Resource Name]`, `[Base Resource Name]-config`, and `[Base Resource Name]-luis` will be available at the end of deployment. For example, if you select `kronos` as the base name, the names `kronos`, `kronos-config`, and `kronos-luis` will be available.

* Remember the base resource name that you selected. We will need it later.
6. Fill in the various IDs in the template:
	1. **Bot Client ID**: The application (client) ID of the Microsoft Teams Bot app
	2. **Bot Client Secret**: The client secret of the Microsoft Teams Bot app.
	3. **Tenant Id**: The tenant ID saved in previous steps.

Make sure that the values are copied as-is, with no extra spaces. The template checks that GUIDs are exactly 36 characters.

7. Provide the below values
	1. **LUISRegion**: The region where the LUIS resource will be deployed.
	2. **appDisplayName**: App display name.
	3. **appDescription**: App description.

8. Fill in the "Config Admin UPN List", which is a semicolon-delimited list of users who will be allowed to access the configuration app.
* For example, to allow Megan Bowen (meganb@contoso.com) and Adele Vance (adelev@contoso.com) to access the configuration app, set this parameter to `meganb@contoso.com;adelv@contoso.com`.
* You can change this list later by going to the configuration app service's "Configuration" blade.
9. If you wish to change the app name, description, and icon from the defaults, modify the corresponding template parameters.
10. Provide the  **KronosWfcSuperUserName** and **KronosWfcSuperUserPwd**. These values will be saved in key vault and later can be changed using Configuration App.
11. Agree to the Azure terms and conditions by clicking on the check box "I agree to the terms and conditions stated above" located at the bottom of the page.
12. Click on "Purchase" to start the deployment.
13. Wait for the deployment to finish. You can check the progress of the deployment from the "Notifications" pane of the Azure Portal. It can take more than 25 minutes for the deployment to finish.

14. Once the deployment has finished, you would be directed to a page that has the following fields:
* botId - This is the Microsoft Application ID for the Kronos workforce central bot.
* appDomain - This is the base domain for the Kronos workforce central Bot.
* configurationAppUrl - This is the URL for the configuration web application.

# Step 3: Configure Key Vault access policies

 1. Go to App Service. The name will be same as the bot resource name provided in "Deploy to Azure subscription step". , for example you choose "kronosbot"
 2. Go to settings then click on Identity 
 3.  click on 'On' to the status and wait till object ID will not get generated, as soon as the object ID get generated copy that ID.
 4. Go to KeyVault, for example you choose "kronosbotvault".
 5. Go to settings then click on Access policies and then add policy there.
 6. In add policy, select 'secret management' from the configuration from template.
 7. click on select principal paste the object ID which you have copied from App Service and select it.
 8. Last step is just to click on Add.
 9. Repeat the steps from 1 to 8 for Configuration App service (kronosbot-config).
 
# Step 4:Set up authentication for the configuration app

1. Note location for configurator app that you deployed, which is `https://[BaseResourceName]-config.azurewebsites.net`. For example, if you chose "kronos" as the base name, the configuration app will be at `https://kronos-config.azurewebsites.net`
2. Go back to the "App Registrations" page [here](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredAppsPreview).
3. Click on the configuration app in the application list. Under "Manage", click on "Authentication" to bring up authentication settings.

4. Add a new entry to "Redirect URIs":
* **Type**: Web
* **Redirect URI**: Location (URL) of your configuration app.
5. Under "Implicit grant", check "ID tokens".
6. Click "Save" to commit your changes.
![azure-config-app-step](https://github.com/OfficeDev/Kronos-Workforce-Central-Bot/wiki/Images/azure-config-app-step.png)
# Step 5 : Configure LUIS app
1. If you want to create a luis public app with your microsoft ID, following the instructions in the [Sign in to LUIS portal](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-how-to-start-new-app#sign-in-to-luis-portal) (https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-how-to-start-new-app#create-new-app-in-luis).
2. Import json from [https://github.com/OfficeDev/Kronos-Workforce-Central-Bot/blob/kronosext/LuisTraining.json](https://github.com/OfficeDev/Kronos-Workforce-Central-Bot/blob/kronosext/LuisTraining.json)

3. Update the LUIS Application ID in ARM template under variables before starting the deployment.

# Step 6: Finish configuring the Kronos app
1. Go to the configuration app, which is at `https://[BaseResourceName]-config.azurewebsites.net`. For example, if you choose “Kronos” as the base name, the configuration app will be at `https://Kronos-config.azurewebsites.net`.
2. You will be prompted to login with your credentials. Make sure that you log in with **KronosWfcSuperUserName** and **KronosWfcSuperUserPwd** entered during the deployment.

3. Fill the Tenant configuration : Tenant Id (you will get it from azure portal), Kronos endpoint URL(provided by kronos).
4. Provide the Superuser name and password.
5. Provide the Paycode configuration, Select paycode type from dropdown and Paycode name then add them.
6.  click on submit to proceed.
# Step 7: Create the Teams app packages
Create Teams app package for end-users to install personally.
1. Open the `Manifest\manifest.json` file in a text editor available at [https://github.com/OfficeDev/Kronos-Workforce-Central-Bot/blob/kronosext/Manifest/manifest.json](https://github.com/OfficeDev/Kronos-Workforce-Central-Bot/blob/kronosext/Manifest/manifest.json)

2. Change the placeholder fields in the manifest to values appropriate for your organization.
	* `developer.name` ([What's this?](https://docs.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema#developer))
	
	* `developer.websiteUrl`

	* `developer.privacyUrl`

	* `developer.termsOfUseUrl`
3. Get the bot service, name will be same as the bot resource name provided during deployment. Under Settings menu option shown in left rail, copy the id under Microsoft App ID (Manage). Change the `<<botId>>` placeholder with the copied id.
4. In the "validDomains" section, replace the `<<appDomain>>` with your Bot App Service's domain. This will be `[BaseResourceName].azurewebsites.net`. For example if you chose "Kronos" as the base name, change the placeholder to `Kronos.azurewebsites.net`.
5. Create a ZIP package with the `manifest.json`,`color.png`, and `outline.png`. The two image files are the icons for your app in Teams.
	* Name this package `manifest.zip`, so you know that this is the app for end-users.
	* Make sure that the 3 files are the _top level_ of the ZIP package, with no nested folders.
![file-explorer](https://github.com/OfficeDev/Kronos-Workforce-Central-Bot/wiki/Images/file%20explorer.png)

# Step 8: Run the apps in Microsoft Teams
1. If your tenant has side-loading apps enabled, you can install your app by following the instructions [here](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/apps/apps-upload#load-your-package-into-teams)
2. You can also upload it to your tenant's app catalog, so that it can be available for everyone in your tenant to install. See [here](https://docs.microsoft.com/en-us/microsoftteams/tenant-apps-catalog-teams)
3. Install the bot (the `manifest.zip` package) to your users.

# Troubleshooting
Please see our [Troubleshooting](/wiki/Troubleshooting.md) page.

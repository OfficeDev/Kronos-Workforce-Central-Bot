//-----------------------------------------------------------------------
// <copyright file="AppSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Common
{
    using System;
    using System.Configuration;
    using Microsoft.Teams.App.KronosWfc.Common.Core;

    /// <summary>
    /// AppSettings class.
    /// </summary>
    [Serializable]
    public sealed class AppSettings : IAppSettings
    {
        private readonly IKeyVaultHelper keyVaultHelper;

        /// <summary>
        /// App Settings instance.
        /// </summary>
        [NonSerialized]
        private static readonly Lazy<AppSettings> instance = new Lazy<AppSettings>(() => new AppSettings());

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettings" /> class.
        /// </summary>
        public AppSettings()
            : this(new KeyVaultHelper())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettings" /> class.
        /// </summary>
        /// <param name="keyVaultHelper">KeyVaultHelper class object</param>
        public AppSettings(IKeyVaultHelper keyVaultHelper)
        {
            this.keyVaultHelper = keyVaultHelper;
        }

        /// <summary>
        /// Gets AppSettings Instance.
        /// </summary>
        public static AppSettings Instance
        {
            get
            {
                return instance.Value;
            }
        }

        /// <summary>
        /// Gets Azure table storage connectionstring
        /// </summary>
        public string AzureTableStorageConnectionString
        {
            get => this.keyVaultHelper.GetSecretByUri(ConfigurationManager.AppSettings["AzureTableStorageConnectionString"]);

        }

        public string TableName { get => ConfigurationManager.AppSettings["TenantMappingTable"]; }

        public string AppInsightKey { get => ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"]; }

        public string LogInsightsFlag { get => ConfigurationManager.AppSettings["LogInsightsFlag"]; }

        public string PayPeriodMonthly { get => ConfigurationManager.AppSettings["PayPeriodMonthly"]; }

        public string MicrosoftAppId { get => this.keyVaultHelper.GetSecretByUri(ConfigurationManager.AppSettings["MicrosoftAppIdUrl"]); }

        public string MicrosoftAppPassword { get => this.keyVaultHelper.GetSecretByUri(ConfigurationManager.AppSettings["MicrosoftAppPwdUrl"]); }

        public string EncryptionKey
        {
            get => this.keyVaultHelper.GetSecretByUri(ConfigurationManager.AppSettings["EncryptionKey"]);

        }

        public string EncryptionIV
        {
            get => this.keyVaultHelper.GetSecretByUri(ConfigurationManager.AppSettings["EncryptionIV"]);
        }

        public string OvertimeMappingtableName { get => ConfigurationManager.AppSettings["PaycodeMappingTable"]; }

        public string KronosUserTableName { get => ConfigurationManager.AppSettings["KronosUserDetails"]; }

        public string LuisModelId { get => ConfigurationManager.AppSettings["LuisModelId"]; }

        public string LuisSubscriptionKey { get => ConfigurationManager.AppSettings["LuisSubscriptionKey"]; }
    }
}

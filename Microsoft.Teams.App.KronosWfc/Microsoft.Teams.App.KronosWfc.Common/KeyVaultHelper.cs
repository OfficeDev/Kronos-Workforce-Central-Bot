//-----------------------------------------------------------------------
// <copyright file="KeyVaultHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Common
{
    using System;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;

    /// <summary>
    /// KeyVaultHelper class to read key values
    /// </summary>
    [Serializable]
    public class KeyVaultHelper: IKeyVaultHelper
    {
        /// <summary>
        /// Get secret from azure key vault by URI
        /// </summary>
        /// <param name="resourceUri">URI of secret to be fetched</param>
        /// <returns>returns the secret string value</returns>
        public string GetSecretByUri(string resourceUri)
        {
            string secret = string.Empty;
            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyvaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                secret = keyvaultClient.GetSecretAsync(resourceUri).Result.Value;
            }
            catch (Exception ex)
            {
                // keyvault is used to get Azure storage explorer connection string for logger, this will cause deadlock
                AppInsightsLogger.Error(ex);
            }

            return secret;
        }

    }
}

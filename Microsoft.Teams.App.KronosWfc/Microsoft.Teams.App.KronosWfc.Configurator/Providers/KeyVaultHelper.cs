using Microsoft.Azure.Services.AppAuthentication;
using System;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Teams.App.KronosWfc.Configurator.Providers
{
    [Serializable]
    public class KeyVaultHelper : IKeyVaultHelper
    {
        private readonly IConfiguration configuration;
        public KeyVaultHelper(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Get secret from azure key vault by URI
        /// </summary>
        /// <param name="resourceUri">URI of secret to be fetched</param>
        /// <returns>returns the secret string value</returns>
        public string GetSecret(string secretKey)
        {
            string secret = string.Empty;
            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyvaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                secret = keyvaultClient.GetSecretAsync(this.configuration["KeyVaultBaseURL"], secretKey).Result.Value;
            }
            catch (Exception ex)
            {
            }

            return secret;
        }

        /// <summary>
        /// Get secret from azure key vault by URI
        /// </summary>
        /// <param name="resourceUri">URI of secret to be fetched</param>
        /// <returns>returns the secret string value</returns>
        public async System.Threading.Tasks.Task<string> SetSecretAsync(string secretKey, string secretValue)
        {
            string secret = string.Empty;
            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyvaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                await keyvaultClient.SetSecretAsync(this.configuration["KeyVaultBaseURL"], secretKey, secretValue);
            }
            catch (Exception ex)
            {
            }

            return secret;
        }

    }
}

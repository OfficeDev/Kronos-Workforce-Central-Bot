using System.Threading.Tasks;

namespace Microsoft.Teams.App.KronosWfc.Configurator.Providers
{
    public interface IKeyVaultHelper
    {
        string GetSecret(string secretKey);
        Task<string> SetSecretAsync(string secretKey, string secretValue);
    }
}
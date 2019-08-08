namespace Microsoft.Teams.App.KronosWfc.Provider.Core
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Logon;

    public interface IAuthenticationService
    {
        Task SendAuthCardAsync(IDialogContext context, Activity activity);

        Task<Response> LoginUser(Activity activity);

        Task<Response> InstallBot(Activity activity);

        Task<Response> LoginSuperUser(Activity activity);
    }
}

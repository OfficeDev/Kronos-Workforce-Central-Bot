using Microsoft.Teams.App.KronosWfc.Configurator.Helpers;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Microsoft.Teams.App.KronosWfc.Configurator.Providers
{
    public interface IIdentityService
    {
        bool IsAuthenticated();
        string GetMail();
        string GetId();
    }

    public class AzureAdIdentityService : IIdentityService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public AzureAdIdentityService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        // Indicates if the user is authenticated
        public bool IsAuthenticated()
        {
            return this.httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
        }

        // Returns the principal user login (ie. principal account mail)
        public string GetMail()
        {
            return this.httpContextAccessor.HttpContext.User.Identity.Name;
        }

        // Returns the id of the user in Azure AD (GUID format)
        public string GetId()
        {
            var idClaims = this.httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == AzureAdClaimTypes.ObjectId);

            return idClaims?.Value;
        }
    }
}

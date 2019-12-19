using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Teams.App.KronosWfc.Configurator.Models;
using Microsoft.Teams.App.KronosWfc.Configurator.Providers;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Teams.App.KronosWfc.Configurator.Controllers
{
    public class HomeController : Controller
    {
        private readonly IIdentityService identityService;
        private readonly IConfiguration configuration;

        public HomeController(IIdentityService identityService, IConfiguration configuration)
        {
            this.identityService = identityService;
            this.configuration = configuration;
        }
        public IActionResult Index()
        {
            TempData["ClientId"] = this.configuration["ClientId"];
            TempData["TenantId"] = this.configuration["TenantId"];
            TempData["TokenEnpoint"] = this.configuration["TokenEnpoint"];

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

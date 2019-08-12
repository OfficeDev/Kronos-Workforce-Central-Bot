namespace Microsoft.Teams.App.KronosWfc
{
    using System;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Microsoft.Teams.App.KronosWfc.App_Start;

    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            AutofacRegistrationsConfig.ConfigureAutofacRegistrations();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        }
    }
}
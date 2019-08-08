
//-----------------------------------------------------------------------
// <copyright file="RouteConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc
{
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    ///  route config class
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// register routes
        /// </summary>
        /// <param name="routes">route collection</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

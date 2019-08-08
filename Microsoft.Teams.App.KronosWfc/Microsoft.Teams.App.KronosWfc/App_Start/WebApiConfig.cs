
//-----------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc
{
    using System.Web.Http;
    using Microsoft.Teams.App.KronosWfc.Filters;

    /// <summary>
    /// web api configuration class
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// web api configuration
        /// </summary>
        /// <param name="config">config object</param>
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Filters.Add(new CustomExceptionFilter());
        }
    }
}

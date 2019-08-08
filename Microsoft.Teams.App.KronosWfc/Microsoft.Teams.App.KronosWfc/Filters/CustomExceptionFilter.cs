//-----------------------------------------------------------------------
// <copyright file="CustomExceptionFilter.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Filters
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Filters;
    using Microsoft.Teams.App.KronosWfc.Common;

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class CustomExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext?.Exception != null)
            {
                if (AppSettings.Instance.LogInsightsFlag == "1")
                {
                    AppInsightsLogger.Error(actionExecutedContext.Exception);
                }

                var errorMessagError = new System.Web.Http.HttpError(actionExecutedContext.Exception.Message);
                actionExecutedContext.Response =
                   actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessagError);
            }
            else
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("An unhandled exception was thrown by service"),
                    ReasonPhrase = "Internal Server Error.Please Contact your Administrator.",
                };
                actionExecutedContext.Response = response;
            }
        }
    }
}
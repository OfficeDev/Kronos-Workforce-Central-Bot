//-----------------------------------------------------------------------
// <copyright file="LoginController.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Controllers.Login
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Core;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.ErrorHandler;
    using Microsoft.Teams.App.KronosWfc.Models;

    /// <summary>
    /// Login controller class.
    /// </summary>
    [AiHandleError]
    public class LoginController : Controller
    {
        private readonly ILogonActivity logonActivity;
        private LoginResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginController"/> class.
        /// </summary>
        /// <param name="logonActivity">logon activity.</param>
        /// <param name="loginResponse">logon response.</param>
        public LoginController(ILogonActivity logonActivity, LoginResponse loginResponse)
        {
            this.logonActivity = logonActivity;
            this.response = loginResponse;
        }

        /// <summary>
        /// login index action method.
        /// </summary>
        /// <param name="tid">tenant id.</param>
        /// <returns>login view.</returns>
        [HttpGet]
        public ActionResult Index(string tid)
        {
            this.ViewBag.Tid = tid;
            return this.View();
        }

        /// <summary>
        /// submit login action method.
        /// </summary>
        /// <param name="user">user object.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginAction(User user)
        {
            try
            {
                if (this.ModelState.IsValid)
                {
                    var result = await this.logonActivity.Logon(user);
                    if (result.Status == ApiConstants.Success)
                    {
                        ////set the key and value
                        this.response.JsessionID = result.Jsession;
                        this.response.PersonNumber = result.PersonNumber;
                        this.response.Name = result.Username;

                        return this.View("Close", this.response);
                    }
                    else
                    {
                        // handle error response
                        this.ModelState.AddModelError("Invalid", Constants.InvalidCred);
                        this.ViewBag.Tid = user.TenantId;
                        return this.View("Index");
                    }
                }

                return this.View("Index");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// get schedule url based on tenant.
        /// </summary>
        /// <param name="tenant">tenant id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public async Task<JsonResult> ScheduleTab(string tenant)
        {
            string result = string.Empty;
            try
            {
                string url = await this.logonActivity.GetScheduleUrl(tenant);
                result = url;
            }
            catch (Exception)
            {
                this.Json(new { message = "Something went wrong!" });
            }

            return this.Json(new { success = result }, JsonRequestBehavior.AllowGet);
        }
    }
}
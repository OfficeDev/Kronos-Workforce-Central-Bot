//-----------------------------------------------------------------------
// <copyright file="AuthenticateUser.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

////  using Microsoft.Teams.App.KronosWfc.Models;
////  using System;
////  using System.Collections.Generic;
////  using System.Linq;
////  using System.Text;
////  using System.Threading.Tasks;

////  namespace Microsoft.Teams.App.KronosWfc.Common
////  {
////    public class AuthenticateUser
////    {
////        private async Task<LoginResponse> GetUserInfo(IDialogContext context, Activity activity)
////        {
////            context.UserData.TryGetValue(activity.From.Id, out _response);

////            if (string.IsNullOrEmpty(_response?.JsessionID) && activity.Name != Constants.VerifyState)
////            {
////                //User is not logged in - Send Sign in card
////                await authenticationService.SendAuthCardAsync(context, activity);
////            }
////            else if (activity.Name == Constants.VerifyState)
////            {
////                var result = await authenticationService.LoginUser(activity);
////                if (result.Status == ApiConstants.Success)
////                {
////                    await context.PostAsync("Sign in success!");
////                    //set the key and value
////                    _response.JsessionID = result.Jsession;
////                    _response.PersonNumber = result.PersonNumber;

////                    context.UserData.SetValue(activity.From.Id, _response);
////                }
////                else
////                {
////                    //handle error response
////                }
////            }

////            return _response;
////        }
////    }
////  }

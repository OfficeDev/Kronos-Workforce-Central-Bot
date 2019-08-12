//-----------------------------------------------------------------------
// <copyright file="SupervisorViewTimeOffDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.Dialogs.SupervisorViewTimeoff
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams.Models;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.CommentList;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Common;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.HyperFind;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Role;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.SupervisorViewTimeOff;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.SupervisorViewTimeOffRequests;
    using Microsoft.Teams.App.KronosWfc.CommandHandling;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;
    using Microsoft.Teams.App.KronosWfc.Resources;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Task = System.Threading.Tasks.Task;
    using TimeOffResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOffRequests;

    /// <summary>
    /// SupervisorViewTimeOffDialog class.
    /// </summary>
    [Serializable]
    public class SupervisorViewTimeOffDialog : IDialog<object>
    {
        private readonly TimeOffRequestCard timeOffCard;
        private readonly IAuthenticationService authenticationService;
        private readonly ISupervisorViewTimeOffActivity timeOffActivity;
        private readonly ITimeOffActivity timeOffRequestActivity;
        private readonly ICommentsActivity commentsActivity;
        private readonly IHyperFindActivity hyperFindActivity;
        private readonly ICommonActivity commonActivity;
        private readonly IRoleActivity roleActivity;
        private readonly LoginResponse response;
        private readonly AuthenticateUser authUser;
        private readonly SupervisorViewTimeOffRequestsCard supervisorTimeOffcard;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupervisorViewTimeOffDialog"/> class.
        /// </summary>
        /// <param name="roleActivity">Role activity.</param>
        /// <param name="commentsActivity">Comments activity.</param>
        /// <param name="commonActivity">Common activity.</param>
        /// <param name="authUser">AuthenticateUser class object.</param>
        /// <param name="response">Login response.</param>
        /// <param name="authenticationService">AuthenticateService.</param>
        /// <param name="timeOffRequestActivity">TimeOffRequestActivity.</param>
        /// <param name="timeOffActivity">TimeOffActivity.</param>
        /// <param name="timeOffCard">Time off card.</param>
        /// <param name="hyperFindActivity">HyperFindActivity.</param>
        /// <param name="card">SupervisorViewTimeOffRequestsCard class object.</param>
        public SupervisorViewTimeOffDialog(IRoleActivity roleActivity, ICommentsActivity commentsActivity, ICommonActivity commonActivity, AuthenticateUser authUser, LoginResponse response, IAuthenticationService authenticationService, ITimeOffActivity timeOffRequestActivity, ISupervisorViewTimeOffActivity timeOffActivity, TimeOffRequestCard timeOffCard, IHyperFindActivity hyperFindActivity, SupervisorViewTimeOffRequestsCard card)
        {
            this.timeOffCard = timeOffCard;
            this.response = response;
            this.authenticationService = authenticationService;
            this.timeOffActivity = timeOffActivity;
            this.hyperFindActivity = hyperFindActivity;
            this.supervisorTimeOffcard = card;
            this.commentsActivity = commentsActivity;
            this.timeOffRequestActivity = timeOffRequestActivity;
            this.authUser = authUser;
            this.commonActivity = commonActivity;
            this.roleActivity = roleActivity;
        }

        /// <inheritdoc/>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(this.ProcessRequest);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Send notification to employee once supervisor approves/refuses time off request.
        /// </summary>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="context">Dialog context.</param>
        /// <param name="status">Approval status.</param>
        /// <param name="conversationId">Conversation Id of requestor.</param>
        /// <param name="note">Note entered while approval.</param>
        /// <param name="queryDateSpan">Query date span for request.</param>
        /// <param name="reqId">Request Id of request.</param>
        /// <returns>Task.</returns>
        public async Task SendApprovalResponseNotiToEmployee(string tenantId, IDialogContext context, string status, string conversationId, string note, string queryDateSpan, string reqId)
        {
            var superUserLogonRes = await this.authenticationService.LoginSuperUser((Activity)context.Activity);
            if (superUserLogonRes?.Status == ApiConstants.Success)
            {
                context.UserData.SetValue(context.Activity.From.Id + Constants.SuperUser, superUserLogonRes.Jsession);
            }

            try
            {
                context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);
                var activity = context.Activity as Activity;
                JToken token = JObject.Parse(activity.Value.ToString());
                string personNumber = (string)token.SelectToken("data").SelectToken("PersonNumber");
                string empName = (string)token.SelectToken("data").SelectToken("EmpName");
                string jSessionId = this.response?.JsessionID;

                var info = await this.commonActivity.GetJobAssignment(personNumber, tenantId, superSession);
                var reviewer = info?.JobAssign?.jobAssignDetData?.JobAssignDet?.SupervisorName;
                var channelData = new TeamsChannelData
                {
                    Tenant = new TenantInfo()
                    {
                        Id = tenantId,
                    },
                };

                var message = Activity.CreateMessageActivity();
                message.From = new ChannelAccount(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword);

                MicrosoftAppCredentials.TrustServiceUrl(context.Activity.ServiceUrl);
                message.ChannelData = channelData;
                message.ChannelId = Constants.ActivityChannelId;
                message.Conversation = new ConversationAccount(
                    conversationType: "personal",
                    id: conversationId,
                    isGroup: false);

                var requests = await this.timeOffRequestActivity.GetTimeOffRequestDetails(tenantId, superSession, queryDateSpan, personNumber);
                var filteredRequest = (from d in requests.RequestMgmt.RequestItems.GlobalTimeOffRequestItem where d.Id == reqId select d).FirstOrDefault();

                for (int i = 0; i < filteredRequest.RequestStatusChanges.RequestStatusChange.Count; i++)
                {
                    var personInfo = await this.roleActivity.GetPersonInfo(tenantId, filteredRequest.RequestStatusChanges.RequestStatusChange[i].User.PersonIdentity.PersonNumber, superSession);

                    filteredRequest.RequestStatusChanges.RequestStatusChange[i].User.PersonIdentity.PersonNumber = personInfo.PersonInformation.PersonDat.PersonDetail.FullName;
                }

                AdaptiveCard card = this.supervisorTimeOffcard.GetEmployeeNotificationCard(context, status, reviewer, note, filteredRequest);

                message.Attachments.Add(new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = card,
                });

                IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                await factory.MakeConnectorClient().Conversations.SendToConversationAsync((Activity)message);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// get partial list.
        /// </summary>
        /// <typeparam name="T">list object type.</typeparam>
        /// <param name="list">list object.</param>
        /// <param name="page">page number.</param>
        /// <param name="pageSize">page size.</param>
        /// <returns>paginated list.</returns>
        public List<TimeOffResponse.GlobalTimeOffRequestItem> GetPage(List<TimeOffResponse.GlobalTimeOffRequestItem> list, int page, int pageSize)
        {
            return list.Skip(page * pageSize).Take(pageSize).ToList();
        }

        /// <summary>
        /// process the command and call respective method.
        /// </summary>
        /// <param name="context">dialog context.</param>
        /// <param name="result">awaitable string.</param>
        /// <returns>time off request.</returns>
        private async Task ProcessRequest(IDialogContext context, IAwaitable<string> result)
        {
            var activity = context.Activity as Activity;

            string isManager = await this.authUser.IsUserManager(context);
            if (isManager.ToLowerInvariant().Equals(Constants.Yes.ToLowerInvariant()))
            {
                var message = activity.Text?.ToLowerInvariant().Trim();
                JObject tenant = context.Activity.ChannelData as JObject;
                string tenantId = tenant?["tenant"].SelectToken("id").ToString();
                AppInsightsLogger.CustomEventTrace("SupervisorViewTimeOffDialog", new Dictionary<string, string>() { { "TenantId", tenantId }, { "User", context.Activity.From.Id }, { "methodName", "ProcessRequest" }, { "Command", message } });

                if (message.ToLowerInvariant() == Constants.SupervisorTORList.ToLowerInvariant())
                {
                    var managerCheck = await this.authUser.IsUserManager(context);
                    if (!string.IsNullOrEmpty(managerCheck))
                    {
                        if (managerCheck.ToLowerInvariant().Equals(Constants.Yes.ToLowerInvariant()))
                        {
                            context.PrivateConversationData.RemoveValue("FiltersHashTable");
                            await this.ShowTimeOffRequests(context);
                        }
                    }
                }
                else if (message.ToLowerInvariant() == Constants.TORRemoveEmpName.ToLowerInvariant())
                {
                    await this.ApplyFilter(context, message);
                }
                else if (message.ToLowerInvariant() == Constants.TORRemoveDateRange.ToLowerInvariant())
                {
                    await this.ApplyFilter(context, message);
                }
                else if (message.ToLowerInvariant() == Constants.TORRemoveShowCompletedRequests.ToLowerInvariant())
                {
                    await this.ApplyFilter(context, message);
                }
                else if (message.ToLowerInvariant() == Constants.TORListNext.ToLowerInvariant())
                {
                    await this.Next(context);
                }
                else if (message.ToLowerInvariant() == Constants.TORListPrevious.ToLowerInvariant())
                {
                    await this.Previous(context);
                }
                else if (message.ToLowerInvariant() == Constants.TORListRefresh.ToLowerInvariant())
                {
                    await this.ApplyFilter(context, message);
                }
                else if (message.ToLowerInvariant() == Constants.TORListApplyFilter.ToLowerInvariant())
                {
                    context.PrivateConversationData.RemoveValue("FiltersHashTable");
                    await this.ApplyFilter(context, message);
                }
                else if (message.ToLowerInvariant() == Constants.TORApproveRequest.ToLowerInvariant() || message.ToLowerInvariant() == Constants.TORRefuseRequest.ToLowerInvariant())
                {
                    await this.SubmitTimeOffApproval(context, message);
                }
                else
                {
                    context.PrivateConversationData.RemoveValue("FiltersHashTable");
                    await this.ShowTimeOffRequests(context);
                }
            }

            context.Done(default(string));
        }

        private async Task ShowTimeOffRequests(IDialogContext context)
        {
            var superUserLogonRes = await this.authenticationService.LoginSuperUser((Activity)context.Activity);
            if (superUserLogonRes?.Status == ApiConstants.Success)
            {
                context.UserData.SetValue(context.Activity.From.Id + Constants.SuperUser, superUserLogonRes.Jsession);
            }

            try
            {
                var activity = context.Activity as Activity;
                JObject tenant = context.Activity.ChannelData as JObject;
                string tenantId = tenant["tenant"].SelectToken("id").ToString();
                string jSessionId = this.response?.JsessionID;
                string personNumber = this.response?.PersonNumber;
                context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);
                var employeesResult = await this.hyperFindActivity.GetHyperFindQueryValues(tenantId, jSessionId, DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), "Report", "Private");
                if (employeesResult.Status == ApiConstants.Failure && employeesResult.Error.ErrorCode == ApiConstants.UserNotLoggedInError)
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }

                if (employeesResult.Status == ApiConstants.Success)
                {
                    Hashtable employeesHashTable = new Hashtable();
                    Hashtable filtersHashTable = new Hashtable();
                    Hashtable employeesRoleHashTable = new Hashtable();
                    foreach (var item in employeesResult.HyperFindResult)
                    {
                        employeesHashTable.Add(item.PersonNumber, item.FullName);
                    }

                    if (employeesHashTable.Count > 0)
                    {
                        context.PrivateConversationData.SetValue("EmployeeHashTable", employeesHashTable);

                        var response = await this.timeOffActivity.GetTimeOffRequest(tenantId, jSessionId, DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), employeesResult.HyperFindResult);
                        var requests = response.RequestMgmt?.RequestItems?.GlobalTimeOffRequestItem;

                        var personNumbers = (from d in requests select d.Employee.PersonIdentity.PersonNumber).Distinct().ToList();
                        var tasks = personNumbers.Select(async emp =>
                        {
                            if (!employeesRoleHashTable.ContainsKey(Convert.ToString(emp)))
                            {
                                var jobAssignent = await this.commonActivity.GetJobAssignment(Convert.ToString(emp), tenantId, superSession);
                                var role = jobAssignent?.JobAssign?.PrimaryLaborAccList?.PrimaryLaborAcc?.OrganizationPath?.Split('/').LastOrDefault();
                                employeesRoleHashTable.Add(Convert.ToString(emp), role);
                            }
                        });
                        await Task.WhenAll(tasks);
                        context.PrivateConversationData.SetValue("EmployeeRoleHashTable", employeesRoleHashTable);

                        requests = (from d in requests where d.StatusName.ToLower().Equals(Constants.Submitted.ToLower()) select d).ToList();
                        for (int i = 0; i < requests.Count; i++)
                        {
                            DateTime.TryParse(requests[i].TimeOffPeriods.TimeOffPeriod.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sdt);
                            requests[i].TimeOffPeriods.TimeOffPeriod.Sdt = sdt;
                        }

                        requests = requests.OrderBy(w => w.TimeOffPeriods.TimeOffPeriod.Sdt).ToList();
                        int totalPages = (requests?.Count > 0) ? (int)Math.Ceiling((double)requests.Count / 5) : 0;
                        var currentPageItems = this.GetPage(requests, 0, 5);

                        Hashtable pagewiseRequests = new Hashtable();
                        for (int i = 0; i < totalPages; i++)
                        {
                            pagewiseRequests.Add(i.ToString(), JsonConvert.SerializeObject(this.GetPage(requests, i, 5)).ToString());
                        }

                        context.PrivateConversationData.SetValue("PagewiseRequests", pagewiseRequests);
                        ViewTorListObj obj = new ViewTorListObj
                        {
                            Employees = employeesHashTable,
                            EmployeesRoles = employeesRoleHashTable,
                            Filters = filtersHashTable,
                            TotalPages = totalPages,
                            CurrentPageCount = 1,
                        };

                        // string actionSet = "FR";
                        string actionSet = "F";
                        if (obj.CurrentPageCount < obj.TotalPages)
                        {
                            // actionSet = "FRN";
                            actionSet = "FN";
                        }

                        obj.ConversationId = context.Activity.Conversation.Id;
                        obj.ActivityId = context.Activity.ReplyToId;

                        var comments = await this.commentsActivity.GetComments(tenantId, superSession);
                        context.PrivateConversationData.SetValue("Comments", comments?.Comments);
                        AdaptiveCard card = new AdaptiveCard("1.0");
                        if (requests.Count > 0)
                        {
                            var page = JsonConvert.DeserializeObject<List<TimeOffResponse.GlobalTimeOffRequestItem>>(Convert.ToString(pagewiseRequests[Convert.ToString(obj.CurrentPageCount - 1)]));
                            card = this.supervisorTimeOffcard.GetCard(page, obj, actionSet, comments?.Comments);
                        }
                        else
                        {
                            card = this.supervisorTimeOffcard.GetCard(new List<TimeOffResponse.GlobalTimeOffRequestItem>(), obj, actionSet, comments?.Comments);
                        }

                        var reply = activity.CreateReply();
                        reply.Attachments = new List<Attachment>
                        {
                            new Attachment()
                            {
                                Content = card,
                                ContentType = "application/vnd.microsoft.card.adaptive",
                            },
                        };
                        await context.PostAsync(reply);
                    }
                    else
                    {
                        await context.PostAsync(KronosResourceText.NoEmployees);
                    }
                }
                else
                {
                    await context.PostAsync(KronosResourceText.NoEmployees);
                }

                context.Done(default(string));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task ApplyFilter(IDialogContext context, string command)
        {
            var superUserLogonRes = await this.authenticationService.LoginSuperUser((Activity)context.Activity);
            if (superUserLogonRes?.Status == ApiConstants.Success)
            {
                context.UserData.SetValue(context.Activity.From.Id + Constants.SuperUser, superUserLogonRes.Jsession);
            }

            try
            {
                context.UserData.TryGetValue(context.Activity.From.Id + Constants.SuperUser, out string superSession);
                var activity = context.Activity as Activity;
                JObject tenant = context.Activity.ChannelData as JObject;
                string tenantId = tenant["tenant"].SelectToken("id").ToString();
                string jSessionId = this.response?.JsessionID;
                string personNumber = this.response?.PersonNumber;
                JToken token = JObject.Parse(activity.Value.ToString());
                var empName = Convert.ToString(token.SelectToken("EmpName"));
                var startDate = Convert.ToString(token.SelectToken("StartDate"));
                var endDate = Convert.ToString(token.SelectToken("EndDate"));
                var showCompletedRequests = Convert.ToString(token.SelectToken("ShowCompletedToggle"));
                var filters = context.PrivateConversationData.ContainsKey("FiltersHashTable") ? context.PrivateConversationData.GetValue<Hashtable>("FiltersHashTable") : new Hashtable();
                if (command.ToLowerInvariant() == Constants.TORRemoveEmpName.ToLowerInvariant())
                {
                    empName = string.Empty;
                    startDate = filters.ContainsKey("DateRange") ? Convert.ToString(filters["DateRange"]).Split(';')[0] : null;
                    endDate = filters.ContainsKey("DateRange") ? Convert.ToString(filters["DateRange"]).Split(';')[1] : null;
                    showCompletedRequests = filters.ContainsKey("ShowCompletedRequests") ? Convert.ToString(filters["ShowCompletedRequests"]) : "false";
                }

                if (command.ToLowerInvariant() == Constants.TORRemoveDateRange.ToLowerInvariant())
                {
                    startDate = endDate = null;
                    empName = filters.ContainsKey("EmpName") ? Convert.ToString(filters["EmpName"]) : string.Empty;
                }

                if (command.ToLowerInvariant() == Constants.TORRemoveShowCompletedRequests.ToLowerInvariant())
                {
                    empName = filters.ContainsKey("EmpName") ? Convert.ToString(filters["EmpName"]) : string.Empty;
                    startDate = filters.ContainsKey("DateRange") ? Convert.ToString(filters["DateRange"]).Split(';')[0] : null;
                    endDate = filters.ContainsKey("DateRange") ? Convert.ToString(filters["DateRange"]).Split(';')[1] : null;
                    showCompletedRequests = "false";
                }

                if (command.ToLowerInvariant() == Constants.TORListRefresh.ToLowerInvariant())
                {
                    empName = filters.ContainsKey("EmpName") ? Convert.ToString(filters["EmpName"]) : string.Empty;
                    startDate = filters.ContainsKey("DateRange") ? Convert.ToString(filters["DateRange"]).Split(';')[0] : null;
                    endDate = filters.ContainsKey("DateRange") ? Convert.ToString(filters["DateRange"]).Split(';')[1] : null;
                    showCompletedRequests = filters.ContainsKey("ShowCompletedRequests") ? Convert.ToString(filters["ShowCompletedRequests"]) : "false";
                }

                AdaptiveCard card = new AdaptiveCard("1.0");
                DateTime sdt1 = DateTime.Now;
                DateTime edt = DateTime.Now;
                if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
                {
                    await context.PostAsync(Resources.KronosResourceText.SelectEndDate);
                    context.Done(default(string));
                    return;
                }

                if (string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    await context.PostAsync(Resources.KronosResourceText.SelectStartDate);
                    context.Done(default(string));
                    return;
                }

                var employeesResult = await this.hyperFindActivity.GetHyperFindQueryValues(tenantId, jSessionId, DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), "Report", "Private");
                if (employeesResult.Status == ApiConstants.Failure && employeesResult.Error.ErrorCode == ApiConstants.UserNotLoggedInError)
                {
                    await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                }

                if (employeesResult.Status == ApiConstants.Success)
                {
                    Hashtable employeesHashTable = new Hashtable();
                    Hashtable employeesRoleHashTable = new Hashtable();
                    Hashtable filtersHashTable = new Hashtable();
                    List<Models.ResponseEntities.HyperFind.ResponseHyperFindResult> filteredEmployees = new List<Models.ResponseEntities.HyperFind.ResponseHyperFindResult>();
                    if (!string.IsNullOrEmpty(empName))
                    {
                        var empSplit = empName.Split(';');
                        filteredEmployees = employeesResult.HyperFindResult.Where(n => empSplit.Any(m => n.FullName.ToLower().Contains(m.ToLower()))).ToList();
                    }
                    else
                    {
                        filteredEmployees = employeesResult.HyperFindResult;
                    }

                    empName = string.Join(";", filteredEmployees.Select(w => w.FullName).ToArray());
                    filtersHashTable.Add("EmpName", empName);

                    foreach (var item in filteredEmployees)
                    {
                        employeesHashTable.Add(item.PersonNumber, item.FullName);
                    }

                    context.PrivateConversationData.SetValue("EmployeeHashTable", employeesHashTable);

                    if (!string.IsNullOrEmpty(startDate))
                    {
                        DateTime.TryParse(startDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out sdt1);
                        DateTime.TryParse(endDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out edt);
                        filtersHashTable.Add("DateRange", startDate + ";" + endDate);
                    }

                    if (filteredEmployees.Count == 0)
                    {
                        await context.PostAsync(KronosResourceText.NoEmployees);
                    }
                    else
                    {
                        var response = await this.timeOffActivity.GetTimeOffRequest(tenantId, jSessionId, sdt1.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), edt.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), filteredEmployees);

                        var requests = response.RequestMgmt?.RequestItems?.GlobalTimeOffRequestItem;
                        requests = requests.Where(w => w.StatusName.ToLowerInvariant() == Constants.Submitted.ToLowerInvariant() || w.StatusName.ToLowerInvariant() == Constants.Approved.ToLowerInvariant() || w.StatusName.ToLowerInvariant() == Constants.Refused.ToLowerInvariant()).ToList();
                        for (int i = 0; i < requests.Count; i++)
                        {
                            DateTime.TryParse(requests[i].TimeOffPeriods.TimeOffPeriod.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sdt);
                            requests[i].TimeOffPeriods.TimeOffPeriod.Sdt = sdt;
                        }

                        requests = requests.OrderBy(w => w.TimeOffPeriods.TimeOffPeriod.Sdt).ToList();

                        var personNumbers = (from d in requests select d.Employee.PersonIdentity.PersonNumber).Distinct().ToList();
                        var tasks = personNumbers.Select(async emp =>
                        {
                            if (!employeesRoleHashTable.ContainsKey(Convert.ToString(emp)))
                            {
                                var jobAssignent = await this.commonActivity.GetJobAssignment(Convert.ToString(emp), tenantId, superSession);
                                var role = jobAssignent?.JobAssign?.PrimaryLaborAccList?.PrimaryLaborAcc?.OrganizationPath?.Split('/').LastOrDefault();
                                employeesRoleHashTable.Add(Convert.ToString(emp), role);
                            }
                        });
                        await Task.WhenAll(tasks);
                        context.PrivateConversationData.SetValue("EmployeeRoleHashTable", employeesRoleHashTable);
                        if (!string.IsNullOrEmpty(showCompletedRequests))
                        {
                            if (showCompletedRequests.ToLowerInvariant() == "true")
                            {
                                requests = requests.Where(w => (w.StatusName.ToLowerInvariant() == Constants.Approved.ToLowerInvariant()) || (w.StatusName.ToLowerInvariant() == Constants.Refused.ToLowerInvariant())).ToList();
                            }
                            else
                            {
                                requests = requests.Where(w => (w.StatusName.ToLowerInvariant() == Constants.Submitted.ToLowerInvariant())).ToList();
                            }

                            filtersHashTable.Add("ShowCompletedRequests", showCompletedRequests);
                        }
                        else
                        {
                            requests = requests.Where(w => (w.StatusName.ToLowerInvariant() == Constants.Submitted.ToLowerInvariant())).ToList();
                        }

                        int totalPages = (requests?.Count > 0) ? (int)Math.Ceiling((double)requests.Count / 5) : 0;
                        var currentPageItems = this.GetPage(requests, 0, 5);

                        Hashtable pagewiseRequests = new Hashtable();
                        for (int i = 0; i < totalPages; i++)
                        {
                            pagewiseRequests.Add(i.ToString(), JsonConvert.SerializeObject(this.GetPage(requests, i, 5)).ToString());
                        }

                        context.PrivateConversationData.SetValue("PagewiseRequests", pagewiseRequests);
                        context.PrivateConversationData.SetValue("FiltersHashTable", filtersHashTable);
                        ViewTorListObj obj = new ViewTorListObj
                        {
                            Employees = employeesHashTable,
                            EmployeesRoles = employeesRoleHashTable,
                            Filters = filtersHashTable,
                            TotalPages = totalPages,
                            CurrentPageCount = 1,
                        };

                        // string actionSet = "FR";
                        string actionSet = "F";
                        if (obj.CurrentPageCount < obj.TotalPages)
                        {
                            // actionSet = "FRN";
                            actionSet = "FN";
                        }

                        obj.ConversationId = context.Activity.Conversation.Id;
                        obj.ActivityId = context.Activity.ReplyToId;

                        var comments = context.PrivateConversationData.GetValue<List<Models.ResponseEntities.CommentList.Comment>>("Comments");
                        if (requests.Count > 0)
                        {
                            card = this.supervisorTimeOffcard.GetCard(JsonConvert.DeserializeObject<List<TimeOffResponse.GlobalTimeOffRequestItem>>(Convert.ToString(pagewiseRequests[Convert.ToString(obj.CurrentPageCount - 1)])), obj, actionSet, comments);
                        }
                        else
                        {
                            card = this.supervisorTimeOffcard.GetCard(new List<TimeOffResponse.GlobalTimeOffRequestItem>(), obj, actionSet, comments);
                        }

                        var conversationId = context.Activity.Conversation.Id;
                        var activityId = context.Activity.ReplyToId;
                        context.PrivateConversationData.SetValue("conversationId", conversationId);
                        context.PrivateConversationData.SetValue("activityId", activityId);
                        IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                        activity.Text = null;
                        activity.Value = null;
                        activity.Attachments.Add(new Attachment()
                        {
                            Content = card,
                            ContentType = "application/vnd.microsoft.card.adaptive",
                        });
                        await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
                    }
                }

                context.Done(default(string));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task Next(IDialogContext context)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            var currentPage = Convert.ToInt32(token.SelectToken("CurrentPage"));
            var employeesHashTable = context.PrivateConversationData.GetValue<Hashtable>("EmployeeHashTable");
            var employeesRoleHashTable = context.PrivateConversationData.GetValue<Hashtable>("EmployeeRoleHashTable");
            var filtersHashTable = context.PrivateConversationData.GetValue<Hashtable>("FiltersHashTable");
            var pagewiseRequests = context.PrivateConversationData.GetValue<Hashtable>("PagewiseRequests");
            ViewTorListObj obj = new ViewTorListObj
            {
                Employees = employeesHashTable,
                EmployeesRoles = employeesRoleHashTable,
                Filters = filtersHashTable,
                TotalPages = pagewiseRequests.Count,
                CurrentPageCount = currentPage + 1,
            };

            // string actionSet = "FRP";
            string actionSet = "FP";
            if (obj.CurrentPageCount < obj.TotalPages)
            {
                // actionSet = "FRNP";
                actionSet = "FNP";
            }

            obj.ConversationId = context.Activity.Conversation.Id;
            obj.ActivityId = context.Activity.ReplyToId;

            var comments = context.PrivateConversationData.GetValue<List<Models.ResponseEntities.CommentList.Comment>>("Comments");
            var current = JsonConvert.DeserializeObject<List<TimeOffResponse.GlobalTimeOffRequestItem>>(Convert.ToString(pagewiseRequests[(obj.CurrentPageCount - 1).ToString()]));
            var card = this.supervisorTimeOffcard.GetCard(current, obj, actionSet, comments);

            var conversationId = context.Activity.Conversation.Id;
            var activityId = context.Activity.ReplyToId;

            IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
            activity.Text = null;
            activity.Value = null;
            activity.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
            });
            await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
            context.Done(default(string));
        }

        private async Task Previous(IDialogContext context)
        {
            var activity = context.Activity as Activity;
            JToken token = JObject.Parse(activity.Value.ToString());
            var currentPage = Convert.ToInt32(token.SelectToken("CurrentPage"));
            var employeesHashTable = context.PrivateConversationData.GetValue<Hashtable>("EmployeeHashTable");
            var employeesRoleHashTable = context.PrivateConversationData.GetValue<Hashtable>("EmployeeRoleHashTable");
            var filtersHashTable = context.PrivateConversationData.GetValue<Hashtable>("FiltersHashTable");
            var pagewiseRequests = context.PrivateConversationData.GetValue<Hashtable>("PagewiseRequests");
            ViewTorListObj obj = new ViewTorListObj
            {
                Employees = employeesHashTable,
                EmployeesRoles = employeesRoleHashTable,
                Filters = filtersHashTable,
                TotalPages = pagewiseRequests.Count,
                CurrentPageCount = currentPage - 1,
            };

            // string actionSet = "FRN";
            string actionSet = "FN";
            if (obj.CurrentPageCount != 1)
            {
                // actionSet = "FRNP";
                actionSet = "FNP";
            }

            obj.ConversationId = context.Activity.Conversation.Id;
            obj.ActivityId = context.Activity.ReplyToId;

            var comments = context.PrivateConversationData.GetValue<List<Models.ResponseEntities.CommentList.Comment>>("Comments");
            var current = JsonConvert.DeserializeObject<List<TimeOffResponse.GlobalTimeOffRequestItem>>(Convert.ToString(pagewiseRequests[(obj.CurrentPageCount - 1).ToString()]));
            var card = this.supervisorTimeOffcard.GetCard(current, obj, actionSet, comments);

            var conversationId = context.Activity.Conversation.Id;
            var activityId = context.Activity.ReplyToId;

            IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
            activity.Text = null;
            activity.Value = null;
            activity.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
            });
            await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
            context.Done(default(string));
        }

        private async Task SubmitTimeOffApproval(IDialogContext context, string command)
        {
            try
            {
                var activity = context.Activity as Activity;
                JToken token = JObject.Parse(activity.Value.ToString()).SelectToken("data");
                JObject tenant = context.Activity.ChannelData as JObject;
                string tenantId = tenant["tenant"].SelectToken("id").ToString();

                string jSessionId = this.response?.JsessionID;
                string personNumber = (string)token.SelectToken("PersonNumber");
                string querySpan = (string)token.SelectToken("QueryDateSpan");
                string reqId = (string)token.SelectToken("RequestId");
                string index = (string)token.SelectToken("Index");
                string currentPage = (string)token.SelectToken("CurrentPage");
                string comment = (string)token.SelectToken("Comment");
                string note = (string)token.SelectToken("note");
                string conversationId = (string)token.SelectToken("conversationId");
                string activityId = (string)token.SelectToken("activityId");
                command = command.ToLowerInvariant() == Constants.TORApproveRequest.ToLowerInvariant() ? Constants.ApproveTimeoff : Constants.RefuseTimeoff;
                Models.ResponseEntities.TimeOff.SubmitResponse.Response response = await this.timeOffRequestActivity.SubmitApproval(tenantId, jSessionId, reqId, personNumber, command, querySpan, comment, note);
                if (response?.Status == ApiConstants.Failure)
                {
                    if (response.Error?.ErrorCode == ApiConstants.UserNotLoggedInError)
                    {
                        await this.authenticationService.SendAuthCardAsync(context, (Activity)context.Activity);
                    }
                    else
                    {
                        await context.PostAsync(response.Error.DetailErrors.Error.FirstOrDefault().Message.Substring(9));
                    }
                }

                if (response?.Status == ApiConstants.Success)
                {
                    var empConversationId = await this.commonActivity.GetEmpConversationId(personNumber, tenantId, jSessionId, activity.ChannelId);

                    var status = command.ToLowerInvariant() == Constants.ApproveTimeoff.ToLowerInvariant() ? Constants.Approved.ToUpper() : Constants.Refused.ToUpper();

                    if (empConversationId == null)
                    {
                        await context.PostAsync(Resources.KronosResourceText.EmpNotFoundForNotification);
                    }
                    else
                    {
                        await this.SendApprovalResponseNotiToEmployee(tenantId, context, status, empConversationId, note, querySpan, reqId);
                    }

                    var pagewiseRequests = context.PrivateConversationData.GetValue<Hashtable>("PagewiseRequests");
                    var toChange = JsonConvert.DeserializeObject<List<TimeOffResponse.GlobalTimeOffRequestItem>>(Convert.ToString(pagewiseRequests[Convert.ToString(Convert.ToInt32(currentPage) - 1)]));
                    for (int i = 0; i < toChange.Count; i++)
                    {
                        if (i == Convert.ToInt32(index))
                        {
                            toChange[i].StatusName = status;
                        }
                    }

                    pagewiseRequests[Convert.ToString(Convert.ToInt32(currentPage) - 1)] = JsonConvert.SerializeObject(toChange).ToString();
                    context.PrivateConversationData.SetValue("PagewiseRequests", pagewiseRequests);
                    var employeesHashTable = context.PrivateConversationData.GetValue<Hashtable>("EmployeeHashTable");
                    var employeesRoleHashTable = context.PrivateConversationData.GetValue<Hashtable>("EmployeeRoleHashTable");
                    var filtersHashTable = context.PrivateConversationData.GetValue<Hashtable>("FiltersHashTable");
                    ViewTorListObj obj = new ViewTorListObj
                    {
                        Employees = employeesHashTable,
                        Filters = filtersHashTable,
                        EmployeesRoles = employeesRoleHashTable,
                        TotalPages = pagewiseRequests.Count,
                        CurrentPageCount = Convert.ToInt32(currentPage),
                    };

                    // string actionSet = "FR";
                    string actionSet = "F";
                    if (obj.TotalPages != 1)
                    {
                        if (obj.CurrentPageCount < obj.TotalPages && obj.CurrentPageCount > 1)
                        {
                            // actionSet = "FRNP";
                            actionSet = "FNP";
                        }
                        else if (obj.CurrentPageCount == 1)
                        {
                            // actionSet = "FRN";
                            actionSet = "FN";
                        }
                        else if (obj.CurrentPageCount == obj.TotalPages)
                        {
                            // actionSet = "FRP";
                            actionSet = "FP";
                        }
                    }

                    var comments = context.PrivateConversationData.GetValue<List<Models.ResponseEntities.CommentList.Comment>>("Comments");
                    AdaptiveCard card = new AdaptiveCard("1.0");
                    card = this.supervisorTimeOffcard.GetCard(JsonConvert.DeserializeObject<List<TimeOffResponse.GlobalTimeOffRequestItem>>(Convert.ToString(pagewiseRequests[Convert.ToString(obj.CurrentPageCount - 1)])), obj, actionSet, comments);

                    IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                    activity.Text = null;
                    activity.Value = null;
                    activity.Attachments.Add(new Attachment()
                    {
                        Content = card,
                        ContentType = "application/vnd.microsoft.card.adaptive",
                    });
                    await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, activity);
                }

                context.Done(default(string));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
//-----------------------------------------------------------------------
// <copyright file="TimeOffActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.RequestEntities;
    using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.TimeOff.AddRequest;
    using TimeOffAddRequest = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.TimeOff.AddRequest;
    using TimeOffAddResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOff.AddResponse;
    using TimeOffRequest = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.TimeOffRequests;
    using TimeOffResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOffRequests;
    using TimeOffSubmitRequest = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.TimeOff.SubmitRequest;
    using TimeOffSubmitResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOff.SubmitResponse;

    /// <summary>
    /// Time Off Activity Class.
    /// </summary>
    [Serializable]
    public class TimeOffActivity : ITimeOffActivity
    {
        /// <summary>
        /// Azure table storage helper.
        /// </summary>
        private readonly AzureTableStorageHelper azureTableStorageHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeOffActivity" /> class.
        /// </summary>
        public TimeOffActivity()
        {
            this.azureTableStorageHelper = new AzureTableStorageHelper();
        }

        /// <summary>
        /// Send time off request to Kronos API and get response.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="jSession">J Session.</param>
        /// <param name="startDate">Start Date.</param>
        /// <param name="endDate">End Date.</param>
        /// <param name="personNumber">Person Number.</param>
        /// <param name="reason">Reason string.</param>
        /// <returns>Time of add response.</returns>
        public async Task<TimeOffAddResponse.Response> TimeOffRequest(string tenantId, string jSession, string startDate, string endDate, string personNumber, string reason)
        {
            string xmlTimeOffRequest = this.CreateAddTimeOffRequest(startDate, endDate, personNumber, reason);
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlTimeOffRequest, ApiConstants.SoapEnvClose, jSession);

            TimeOffAddResponse.Response timeOffResponse = this.ProcessResponse(tupleResponse.Item1);

            return timeOffResponse;
        }

        /// <summary>
        /// Send advanced time off request to Kronos API and get response.
        /// </summary>
        /// <param name="tenantId">Tensnt ID.</param>
        /// <param name="jSession">J Session.</param>
        /// <param name="personNumber">Person Number.</param>
        /// <param name="obj">Submitted values from employee.</param>
        /// <returns>Time of add response.</returns>
        public async Task<TimeOffAddResponse.Response> AdvancedTimeOffRequest(string tenantId, string jSession, string personNumber, AdvancedTimeOff obj)
        {
            string xmlTimeOffRequest = this.CreateAddAdvancedTimeOffRequest(personNumber, obj);
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlTimeOffRequest, ApiConstants.SoapEnvClose, jSession);

            TimeOffAddResponse.Response timeOffResponse = this.ProcessResponse(tupleResponse.Item1);

            return timeOffResponse;
        }

        /// <summary>
        /// Submit time of request which is in draft.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="jSession">jSession object.</param>
        /// <param name="personNumber">Person number.</param>
        /// <param name="reqId">RequestId of the time off request.</param>
        /// <param name="querySpan">Query Span.</param>
        /// <returns>Time of submit response.</returns>
        public async Task<TimeOffSubmitResponse.Response> SubmitTimeOffRequest(string tenantId, string jSession, string personNumber, string reqId, string querySpan)
        {
            string xmlTimeOffRequest = this.CreateSubmitTimeOffRequest(personNumber, reqId, querySpan);
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlTimeOffRequest, ApiConstants.SoapEnvClose, jSession);

            TimeOffSubmitResponse.Response timeOffSubmitResponse = this.ProcessSubmitResponse(tupleResponse.Item1);

            return timeOffSubmitResponse;
        }

        /// <summary>
        /// Send Approval request to Kronos as per action taken by supervisor.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="jSession">j Session.</param>
        /// <param name="reqId">RequestId of the time off request.</param>
        /// <param name="personNumber">Person number.</param>
        /// <param name="command">command string.</param>
        /// <param name="querySpan">Query Span.</param>
        /// <param name="comment">Selected comment.</param>
        /// <param name="note">Entered note.</param>
        /// <returns>Time of submit response.</returns>
        public async Task<TimeOffSubmitResponse.Response> SubmitApproval(string tenantId, string jSession, string reqId, string personNumber, string command, string querySpan, string comment, string note)
        {
            string xmlTimeOffRequest = this.CreateApprovalRequest(personNumber, reqId, command, querySpan, comment, note);
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlTimeOffRequest, ApiConstants.SoapEnvClose, jSession);

            TimeOffSubmitResponse.Response timeOffResponse = this.ProcessSubmitResponse(tupleResponse.Item1);

            return timeOffResponse;
        }

        /// <summary>
        /// Get vacation list.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="jSession">J Session.</param>
        /// <param name="personNumber">Person Number.</param>
        /// <param name="cmd">cmd string.</param>
        /// <returns>vacation list.</returns>
        public async Task<TimeOffAddResponse.Response> getVacationsList(string tenantId, string jSession, string personNumber, string cmd)
        {
            string xmlTimeOffRequest = this.CreateVacationListRequest(personNumber, cmd);
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlTimeOffRequest, ApiConstants.SoapEnvClose, jSession);

            TimeOffAddResponse.Response vacationResponse = this.ProcessResponse(tupleResponse.Item1);

            return vacationResponse;
        }

        /// <summary>
        /// Create XML to add time off request.
        /// </summary>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">End Date.</param>
        /// <param name="personNumber">Person number.</param>
        /// <param name="reason">Reason string.</param>
        /// <returns>Add time of request.</returns>
        public string CreateAddTimeOffRequest(string startDate, string endDate, string personNumber, string reason)
        {
            TimeOffAddRequest.TimeOffPeriod[] t = new TimeOffAddRequest.TimeOffPeriod[]
            {
                new TimeOffPeriod()
            {
                Duration = "FULL_DAY",
                EndDate = endDate,
                PayCodeName = reason,
                StartDate = startDate,
            },
            };
            Request rq = new Request()
            {
                Action = ApiConstants.AddRequests,
                EmployeeRequestMgm = new TimeOffAddRequest.EmployeeRequestMgmt()
                {
                    Employees = new TimeOffAddRequest.Employee() { PersonIdentity = new TimeOffAddRequest.PersonIdentity() { PersonNumber = personNumber } },
                    QueryDateSpan = $"{startDate} - {endDate}",
                    RequestItems = new TimeOffAddRequest.RequestItems()
                    {
                        GlobalTimeOffRequestItem = new TimeOffAddRequest.GlobalTimeOffRequestItem()
                        {
                            Employee = new TimeOffAddRequest.Employee() { PersonIdentity = new TimeOffAddRequest.PersonIdentity() { PersonNumber = personNumber } },
                            RequestFor = "TOR",
                            TimeOffPeriods = new TimeOffAddRequest.TimeOffPeriods() { TimeOffPeriod = t },
                        },
                    },
                },
            };

            return rq.XmlSerialize<TimeOffAddRequest.Request>();
        }

        /// <summary>
        /// Fecth time off request details for displaying history.
        /// </summary>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="jSession">JSession.</param>
        /// <param name="queryDateSpan">QueryDateSpan string.</param>
        /// <param name="personNumber">Person number who created request.</param>
        /// <returns>Request details response object.</returns>
        public async Task<TimeOffResponse.Response> GetTimeOffRequestDetails(string tenantId, string jSession, string queryDateSpan, string personNumber)
        {
            string xmlTimeOffRequest = this.CreateTORRequest(personNumber, queryDateSpan);
            TenantMapEntity tenantMapEntity = await this.azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlTimeOffRequest, ApiConstants.SoapEnvClose, jSession);

            TimeOffResponse.Response timeOffResponse = this.ProcessTORResponse(tupleResponse.Item1);

            return timeOffResponse;
        }

        /// <summary>
        /// Create request.
        /// </summary>
        /// <param name="personNumber">Person number who created request.</param>
        /// <param name="queryDateSpan">QueryDateSpan string.</param>
        /// <returns>XML request string.</returns>
        public string CreateTORRequest(string personNumber, string queryDateSpan)
        {
            TimeOffRequest.Request rq = new TimeOffRequest.Request
            {
                Action = ApiConstants.RetrieveWithDetails,
                RequestMgmt = new TimeOffRequest.RequestMgmt
                {
                    QueryDateSpan = $"{queryDateSpan}",
                    RequestFor = "TOR",
                    Employees = new TimeOffRequest.Employees
                    {
                        PersonIdentity = new List<TimeOffRequest.PersonIdentity>(),
                    },
                },
            };

            rq.RequestMgmt.Employees.PersonIdentity.Add(new TimeOffRequest.PersonIdentity { PersonNumber = personNumber });
            return rq.XmlSerialize<TimeOffRequest.Request>();
        }

        /// <summary>
        /// Process response for request details.
        /// </summary>
        /// <param name="strResponse">Response received from request for TOR detail.</param>
        /// <returns>Request details response object.</returns>
        public TimeOffResponse.Response ProcessTORResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<TimeOffResponse.Response>(xResponse.ToString());
        }

        /// <summary>
        /// Create XML to add advanced time off request.
        /// </summary>
        /// <param name="personNumber">Person Number.</param>
        /// <param name="obj">Submitted values from employee.</param>
        /// <returns>Advanced time of request.</returns>
        public string CreateAddAdvancedTimeOffRequest(string personNumber, AdvancedTimeOff obj)
        {
            var monthStartDt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var monthEndDt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(monthStartDt.Year, monthStartDt.Month));
            double? length = null;
            if (obj.duration.ToLowerInvariant() == Constants.Hours.ToLowerInvariant())
            {
                var shr = Convert.ToInt32(obj.StartTime.Split(' ')[0].Split(':')[0]);
                var smin = Convert.ToInt32(obj.StartTime.Split(' ')[0].Split(':')[1]);
                var ehr = Convert.ToInt32(obj.EndTime.Split(' ')[0].Split(':')[0]);
                var emin = Convert.ToInt32(obj.EndTime.Split(' ')[0].Split(':')[1]);

                length = new DateTime(2000, 1, 1, ehr, emin, 0).Subtract(new DateTime(2000, 1, 1, shr, smin, 0)).TotalHours;
            }

            TimeOffAddRequest.TimeOffPeriod[] t = new TimeOffAddRequest.TimeOffPeriod[]
            {
                new TimeOffAddRequest.TimeOffPeriod()
            {
                Duration = obj.duration,
                EndDate = obj.edt,
                PayCodeName = obj.DeductFrom,
                StartDate = obj.sdt,
                StartTime = obj.duration.ToLowerInvariant() == Constants.Hours.ToLowerInvariant() ? obj.StartTime : null,
                Length = length?.ToString(),
            },
            };
            Request rq = new Request()
            {
                Action = ApiConstants.AddRequests,
                EmployeeRequestMgm = new EmployeeRequestMgmt()
                {
                    Employees = new Employee() { PersonIdentity = new TimeOffAddRequest.PersonIdentity() { PersonNumber = personNumber } },
                    QueryDateSpan = monthStartDt.ToString("M/d/yyyy", CultureInfo.InvariantCulture) + "-" + monthEndDt.ToString("M/d/yyyy", CultureInfo.InvariantCulture),
                    RequestItems = new RequestItems()
                    {
                        GlobalTimeOffRequestItem = new GlobalTimeOffRequestItem()
                        {
                            Employee = new Employee() { PersonIdentity = new PersonIdentity() { PersonNumber = personNumber } },
                            RequestFor = "TOR",
                            TimeOffPeriods = new TimeOffPeriods() { TimeOffPeriod = t },
                            Comments = new Comments { Comment = null },
                        },
                    },
                },
            };
            if (!string.IsNullOrEmpty(obj.Note))
            {
                rq.EmployeeRequestMgm.RequestItems.GlobalTimeOffRequestItem.Comments.Comment = new List<Comment>
                {
                    new Comment { CommentText = obj.Comment, Notes = new Notes { Note = new Note { Text = obj.Note } } },
                };
            }
            else
            {
                rq.EmployeeRequestMgm.RequestItems.GlobalTimeOffRequestItem.Comments = null;
            }

            return rq.XmlSerialize<TimeOffAddRequest.Request>();
        }

        /// <summary>
        /// Create XML to submit time off request which is in draft.
        /// </summary>
        /// <param name="personNumber">Person Number.</param>
        /// <param name="reqId">RequestId of the time off request.</param>
        /// <param name="querySpan">Query Span.</param>
        /// <returns>Submit time off request.</returns>
        public string CreateSubmitTimeOffRequest(string personNumber, string reqId, string querySpan)
        {
            var monthStartDt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var monthEndDt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(monthStartDt.Year, monthStartDt.Month));
            TimeOffSubmitRequest.Request rq = new TimeOffSubmitRequest.Request()
            {
                Action = ApiConstants.SubmitRequests,
                EmployeeRequestMgm = new TimeOffSubmitRequest.EmployeeRequestMgmt()
                {
                    Employees = new TimeOffSubmitRequest.Employee() { PersonIdentity = new TimeOffSubmitRequest.PersonIdentity() { PersonNumber = personNumber } },
                    QueryDateSpan = querySpan,
                    RequestIds = new TimeOffSubmitRequest.RequestIds() { RequestId = new TimeOffSubmitRequest.RequestId[] { new TimeOffSubmitRequest.RequestId() { Id = reqId } } },
                },
            };

            return rq.XmlSerialize();
        }

        /// <summary>
        /// Create XML for approval sent by supervisor.
        /// </summary>
        /// <param name="personNumber">PersonNumber who sent time off request.</param>
        /// <param name="reqId">RequestId for which approval needs to be done.</param>
        /// <param name="command">Command String.</param>
        /// <param name="querySpan">Query Span.</param>
        /// <param name="comment">Selected comment.</param>
        /// <param name="note">Note entered while approving request.</param>
        /// <returns>Approval request.</returns>
        public string CreateApprovalRequest(string personNumber, string reqId, string command, string querySpan, string comment, string note)
        {
            var status = command == Constants.ApproveTimeoff ? Constants.Approved.ToUpper() : Constants.Refused.ToUpper();
            RequestManagement.Request rq = new RequestManagement.Request()
            {
                Action = ApiConstants.UpdateStatus,
                RequestMgmt = new RequestManagement.RequestMgmt()
                {
                    Employees = new RequestManagement.Employee() { PersonIdentity = new RequestManagement.PersonIdentity() { PersonNumber = personNumber } },
                    QueryDateSpan = querySpan,
                    RequestStatusChanges = new RequestManagement.RequestStatusChanges { RequestStatusChange = new System.Collections.Generic.List<RequestManagement.RequestStatusChange>() },
                },
            };

            List<Comment> cmt = null;
            if (!string.IsNullOrEmpty(note))
            {
                cmt = new List<Comment>
                {
                    new Comment { CommentText = comment, Notes = new Notes { Note = new Note { Text = note } } },
                };
            }

            rq.RequestMgmt.RequestStatusChanges.RequestStatusChange.Add(new RequestManagement.RequestStatusChange { RequestId = reqId, ToStatusName = status, Comments = new Comments { Comment = cmt } });

            return rq.XmlSerialize();
        }

        /// <summary>
        /// Create Vacation List Request.
        /// </summary>
        /// <param name="personNumber">Person Number.</param>
        /// <param name="cmd">command string.</param>
        /// <returns>vacation list.</returns>
        public string CreateVacationListRequest(string personNumber, string cmd)
        {
            var start = DateTime.Now;
            var end = DateTime.Now;

            if (cmd == Constants.NextVacation)
            {
                start = DateTime.Now.AddDays(1);
                end = DateTime.Now.AddMonths(1);
            }
            else
            {
                start = DateTime.Now.AddMonths(-12);
                end = DateTime.Now;
            }

            TimeOffSubmitRequest.Request rq = new TimeOffSubmitRequest.Request()
            {
                Action = ApiConstants.RetrieveRequests,
                EmployeeRequestMgm = new TimeOffSubmitRequest.EmployeeRequestMgmt()
                {
                    Employees = new TimeOffSubmitRequest.Employee() { PersonIdentity = new TimeOffSubmitRequest.PersonIdentity() { PersonNumber = personNumber } },
                    QueryDateSpan = start.ToString("M/d/yyyy", CultureInfo.InvariantCulture) + "-" + end.ToString("M/d/yyyy", CultureInfo.InvariantCulture),
                },
            };

            return rq.XmlSerialize();
        }

        /// <summary>
        /// Read the xml response into Response object.
        /// </summary>
        /// <param name="strResponse">xml response string.</param>
        /// <returns>Response object.</returns>
        public TimeOffAddResponse.Response ProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<TimeOffAddResponse.Response>(xResponse.ToString());
        }

        /// <summary>
        /// Deserialize xml response for submit request operation.
        /// </summary>
        /// <param name="strResponse">Response string.</param>
        /// <returns>Process submit response.</returns>
        public TimeOffSubmitResponse.Response ProcessSubmitResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<TimeOffSubmitResponse.Response>(xResponse.ToString());
        }
    }
}

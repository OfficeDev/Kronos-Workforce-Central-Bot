//-----------------------------------------------------------------------
// <copyright file="SwapShiftActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Teams.App.KronosWfc.Common;
using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
using System.Xml.Linq;
using JobRequest = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.SwapShift.JobRequest;
using Jobresponse=  Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift.JobResponse;
using LoadEligibleEmployeesResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift.LoadEligibleEmployees;
using LoadEligibleEmployeesRequest = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.SwapShift.LoadEligibleEmployees;
using SwapShiftRequest = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.SwapShift.SubmitRequest;
using SwapShiftResponse = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift.CreateSwapShift;

using RequestMgmt = Microsoft.Teams.App.KronosWfc.Models.RequestEntities;
using Microsoft.Teams.App.KronosWfc.Models;
using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.TimeOff.AddRequest;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Logon;
using logonReq = Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Logon;
using Microsoft.Teams.App.KronosWfc.Models.RequestEntities;
using System.Globalization;

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.SwapShift
{
    [Serializable]
    public class SwapShiftActivity : ISwapShiftActivity
    {
        private readonly AzureTableStorageHelper azureTableStorageHelper;
        private readonly logonReq.Request loginRequest;
        

        public SwapShiftActivity()
        {
            this.azureTableStorageHelper = new AzureTableStorageHelper();
            this.loginRequest = new logonReq.Request();
          
        }
   

        public string CreateLoadJobRequest(string PersonNumber, string QueryDate, string ShiftSwapDate, string StartTime, string EndTime)
        {
            JobRequest.Request request = new JobRequest.Request()
            {
                Action = ApiConstants.LoadJobs,
                SwapShiftJob = new JobRequest.SwapShiftJobs
                {
                    StartTime = StartTime,
                    EndTime = EndTime,
                    QueryDate = QueryDate,

                    Emp = new JobRequest.Employee { PersonId = new JobRequest.PersonIdentity { PersonNumber = PersonNumber } }
                }
            };

            return request.XmlSerialize<JobRequest.Request>();
        }

        public async Task<Jobresponse.Response> LoadAllJobs(string tenantId, string jSession, string PersonNumber, string QueryDate, string ShiftSwapDate, string StartTime, string EndTime)
        {
            try
            {
                string xmlTimeOffRequest = CreateLoadJobRequest(PersonNumber, QueryDate, ShiftSwapDate, StartTime, EndTime);
                TenantMapEntity tenantMapEntity = await azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
                var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlTimeOffRequest, ApiConstants.SoapEnvClose, jSession);

                Jobresponse.Response Response = ProcessAllJobsResponse(tupleResponse.Item1);

                return Response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Jobresponse.Response ProcessAllJobsResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Jobresponse.Response>(xResponse.ToString());
        }

        public async Task<LoadEligibleEmployeesResponse.Response> LoadEligibleEmployees(string tenantId, string jSession, string PersonNumber, string ShiftSwapDate, string StartTime, string EndTime)
        {
            try
            {         
                string xmlRequest = CreateLoadEligibleEmpRequest(PersonNumber, ShiftSwapDate, ShiftSwapDate, StartTime, EndTime);
                TenantMapEntity tenantMapEntity = await azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
                var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlRequest, ApiConstants.SoapEnvClose, jSession);

                LoadEligibleEmployeesResponse.Response Response = processEligibleEmployeesResponse(tupleResponse.Item1);

                return Response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Response> LogonSuperUser(User user)
        {
            try
            {
                string xmlLoginRequest = CreateLogOnRequest(user);
                TenantMapEntity tenantMapEntity = await azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, user.TenantId);
                var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlLoginRequest, ApiConstants.SoapEnvClose, string.Empty);

                Response logonResponse = ProcessResponse(tupleResponse.Item1);
                logonResponse.Jsession = tupleResponse.Item2;


                return logonResponse;
            }
            catch (Exception)
            {
                throw;
            }

        }
        /// <summary>
        /// Used to create xml logon request string
        /// </summary>
        /// <param name="user">User request object</param>
        /// <returns>Logon request xml string</returns>
        public string CreateLogOnRequest(User user)
        {
            loginRequest.Object = ApiConstants.System;
            loginRequest.Action = ApiConstants.LogonAction;
            loginRequest.Username = user.UserName;
            loginRequest.Password = user.Password;
            return loginRequest.XmlSerialize<logonReq.Request>();
        }

        /// <summary>
        /// Read the xml response into Response object
        /// </summary>
        /// <param name="strResponse">xml response string</param>
        /// <returns>Response object</returns>
        public Response ProcessResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<Response>(xResponse.ToString());
        }


        public async Task<SwapShiftResponse.Response> DraftSwapShift(string tenantId, string jSession, SwapShiftObj obj)
        {
            try
            {            
                string xmlRequest = CreateSwapShiftDraftRequest(obj);
                TenantMapEntity tenantMapEntity = await azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
                var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlRequest, ApiConstants.SoapEnvClose, jSession);

                SwapShiftResponse.Response Response = ProcessSwapShiftDraftResponse(tupleResponse.Item1);

                return Response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SwapShiftResponse.Response> SubmitSwapShift(string tenantId, string jSession, string personNumber, string reqId, string querySpan, string comment)
        {
            try
            {             

                string xmlRequest = CreateSwapShiftSubmitRequest(personNumber, reqId, querySpan, comment);
                TenantMapEntity tenantMapEntity = await azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
                var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlRequest, ApiConstants.SoapEnvClose, jSession);
                SwapShiftResponse.Response Response = ProcessSwapShiftDraftResponse(tupleResponse.Item1);
                return Response;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public string CreateLoadEligibleEmpRequest(string PersonNumber, string QueryDate, string ShiftSwapDate, string StartTime, string EndTime)
        {
            LoadEligibleEmployeesRequest.Request rq = new LoadEligibleEmployeesRequest.Request()
            {
                Action = ApiConstants.LoadEligibleEmployees,
                SwapShiftEmployees = new LoadEligibleEmployeesRequest.SwapShiftEmployees
                {
                    StartTime = StartTime,
                    EndTime = EndTime,
                    QueryDate = QueryDate,
                    ShiftSwapDate = ShiftSwapDate,
                    Employee = new Models.RequestEntities.Hours.Employee { PersonIdentity = new Models.RequestEntities.Hours.PersonIdentity { PersonNumber = PersonNumber } }
                }
            };

            return rq.XmlSerialize<LoadEligibleEmployeesRequest.Request>();

        }

        public LoadEligibleEmployeesResponse.Response processEligibleEmployeesResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<LoadEligibleEmployeesResponse.Response>(xResponse.ToString());
        }

        public string CreateSwapShiftDraftRequest(SwapShiftObj obj)
        {
            var requestorEmployee = new Models.RequestEntities.Hours.Employee
            {
                PersonIdentity = new Models.RequestEntities.Hours.PersonIdentity { PersonNumber = obj.RequestorPersonNumber }
            };

            var requestedToEmployee = new Models.RequestEntities.Hours.Employee
            {
                PersonIdentity = new Models.RequestEntities.Hours.PersonIdentity { PersonNumber = obj.RequestedToPersonNumber }
            };

            SwapShiftRequest.Request rq = new SwapShiftRequest.Request()
            {
                Action = ApiConstants.AddRequests,
                EmployeeRequestMgmt = new SwapShiftRequest.EmployeeRequestMgmt
                {
                    Employee = requestorEmployee,
                    QueryDateSpan = obj.QueryDateSpan,
                    RequestItems = new SwapShiftRequest.RequestItems
                    {
                        SwapShiftRequestItem = new SwapShiftRequest.SwapShiftRequestItem
                        {
                            Employee = requestorEmployee,
                            RequestFor = "Shift Swap Request",
                            OfferedShift = new SwapShiftRequest.OfferedShift
                            {
                                ShiftRequestItem = new SwapShiftRequest.ShiftRequestItem
                                {
                                    Employee = requestorEmployee,
                                    EndDateTime = obj.Emp1ToDateTime.ToString("MM/d/yyyy hh:mmtt", CultureInfo.InvariantCulture),
                                    OrgJobPath = obj.SelectedJob,
                                    StartDateTime = obj.Emp1FromDateTime.ToString("MM/d/yyyy hh:mmtt", CultureInfo.InvariantCulture),
                                }
                            },
                            RequestedShift = new SwapShiftRequest.RequestedShift
                            {
                                ShiftRequestItem = new SwapShiftRequest.ShiftRequestItem
                                {
                                    Employee = requestedToEmployee,
                                    EndDateTime = obj.Emp2ToDateTime.ToString("MM/d/yyyy hh:mmtt", CultureInfo.InvariantCulture),
                                    OrgJobPath = obj.SelectedJob,
                                    StartDateTime = obj.Emp2FromDateTime.ToString("MM/d/yyyy hh:mmtt", CultureInfo.InvariantCulture),
                                }
                            }
                        }
                    }
                }
            };

            return rq.XmlSerialize<SwapShiftRequest.Request>();

        }

        public SwapShiftResponse.Response ProcessSwapShiftDraftResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<SwapShiftResponse.Response>(xResponse.ToString());
        }


        public string CreateSwapShiftSubmitRequest(string personNumber, string reqId, string querySpan, string comment)
        {
            var status = Constants.Offered.ToUpper();
            RequestManagement.Request rq = new RequestManagement.Request()
            {
                Action = ApiConstants.UpdateStatus,
                RequestMgmt = new RequestManagement.RequestMgmt()
                {
                    Employees = new RequestManagement.Employee() { PersonIdentity = new RequestManagement.PersonIdentity() { PersonNumber = personNumber } },
                    QueryDateSpan = querySpan,
                    RequestStatusChanges = new RequestManagement.RequestStatusChanges { RequestStatusChange = new System.Collections.Generic.List<RequestManagement.RequestStatusChange>() }
                }
            };

            List<Comment> cmt = null;
            if (!string.IsNullOrEmpty(comment))
            {
                cmt = new List<Comment>
                {
                    new Comment { CommentText = "Other reason", Notes = new Notes { Note = new Note { Text = comment } } }
                };
            }

            rq.RequestMgmt.RequestStatusChanges.RequestStatusChange.Add(new RequestManagement.RequestStatusChange { RequestId = reqId, ToStatusName = status, Comments = new Comments { Comment = cmt } });

            return rq.XmlSerialize();

        }

        public SwapShiftResponse.Response ProcessSwapShiftResponse(string strResponse)
        {
            XDocument xDoc = XDocument.Parse(strResponse);
            var xResponse = xDoc.Root.Descendants().FirstOrDefault(d => d.Name.LocalName.Equals(ApiConstants.Response));
            return XmlConvertHelper.DeserializeObject<SwapShiftResponse.Response>(xResponse.ToString());
        }

        public async Task<SwapShiftResponse.Response> SubmitApproval(string tenantId, string jSession, string reqId, string personNumber, string status, string querySpan, string comment, string note)
        {
            string xmlRequest = CreateApprovalRequest(personNumber, reqId, status, querySpan, comment, note);
            TenantMapEntity tenantMapEntity = await azureTableStorageHelper.ExecuteQueryUsingPointQueryAsync<TenantMapEntity>(Constants.ActivityChannelId, tenantId);
            var tupleResponse = await ApiHelper.Instance.SendSoapPostRequest(tenantMapEntity.EndpointUrl, ApiConstants.SoapEnvOpen, xmlRequest, ApiConstants.SoapEnvClose, jSession);

            SwapShiftResponse.Response timeOffResponse = ProcessSwapShiftResponse(tupleResponse.Item1);

            return timeOffResponse;
        }

        public string CreateApprovalRequest(string personNumber, string reqId, string status, string querySpan, string comment, string note)
        {

            RequestManagement.Request rq = new RequestManagement.Request()
            {
                Action = ApiConstants.UpdateStatus,
                RequestMgmt = new RequestManagement.RequestMgmt()
                {
                    Employees = new RequestManagement.Employee() { PersonIdentity = new RequestManagement.PersonIdentity() { PersonNumber = personNumber } },
                    QueryDateSpan = querySpan,
                    RequestStatusChanges = new RequestManagement.RequestStatusChanges { RequestStatusChange = new System.Collections.Generic.List<RequestManagement.RequestStatusChange>() }
                }
            };

            List<Comment> cmt = null;
            if (!string.IsNullOrEmpty(note))
            {
                cmt = new List<Comment>
                {
                    new Comment { CommentText = comment, Notes = new Notes { Note = new Note { Text = note } } }
                };
            }

            rq.RequestMgmt.RequestStatusChanges.RequestStatusChange.Add(new RequestManagement.RequestStatusChange { RequestId = reqId, ToStatusName = status, Comments = new Comments { Comment = cmt } });

            return rq.XmlSerialize();

        }

    }
}

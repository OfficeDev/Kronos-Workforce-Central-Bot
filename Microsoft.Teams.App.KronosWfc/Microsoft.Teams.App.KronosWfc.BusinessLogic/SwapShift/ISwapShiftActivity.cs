using System.Threading.Tasks;
using Microsoft.Teams.App.KronosWfc.Models;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Logon;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift.CreateSwapShift;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift.JobResponse;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift.LoadEligibleEmployees;

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.SwapShift
{
    public interface ISwapShiftActivity
    {
        string CreateApprovalRequest(string personNumber, string reqId, string status, string querySpan, string comment, string note);
        string CreateLoadEligibleEmpRequest(string PersonNumber, string QueryDate, string ShiftSwapDate, string StartTime, string EndTime);
        string CreateLoadJobRequest(string PersonNumber, string QueryDate, string ShiftSwapDate, string StartTime, string EndTime);
        string CreateLogOnRequest(User user);
        string CreateSwapShiftDraftRequest(SwapShiftObj obj);
        string CreateSwapShiftSubmitRequest(string personNumber, string reqId, string querySpan, string comment);
        Task<Models.ResponseEntities.SwapShift.CreateSwapShift.Response> DraftSwapShift(string tenantId, string jSession, SwapShiftObj obj);
        Task<Models.ResponseEntities.SwapShift.JobResponse.Response> LoadAllJobs(string tenantId, string jSession, string PersonNumber, string QueryDate, string ShiftSwapDate, string StartTime, string EndTime);
        Task<Models.ResponseEntities.SwapShift.LoadEligibleEmployees.Response> LoadEligibleEmployees(string tenantId, string jSession, string PersonNumber, string ShiftSwapDate, string StartTime, string EndTime);
        Task<Models.ResponseEntities.Logon.Response> LogonSuperUser(User user);
        Models.ResponseEntities.SwapShift.JobResponse.Response ProcessAllJobsResponse(string strResponse);
        Models.ResponseEntities.SwapShift.LoadEligibleEmployees.Response processEligibleEmployeesResponse(string strResponse);
        Models.ResponseEntities.Logon.Response ProcessResponse(string strResponse);
        Models.ResponseEntities.SwapShift.CreateSwapShift.Response ProcessSwapShiftDraftResponse(string strResponse);
        Models.ResponseEntities.SwapShift.CreateSwapShift.Response ProcessSwapShiftResponse(string strResponse);
        Task<Models.ResponseEntities.SwapShift.CreateSwapShift.Response> SubmitApproval(string tenantId, string jSession, string reqId, string personNumber, string status, string querySpan, string comment, string note);
        Task<Models.ResponseEntities.SwapShift.CreateSwapShift.Response> SubmitSwapShift(string tenantId, string jSession, string personNumber, string reqId, string querySpan, string comment);
    }
}
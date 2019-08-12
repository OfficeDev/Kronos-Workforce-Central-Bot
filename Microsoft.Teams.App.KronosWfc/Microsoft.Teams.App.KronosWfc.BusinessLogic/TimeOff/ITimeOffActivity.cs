using System.Threading.Tasks;
using Microsoft.Teams.App.KronosWfc.Models;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOff.AddResponse;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOff.SubmitResponse;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOffRequests;

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic
{
    public interface ITimeOffActivity
    {
        Task<Models.ResponseEntities.TimeOff.AddResponse.Response> AdvancedTimeOffRequest(string tenantId, string jSession, string personNumber, AdvancedTimeOff obj);
        string CreateAddAdvancedTimeOffRequest(string personNumber, AdvancedTimeOff obj);
        string CreateAddTimeOffRequest(string startDate, string endDate, string personNumber, string reason);
        string CreateApprovalRequest(string personNumber, string reqId, string command, string querySpan, string comment, string note);
        string CreateSubmitTimeOffRequest(string personNumber, string reqId, string querySpan);
        string CreateTORRequest(string personNumber, string queryDateSpan);
        string CreateVacationListRequest(string personNumber, string cmd);
        Task<Models.ResponseEntities.TimeOffRequests.Response> GetTimeOffRequestDetails(string tenantId, string jSession, string queryDateSpan, string personNumber);
        Task<Models.ResponseEntities.TimeOff.AddResponse.Response> getVacationsList(string tenantId, string jSession, string personNumber, string cmd);
        Models.ResponseEntities.TimeOff.AddResponse.Response ProcessResponse(string strResponse);
        Models.ResponseEntities.TimeOff.SubmitResponse.Response ProcessSubmitResponse(string strResponse);
        Models.ResponseEntities.TimeOffRequests.Response ProcessTORResponse(string strResponse);
        Task<Models.ResponseEntities.TimeOff.SubmitResponse.Response> SubmitApproval(string tenantId, string jSession, string reqId, string personNumber, string command, string querySpan, string comment, string note);
        Task<Models.ResponseEntities.TimeOff.SubmitResponse.Response> SubmitTimeOffRequest(string tenantId, string jSession, string personNumber, string reqId, string querySpan);
        Task<Models.ResponseEntities.TimeOff.AddResponse.Response> TimeOffRequest(string tenantId, string jSession, string startDate, string endDate, string personNumber, string reason);
    }
}
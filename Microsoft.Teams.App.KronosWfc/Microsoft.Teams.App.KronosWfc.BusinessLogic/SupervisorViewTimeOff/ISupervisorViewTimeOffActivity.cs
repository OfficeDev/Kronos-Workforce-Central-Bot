using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.HyperFind;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOffRequests;

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.SupervisorViewTimeOff
{
    public interface ISupervisorViewTimeOffActivity
    {
        string CreateRequest(List<ResponseHyperFindResult> employees, string startdate, string enddate);
        Task<Models.ResponseEntities.TimeOffRequests.Response> GetTimeOffRequest(string tenantId, string jSession, string startDate, string endDate, List<ResponseHyperFindResult> employees);
        Models.ResponseEntities.TimeOffRequests.Response ProcessResponse(string strResponse);
    }
}
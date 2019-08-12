using System.Threading.Tasks;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.JobAssignment;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.PersonInformation;
using SubTypeParams = Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SubTypeParams;
namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.Common
{
    public interface ICommonActivity
    {
        string CreateJobAssignRequest(string personNumber);
        string CreatePIRequest(string personNumber);
        Task<string> GetConversationId(string personNumber, string tenantId, string jSession, string channelId);
        Task<string> GetEmpConversationId(string personNumber, string tenantId, string jSession, string channelId);
        Task<Models.ResponseEntities.JobAssignment.Response> GetJobAssignment(string personNumber, string tenantId, string jSession);
        Task<Models.ResponseEntities.PersonInformation.Response> GetPersonInformation(string tenantId, string superuserJSession, string PersonNumber);
        Models.ResponseEntities.JobAssignment.Response ProcessJobAssignResponse(string strResponse);
        Models.ResponseEntities.PersonInformation.Response ProcessPIResponse(string strResponse);

        Task<SubTypeParams.Response> GetRequestSubTypeParams(string tenantId, string jSession);
    }
}
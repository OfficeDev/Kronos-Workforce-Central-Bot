using System.Threading.Tasks;
using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.CommentList;

namespace Microsoft.Teams.App.KronosWfc.BusinessLogic.CommentList
{
    public interface ICommentsActivity
    {
        string CreateRequest();
        Task<Response> GetComments(string tenantId, string jSession);
        Response ProcessResponse(string strResponse);
    }
}
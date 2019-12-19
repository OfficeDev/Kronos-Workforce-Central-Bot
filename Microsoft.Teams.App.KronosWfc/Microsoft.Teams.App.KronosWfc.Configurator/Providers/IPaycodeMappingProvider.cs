using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Teams.App.KronosWfc.Configurator.Models;

namespace Microsoft.Teams.App.KronosWfc.Configurator.Providers
{
    public interface IPaycodeMappingProvider
    {
        Task<bool> AddBatchAsync(List<PaycodeMappingEntities> paycodes);
        Task<bool> DeleteAllAsync();
        Task<List<PaycodeMappingEntities>> RetrieveAllAsync();
    }
}
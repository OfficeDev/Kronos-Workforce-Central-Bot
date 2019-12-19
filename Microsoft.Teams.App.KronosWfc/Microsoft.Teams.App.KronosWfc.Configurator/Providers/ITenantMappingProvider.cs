using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Teams.App.KronosWfc.Configurator.Models;

namespace Microsoft.Teams.App.KronosWfc.Configurator.Providers
{
    public interface ITenantMappingProvider
    {
        Task<bool> DeleteAllAsync();
        Task<List<TenantMappingEntities>> GetAsync();
        Task<bool> SetAsync(TenantMappingEntities tenantMappingEntity);
    }
}
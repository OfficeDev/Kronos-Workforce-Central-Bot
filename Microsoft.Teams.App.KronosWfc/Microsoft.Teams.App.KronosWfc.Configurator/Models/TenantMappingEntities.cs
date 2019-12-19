namespace Microsoft.Teams.App.KronosWfc.Configurator.Models
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// TenantMappingEntities class.
    /// </summary>
    public class TenantMappingEntities : TableEntity
    {
        public string EndpointUrl { get; set; }
        public string Name { get; set; }
        public string SuperUserName { get; set; }
        public string SuperUserPwd { get; set; }
    }
}

using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Teams.App.KronosWfc.Configurator.Models
{
    public class PaycodeMappingEntities : TableEntity
    {
        public string PayCodeName { get; set; }
        public string PayCodeType { get; set; }
    }
}

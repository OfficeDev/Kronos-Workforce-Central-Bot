using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.SwapShift
{
    [Serializable]   
    public class ShiftDetailsRequest
    {
       public ShiftDetailsRequest()
        {
            SwapShiftObject = new SwapShiftObj();
        }

        public string ShiftDetails { get; set; }

        
        public string TenantId { get; set; }

        public SwapShiftObj SwapShiftObject { get; set; }
    }   
}

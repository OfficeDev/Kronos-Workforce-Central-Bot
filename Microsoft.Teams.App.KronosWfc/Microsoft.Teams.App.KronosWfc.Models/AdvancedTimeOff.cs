using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Teams.App.KronosWfc.Models
{
    public class AdvancedTimeOff
    {
        public string sdt { get; set; }
        public string edt { get; set; }
        public string duration { get; set; }
        public string DeductFrom { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Comment { get; set; }
        public string Note { get; set; }
    }
}
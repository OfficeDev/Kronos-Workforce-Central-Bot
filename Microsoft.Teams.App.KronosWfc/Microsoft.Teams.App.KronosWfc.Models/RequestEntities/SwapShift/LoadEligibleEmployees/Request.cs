using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Hours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.SwapShift.LoadEligibleEmployees
{
    [XmlRoot]
    public class Request
    {
        [XmlAttribute]
        public string Action { get; set; }
        [XmlElement]
        public SwapShiftEmployees SwapShiftEmployees { get; set; }
    }

    public class SwapShiftEmployees {
        [XmlAttribute]
        public string QueryDate { get; set; }

        [XmlAttribute]
        public string ShiftSwapDate { get; set; }

        [XmlAttribute]
        public string StartTime { get; set; }

        [XmlAttribute]
        public string EndTime { get; set; }

        [XmlElement]
        public Employee Employee { get; set; }
    }
}

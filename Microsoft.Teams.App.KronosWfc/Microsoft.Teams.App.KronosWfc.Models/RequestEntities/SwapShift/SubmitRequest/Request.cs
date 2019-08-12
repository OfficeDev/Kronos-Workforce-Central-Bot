using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Hours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.SwapShift.SubmitRequest
{
    [XmlRoot]
    public class Request
    {
        [XmlAttribute]
        public string Action { get; set; }
        [XmlElement]
        public EmployeeRequestMgmt EmployeeRequestMgmt { get; set; }
    }

    public class EmployeeRequestMgmt {
        [XmlAttribute]
        public string QueryDateSpan { get; set; }
        [XmlElement]
        public Employee Employee { get; set; }

        [XmlElement]
        public RequestItems RequestItems { get; set; }
    }

    public class RequestItems {
        [XmlElement]
        public SwapShiftRequestItem SwapShiftRequestItem { get; set; }
    }

    public class SwapShiftRequestItem
    {
        [XmlElement]
        public Employee Employee { get; set; }

        [XmlElement]
        public OfferedShift OfferedShift { get; set; }

        [XmlElement]
        public RequestedShift RequestedShift { get; set; }

        [XmlAttribute]
        public string RequestFor { get; set; }
    }

    public class OfferedShift {
        [XmlElement]
        public ShiftRequestItem ShiftRequestItem { get; set; }
    }

    public class ShiftRequestItem {
        [XmlAttribute]
        public string StartDateTime { get; set; }

        [XmlAttribute]
        public string EndDateTime { get; set; }

        [XmlAttribute]
        public string OrgJobPath { get; set; }

        [XmlElement]
        public Employee Employee { get; set; }
    }

    public class RequestedShift {
        [XmlElement]
        public ShiftRequestItem ShiftRequestItem { get; set; }
    }
}

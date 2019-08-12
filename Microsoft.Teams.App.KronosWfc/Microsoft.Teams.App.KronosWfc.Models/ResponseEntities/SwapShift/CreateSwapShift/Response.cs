using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Hours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift.CreateSwapShift
{
    [XmlRoot]
    public class Response
    {
        [XmlElement("EmployeeRequestMgmt")]
        public EmployeeRequestMgmt EmployeeRequestMgm { get; set; }

        [XmlAttribute]
        public string Status { get; set; }


        [XmlAttribute]
        public string Action { get; set; }

        [XmlAttribute]
        public string Sequence { get; set; }

        public Error Error { get; set; }
    }

    public class EmployeeRequestMgmt
    {
        [XmlAttribute]
        public string QueryDateSpan { get; set; }

        [XmlElement("Employee")]
        public Employee Employees { get; set; }
        [XmlElement("RequestItems")]
        public RequestItems RequestItem { get; set; }
    }

    public class RequestItems
    {
        [XmlElement("EmployeeSwapShiftRequestItem")]
        public EmployeeSwapShiftRequestItem[] EmployeeSwapShiftRequestItems { get; set; }

    }


    //public class EmployeeSwapShiftRequestItems
    //{
    //    public EmployeeSwapShiftRequestItem[] EmployeeSwapShiftRequestItem { get; set; }

    //}

    public class EmployeeSwapShiftRequestItem {
        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string RequestFor { get; set; }

        [XmlAttribute]
        public string DateTime { get; set; }

        [XmlElement("OfferedShift")]
        public OfferedShift OfferedShift { get; set; }

        [XmlElement("RequestedShift")]
        public RequestedShift RequestedShift { get; set; }
    }

    public class OfferedShift
    {
        [XmlElement("ShiftRequestItem")]
        public ShiftRequestItem ShiftRequestItem { get; set; }
    }

    public class ShiftRequestItem
    {
        [XmlAttribute]
        public string StartDateTime { get; set; }

        [XmlAttribute]
        public string EndDateTime { get; set; }

        [XmlAttribute]
        public string OrgJobPath { get; set; }

        [XmlElement]
        public Employee Employee { get; set; }
    }

    public class RequestedShift
    {
        [XmlElement("ShiftRequestItem")]
        public ShiftRequestItem ShiftRequestItem { get; set; }
    }
}

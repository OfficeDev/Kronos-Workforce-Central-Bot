using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOff.AddResponse
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

    public class Employee
    {
        [XmlElement("PersonIdentity")]
        public PersonIdentity PersonIdentit { get; set; }
    }
    public class PersonIdentity
    {
        [XmlAttribute]


        public string PersonNumber { get; set; }
    }

    public class RequestItems
    {
        [XmlElement("EmployeeGlobalTimeOffRequestItem")]
        public EmployeeGlobalTimeOffRequestItem[] GlobalTimeOffRequestItms { get; set; }

    }


    public class EmployeeGlobalTimeOffRequestItems
    {       
        public EmployeeGlobalTimeOffRequestItem[] GlobalTimeOffRequestItm { get; set; }

    }


    public class EmployeeGlobalTimeOffRequestItem
    {
        [XmlElement("TimeOffPeriods")]
        public TimeOffPeriods TimeOffPeriodsList { get; set; }

        [XmlAttribute]
        public string CreationDateTime { get; set; }

        [XmlAttribute]

        public string StatusName { get; set; }

        [XmlAttribute]

        public string Id { get; set; }

        [XmlAttribute]
        public string RequestFor { get; set; }


        [XmlElement("HolidayRequestSettings")]

        public HolidayRequestSettings HolidayRequestSettingList { get; set; }





    }


    public class TimeOffPeriods
    {
        [XmlElement("TimeOffPeriod")]
        public TimeOffPeriod[] TimeOffPerd { get; set; }

    }

    public class TimeOffPeriod
    {
        [XmlAttribute("StartDate")]
        public string StartDate { get; set; }

        [XmlAttribute("Duration")]
        public string Duration { get; set; }

        [XmlAttribute("EndDate")]

        public string EndDate { get; set; }

        [XmlAttribute("PayCodeName")]
        public string PayCodeName { get; set; }
        [XmlAttribute]
        public string StartTime { get; set; }
        [XmlAttribute]
        public string Length { get; set; }

        public DateTime sdt { get; set; }
        public DateTime edt { get; set; }
    }


    public class HolidayRequestSettings
    {
        [XmlElement("HolidayRequestSetting")]
        public HolidayRequestSetting[] HolidayRequestSettingsList { get; set; }
    }

    public class HolidayRequestSetting
    {
        [XmlAttribute]
        public string GeneratePayCodeWithSchedule { get; set; }

        [XmlAttribute]
        public string GeneratePayCodeWithOutSchedule { get; set; }
    }
}

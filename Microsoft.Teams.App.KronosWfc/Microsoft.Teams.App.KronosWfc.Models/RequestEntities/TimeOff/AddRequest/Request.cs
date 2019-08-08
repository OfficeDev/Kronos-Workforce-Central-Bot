using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.TimeOff.AddRequest
{
    [XmlRoot]
    public class Request
    {
        [XmlElement("EmployeeRequestMgmt")]
        public EmployeeRequestMgmt EmployeeRequestMgm { get; set; }

        [XmlAttribute]
        public string Action { get; set; }
    }

    public class EmployeeRequestMgmt
    {
        [XmlAttribute]
        public string QueryDateSpan { get; set; }

        [XmlElement("Employee")]
        public Employee Employees { get; set; }
        [XmlElement("RequestItems")]
        public RequestItems RequestItems { get; set; }
       
    }


    //public class Comments
    //{
    //    [XmlElement("Comment")]
    //    public Comment[] Comment { get; set; }
    //}

   
    //public class Comment
    //{
    //    [XmlAttribute]
    //    public string CommentText { get; set; }
    //}

    public class Employee
    {
        [XmlElement("PersonIdentity")]
        public PersonIdentity PersonIdentity { get; set; }
    }
    public class PersonIdentity
    {
        [XmlAttribute]
        public string PersonNumber { get; set; }
    }

    public class RequestItems
    {
        [XmlElement("GlobalTimeOffRequestItem")]
        public GlobalTimeOffRequestItem GlobalTimeOffRequestItem { get; set; }
    }

    public class GlobalTimeOffRequestItem
    {
        [XmlElement("TimeOffPeriods")]
        public TimeOffPeriods TimeOffPeriods { get; set; }
        [XmlElement("RequestFor")]
        public string RequestFor { get; set; }

        [XmlElement("Employee")]
        public Employee Employee { get; set; }

        [XmlElement("Comments")]
        public Comments Comments { get; set; }
    }

    public class TimeOffPeriods
    {
        [XmlElement("TimeOffPeriod")]
        public TimeOffPeriod[] TimeOffPeriod { get; set; }

    }

    public class TimeOffPeriod
    {
        [XmlAttribute]
        public String StartDate { get; set; }
        [XmlAttribute]
        public string EndDate { get; set; }
        [XmlAttribute]
        public string Duration { get; set; }
        [XmlAttribute]
        public string PayCodeName { get; set; }
        [XmlAttribute]
        public string StartTime { get; set; }
        [XmlAttribute]
        public string Length { get; set; }
    }
    public class StartDate
    {
        [XmlText]
        public string Date { get; set; }
    }

    public class RequestForItems
    {
        [XmlElement("RequestFor")]
        public RequestFor RequestFor { get; set; }
    }

    public class RequestFor
    {
        [XmlText]
        public string For { get; set; }
    }
}

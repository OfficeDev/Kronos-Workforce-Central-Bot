using Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOffRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities
{
    public class RequestManagement
    {
        [XmlRoot]
        public class Request
        {
            [XmlElement("RequestMgmt")]
            public RequestMgmt RequestMgmt { get; set; }

            [XmlAttribute]
            public string Action { get; set; }
        }

        public class RequestMgmt
        {
            [XmlAttribute]
            public string QueryDateSpan { get; set; }

            [XmlElement("Employees")]
            public Employee Employees { get; set; }

            [XmlElement(ElementName = "RequestStatusChanges")]
            public RequestStatusChanges RequestStatusChanges { get; set; }
        }

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

        [XmlRoot(ElementName = "RequestStatusChange")]
        public class RequestStatusChange
        {
            [XmlElement(ElementName = "User")]
            public User User { get; set; }
            [XmlElement(ElementName = "Comments")]
            public Comments Comments { get; set; }
            [XmlAttribute(AttributeName = "ToStatusName")]
            public string ToStatusName { get; set; }
            [XmlAttribute(AttributeName = "ChangeDateTime")]
            public string ChangeDateTime { get; set; }
            [XmlAttribute(AttributeName = "FromStatusName")]
            public string FromStatusName { get; set; }
            [XmlAttribute(AttributeName = "RequestId")]
            public string RequestId { get; set; }
        }

        [XmlRoot(ElementName = "RequestStatusChanges")]
        public class RequestStatusChanges
        {
            [XmlElement(ElementName = "RequestStatusChange")]
            public List<RequestStatusChange> RequestStatusChange { get; set; }
        }
    }
}

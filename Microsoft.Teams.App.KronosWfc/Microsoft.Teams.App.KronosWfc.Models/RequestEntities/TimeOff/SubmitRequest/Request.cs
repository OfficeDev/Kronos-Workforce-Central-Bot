using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.TimeOff.SubmitRequest
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

        [XmlElement("RequestIds")]
        public RequestIds RequestIds { get; set; }
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

    public class RequestIds
    {
        [XmlElement("RequestId")]
        public RequestId[] RequestId { get; set; }
    }

    public class RequestId    {

        [XmlAttribute]
        public string Id { get; set; }

    }
}

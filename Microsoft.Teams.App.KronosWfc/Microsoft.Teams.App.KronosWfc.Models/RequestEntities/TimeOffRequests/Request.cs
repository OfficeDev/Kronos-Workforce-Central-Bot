using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.TimeOffRequests
{
    [XmlRoot(ElementName = "PersonIdentity")]
    public class PersonIdentity
    {
        [XmlAttribute(AttributeName = "PersonNumber")]
        public string PersonNumber { get; set; }
    }

    [XmlRoot(ElementName = "Employees")]
    public class Employees
    {
        [XmlElement(ElementName = "PersonIdentity")]
        public List<PersonIdentity> PersonIdentity { get; set; }
    }

    [XmlRoot(ElementName = "RequestMgmt")]
    public class RequestMgmt
    {
        [XmlElement(ElementName = "Employees")]
        public Employees Employees { get; set; }
        [XmlAttribute(AttributeName = "QueryDateSpan")]
        public string QueryDateSpan { get; set; }
        [XmlAttribute(AttributeName = "RequestFor")]
        public string RequestFor { get; set; }
    }

    [XmlRoot(ElementName = "Request")]
    public class Request
    {
        [XmlElement(ElementName = "RequestMgmt")]
        public RequestMgmt RequestMgmt { get; set; }
        [XmlAttribute(AttributeName = "Action")]
        public string Action { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.PersonInformation
{
    [XmlRoot]
    public class Response
    {

        [XmlAttribute]
        public string Status { get; set; }


        [XmlAttribute]
        public string Action { get; set; }

        [XmlElement]
        public PersonInformation PersonInformation { get; set; }

        public Error Error { get; set; }
    }

    public class PersonInformation {
        [XmlElement]
        public PersonData PersonData { get; set; }
        [XmlElement]
        public SupervisorData SupervisorData { get; set; }
    }

    public class PersonData
    {
        [XmlElement]
        public Person Person { get; set; }
    }

    public class Person
    {
        [XmlAttribute]
        public string FullName { get; set; }
        [XmlAttribute]
        public string PersonNumber { get; set; }
    }

    public class SupervisorData
    {
        [XmlElement]
        public Supervisor Supervisor { get; set; }
    }

    public class Supervisor {
        [XmlAttribute]
        public string FullName { get; set; }
        [XmlAttribute]
        public string PersonNumber { get; set; }
    }
}

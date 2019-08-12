using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift.LoadEligibleEmployees
{
    [XmlRoot]
    public class Response
    {
        [XmlElement("Person")]
        public Person[] Person { get; set; }
    }

    //public class Persons
    //{
    //   public Person[] PersonList { get; set; }
    //}

    public class Person
    {
        [XmlAttribute]
        public string FullName { get; set; }
        [XmlAttribute]
        public string PersonNumber { get; set; }
    }
}

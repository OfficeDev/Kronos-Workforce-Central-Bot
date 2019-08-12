using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.PersonInfo
{
    [XmlRoot]
    public class Request
    {

        [XmlElement("PersonInformation")]
        public PersonInformation PersonInformation { get; set; }
        [XmlAttribute]
        public string Action { get; set; }

    }
    [Serializable]
    public class PersonInformation
    {
        [XmlElement("Identity")]
        public Identity Identity { get; set; }
    }
    [Serializable]
    public class Identity
    {
        [XmlElement("PersonIdentity")]
        public PersonIdentity PersonID { get; set; }
    }
    [Serializable]
    public class PersonIdentity
    {
        [XmlAttribute]
        public string PersonNumber { get; set; }
    }

}

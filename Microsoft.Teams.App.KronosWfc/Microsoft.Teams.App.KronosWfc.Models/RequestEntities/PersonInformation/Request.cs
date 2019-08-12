using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.PersonInformation
{
    [XmlRoot]
    public class Request
    {
        [XmlAttribute]
        public string Action { get; set; }
        [XmlElement]
        public PersonInformation PersonInformation { get; set; }
    }

    public class PersonInformation {
        [XmlElement]
        public Identity Identity { get; set; }
    }

    public class Identity {
        [XmlElement]
        public CurrentUser CurrentUser { get; set; }
        [XmlElement]
        public PersonIdentity PersonIdentity { get; set; }
    }

    public class CurrentUser {

    }

    public class PersonIdentity {
        [XmlAttribute]
        public string PersonNumber { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Schedule
{
    [Serializable]
    public class Request {
        public Schedule Schedule { get; set; }

        [XmlAttribute]
        public string Action { get; set; }
    }

    [Serializable]
    public class Schedule
    {
        public List<PersonIdentity> Employees { get; set; }

        [XmlAttribute]
        public string QueryDateSpan { get; set; }
        
    }

    [Serializable]
    public class PersonIdentity
    {
        [XmlAttribute]
        public string PersonNumber { get; set; }
    }
}

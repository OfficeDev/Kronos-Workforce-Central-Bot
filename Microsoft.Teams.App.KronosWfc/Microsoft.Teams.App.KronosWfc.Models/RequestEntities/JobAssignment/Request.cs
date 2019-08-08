using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.JobAssignment
{
    public class Request
    {
        [XmlElement("JobAssignment")]
        public JobAssignment JobAssign { get; set; }


        [XmlAttribute("Action")]
        public string Action { get; set; }
    }


    public class JobAssignment
    {
        //[XmlAttribute]
        //public string HasPersonalOvertimeFlag { get; set; }

        //[XmlAttribute]
        //public string UseMultipleAssignmentsFlag { get; set; }
        [XmlElement("Identity")]
        public Identity Ident { get; set; }
    }

    //public class BaseWageRates
    //{

    //}

    //public class BaseWageRate
    //{

    //}

    public class Identity
    {
        [XmlElement("PersonIdentity")]
        public PersonIdentity PersonIdentit { get; set; }
    }
    public class PersonIdentity
    {
        [XmlAttribute]

        public string PersonNumber { get; set; }
    }
}

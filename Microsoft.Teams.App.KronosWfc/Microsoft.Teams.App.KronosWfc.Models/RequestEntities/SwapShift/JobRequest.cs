using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.SwapShift.JobRequest
{
    [Serializable]
  public  class Request
    {
        [XmlAttribute]
        public string Action { get; set; }

        [XmlElement("SwapShiftJobs")]
        public SwapShiftJobs SwapShiftJob { get; set; }
    }
    public class SwapShiftJobs
    {
        [XmlAttribute]
        public string QueryDate { get; set; }

            [XmlAttribute]
        public string StartTime { get; set; }

        [XmlAttribute]
        public string EndTime { get; set; }
        
        [XmlElement("Employee")]
        public Employee Emp { get; set; }

    }

    public class Employee
    {
        [XmlElement("PersonIdentity")]
        public PersonIdentity PersonId { get; set; }
    }

    public class PersonIdentity
    {
        [XmlAttribute]
        public string PersonNumber { get; set; }
    }
}

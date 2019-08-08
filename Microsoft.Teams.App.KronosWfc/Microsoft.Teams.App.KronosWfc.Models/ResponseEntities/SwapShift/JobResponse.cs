using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SwapShift.JobResponse
{
    public class Response
    {

        [XmlAttribute]
        public string Action { get; set; }


        [XmlAttribute]
        public string Status { get; set; }




        [XmlElement("OrgJob")]
        public OrgJob[] OrgJobPath { get; set; }
    }

    //public  class OrgJobs
    //{
    //}
    public class OrgJob
    {
        [XmlAttribute]
        public string FullPath { get; set; }
    }
}

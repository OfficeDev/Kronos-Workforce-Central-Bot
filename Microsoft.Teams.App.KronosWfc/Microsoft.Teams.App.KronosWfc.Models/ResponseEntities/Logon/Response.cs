using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Logon
{
    [Serializable]
    public class Response
    {
        [XmlAttribute]
        public string Status { get; set; }

        [XmlAttribute]
        public string Timeout { get; set; }

        [XmlAttribute]
        public string Message { get; set; }

        [XmlAttribute]
        public string Action { get; set; }
       

        [XmlAttribute]
        public string ErrorCode { get; set;  }
        

        [XmlAttribute]
        public string Username { get; set; }
      

        [XmlAttribute]
        public string Object { get; set; }

        [XmlAttribute]
        public string PersonNumber { get; set; }
        
        [XmlIgnore]
        public string Jsession { get; set; }
    }
}

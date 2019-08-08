using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Logon
{
    [Serializable]
    public class Request
    {
        [XmlAttribute]
        public string Object { get; set; }

        [XmlAttribute]
        public string Action { get; set; }

        [XmlAttribute]
        public string Username { get; set; }
        
        [XmlAttribute]
        public string Password { get; set; }
       
    }

}

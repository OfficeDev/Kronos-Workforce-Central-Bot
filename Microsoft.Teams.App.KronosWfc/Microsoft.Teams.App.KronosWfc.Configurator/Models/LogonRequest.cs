using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Configurator.Models
{
    [Serializable]
    public class LogonRequest
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

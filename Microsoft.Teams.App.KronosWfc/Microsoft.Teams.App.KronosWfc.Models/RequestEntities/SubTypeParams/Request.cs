using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.SubTypeParams
{
    [XmlRoot]
    public class Request
    {
        [XmlAttribute]
        public string Action { get; set; }
        [XmlElement]
        public RequestSubtype RequestSubtype { get; set; }
    }

    public class RequestSubtype
    {

    }
}

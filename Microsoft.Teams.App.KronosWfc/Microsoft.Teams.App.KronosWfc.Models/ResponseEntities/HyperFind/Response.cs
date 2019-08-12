using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.HyperFind
{
    public class Response
    {
        [XmlElement]
        public List<ResponseHyperFindResult> HyperFindResult { get; set; }

        [XmlAttribute]
        public string Status { get; set; }

        [XmlAttribute]
        public string Action { get; set; }

        public Error Error { get; set; }
        
    }

    public class ResponseHyperFindResult
    {
        [XmlAttribute]
        public string FullName { get; set; }

        [XmlAttribute]
        public string PersonNumber { get; set; }
        
    }
}

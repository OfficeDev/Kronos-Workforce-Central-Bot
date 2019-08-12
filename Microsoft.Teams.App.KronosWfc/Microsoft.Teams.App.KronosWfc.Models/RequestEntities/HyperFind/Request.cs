using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.HyperFind
{
    public  class Request
    {
        public RequestHyperFindQuery HyperFindQuery { get; set; }
        
        [XmlAttribute]
        public string Action { get; set; }
    }

    public class RequestHyperFindQuery
    {
        [XmlAttribute]
        public string HyperFindQueryName { get; set; }

        [XmlAttribute]
        public string VisibilityCode { get; set; }
        
        [XmlAttribute]
        public string QueryDateSpan { get; set; }
    }

}

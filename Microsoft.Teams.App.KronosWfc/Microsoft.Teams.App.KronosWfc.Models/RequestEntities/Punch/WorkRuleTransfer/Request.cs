namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Punch.WorkRuleTransfer
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Request")]
    public class Request
    {
        [XmlElement(ElementName = "WorkRule")]
        public string WorkRule { get; set; }
        [XmlAttribute(AttributeName = "Action")]
        public string Action { get; set; }
    }
}

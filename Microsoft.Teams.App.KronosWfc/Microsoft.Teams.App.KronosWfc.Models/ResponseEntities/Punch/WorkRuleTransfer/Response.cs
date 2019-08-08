namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.WorkRuleTransfer
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "WorkRule")]
    public class WorkRule
    {
        [XmlAttribute(AttributeName = "ManagerHasAccessFlag")]
        public string ManagerHasAccessFlag { get; set; }
        [XmlAttribute(AttributeName = "ProfessionalHasAccessFlag")]
        public string ProfessionalHasAccessFlag { get; set; }
        [XmlAttribute(AttributeName = "WorkRuleName")]
        public string WorkRuleName { get; set; }
    }

    [XmlRoot(ElementName = "Response")]
    public class Response
    {
        [XmlElement(ElementName = "WorkRule")]
        public List<WorkRule> WorkRule { get; set; }
        [XmlAttribute(AttributeName = "Status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "Action")]
        public string Action { get; set; }
        public Error Error { get; set; }
    }
}

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.PresentEmployees
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Request")]
    public class Request
    {
        [XmlElement(ElementName = "PersonInformation")]
        public PersonInformation PersonInformation { get; set; }
        [XmlAttribute(AttributeName = "Action")]
        public string Action { get; set; }
    }

    [XmlRoot(ElementName = "PersonIdentity")]
    public class PersonIdentity
    {
        [XmlAttribute(AttributeName = "PersonNumber")]
        public string PersonNumber { get; set; }
    }

    [XmlRoot(ElementName = "Identity")]
    public class Identity
    {
        [XmlElement(ElementName = "PersonIdentity")]
        public PersonIdentity PersonIdentity { get; set; }
    }

    [XmlRoot(ElementName = "PersonInformation")]
    public class PersonInformation
    {
        [XmlElement(ElementName = "Identity")]
        public Identity Identity { get; set; }
    }
}

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.AddPunch
{
    using System;
    using System.Xml.Serialization;

    [Serializable]
    [XmlRoot]
    public class Response
    {
        [XmlAttribute]
        public string Status { get; set; }

        [XmlAttribute]
        public string Action { get; set; }

        public Error Error { get; set; }
    }
}

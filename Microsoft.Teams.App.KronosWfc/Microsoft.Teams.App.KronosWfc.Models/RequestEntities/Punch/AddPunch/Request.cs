namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Punch.AddPunch
{
    using System;
    using System.Xml.Serialization;

    [Serializable]
    public class Request
    {
        public Punch Punch { get; set; }

        [XmlAttribute]
        public string Action { get; set; }
    }

    public class Punch
    {
        public Employee Employee { get; set; }

        [XmlAttribute]
        public string Date { get; set; }

        [XmlAttribute]
        public string Time { get; set; }

        [XmlAttribute]
        public string KronosTimeZone { get; set; }

        [XmlAttribute]
        public string WorkRuleName { get; set; }
    }

    public class Employee
    {
        public PersonIdentity PersonIdentity { get; set; }
    }

    public class PersonIdentity
    {
        [XmlAttribute]
        public string PersonNumber { get; set; }
    }
}

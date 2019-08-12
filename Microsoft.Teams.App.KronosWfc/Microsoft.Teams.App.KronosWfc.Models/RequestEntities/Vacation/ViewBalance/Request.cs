namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Vacation.ViewBalance
{
    using System;
    using System.Xml.Serialization;

    [Serializable]
    public class Request
    {
        public AccrualData AccrualData { get; set; }

        [XmlAttribute]
        public string Action { get; set; }
    }

    public class AccrualData
    {
        public Employee Employee { get; set; }

        [XmlAttribute]
        public string BalanceDate { get; set; }
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

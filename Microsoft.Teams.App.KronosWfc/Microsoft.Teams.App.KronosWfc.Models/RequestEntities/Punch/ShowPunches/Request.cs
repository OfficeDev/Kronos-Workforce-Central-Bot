namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Punch.ShowPunches
{
    using System;
    using System.Xml.Serialization;

    [Serializable]
    [XmlRoot(ElementName = "Request")]
    public class Request
    {
        [XmlElement(ElementName = "Timesheet")]
        public Timesheet Timesheet { get; set; }
        [XmlAttribute(AttributeName = "Action")]
        public string Action { get; set; }
    }

    [XmlRoot(ElementName = "PersonIdentity")]
    public class PersonIdentity
    {
        [XmlAttribute(AttributeName = "PersonNumber")]
        public string PersonNumber { get; set; }
    }

    [XmlRoot(ElementName = "Employee")]
    public class Employee
    {
        [XmlElement(ElementName = "PersonIdentity")]
        public PersonIdentity PersonIdentity { get; set; }
    }

    [XmlRoot(ElementName = "TimeFramePeriod")]
    public class TimeFramePeriod
    {
        [XmlAttribute(AttributeName = "PeriodDateSpan")]
        public string PeriodDateSpan { get; set; }
    }

    [XmlRoot(ElementName = "Period")]
    public class Period
    {
        [XmlElement(ElementName = "TimeFramePeriod")]
        public TimeFramePeriod TimeFramePeriod { get; set; }
    }

    [XmlRoot(ElementName = "Timesheet")]
    public class Timesheet
    {
        [XmlElement(ElementName = "Employee")]
        public Employee Employee { get; set; }
        [XmlElement(ElementName = "Period")]
        public Period Period { get; set; }
    }
}

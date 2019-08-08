namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.PresentEmployees
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Response")]
    public class Response
    {
        public List<Employee> Employees { get; set; }
        public string Role { get; set; }
        [XmlElement(ElementName = "PersonInformation")]
        public PersonInformation PersonInformation { get; set; }
        [XmlAttribute(AttributeName = "Status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "Action")]
        public string Action { get; set; }
        public Error Error { get; set; }
    }

    public class Employee
    {
        public string PersonNumber { get; set; }
    }

    [XmlRoot(ElementName = "PersonInformation")]
    public class PersonInformation
    {
        [XmlElement(ElementName = "PersonData")]
        public PersonData PersonData { get; set; }
        [XmlElement(ElementName = "SupervisorData")]
        public SupervisorData SupervisorData { get; set; }
    }

    [XmlRoot(ElementName = "PersonData")]
    public class PersonData
    {
        [XmlElement(ElementName = "Person")]
        public Person Person { get; set; }
    }

    [XmlRoot(ElementName = "Person")]
    public class Person
    {
        [XmlAttribute(AttributeName = "FirstName")]
        public string FirstName { get; set; }
        [XmlAttribute(AttributeName = "FullName")]
        public string FullName { get; set; }
        [XmlAttribute(AttributeName = "HireDate")]
        public string HireDate { get; set; }
        [XmlAttribute(AttributeName = "LastName")]
        public string LastName { get; set; }
        [XmlAttribute(AttributeName = "PersonNumber")]
        public string PersonNumber { get; set; }
        [XmlAttribute(AttributeName = "ShortName")]
        public string ShortName { get; set; }
        [XmlAttribute(AttributeName = "FullTimePercentage")]
        public string FullTimePercentage { get; set; }
        [XmlAttribute(AttributeName = "AccrualProfileName")]
        public string AccrualProfileName { get; set; }
        [XmlAttribute(AttributeName = "ManagerSignoffThruDateTime")]
        public string ManagerSignoffThruDateTime { get; set; }
        [XmlAttribute(AttributeName = "PayrollLockoutThruDateTime")]
        public string PayrollLockoutThruDateTime { get; set; }
        [XmlAttribute(AttributeName = "FingerRequiredFlag")]
        public string FingerRequiredFlag { get; set; }
        [XmlAttribute(AttributeName = "BaseWageHourly")]
        public string BaseWageHourly { get; set; }
        [XmlAttribute(AttributeName = "HasKmailNotificationDelivery")]
        public string HasKmailNotificationDelivery { get; set; }
    }

    [XmlRoot(ElementName = "SupervisorData")]
    public class SupervisorData
    {
        [XmlElement(ElementName = "Supervisor")]
        public Supervisor Supervisor { get; set; }
    }

    [XmlRoot(ElementName = "Supervisor")]
    public class Supervisor
    {
        [XmlAttribute(AttributeName = "FullName")]
        public string FullName { get; set; }
        [XmlAttribute(AttributeName = "PersonNumber")]
        public string PersonNumber { get; set; }
    }
}

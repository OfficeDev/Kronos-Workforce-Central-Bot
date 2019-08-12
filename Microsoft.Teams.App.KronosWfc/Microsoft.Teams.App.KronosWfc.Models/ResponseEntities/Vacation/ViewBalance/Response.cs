namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Vacation.ViewBalance
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "AccrualBalanceSummary")]
    public class AccrualBalanceSummary
    {
        [XmlAttribute(AttributeName = "AccrualCodeId")]
        public string AccrualCodeId { get; set; }
        [XmlAttribute(AttributeName = "AccrualCodeName")]
        public string AccrualCodeName { get; set; }
        [XmlAttribute(AttributeName = "AccrualType")]
        public string AccrualType { get; set; }
        [XmlAttribute(AttributeName = "EncumberedBalanceInTime")]
        public string EncumberedBalanceInTime { get; set; }
        [XmlAttribute(AttributeName = "HoursPerDay")]
        public string HoursPerDay { get; set; }
        [XmlAttribute(AttributeName = "ProjectedVestedBalanceInTime")]
        public string ProjectedVestedBalanceInTime { get; set; }
        [XmlAttribute(AttributeName = "ProjectedDate")]
        public string ProjectedDate { get; set; }
        [XmlAttribute(AttributeName = "ProjectedGrantAmountInTime")]
        public string ProjectedGrantAmountInTime { get; set; }
        [XmlAttribute(AttributeName = "ProjectedTakingAmountInTime")]
        public string ProjectedTakingAmountInTime { get; set; }
        [XmlAttribute(AttributeName = "VestedBalanceInTime")]
        public string VestedBalanceInTime { get; set; }
        [XmlAttribute(AttributeName = "ProbationaryBalanceInTime")]
        public string ProbationaryBalanceInTime { get; set; }
    }

    [XmlRoot(ElementName = "AccrualBalances")]
    public class AccrualBalances
    {
        [XmlElement(ElementName = "AccrualBalanceSummary")]
        public List<AccrualBalanceSummary> AccrualBalanceSummary { get; set; }
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

    [XmlRoot(ElementName = "AccrualData")]
    public class AccrualData
    {
        [XmlElement(ElementName = "AccrualBalances")]
        public AccrualBalances AccrualBalances { get; set; }
        [XmlElement(ElementName = "Employee")]
        public Employee Employee { get; set; }
        [XmlAttribute(AttributeName = "BalanceDate")]
        public string BalanceDate { get; set; }
    }

    [XmlRoot(ElementName = "Response")]
    public class Response
    {
        [XmlElement(ElementName = "AccrualData")]
        public AccrualData AccrualData { get; set; }
        [XmlAttribute(AttributeName = "Status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "Action")]
        public string Action { get; set; }
        public Error Error { get; set; }
    }
}

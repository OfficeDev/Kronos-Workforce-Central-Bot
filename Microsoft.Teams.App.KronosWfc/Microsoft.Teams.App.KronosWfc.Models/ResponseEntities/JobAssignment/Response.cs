using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.JobAssignment
{
    [XmlRoot]
    public class Response
    {
        [XmlElement("JobAssignment")]
        public JobAssignment JobAssign { get; set; }


        [XmlAttribute]
        public string Status { get; set; }


        [XmlAttribute]
        public string Action { get; set; }

        public Error Error { get; set; }
    }

    public class JobAssignment
    {
        [XmlElement("PrimaryLaborAccounts")]
        public PrimaryLaborAccounts PrimaryLaborAccList { get; set; }

        [XmlElement("BaseWageRates")]

        public BaseWageRates BaseWageRats { get; set; }

        [XmlElement("Period")]

        public Period Perd { get; set; }

        [XmlElement("JobAssignmentDetailsData")]
        public JobAssignmentDetailsData jobAssignDetData { get; set; }

    }

    public class JobAssignmentDetailsData
    {
        [XmlElement("JobAssignmentDetails")]

        public JobAssignmentDetails JobAssignDet { get; set; }
    }

    public class JobAssignmentDetails
    {
        [XmlAttribute]
        public string PayRuleName { get; set; }

        [XmlAttribute]

        public string SupervisorPersonNumber { get; set; }

        [XmlAttribute]

        public string SupervisorName { get; set; }

        [XmlAttribute]

        public string TimeZoneName { get; set; }

        [XmlAttribute]

        public string BaseWageHourly { get; set; }
    }


    public class Period
    {
        [XmlElement("TimeFramePeriod")]
        public TimeFramePeriod TimeFramePerd { get; set; }
    }

    public class TimeFramePeriod
    {
        [XmlAttribute]
        public string PeriodDateSpan { get; set; }

        [XmlAttribute]
        public string TimeFrameName { get; set; }
    }

    public class PrimaryLaborAccounts
    {
        [XmlElement("PrimaryLaborAccount")]
        public PrimaryLaborAccount PrimaryLaborAcc { get; set; }

    }

    public class PrimaryLaborAccount
    {
        [XmlAttribute]
        public string EffectiveDate { get; set; }

        [XmlAttribute]
        public string ExpirationDate { get; set; }

        [XmlAttribute]
        public string OrganizationPath { get; set; }

        [XmlAttribute]
        public string LaborAccountName { get; set; }


    }

    public class BaseWageRates
    {
        [XmlElement("BaseWageRate")]
        public BaseWageRate[] BaseWageRt { get; set; }
    }

    public class BaseWageRate
    {
        [XmlAttribute]
        public string HourlyRate { get; set; }
        [XmlAttribute]
        public string EffectiveDate { get; set; }

        [XmlAttribute]
        public string ExpirationDate { get; set; }
    }
}

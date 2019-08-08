namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Punch.ShowPunches
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "DateTotals")]
    public class DateTotals
    {
        [XmlAttribute(AttributeName = "Date")]
        public string Date { get; set; }
        [XmlAttribute(AttributeName = "GrandTotal")]
        public string GrandTotal { get; set; }
        [XmlElement(ElementName = "Totals")]
        public Totals Totals { get; set; }
    }

    [XmlRoot(ElementName = "Total")]
    public class Total
    {
        [XmlAttribute(AttributeName = "IsCurrencyFlag")]
        public string IsCurrencyFlag { get; set; }
        [XmlAttribute(AttributeName = "LaborAccountDescription")]
        public string LaborAccountDescription { get; set; }
        [XmlAttribute(AttributeName = "LaborAccountId")]
        public string LaborAccountId { get; set; }
        [XmlAttribute(AttributeName = "LaborAccountName")]
        public string LaborAccountName { get; set; }
        [XmlAttribute(AttributeName = "AmountInCurrency")]
        public string AmountInCurrency { get; set; }
        [XmlAttribute(AttributeName = "PayCodeId")]
        public string PayCodeId { get; set; }
        [XmlAttribute(AttributeName = "PayCodeName")]
        public string PayCodeName { get; set; }
        [XmlAttribute(AttributeName = "AmountInTime")]
        public string AmountInTime { get; set; }
        [XmlAttribute(AttributeName = "AmountInDays")]
        public string AmountInDays { get; set; }
        [XmlAttribute(AttributeName = "OrgJobId")]
        public string OrgJobId { get; set; }
        [XmlAttribute(AttributeName = "OrgJobName")]
        public string OrgJobName { get; set; }
        [XmlAttribute(AttributeName = "OrgJobDescription")]
        public string OrgJobDescription { get; set; }
    }

    [XmlRoot(ElementName = "Totals")]
    public class Totals
    {
        [XmlElement(ElementName = "Total")]
        public List<Total> Total { get; set; }
    }

    [XmlRoot(ElementName = "DailyTotals")]
    public class DailyTotals
    {
        [XmlElement(ElementName = "DateTotals")]
        public List<DateTotals> DateTotals { get; set; }
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

    [XmlRoot(ElementName = "PeriodTotals")]
    public class PeriodTotals
    {
        [XmlElement(ElementName = "Totals")]
        public Totals Totals { get; set; }
        [XmlAttribute(AttributeName = "PeriodDateSpan")]
        public string PeriodDateSpan { get; set; }
    }

    [XmlRoot(ElementName = "PeriodTotalData")]
    public class PeriodTotalData
    {
        [XmlElement(ElementName = "PeriodTotals")]
        public PeriodTotals PeriodTotals { get; set; }
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

    [XmlRoot(ElementName = "TotaledScheduledPayCodeEditUneditable")]
    public class TotaledScheduledPayCodeEditUneditable
    {
        [XmlElement(ElementName = "Employee")]
        public Employee Employee { get; set; }
        [XmlAttribute(AttributeName = "Date")]
        public string Date { get; set; }
        [XmlAttribute(AttributeName = "EnteredOnDate")]
        public string EnteredOnDate { get; set; }
        [XmlAttribute(AttributeName = "EnteredOnTime")]
        public string EnteredOnTime { get; set; }
        [XmlAttribute(AttributeName = "AmountInTimeOrCurrency")]
        public string AmountInTimeOrCurrency { get; set; }
        [XmlAttribute(AttributeName = "IsCurrencyFlag")]
        public string IsCurrencyFlag { get; set; }
        [XmlAttribute(AttributeName = "PayCodeName")]
        public string PayCodeName { get; set; }
        [XmlAttribute(AttributeName = "TerminalPayCodeNum")]
        public string TerminalPayCodeNum { get; set; }
        [XmlAttribute(AttributeName = "StartTime")]
        public string StartTime { get; set; }
        [XmlAttribute(AttributeName = "IsTransferTotalizerGenerated")]
        public string IsTransferTotalizerGenerated { get; set; }
    }

    [XmlRoot(ElementName = "TotaledPayCodeEdits")]
    public class TotaledPayCodeEdits
    {
        [XmlElement(ElementName = "TotaledScheduledPayCodeEditUneditable")]
        public TotaledScheduledPayCodeEditUneditable TotaledScheduledPayCodeEditUneditable { get; set; }
    }

    [XmlRoot(ElementName = "TimekeepingException")]
    public class TimekeepingException
    {
        [XmlAttribute(AttributeName = "InPunchFlag")]
        public string InPunchFlag { get; set; }
        [XmlAttribute(AttributeName = "ExceptionTypeName")]
        public string ExceptionTypeName { get; set; }
        [XmlAttribute(AttributeName = "MissedOutDateTime")]
        public string MissedOutDateTime { get; set; }
        [XmlAttribute(AttributeName = "DifferenceToLimit")]
        public string DifferenceToLimit { get; set; }
        [XmlAttribute(AttributeName = "DurationOfException")]
        public string DurationOfException { get; set; }
    }

    [XmlRoot(ElementName = "Exceptions")]
    public class Exceptions
    {
        [XmlElement(ElementName = "TimekeepingException")]
        public List<TimekeepingException> TimekeepingException { get; set; }
    }

    [XmlRoot(ElementName = "Punch")]
    public class Punch
    {
        [XmlElement(ElementName = "Employee")]
        public Employee Employee { get; set; }
        [XmlAttribute(AttributeName = "Date")]
        public string Date { get; set; }
        [XmlAttribute(AttributeName = "DaylightSavingsFlag")]
        public string DaylightSavingsFlag { get; set; }
        [XmlAttribute(AttributeName = "Time")]
        public string Time { get; set; }
        [XmlAttribute(AttributeName = "KronosTimeZone")]
        public string KronosTimeZone { get; set; }
        [XmlAttribute(AttributeName = "EnteredOnDate")]
        public string EnteredOnDate { get; set; }
        [XmlAttribute(AttributeName = "EnteredOnTime")]
        public string EnteredOnTime { get; set; }
        [XmlAttribute(AttributeName = "WorkRuleName")]
        public string WorkRuleName { get; set; }
        [XmlAttribute(AttributeName = "OrgJobName")]
        public string OrgJobName { get; set; }
        [XmlAttribute(AttributeName = "LaborAccountName")]
        public string LaborAccountName { get; set; }
    }

    [XmlRoot(ElementName = "InPunch")]
    public class InPunch
    {
        [XmlElement(ElementName = "Punch")]
        public Punch Punch { get; set; }
    }

    [XmlRoot(ElementName = "OutPunch")]
    public class OutPunch
    {
        [XmlElement(ElementName = "Punch")]
        public Punch Punch { get; set; }
    }

    [XmlRoot(ElementName = "TotaledSpan")]
    public class TotaledSpan
    {
        [XmlElement(ElementName = "Employee")]
        public Employee Employee { get; set; }
        [XmlElement(ElementName = "Exceptions")]
        public Exceptions Exceptions { get; set; }
        [XmlElement(ElementName = "InPunch")]
        public InPunch InPunch { get; set; }
        [XmlElement(ElementName = "OutPunch")]
        public OutPunch OutPunch { get; set; }
        [XmlAttribute(AttributeName = "Date")]
        public string Date { get; set; }
        [XmlAttribute(AttributeName = "LaborAccountName")]
        public string LaborAccountName { get; set; }
        [XmlAttribute(AttributeName = "OrgJobId")]
        public string OrgJobId { get; set; }
        [XmlAttribute(AttributeName = "OrgJobName")]
        public string OrgJobName { get; set; }
        [XmlAttribute(AttributeName = "LaborAccountId")]
        public string LaborAccountId { get; set; }
    }

    [XmlRoot(ElementName = "TotaledSpans")]
    public class TotaledSpans
    {
        [XmlElement(ElementName = "TotaledSpan")]
        public List<TotaledSpan> TotaledSpan { get; set; }
    }

    [XmlRoot(ElementName = "Timesheet")]
    public class Timesheet
    {
        [XmlElement(ElementName = "DailyTotals")]
        public DailyTotals DailyTotals { get; set; }
        [XmlElement(ElementName = "Employee")]
        public Employee Employee { get; set; }
        [XmlElement(ElementName = "PeriodTotalData")]
        public PeriodTotalData PeriodTotalData { get; set; }
        [XmlElement(ElementName = "Period")]
        public Period Period { get; set; }
        [XmlElement(ElementName = "TotaledPayCodeEdits")]
        public TotaledPayCodeEdits TotaledPayCodeEdits { get; set; }
        [XmlElement(ElementName = "TotaledSpans")]
        public TotaledSpans TotaledSpans { get; set; }
        [XmlElement(ElementName = "TotaledWorkedDurations")]
        public string TotaledWorkedDurations { get; set; }
        [XmlAttribute(AttributeName = "LastTotalizationDateTime")]
        public string LastTotalizationDateTime { get; set; }
        [XmlAttribute(AttributeName = "ManagerSignoffDateTime")]
        public string ManagerSignoffDateTime { get; set; }
        [XmlAttribute(AttributeName = "TotalsUpToDateFlag")]
        public string TotalsUpToDateFlag { get; set; }
    }

    [XmlRoot(ElementName = "Response")]
    public class Response
    {
        [XmlElement(ElementName = "Timesheet")]
        public Timesheet Timesheet { get; set; }
        [XmlAttribute(AttributeName = "Status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "Action")]
        public string Action { get; set; }
        public Error Error { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.TimeOffRequests
{
    [XmlRoot(ElementName = "HolidayRequestSetting")]
    public class HolidayRequestSetting
    {
        [XmlAttribute(AttributeName = "GeneratePayCodeWithSchedule")]
        public string GeneratePayCodeWithSchedule { get; set; }
        [XmlAttribute(AttributeName = "GeneratePayCodeWithOutSchedule")]
        public string GeneratePayCodeWithOutSchedule { get; set; }
    }

    [XmlRoot(ElementName = "HolidayRequestSettings")]
    public class HolidayRequestSettings
    {
        [XmlElement(ElementName = "HolidayRequestSetting")]
        public HolidayRequestSetting HolidayRequestSetting { get; set; }
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

    [XmlRoot(ElementName = "CreatedByUser")]
    public class CreatedByUser
    {
        [XmlElement(ElementName = "PersonIdentity")]
        public PersonIdentity PersonIdentity { get; set; }
    }

    [XmlRoot(ElementName = "DataSource")]
    public class DataSource
    {
        [XmlAttribute(AttributeName = "ClientName")]
        public string ClientName { get; set; }
        [XmlAttribute(AttributeName = "FunctionalAreaCode")]
        public string FunctionalAreaCode { get; set; }
        [XmlAttribute(AttributeName = "ServerName")]
        public string ServerName { get; set; }
        [XmlAttribute(AttributeName = "UserName")]
        public string UserName { get; set; }
        [XmlAttribute(AttributeName = "FunctionalAreaName")]
        public string FunctionalAreaName { get; set; }
        [XmlElement(ElementName = "DataSource")]
        public DataSource DataSrc { get; set; }
    }

    [XmlRoot(ElementName = "Note")]
    public class Note
    {
        [XmlElement(ElementName = "DataSource")]
        public DataSource DataSource { get; set; }
        [XmlAttribute(AttributeName = "Timestamp")]
        public string Timestamp { get; set; }
        [XmlAttribute(AttributeName = "Text")]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Notes")]
    public class Notes
    {
        [XmlElement(ElementName = "Note")]
        public Note Note { get; set; }
    }

    [XmlRoot(ElementName = "Comment")]
    public class Comment
    {
        [XmlElement(ElementName = "Notes")]
        public Notes Notes { get; set; }
        [XmlAttribute(AttributeName = "CommentText")]
        public string CommentText { get; set; }
    }

    [XmlRoot(ElementName = "Comments")]
    public class Comments
    {
        [XmlElement(ElementName = "Comment")]
        public List<Comment> Comment { get; set; }
    }

    [XmlRoot(ElementName = "TimeOffPeriod")]
    public class TimeOffPeriod
    {
        [XmlAttribute(AttributeName = "StartDate")]
        public string StartDate { get; set; }
        [XmlAttribute(AttributeName = "Duration")]
        public string Duration { get; set; }
        [XmlAttribute(AttributeName = "EndDate")]
        public string EndDate { get; set; }
        [XmlAttribute(AttributeName = "PayCodeName")]
        public string PayCodeName { get; set; }
        [XmlAttribute(AttributeName = "Length")]
        public string Length { get; set; }
        [XmlAttribute(AttributeName = "StartTime")]
        public string StartTime { get; set; }

        public DateTime Sdt { get; set; }
    }

    [XmlRoot(ElementName = "TimeOffPeriods")]
    public class TimeOffPeriods
    {
        [XmlElement(ElementName = "TimeOffPeriod")]
        public TimeOffPeriod TimeOffPeriod { get; set; }
    }

    [XmlRoot(ElementName = "User")]
    public class User
    {
        [XmlElement(ElementName = "PersonIdentity")]
        public PersonIdentity PersonIdentity { get; set; }
    }

    [XmlRoot(ElementName = "RequestStatusChange")]
    public class RequestStatusChange
    {
        [XmlElement(ElementName = "User")]
        public User User { get; set; }
        [XmlElement(ElementName = "Comments")]
        public string Comments { get; set; }
        [XmlAttribute(AttributeName = "ToStatusName")]
        public string ToStatusName { get; set; }
        [XmlAttribute(AttributeName = "ChangeDateTime")]
        public string ChangeDateTime { get; set; }
        [XmlAttribute(AttributeName = "FromStatusName")]
        public string FromStatusName { get; set; }
    }

    [XmlRoot(ElementName = "RequestStatusChanges")]
    public class RequestStatusChanges
    {
        [XmlElement(ElementName = "RequestStatusChange")]
        public List<RequestStatusChange> RequestStatusChange { get; set; }
    }

    [XmlRoot(ElementName = "GlobalTimeOffRequestItem")]
    public class GlobalTimeOffRequestItem
    {
        [XmlElement(ElementName = "HolidayRequestSettings")]
        public HolidayRequestSettings HolidayRequestSettings { get; set; }
        [XmlElement(ElementName = "Employee")]
        public Employee Employee { get; set; }
        [XmlElement(ElementName = "CreatedByUser")]
        public CreatedByUser CreatedByUser { get; set; }
        [XmlElement(ElementName = "Comments")]
        public Comments Comments { get; set; }
        [XmlElement(ElementName = "TimeOffPeriods")]
        public TimeOffPeriods TimeOffPeriods { get; set; }
        [XmlElement(ElementName = "RequestStatusChanges")]
        public RequestStatusChanges RequestStatusChanges { get; set; }
        [XmlAttribute(AttributeName = "CreationDateTime")]
        public string CreationDateTime { get; set; }
        [XmlAttribute(AttributeName = "StatusName")]
        public string StatusName { get; set; }
        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "RequestFor")]
        public string RequestFor { get; set; }
        [XmlElement(ElementName = "ApprovalTimeOffPeriods")]
        public ApprovalTimeOffPeriods ApprovalTimeOffPeriods { get; set; }
    }

    [XmlRoot(ElementName = "ApprovalTimeOffPeriods")]
    public class ApprovalTimeOffPeriods
    {
        [XmlElement(ElementName = "TimeOffPeriod")]
        public TimeOffPeriod TimeOffPeriod { get; set; }
    }

    [XmlRoot(ElementName = "RequestItems")]
    public class RequestItems
    {
        [XmlElement(ElementName = "GlobalTimeOffRequestItem")]
        public List<GlobalTimeOffRequestItem> GlobalTimeOffRequestItem { get; set; }
    }

    [XmlRoot(ElementName = "Employees")]
    public class Employees
    {
        [XmlElement(ElementName = "PersonIdentity")]
        public PersonIdentity PersonIdentity { get; set; }
    }

    [XmlRoot(ElementName = "RequestMgmt")]
    public class RequestMgmt
    {
        [XmlElement(ElementName = "RequestItems")]
        public RequestItems RequestItems { get; set; }
        [XmlElement(ElementName = "Employees")]
        public Employees Employees { get; set; }
        [XmlAttribute(AttributeName = "QueryDateSpan")]
        public string QueryDateSpan { get; set; }
    }

    [XmlRoot(ElementName = "Response")]
    public class Response
    {
        [XmlElement(ElementName = "RequestMgmt")]
        public RequestMgmt RequestMgmt { get; set; }
        [XmlAttribute(AttributeName = "Status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "Action")]
        public string Action { get; set; }

        public Error Error { get; set; }
    }
}

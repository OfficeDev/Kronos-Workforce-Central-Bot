using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.Schedule
{
    using System.Xml.Serialization;

    [Serializable]
    public class PersonIdentity
    {
       [XmlAttribute]
        public string PersonNumber { get; set; }
    }

    [XmlRoot]
    public class Response
    {
        [XmlElement]
        public Schedule Schedule { get; set; }

        [XmlAttribute]
        public string Status { get; set; }

        [XmlAttribute]
        public string Action { get; set; }

        [XmlIgnore]
        public string Jsession { get; set; }

        public Error Error{ get; set; }
    }

   
    public  class Schedule
    {

        //[XmlArray]
        public List<PersonIdentity> Employees { get; set; }
        [XmlElement]
        public ScheduleItems ScheduleItems { get; set; }

        [XmlAttribute]
        public string QueryDateSpan { get; set; }
       
    }

    public class ScheduleItems {
        [XmlElement("SchedulePayCodeEdit", typeof(SchedulePayCodeEdit))]
        [XmlElement("ScheduleShift", typeof(ScheduleShift))]
        public object[] Items { get; set; }
    }
    public  class ScheduleShift
    {
        //[XmlArray]
        public List<PersonIdentity> Employee { get; set; }
        
        public List<ShiftSegment> ShiftSegments { get; set; }
        
        [XmlAttribute]
        public string LockedFlag { get; set; }

        [XmlAttribute]
        public string StartDate { get; set; }

        [XmlAttribute]
        public string IsDeleted { get; set; }

    }

    public  class ShiftSegment
    {
        [XmlAttribute]
        public string SegmentTypeName { get; set; }

        [XmlAttribute]
        public string StartDate { get; set; }

        [XmlAttribute]
        public string StartTime { get; set; }

        [XmlAttribute]
        public string StartDayNumber { get; set; }

        [XmlAttribute]
        public string EndDate { get; set; }

        [XmlAttribute]
        public string EndTime { get; set; }

        [XmlAttribute]
        public string EndDayNumber { get; set; }
    }

    public class SchedulePayCodeEdit
    {
        public List<PersonIdentity> Employee { get; set; }

        [XmlAttribute]
        public bool LockedFlag { get; set; }

        [XmlAttribute]
        public string StartDate { get; set; }

        [XmlAttribute]
        public bool IsDeleted { get; set; }


        [XmlAttribute]
        public string AmountInTime { get; set; }

        [XmlAttribute]
        public string DisplayTime { get; set; }


        [XmlAttribute]
        public string OrgJobPath { get; set; }


        [XmlAttribute]
        public string PayCodeName { get; set; }

    }

}

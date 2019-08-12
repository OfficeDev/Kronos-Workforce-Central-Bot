using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models
{
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.CommentList
{
    [Serializable]
    public class Response
    {

        [XmlAttribute]
        public string Status { get; set; }


        [XmlAttribute]
        public string Action { get; set; }


        public Error Error { get; set; }

        [XmlElement("Comment")]
        public List<Comment> Comments { get; set; }
    }

    public class Comment {
        [XmlAttribute]
        public string CommentText { get; set; }
    }
}

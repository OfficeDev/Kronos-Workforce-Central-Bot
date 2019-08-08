using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.RequestEntities.CommentList
{
    [Serializable]
    public class Request
    {
        [XmlElement]
        public Comment Comment { get; set; }

        [XmlAttribute]
        public string Action { get; set; }

    }

    public class Comment
    {
        [XmlAttribute]
        public string CommentCategory { get; set; }        

    }

}

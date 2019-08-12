using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities
{
    [Serializable]
    public class Error
    {
        [XmlAttribute]
        public string Message { get; set; }

       [XmlAttribute]
        public string ErrorCode { get; set; }

       [XmlAttribute]
        public string AtIndex { get; set; }

        [XmlElement("DetailErrors")]
        public ErrorArr DetailErrors { get; set; }



    }

    public class ErrorArr
    {
        [XmlElement]
        public Error[] Error { get; set; }
    }


}

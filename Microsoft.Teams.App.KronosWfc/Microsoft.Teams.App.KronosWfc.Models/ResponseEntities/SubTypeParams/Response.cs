using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Teams.App.KronosWfc.Models.ResponseEntities.SubTypeParams
{
    //[XmlRoot]
    //public class Response
    //{
    //    [XmlAttribute]
    //    public string Status { get; set; }

    //    [XmlElement("RequestSubtype")]
    //    public ReqSubTypes RequestSubtype { get; set; }

    //}

    //public class ReqSubTypes
    //{
    //    //[XmlElement("RequestSubtype")]
    //    public RequestSubType[] RequestSubTypes { get; set; }

    //}

    //public class RequestSubType {
    //    [XmlAttribute]
    //    public string Description { get; set; }
    //    [XmlAttribute]
    //    public string RequestTypeName { get; set; }
    //    [XmlAttribute]
    //    public string Symbol { get; set; }
    //    [XmlAttribute]
    //    public string Name { get; set; }
    //    [XmlElement("RequestParamValues")]
    //    public ReqParamValues RequestParamValues { get; set; }
    //}

    //public class ReqParamValues {
    //    [XmlElement("RequestParamValue")]
    //    public RequestParamValue[] RequestParamValues { get; set; }
    //}

    //public class RequestParamValue {
    //    [XmlAttribute]
    //    public bool Value { get; set; }
    //    [XmlAttribute]
    //    public string Name { get; set; }
    //}


    [XmlRoot(ElementName = "RequestParamValue")]
    public class RequestParamValue
    {
        [XmlAttribute(AttributeName = "Value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "RequestParamValues")]
    public class RequestParamValues
    {
        [XmlElement(ElementName = "RequestParamValue")]
        public List<RequestParamValue> RequestParamValue { get; set; }
    }

    [XmlRoot(ElementName = "RequestSubtype")]
    public class RequestSubtype
    {
        [XmlElement(ElementName = "RequestParamValues")]
        public RequestParamValues RequestParamValues { get; set; }
        [XmlAttribute(AttributeName = "Description")]
        public string Description { get; set; }
        [XmlAttribute(AttributeName = "RequestTypeName")]
        public string RequestTypeName { get; set; }
        [XmlAttribute(AttributeName = "Symbol")]
        public string Symbol { get; set; }
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Response")]
    public class Response
    {
        [XmlElement(ElementName = "RequestSubtype")]
        public List<RequestSubtype> RequestSubtype { get; set; }
        [XmlAttribute(AttributeName = "Status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "Action")]
        public string Action { get; set; }
    }

}

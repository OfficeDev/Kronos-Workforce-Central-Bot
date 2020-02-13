//-----------------------------------------------------------------------
// <copyright file="XmlConvertHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Teams.App.KronosWfc.Configurator.Helpers
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// XML Convert helper class
    /// </summary>
    public static class XmlConvertHelper
    {
        public static string SerializeObject<T>(this T dataObject)
        {
            if (dataObject == null)
            {
                return string.Empty;
            }

            try
            {
                using (StringWriter stringWriter = new System.IO.StringWriter())
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(stringWriter, dataObject);
                    return stringWriter.ToString();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string XmlSerialize<T>(this T entity) where T : class
        {
            // removes version
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;

            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
            using (StringWriter sw = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sw, settings))
                {
                    // removes namespace
                    var xmlns = new XmlSerializerNamespaces();
                    xmlns.Add(string.Empty, string.Empty);

                    xsSubmit.Serialize(writer, entity, xmlns);
                    return sw.ToString(); // Your XML
                }
            }
        }

        public static T DeserializeObject<T>(this string xml) where T : new()
        {
            if (string.IsNullOrEmpty(xml))
            {
                return new T();
            }

            try
            {
                using (var stringReader = new StringReader(xml))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(stringReader);
                }
            }
            catch (Exception)
            {
                return new T();
            }
        }
    }
}

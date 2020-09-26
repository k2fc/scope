using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace scope
{
    /// <summary>
    /// XML serializer helper class. Serializes and deserializes objects from/to XML
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize/deserialize.
    /// Must have a parameterless constructor and implement <see cref="Serializable"/></typeparam>
    public class XmlSerializer<T> where T : class, new()
    {
        /// <summary>
        /// Deserializes a XML string into an object
        /// Default encoding: <c>UTF8</c>
        /// </summary>
        /// <param name="xml">The XML string to deserialize</param>
        /// <returns>An object of type <c>T</c></returns>
        public static T Deserialize(string xml)
        {
            return Deserialize(xml, Encoding.UTF8, null);
        }

        /// <summary>
        /// Deserializes a XML string into an object
        /// Default encoding: <c>UTF8</c>
        /// </summary>
        /// <param name="xml">The XML string to deserialize</param>
        /// <param name="encoding">The encoding</param>
        /// <returns>An object of type <c>T</c></returns>
        public static T Deserialize(string xml, Encoding encoding)
        {
            return Deserialize(xml, encoding, null);
        }

        /// <summary>
        /// Deserializes a XML string into an object
        /// </summary>
        /// <param name="xml">The XML string to deserialize</param>
        /// <param name="settings">XML serialization settings. <see cref="System.Xml.XmlReaderSettings"/></param>
        /// <returns>An object of type <c>T</c></returns>
        public static T Deserialize(string xml, XmlReaderSettings settings)
        {
            return Deserialize(xml, Encoding.UTF8, settings);
        }

        /// <summary>
        /// Deserializes a XML string into an object
        /// </summary>
        /// <param name="xml">The XML string to deserialize</param>
        /// <param name="encoding">The encoding</param>
        /// <param name="settings">XML serialization settings. <see cref="System.Xml.XmlReaderSettings"/></param>
        /// <returns>An object of type <c>T</c></returns>
        public static T Deserialize(string xml, Encoding encoding, XmlReaderSettings settings)
        {
            if (string.IsNullOrEmpty(xml))
                throw new ArgumentException("XML cannot be null or empty", "xml");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (MemoryStream memoryStream = new MemoryStream(encoding.GetBytes(xml)))
            {
                using (XmlReader xmlReader = XmlReader.Create(memoryStream, settings))
                {
                    return (T)xmlSerializer.Deserialize(xmlReader);
                }
            }
        }

        /// <summary>
        /// Deserializes a XML file.
        /// </summary>
        /// <param name="filename">The filename of the XML file to deserialize</param>
        /// <returns>An object of type <c>T</c></returns>
        public static T DeserializeFromFile(string filename)
        {
            return DeserializeFromFile(filename, new XmlReaderSettings());
        }

        /// <summary>
        /// Deserializes a XML file.
        /// </summary>
        /// <param name="filename">The filename of the XML file to deserialize</param>
        /// <param name="settings">XML serialization settings. <see cref="System.Xml.XmlReaderSettings"/></param>
        /// <returns>An object of type <c>T</c></returns>
        public static T DeserializeFromFile(string filename, XmlReaderSettings settings)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("filename", "XML filename cannot be null or empty");

            if (!File.Exists(filename))
                throw new FileNotFoundException("Cannot find XML file to deserialize", filename);

            // Create the stream writer with the specified encoding
            using (XmlReader reader = XmlReader.Create(filename, settings))
            {
                System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <returns>A XML string that represents the object to be serialized</returns>
        public static string Serialize(T source)
        {
            // indented XML by default
            return Serialize(source, null, GetIndentedSettings());
        }

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="namespaces">Namespaces to include in serialization</param>
        /// <returns>A XML string that represents the object to be serialized</returns>
        public static string Serialize(T source, XmlSerializerNamespaces namespaces)
        {
            // indented XML by default
            return Serialize(source, namespaces, GetIndentedSettings());
        }

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="settings">XML serialization settings. <see cref="System.Xml.XmlWriterSettings"/></param>
        /// <returns>A XML string that represents the object to be serialized</returns>
        public static string Serialize(T source, XmlWriterSettings settings)
        {
            return Serialize(source, null, settings);
        }

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="namespaces">Namespaces to include in serialization</param>
        /// <param name="settings">XML serialization settings. <see cref="System.Xml.XmlWriterSettings"/></param>
        /// <returns>A XML string that represents the object to be serialized</returns>
        public static string Serialize(T source, XmlSerializerNamespaces namespaces, XmlWriterSettings settings)
        {
            if (source == null)
                throw new ArgumentNullException("source", "Object to serialize cannot be null");

            string xml = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(source.GetType());

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, settings))
                    {
                        System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(T));
                        x.Serialize(xmlWriter, source, namespaces);
                    }

                    memoryStream.Position = 0; // rewind the stream before reading back.
                    using (StreamReader sr = new StreamReader(memoryStream))
                    {
                        xml = sr.ReadToEnd();
                    }

                }
            }
            catch { }
            return xml;
        }

        /// <summary>
        /// Serialize an object to a XML file
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="filename">The file to generate</param>
        public static void SerializeToFile(T source, string filename)
        {
            // indented XML by default
            SerializeToFile(source, filename, null, GetIndentedSettings());
        }

        /// <summary>
        /// Serialize an object to a XML file
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="filename">The file to generate</param>
        /// <param name="namespaces">Namespaces to include in serialization</param>
        public static void SerializeToFile(T source, string filename, XmlSerializerNamespaces namespaces)
        {
            // indented XML by default
            SerializeToFile(source, filename, namespaces, GetIndentedSettings());
        }

        /// <summary>
        /// Serialize an object to a XML file
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="filename">The file to generate</param>
        /// <param name="settings">XML serialization settings. <see cref="System.Xml.XmlWriterSettings"/></param>
        public static void SerializeToFile(T source, string filename, XmlWriterSettings settings)
        {
            SerializeToFile(source, filename, null, settings);
        }

        /// <summary>
        /// Serialize an object to a XML file
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="filename">The file to generate</param>
        /// <param name="namespaces">Namespaces to include in serialization</param>
        /// <param name="settings">XML serialization settings. <see cref="System.Xml.XmlWriterSettings"/></param>
        public static void SerializeToFile(T source, string filename, XmlSerializerNamespaces namespaces, XmlWriterSettings settings)
        {
            if (source == null)
                throw new ArgumentNullException("source", "Object to serialize cannot be null");

            XmlSerializer serializer = new XmlSerializer(source.GetType());

            using (XmlWriter xmlWriter = XmlWriter.Create(filename, settings))
            {
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(T));
                x.Serialize(xmlWriter, source, namespaces);
            }
        }

        #region Private methods


        private static XmlWriterSettings GetIndentedSettings()
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "\t";

            return xmlWriterSettings;
        }

        #endregion
    }
}

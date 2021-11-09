using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using DGScope.Receivers;

namespace DGScope
{
     public class ListOfIReceiver : List<Receiver>, IXmlSerializable
    {
        public ListOfIReceiver() : base() { }
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return;
            reader.ReadStartElement("Receivers");

            while (reader.IsStartElement("Receiver"))
            {
                Type type = Type.GetType(reader.GetAttribute("AssemblyQualifiedName"));
                XmlSerializer serial = new XmlSerializer(type);

                reader.ReadStartElement("Receiver");
                this.Add((Receiver)serial.Deserialize(reader));
                reader.ReadEndElement();
            }
            reader.ReadEndElement();

        }
        public void WriteXml(XmlWriter writer)
        {

            foreach (Receiver receiver in this)
            {
                writer.WriteStartElement("Receiver");
                writer.WriteAttributeString("AssemblyQualifiedName", receiver.GetType().AssemblyQualifiedName);
                XmlSerializer xmlSerializer = new XmlSerializer(receiver.GetType());
                xmlSerializer.Serialize(writer, receiver);
                writer.WriteEndElement();
            }

        }
    }

}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;


namespace LittleUmph
{
    public class Serializer
    {
        public static Stream SerializeStream(object source)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            formatter.Serialize(stream, source);
            return stream;
        }

        public static T DeserializeStream<T>(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
        }

        public static T Clone<T>(object source)
        {
            return DeserializeStream<T>(SerializeStream(source));
        }

        public static string SerializeXML(object obj)
        {
            XmlSerializer x = new XmlSerializer(obj.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                x.Serialize(ms, obj);
                return IOFunc.StreamToString(ms);
            }
        }

        public static T DeserializeXML<T>(string obj)
        {
            XmlSerializer x = new XmlSerializer(typeof(T));
            object result = x.Deserialize(IOFunc.StringToStream(obj));
            return (T)result;
        }

        public static XmlNode SerializeXMLNode(object obj)
        {
            string xml = SerializeXML(obj);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement;
        }

        public static T DeserializeXMLNode<T>(XmlNode obj)
        {
            object result = DeserializeXML<T>(obj.OuterXml);
            return (T)result;
        }

    }
}

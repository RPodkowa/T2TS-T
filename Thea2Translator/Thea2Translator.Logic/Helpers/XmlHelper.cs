using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class XmlHelper
    {
        public static XmlNode GetNode(XmlDocument doc, string name, string value)
        {
            XmlNode node = doc.CreateElement(name);
            node.AppendChild(doc.CreateTextNode(value));
            return node;
        }
        public static XmlNode GetNode(XmlDocument doc, string name, DateTime value)
        {
            return GetNode(doc, name, value.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public static XmlAttribute GetAttribute(XmlDocument doc, string name, string value)
        {
            XmlAttribute attribute = doc.CreateAttribute(name);
            attribute.Value = value;
            return attribute;
        }

        public static string GetNodeText(XmlNode element, string xpath)
        {
            var node = element.SelectSingleNode(xpath);
            if (node == null) return "";

            var ret = node.InnerText;
            return ret;
        }

        public static DateTime? GetNodeDate(XmlNode element, string xpath)
        {
            var timeString = GetNodeText(element, xpath);
            if (!DateTime.TryParse(timeString, out DateTime time))
                return null;

            return time;
        }

        public static string GetNodeAttribute(XmlNode xmlNode, string key)
        {
            if (xmlNode.Attributes == null)
                return null;
            
            return xmlNode.Attributes[key]?.Value;
        }

        public static int GetNodeAttribute(XmlNode xmlNode, string key, int dafaultValue)
        {
            if (xmlNode.Attributes == null)
                return dafaultValue;

            int result = dafaultValue;
            if (!int.TryParse(xmlNode.Attributes[key]?.Value, out result))
                return dafaultValue;

            return result;
        }

        public static XmlNodeList GetNodeList(XmlDocument doc, string xpath)
        {
            if (doc == null) return null;
            if (doc.DocumentElement == null) return null;
            if (doc.DocumentElement.ParentNode == null) return null;
            return doc.DocumentElement.ParentNode.SelectNodes(xpath);
        }

        public static XmlNodeList GetChildNodes(XmlNode element, string name)
        {
            if (element == null) return null;
            if (element.ChildNodes == null) return null;
            return element.SelectNodes(name);
        }

        public static XmlNode GetChildNode(XmlNode element, string name)
        {
            var childs = GetChildNodes(element, name);
            if (childs == null) return null;
            if (childs.Count==0) return null;
            return childs[0];
        }
    }
}

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

        public static string GetNodeAttribute(XmlNode xmlNode, string key)
        {
            if (xmlNode.Attributes == null)
                return null;
            
            return xmlNode.Attributes[key]?.Value;
        }

        public static XmlNodeList GetNodeList(XmlDocument doc, string xpath)
        {
            if (doc == null) return null;
            if (doc.DocumentElement == null) return null;
            if (doc.DocumentElement.ParentNode == null) return null;
            return doc.DocumentElement.ParentNode.SelectNodes(xpath);
        }
    }
}

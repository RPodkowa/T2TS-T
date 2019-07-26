using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class NavigationAdventureNodeElem
    {
        public int CacheElemId { get; private set; }        
        public string Group { get; private set; }

        public NavigationAdventureNodeElem(NavigationElem navigationElem)
        {
            CacheElemId = navigationElem.CacheElemId;
            Group = navigationElem.GetMyGroupName();
        }

        public NavigationAdventureNodeElem(XmlNode xmlNode)
        {
            CacheElemId = int.Parse(XmlHelper.GetNodeAttribute(xmlNode, "CacheElemID"));
            Group = XmlHelper.GetNodeAttribute(xmlNode, "Group");
        }

        public XmlNode ToXmlNode(XmlDocument doc)
        {
            XmlNode adventureNodeElem = doc.CreateElement("AdventureNodeElem");
            adventureNodeElem.Attributes.Append(XmlHelper.GetAttribute(doc, "CacheElemID", CacheElemId.ToString()));
            adventureNodeElem.Attributes.Append(XmlHelper.GetAttribute(doc, "Group", Group));
            return adventureNodeElem;
        }

        public static IList<NavigationAdventureNodeElem> GetNodesFromXml(XmlDocument doc)
        {
            var navigationAdventureNodeElems = new List<NavigationAdventureNodeElem>();
            var nodes = XmlHelper.GetNodeList(doc, "Navigation/AdventureNodeElems/AdventureNodeElem");
            foreach (XmlNode node in nodes)
            {
                navigationAdventureNodeElems.Add(new NavigationAdventureNodeElem(node));
            }

            return navigationAdventureNodeElems;
        }
    }
}

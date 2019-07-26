using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class NavigationStartGroup
    {
        public string Group { get; private set; }

        public NavigationStartGroup(NavigationElem navigationElem)
        {
            Group = navigationElem.GetTargetGroupName();
        }

        public NavigationStartGroup(XmlNode xmlNode)
        {
            Group = xmlNode.InnerText;
        }

        public XmlNode ToXmlNode(XmlDocument doc)
        {
            return XmlHelper.GetNode(doc, "StartGroup", Group);
        }

        public static IList<NavigationStartGroup> GetNodesFromXml(XmlDocument doc)
        {
            var navigationStartGroups = new List<NavigationStartGroup>();
            var nodes = XmlHelper.GetNodeList(doc, "Navigation/StartGroups/StartGroup");
            foreach (XmlNode node in nodes)
            {
                navigationStartGroups.Add(new NavigationStartGroup(node));
            }

            return navigationStartGroups;
        }
    }
}

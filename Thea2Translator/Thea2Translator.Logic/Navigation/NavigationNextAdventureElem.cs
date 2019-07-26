using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class NavigationNextAdventureElem
    {
        public IList<NavigationElemRelation> Relations { get; private set; }
        public int CacheElemId { get; private set; }
        public string Group { get; private set; }
        public string Target { get; private set; }

        public NavigationNextAdventureElem(NavigationElem navigationElem, IList<NavigationElem> navigationElems)
        {
            CacheElemId = navigationElem.CacheElemId;
            Group = navigationElem.GetMyGroupName();
            Target = navigationElem.TargetId;
            Relations = NavigationElemRelation.GetNextAdventureElemRelations(navigationElem, navigationElems).Where(x => x.IsAcceptableRealtionForMap()).ToList();
        }

        public NavigationNextAdventureElem(XmlNode xmlNode)
        {
            CacheElemId = int.Parse(XmlHelper.GetNodeAttribute(xmlNode, "CacheElemID"));
            Group = XmlHelper.GetNodeAttribute(xmlNode, "Group");
            Target = XmlHelper.GetNodeAttribute(xmlNode, "Target");

            Relations = new List<NavigationElemRelation>();
            var nextElems = xmlNode.SelectNodes("NextElems/NextElem");
            foreach (XmlNode nextElem in nextElems)
            {
                Relations.Add(new NavigationElemRelation(nextElem));
            }
        }

        public XmlNode ToXmlNode(XmlDocument doc)
        {
            if (Relations.Count == 0)
                return null;

            XmlNode nextAdventureElem = doc.CreateElement("NextAdventureElem");
            nextAdventureElem.Attributes.Append(XmlHelper.GetAttribute(doc, "CacheElemID", CacheElemId.ToString()));
            nextAdventureElem.Attributes.Append(XmlHelper.GetAttribute(doc, "Group", Group));
            nextAdventureElem.Attributes.Append(XmlHelper.GetAttribute(doc, "Target", Target));

            XmlNode nextElems = doc.CreateElement("NextElems");
            foreach (var relation in Relations)
            {
                XmlNode nextElem = XmlHelper.GetNode(doc, "NextElem", relation.NextElem.GetMyGroupName());
                nextElem.Attributes.Append(XmlHelper.GetAttribute(doc, "Steps", relation.GetRelationTypes()));
                nextElems.AppendChild(nextElem);
            }
            nextAdventureElem.AppendChild(nextElems);

            return nextAdventureElem;
        }

        public static IList<NavigationNextAdventureElem> GetNodesFromXml(XmlDocument doc)
        {
            var navigationNextAdventureElems = new List<NavigationNextAdventureElem>();
            var nodes = XmlHelper.GetNodeList(doc, "Navigation/NextAdventureElems/NextAdventureElem");
            foreach (XmlNode node in nodes)
            {
                navigationNextAdventureElems.Add(new NavigationNextAdventureElem(node));
            }

            return navigationNextAdventureElems;
        }
    }
}

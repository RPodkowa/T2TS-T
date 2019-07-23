using System;
using System.Collections.Generic;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class Navigation
    {
        public IList<NavigationElem> NavigationElems { get; private set; }
        private IList<string> StartGroups;
        private IList<string> AdventureNodeElems;
        private IList<string> NextAdventureElems;

        public void SaveElems()
        {
            StartGroups = new List<string>();
            AdventureNodeElems = new List<string>();
            NextAdventureElems = new List<string>();

            foreach (var elem in NavigationElems)
            {
                if (elem.IsNodeStartingElem())
                {
                    StartGroups.Add(elem.GetTargetGroupName());
                    continue;
                }

                if (elem.IsAdventureNodeElem())
                {
                    AdventureNodeElems.Add(elem.GetAdventureNodeElemUniqueId());
                    continue;
                }

                if (elem.IsAdventureOutputElem())
                {
                    var nextAdventures = elem.GetNextAdventures(NavigationElems);
                    if (!string.IsNullOrEmpty(nextAdventures))
                        NextAdventureElems.Add(nextAdventures);

                    continue;
                }
            }

            FileHelper.SaveElemsToFile(NavigationElems, @"D:\test.txt");
            FileHelper.SaveElemsToFile(StartGroups, AdventureNodeElems, NextAdventureElems, @"D:\test2.txt");
        }

        public void SetNodeElementId(int cacheElemId, NavigationElemAdventureInfo adventureInfo, string adventureNodeId)
        {
            foreach (var elem in NavigationElems)
            {
                if (elem.CacheElemId > 0) continue;
                if (!string.IsNullOrEmpty(elem.TargetId)) continue;
                if (!NavigationElemAdventureInfo.IsEquals(adventureInfo, elem.AdventureInfo)) continue;
                if (elem.OwnerId != adventureNodeId) continue;
                elem.SetCacheElemId(cacheElemId);
                break;
            }
        }

        public void SetOutputElementId(int cacheElemId, NavigationElemAdventureInfo adventureInfo, string adventureNodeId, string targetId)
        {
            foreach (var elem in NavigationElems)
            {
                if (elem.CacheElemId > 0) continue;
                if (string.IsNullOrEmpty(elem.TargetId)) continue;
                if (!NavigationElemAdventureInfo.IsEquals(adventureInfo, elem.AdventureInfo)) continue;
                if (elem.OwnerId != adventureNodeId) continue;
                if (elem.TargetId != targetId) continue;
                elem.SetCacheElemId(cacheElemId);
                break;
            }
        }

        public void AddDocument(XmlDocument document, string fileName)
        {
            if (NavigationElems == null) NavigationElems = new List<NavigationElem>();
                        
            var adventures = document.DocumentElement.GetElementsByTagName("Adventure");
            foreach (XmlNode adventure in adventures)
            {
                if (adventure.Attributes == null)
                    continue;

                var adventureName = adventure.Attributes["name"]?.Value;
                var adventureId = adventure.Attributes["uniqueID"]?.Value;

                var adventureInfo = new NavigationElemAdventureInfo(fileName, adventureName, adventureId);

                var nodes = adventure.SelectNodes("nodes");
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes == null)
                        throw new Exception("node.Attributes == null");

                    var xsi_type = node.Attributes["xsi:type"]?.Value;
                    if (string.IsNullOrEmpty(xsi_type))
                        throw new Exception("string.IsNullOrEmpty(xsi_type)");

                    NodeType nodeType = (NodeType)Enum.Parse(typeof(NodeType), xsi_type);

                    var adventureNodeId = node.Attributes["ID"]?.Value;

                    var nodeNavigationElem = new NavigationElem(adventureInfo, nodeType, adventureNodeId);
                    if (NavigationElem.NodeWithLinkedEventUniqueID(nodeType))
                        nodeNavigationElem.SetTargetAdventureId(node.Attributes["linkedEventUniqueID"]?.Value);

                    if (nodeNavigationElem.CanAddNodeElem())
                        NavigationElems.Add(nodeNavigationElem);

                    var outputs = node.SelectNodes("outputs");
                    foreach (XmlNode output in outputs)
                    {
                        if (output.Attributes == null)
                            throw new Exception("output.Attributes == null");

                        var outputNavigationElem = new NavigationElem(nodeNavigationElem, output.Attributes["targetID"]?.Value.ToString());
                        if (!outputNavigationElem.CheckOwnerId(output.Attributes["ownerID"]?.Value.ToString()))
                            throw new Exception("CheckOwnerId");

                        if (NavigationElem.NodeWithName(nodeType))
                            outputNavigationElem.SetName(output.Attributes["name"]?.Value);

                        NavigationElems.Add(outputNavigationElem);
                    }
                }
            }
        }
    }
}

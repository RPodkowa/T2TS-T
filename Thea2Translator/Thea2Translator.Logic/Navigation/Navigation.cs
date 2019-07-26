using System;
using System.Collections.Generic;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class Navigation
    {
        public IList<NavigationElem> NavigationElems { get; private set; }

        public IList<NavigationStartGroup> NavigationStartGroups { get; private set; }
        public IList<NavigationAdventureNodeElem> NavigationAdventureNodeElems { get; private set; }
        public IList<NavigationNextAdventureElem> NavigationNextAdventureElems { get; private set; }
                
        private readonly string FullPath;
        private IList<string> startGroups;

        public Navigation(DirectoryType directoryType = DirectoryType.Cache)
        {
            FullPath = FileHelper.GetLocalFilePatch(directoryType, FilesType.Navigation);            
        }

        public void Reload()
        {
            if (!FileHelper.FileExists(FullPath))
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(FullPath);
            //StartGroups              
            NavigationStartGroups = NavigationStartGroup.GetNodesFromXml(doc);
            NavigationAdventureNodeElems = NavigationAdventureNodeElem.GetNodesFromXml(doc);
            NavigationNextAdventureElems = NavigationNextAdventureElem.GetNodesFromXml(doc);
        }

        public void UpdateAdventureNodeElems(IDataCache dataCache)
        {
            var navigationAdventureNodeElemsCopy = new List<NavigationAdventureNodeElem>(NavigationAdventureNodeElems);
            foreach (var dataCacheElem in dataCache.CacheElems)
            {
                for (int i = navigationAdventureNodeElemsCopy.Count - 1; i > -1; i--)
                {
                    if (navigationAdventureNodeElemsCopy[i].CacheElemId == dataCacheElem.Id)
                    {
                        dataCacheElem.AddAdventureNodeGroup(navigationAdventureNodeElemsCopy[i].Group);
                        navigationAdventureNodeElemsCopy.RemoveAt(i);
                    }
                }
            }
        }

        public void UpdateNextAdventureElems(CacheElem cacheElem)
        {
            foreach (var navigationNextAdventureElem in NavigationNextAdventureElems)
            {
                if (navigationNextAdventureElem.CacheElemId == cacheElem.Id)
                    cacheElem.AddNavigationNextAdventureElem(navigationNextAdventureElem);
            }
        }

        public IList<string> GetStartingGroups()
        {
            if (startGroups != null) return startGroups;
            startGroups = new List<string>();
            foreach(var navigationStartGroup in NavigationStartGroups)
            {
                startGroups.Add(navigationStartGroup.Group);
            }

            return startGroups;
        }

        public void SaveElems()
        {
            FileHelper.CreatedPathIfNotExists(FullPath);
            FileHelper.DeleteFileIfExists(FullPath);

            XmlDocument doc = new XmlDocument();
            XmlNode navigationNode = doc.CreateElement("Navigation");
            doc.AppendChild(navigationNode);

            XmlNode startGroupsNode = doc.CreateElement("StartGroups");
            XmlNode adventureNodeElemsNode = doc.CreateElement("AdventureNodeElems");
            XmlNode nextAdventureElemsNode = doc.CreateElement("NextAdventureElems");

            foreach (var elem in NavigationElems)
            {
                if (elem.IsNodeStartingElem())
                {
                    startGroupsNode.AppendChild(new NavigationStartGroup(elem).ToXmlNode(doc));
                    continue;
                }

                if (elem.IsAdventureNodeElem())
                {
                    adventureNodeElemsNode.AppendChild(new NavigationAdventureNodeElem(elem).ToXmlNode(doc));
                    continue;
                }

                if (elem.IsAdventureOutputElem())
                {
                    var nextAdventures = new NavigationNextAdventureElem(elem, NavigationElems).ToXmlNode(doc);
                    if (nextAdventures!=null)
                        nextAdventureElemsNode.AppendChild(nextAdventures);

                    continue;
                }
            }

            navigationNode.AppendChild(startGroupsNode);
            navigationNode.AppendChild(adventureNodeElemsNode);
            navigationNode.AppendChild(nextAdventureElemsNode);
            doc.Save(FullPath);
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

using System.Collections.Generic;
using System.Linq;

namespace Thea2Translator.Logic
{
    public class NavigationElem
    {
        public NavigationElemAdventureInfo AdventureInfo { get; private set; }        
        public NodeType Type { get; private set; }
        public string Name { get; private set; }
        public string OwnerId { get; private set; }
        public string TargetId { get; private set; }
        public string LinkedEventUniqueId { get; private set; }
        public int CacheElemId { get; private set; }

        public NavigationElem(NavigationElemAdventureInfo adventureInfo, NodeType type, string ownerId)
        {
            AdventureInfo = adventureInfo;
            Type = type;
            OwnerId = ownerId;
        }

        public NavigationElem(NavigationElem nodeNavigationElem, string targetId)
        {
            AdventureInfo = nodeNavigationElem.AdventureInfo;
            Type = nodeNavigationElem.Type;
            OwnerId = nodeNavigationElem.OwnerId;
            TargetId = targetId;
        }

        public bool CheckOwnerId(string ownerId)
        {
            return (ownerId == OwnerId);
        }

        public void SetName(string name)
        {
            Name = name;
        }

        public void SetCacheElemId(int cacheElemId)
        {
            CacheElemId = cacheElemId;
        }

        public void SetTargetAdventureId(string linkedEventUniqueId)
        {
            if (string.IsNullOrEmpty(linkedEventUniqueId)) return;
            if (linkedEventUniqueId == "0") return;
            LinkedEventUniqueId = linkedEventUniqueId;
        }

        public string GetTargetGroupName()
        {
            return AdventureInfo.GetGroupName(TargetId);
        }

        public string GetMyGroupName()
        {
            return AdventureInfo.GetGroupName(OwnerId);
        }

        public string GetAdventureNodeElemUniqueId()
        {
            return $"{GetMyGroupName()}:{CacheElemId}";
        }

        public bool IsNodeStartingElem()
        {
            if (Type != NodeType.NodeStarting) return false;
            if (!IsOkTargetId()) return false;
            return true;
        }

        public bool IsAdventureNodeElem()
        {
            if (Type != NodeType.NodeAdventure) return false;
            if (IsOkTargetId()) return false;
            return true;
        }

        public bool IsAdventureOutputElem()
        {
            if (Type != NodeType.NodeAdventure) return false;
            if (!IsOkTargetId()) return false;
            return true;
        }

        public bool IsOkTargetId()
        {
            return (!string.IsNullOrEmpty(TargetId));
        }

        public string GetNextAdventures(IList<NavigationElem> navigationElems)
        {
            var relations = NavigationElemRelation.GetNextAdventureElemRelations(this, navigationElems);

            var elems = new List<string>();
            foreach (var relation in relations)
            {
                if (!relation.IsAcceptableRealtionForMap())
                    continue;

                elems.Add(relation.ToString());
            }

            if (elems.Count == 0)
                return null;

            return $"{GetMyGroupName()}:{CacheElemId} ({TargetId})->({string.Join(",", elems)})";
        }

        public override string ToString()
        {
            string targetString = "";
            if (IsOkTargetId()) targetString += $"-> {TargetId}";
            if (!string.IsNullOrEmpty(LinkedEventUniqueId)) targetString += $"-> {LinkedEventUniqueId}(A)";

            string nameString = "";
            if (!string.IsNullOrEmpty(Name)) nameString += $" ({Name}) ";

            return $"{AdventureInfo}: ({Type}){nameString} {OwnerId} {targetString} ({CacheElemId})";
        }
               
        public static bool NodeWithName(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.NodeChallenge:
                case NodeType.NodeTrade:
                    return true;
            }

            return false;
        }

        public static bool NodeWithLinkedEventUniqueID(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.NodeAdventureEnd:
                    return true;
            }

            return false;
        }

        public bool CanAddNodeElem()
        {
            switch (Type)
            {
                case NodeType.NodeAdventure:
                case NodeType.NodeAdventureEnd:
                    return true;
            }

            return false;
        }

        public RelationType GetRelationType()
        {
            switch (Type)
            {
                case NodeType.NodeStarting:
                case NodeType.NodeAdventure:
                    return RelationType.Normal;
                case NodeType.NodeAdventureEnd:
                    return RelationType.AdventureEnd;
                case NodeType.NodeChallenge:
                    if (Name == "Win") return RelationType.ChallengeWin;
                    if (Name == "Lose") return RelationType.ChallengeLose;
                    if (Name == "Surrender") return RelationType.ChallengeSurrender;
                    break;
                case NodeType.NodeDealDamage:
                    return RelationType.DealDamage;
                case NodeType.NodeProduceDrop:
                    return RelationType.ProduceDrop;
                case NodeType.NodeSpawnOnMap:
                    return RelationType.SpawnOnMap;
                case NodeType.NodeTrade:
                    if (Name == "Confirm") return RelationType.TradeConfirm;
                    if (Name == "Cancel") return RelationType.TradeCancel;
                    break;
            }

            return RelationType.Unknown;
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Thea2Translator.Logic
{
    public class NavigationElemRelation
    {
        public IList<RelationType> RelationTypes { get; private set; }
        public NavigationElem NextElem { get; private set; }

        private NavigationElemRelation(NavigationElem nextElem, IList<RelationType> relationTypes)
        {
            if (relationTypes!=null)            
                RelationTypes = relationTypes.ToList();
            
            SetNextElem(nextElem);
        }

        public void ExpandRelation(IList<NavigationElem> navigationElems)
        {
            if (GetLastRelation() == RelationType.Normal) return;
        }

        private void SetNextElem(NavigationElem nextElem)
        {
            if (RelationTypes == null) RelationTypes = new List<RelationType>();
            NextElem = nextElem;
            RelationTypes.Add(NextElem.GetRelationType());
        }

        private RelationType GetLastRelation()
        {
            if (RelationTypes == null) return RelationType.Unknown;
            if (RelationTypes.Count == 0) return RelationType.Unknown;
            return RelationTypes[RelationTypes.Count - 1];
        }

        private bool IsAcceptableRealtionForImprove(bool withAdventureEnd)
        {
            return IsAcceptableRealtion(true);
        }

        public bool IsAcceptableRealtionForMap()
        {
            return IsAcceptableRealtion(false);
        }

        private bool IsAcceptableRealtion(bool withAdventureEnd)
        {
            var lastRelationType = GetLastRelation();
            if (withAdventureEnd && lastRelationType == RelationType.AdventureEnd) return true;
            if (lastRelationType == RelationType.Normal) return true;
            return false;
        }

        public static IList<NavigationElemRelation> GetNextAdventureElemRelations(NavigationElem elem, IList<NavigationElem> navigationElems)
        {
            var relations = GetNextAdventureElemRelationsInner(elem, null, navigationElems);                       
            return GetImprovedRelations(relations, navigationElems);
        }

        private static IList<NavigationElemRelation> GetImprovedRelations(IList<NavigationElemRelation> relations, IList<NavigationElem> navigationElems)
        {
            bool improve = false;
            var relationsCount = relations.Count;
            for (int r = 0; r < relationsCount; r++)
            {
                var relation = relations[r];
                if (relation.IsAcceptableRealtion(true)) continue;

                improve = true;
                var nextRelations = GetNextAdventureElemRelationsInner(relation.NextElem, relation.RelationTypes, navigationElems);
                for (int i = 0; i < nextRelations.Count; i++)
                {
                    if (i == 0) relation.SetNextElem(nextRelations[0].NextElem);
                    else relations.Add(nextRelations[i]);
                }
            }

            if (improve) return GetImprovedRelations(relations, navigationElems);
            return relations;
        }

        private static IList<NavigationElemRelation> GetNextAdventureElemRelationsInner(NavigationElem elem, IList<RelationType> relationTypes, IList<NavigationElem> navigationElems)
        {
            var relations = new List<NavigationElemRelation>();
            if (!elem.IsOkTargetId())
                return relations;

            var nextElems = navigationElems.Where(x => NavigationElemAdventureInfo.IsEquals(elem.AdventureInfo, x.AdventureInfo) && x.OwnerId == elem.TargetId && !x.IsAdventureOutputElem()).ToList();

            foreach (var nextElem in nextElems)
            {
                relations.Add(new NavigationElemRelation(nextElem, relationTypes));
            }

            return relations;
        }

        public override string ToString()
        {
            var types = string.Join(":", RelationTypes.ToList());
            return $"{types};{NextElem.GetMyGroupName()}";
        }
    }
}

using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class UtilityIntelligenceItemContainer<TItem> : ItemContainer<TItem>
        where TItem : UtilityIntelligenceMember, IContainerItem
    {
        private UtilityIntelligence intelligence;

        public UtilityIntelligence Intelligence
        {
            get => intelligence;
            internal set
            {
                if (intelligence == value) return;

                intelligence = value;

                foreach (var item in items)
                {
                    item.Intelligence = intelligence;
                }
            }
        }

        protected override void OnItemAdded(TItem item)
        {
            item.Intelligence = intelligence;
        }
    }
}
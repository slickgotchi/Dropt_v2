#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class
        DecisionMakerItemCreatorView : NameItemCreatorView<DecisionMakerListViewModel, DecisionMakerItemViewModel>
    {
        protected override void OnItemAdded(DecisionMakerItemViewModel item)
        {
            base.OnItemAdded(item);
            
            if(ViewModel.Items.Count == 1)
                MakeDecision();
        }

        protected override void OnItemRemoved(DecisionMakerItemViewModel item)
        {
            base.OnItemRemoved(item);
            
            if(ViewModel.Items.Count >= 1)
                MakeDecision();
        }

        public void MakeDecision()
        {
            var intelligenceViewModel = ViewModel.RootViewModel;
            intelligenceViewModel.MakeDecision();
        }
    }
}
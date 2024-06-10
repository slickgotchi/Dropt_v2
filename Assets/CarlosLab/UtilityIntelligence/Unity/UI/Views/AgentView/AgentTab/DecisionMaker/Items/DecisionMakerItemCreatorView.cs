#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class
        DecisionMakerItemCreatorView : NameItemCreatorView<DecisionMakerListViewModel, DecisionMakerViewModel>
    {
        protected override void OnItemAdded(DecisionMakerViewModel item)
        {
            base.OnItemAdded(item);
            
            if(ViewModel.Items.Count == 1)
                MakeDecision();
        }

        protected override void OnItemRemoved(DecisionMakerViewModel item)
        {
            base.OnItemRemoved(item);
            
            if(ViewModel.Items.Count >= 1)
                MakeDecision();
        }

        public void MakeDecision()
        {
            var asset = UtilityIntelligenceEditorUtils.Asset;
            var intelligenceModel = UtilityIntelligenceEditorUtils.Model;
            if (!asset.IsRuntimeAsset && intelligenceModel != null)
                intelligenceModel.Runtime.MakeDecision(null);
        }
    }
}
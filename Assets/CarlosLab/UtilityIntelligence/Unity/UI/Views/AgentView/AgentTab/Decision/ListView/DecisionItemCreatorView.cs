#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionItemCreatorView : NameItemCreatorView<DecisionListViewModel, DecisionViewModel>
    {
        protected override void OnItemAdded(DecisionViewModel item)
        {
            base.OnItemAdded(item);
            
            if(ViewModel.Items.Count == 1)
                MakeDecision();
        }

        protected override void OnItemRemoved(DecisionViewModel item)
        {
            base.OnItemRemoved(item);
            
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
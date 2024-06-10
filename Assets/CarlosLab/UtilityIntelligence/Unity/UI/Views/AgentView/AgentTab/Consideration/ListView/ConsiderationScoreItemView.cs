using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationScoreItemView : ScoreItemView<ConsiderationViewModel>
    {
        public ConsiderationScoreItemView(IListViewWithItem<ConsiderationViewModel> listView) : base(listView)
        {
        }
        
        protected override void OnUpdateView(ConsiderationViewModel viewModel)
        {
            UpdateScoreLabel(Score);
            SetBinding(nameof(Score), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                updateTrigger = BindingUpdateTrigger.OnSourceChanged,
                dataSourcePath = PropertyPath.FromName(nameof(ConsiderationRuntimeContextViewModel.FinalScore))
            });
        }
    }
}
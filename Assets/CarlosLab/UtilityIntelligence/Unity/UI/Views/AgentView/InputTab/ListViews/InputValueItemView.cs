namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputValueItemView : ValueItemView<InputItemViewModel>
    {
        public InputValueItemView(InputListView listView) : base(listView)
        {
            // this.style.flexDirection = FlexDirection.Row;
        }

        protected override void OnEnableEditMode()
        {
            ValueField?.SetEnabled(true);
        }

        protected override void OnEnableRuntimeMode()
        {
            ValueField?.SetEnabled(false);
        }
    }
}
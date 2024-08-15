namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputValueItemView : ValueItemView<InputItemViewModel>
    {
        protected override void OnEnableEditMode()
        {
            SetValueFieldBinding();
            EnableValueField();
        }

        protected override void OnEnableRuntimeMode()
        {
            SetValueFieldBinding();
            DisableValueField();
        }
    }
}
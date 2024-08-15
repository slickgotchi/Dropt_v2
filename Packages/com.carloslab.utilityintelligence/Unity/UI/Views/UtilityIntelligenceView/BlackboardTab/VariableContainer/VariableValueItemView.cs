namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableValueItemView : ValueItemView<VariableViewModel>
    {
        protected override void OnEnableEditMode()
        {
            SetValueFieldBinding();
            EnableValueField();
        }

        protected override void OnEnableRuntimeMode()
        {
            SetValueFieldBinding();
            EnableValueField();
        }
    }
}
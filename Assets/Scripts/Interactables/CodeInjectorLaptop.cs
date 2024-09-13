public class CodeInjectorLaptop : Interactable
{
    public override void OnPressOpenInteraction()
    {
        CodeInjectorCanvas.Instance.SetVisible(true);
        SetPlayerInputEnabled(false);
    }

    public override void OnPressCloseInteraction()
    {
        CodeInjectorCanvas.Instance.SetVisible(false);
        CodeInjector.Instance.ResetUpdatedVariablesValue();
        SetPlayerInputEnabled(true);
    }
}

public class CodeInjectorLaptop : Interactable
{
    public override void OnPressOpenInteraction()
    {
        CodeInjectorCanvas.Instance.ShowCanvas();
        PlayerInputMapSwitcher.Instance.SwitchToInUI();
    }
}

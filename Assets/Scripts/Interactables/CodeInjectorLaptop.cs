public class CodeInjectorLaptop : Interactable
{
    public override void OnPressOpenInteraction()
    {
        CodeInjectorCanvas.Instance.ShowCanvas();
        PlayerInputMapSwitcher.Instance.SwitchToInUI();
    }

    //public override void OnPressCloseInteraction()
    //{
    //    CodeInjectorCanvas.Instance.HideCanvas();
    //    CodeInjector.Instance.ResetUpdatedVariablesValue();
    //    PlayerInputMapSwitcher.Instance.SwitchToInGame();
    //}
}

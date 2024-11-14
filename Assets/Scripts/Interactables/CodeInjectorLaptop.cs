public class CodeInjectorLaptop : Interactable
{
    public override void OnPressOpenInteraction()
    {
        base.OnPressOpenInteraction();

        CodeInjectorCanvas.Instance.interactable = GetComponent<Interactable>();
        CodeInjectorCanvas.Instance.ShowCanvas();
    }

    public override void OnPressCloseInteraction()
    {
        base.OnPressCloseInteraction();

        CodeInjectorCanvas.Instance.HideCanvas();
    }
}

namespace CarlosLab.UtilityIntelligence
{
    public abstract class InputFromSource<T> : Input<T>
    {
        public InputSource InputSource;

        protected UtilityEntity GetInputSource(InputContext context)
        {
            if (InputSource == InputSource.Self)
                return Agent;
            if (InputSource == InputSource.Target)
                return context.Target;

            return null;
        }
    }
}
using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class ConsiderationReference : ItemReference<Consideration, ConsiderationContainer>
    {
        public ConsiderationReference(string name, ConsiderationContainer container = null) : base(name, container)
        {
        }
    }
}
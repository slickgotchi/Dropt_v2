namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class StopAttack : ActionTask
    {
        private CharacterAttacker attacker;
        protected override void OnAwake()
        {
            attacker = GetComponent<CharacterAttacker>();
        }
        
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            attacker.StopAttack();
            return UpdateStatus.Success;
        }
    }
}
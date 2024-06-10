#region

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class ImapInfluenceInput : InputFromSource<float>
    {
        public int MapType;

        //protected override float GetRawInput(DecisionContext context)
        //{
        //    UtilityEntity inputSource = GetInputSource(context);
        //    IImapEntity mapEntity = inputSource.ImapEntity;
        //    float influence = mapEntity.GetInfluence(context.Agent.AgentType, MapType);
        //    return influence;
        //}
    }
}
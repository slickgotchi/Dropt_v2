#region

using CarlosLab.Common;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [CreateAssetMenu(menuName = FrameworkRuntimeConsts.CreateAssetMenuPath, fileName = FrameworkRuntimeConsts.AssetFileName)]
    public class UtilityIntelligenceAsset : DataAsset<UtilityIntelligenceModel, UtilityIntelligence>
    {
        [SerializeField]
        [FormerlySerializedAs("agentType")]
        internal string type;

        [SerializeField]
        [FormerlySerializedAs("agentDescription")]
        internal string description;

        protected override int GetDataVersion()
        {
            return FrameworkRuntimeConsts.DataVersion;
        }

        protected override string GetFrameworkVersion()
        {
            return FrameworkRuntimeConsts.FrameworkVersion;
        }
    }
}
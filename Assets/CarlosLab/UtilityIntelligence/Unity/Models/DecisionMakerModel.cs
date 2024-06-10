#region

using System.Collections.Generic;
using System.Runtime.Serialization;
using CarlosLab.Common;
using CarlosLab.Common.Extensions;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [DataContract]
    public class DecisionMakerModel : ContainerItemModel<DecisionMaker>
    {
        #region Decisions

        [DataMember(Name = nameof(Decisions))]
        private List<DecisionModel> decisions = new();

        public IReadOnlyList<DecisionModel> Decisions => decisions;

        public bool HasDecision(string name)
        {
            return Runtime.HasDecision(name);
        }

        public bool TryAddDecision(int index, string name, DecisionModel decision)
        {
            if (Runtime.TryAddDecision(index, name, decision.Runtime))
            {
                decisions.Insert(index, decision);
                return true;
            }

            return false;
        }

        public DecisionModel GetDecisionById(string id)
        {
            return decisions.Find(d => d.Id == id);
        }

        public bool TryRemoveDecision(string name, DecisionModel decision)
        {
            if (Runtime.TryRemoveDecision(name))
            {
                decisions.Remove(decision);
                return true;
            }

            return false;
        }

        public void MoveDecision(int sourceIndex, int destIndex)
        {
            decisions.Move(sourceIndex, destIndex);
            Runtime.MoveDecision(sourceIndex, destIndex);
        }

        #endregion

        #region Runtime

        private DecisionMaker runtime;

        public override DecisionMaker Runtime
        {
            get
            {
                if (runtime == null)
                {
                    runtime = new DecisionMaker();

                    foreach (DecisionModel decision in decisions)
                    {
                        runtime.TryAddDecision(decision.Name, decision.Runtime);
                    }
                }

                return runtime;
            }
        }

        #endregion
    }
}
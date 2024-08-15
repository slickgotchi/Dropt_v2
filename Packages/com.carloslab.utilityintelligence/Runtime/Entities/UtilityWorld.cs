#region

using System.Collections.Generic;
using System.Linq;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public sealed class UtilityWorld : World<UtilityWorld, UtilityEntity>
    {
        #region Constructors

        public UtilityWorld(float makeDecisionInterval = 0.1f)
        {
            this.makeDecisionInterval = makeDecisionInterval;
        }

        #endregion

        #region Field and Properties

        private readonly float makeDecisionInterval;

        private float makeDecisionElapsedTime;

        private HashSet<UtilityAgent> activeAgents = new();
        public HashSet<UtilityAgent> ActiveAgents => activeAgents;

        #endregion

        #region Event Functions

        protected override void OnStart()
        {
            MakeDecision();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (makeDecisionInterval > 0.0f)
            {
                makeDecisionElapsedTime += deltaTime;
                if (makeDecisionElapsedTime >= makeDecisionInterval)
                {
                    makeDecisionElapsedTime = 0;
                    MakeDecision();
                }
            }
            else
                MakeDecision();

            ExecuteDecision(deltaTime);
        }

        protected override void OnEntityDestroyed(UtilityEntity entity)
        {
            UtilityIntelligenceConsole.Instance.Log($"OnEntityDestroyed Tick: {UpdateTick} Entity: {entity.Name}");
        }

        protected override void OnEntityEnabled(UtilityEntity entity)
        {
            if (entity is UtilityAgent agent)
                activeAgents.Add(agent);
        }

        protected override void OnEntityDisabled(UtilityEntity entity)
        {
            if (entity is UtilityAgent agent)
                activeAgents.Remove(agent);
        }

        #endregion

        #region Decisions

        private void MakeDecision()
        {
#if CARLOSLAB_ENABLE_PROFILER
            using var _ = Profiler.Sample("UtilityWorld - MakeDecision");
#endif
            foreach (UtilityAgent agent in activeAgents)
            {
                agent.MakeDecision(activeEntities);
            }
        }

        private void ExecuteDecision(float deltaTime)
        {
//#if CARLOSLAB_ENABLE_PROFILER
//            using var _ = Profiler.Sample("UtilityWorld - ExecuteDecision");
//#endif

            foreach (UtilityAgent agent in activeAgents)
            {
                agent.ExecuteDecision(deltaTime);
            }
        }

        #endregion
    }
}
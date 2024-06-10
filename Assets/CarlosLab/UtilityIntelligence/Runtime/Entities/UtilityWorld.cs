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

        public HashSet<UtilityAgent> ActiveAgents { get; } = new();

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

            RunDecision(deltaTime);
        }

        protected override void OnEntityDestroyed(UtilityEntity entity)
        {
            UtilityIntelligenceConsole.Instance.Log($"OnEntityDestroyed Tick: {UpdateTick} Entity: {entity.Name}");
        }

        protected override void OnEntityEnabled(UtilityEntity entity)
        {
            if (entity is UtilityAgent agent)
                this.ActiveAgents.Add(agent);
        }

        protected override void OnEntityDisabled(UtilityEntity entity)
        {
            if (entity is UtilityAgent agent)
                this.ActiveAgents.Remove(agent);
        }

        #endregion

        #region Decisions

        private void MakeDecision()
        {
            foreach (UtilityAgent agent in ActiveAgents)
            {
                agent.MakeDecision(ActiveEntities);
            }
        }

        private void RunDecision(float deltaTime)
        {
            using var _ = Profiler.Sample("RunDecision - UtilityWorld");

            foreach (UtilityAgent agent in ActiveAgents)
            {
                agent.RunDecision(deltaTime);
            }
        }

        #endregion
    }
}
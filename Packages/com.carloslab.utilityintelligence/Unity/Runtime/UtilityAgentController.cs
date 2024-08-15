#region

using System.Collections.Generic;
using CarlosLab.Common;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [RequireComponent(typeof(UtilityAgentFacade))]
    [AddComponentMenu(FrameworkRuntimeConsts.AddAgentControllerMenuPath)]
    public class UtilityAgentController : EntityController<UtilityAgent, UtilityIntelligenceAsset>
        , INotifyBindablePropertyChanged
    {
        public UtilityIntelligence Intelligence => Entity?.Intelligence;

        private bool TryGetCurrentActions(out List<ActionTask> currentActions)
        {
            currentActions = null;
            
            var currentDecision = Intelligence.CurrentDecision;

            if (currentDecision != null)
            {
                var decisionContainerModel = Asset.Model.Decisions;
                if (decisionContainerModel.TryGetItem(currentDecision.Name, out DecisionModel decisionModel))
                {
                    currentActions = decisionModel.RuntimeActions;
                    return true;
                }
            }

            return false;
        }

        public void Register(UtilityWorldController world)
        {
            Entity?.Register(world.World);
        }
        
        protected override UtilityAgent CreateEntity()
        {
            var intelligence = RuntimeAsset != null ? RuntimeAsset.Runtime : null;
            UtilityAgent agent = new(intelligence);
            return agent;
        }

        #region Lifecycle

        protected override void OnInit()
        {
            var decisionMakerContainer = Entity.Intelligence.DecisionMakerContainer;
            
            for (int i = 0; i < decisionMakerContainer.Count; i++)
            {
                var decisionMaker = decisionMakerContainer.Items[i];
                
                for (int j = 0; j < decisionMaker.Decisions.Count; j++)
                {
                    Decision decision = decisionMaker.Decisions[j];
                    decision.Task.Awake();
                }

            }
        }

        private void LateUpdate()
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.LateUpdate(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.FixedUpdate(Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Collision/Trigger 3D

        private void OnCollisionEnter(Collision collision)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.CollisionEnter(collision);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.CollisionStay(collision);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.CollisionExit(collision);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.ControllerColliderHit(hit);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.TriggerEnter(other);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.TriggerStay(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.TriggerExit(other);
            }
        }

        #endregion

        #region Collision/Trigger 2D

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.CollisionEnter2D(collision);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.CollisionStay2D(collision);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.CollisionExit2D(collision);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.TriggerEnter2D(collision);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.TriggerStay2D(collision);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.TriggerExit2D(collision);
            }
        }

        #endregion

        #region Animation

        private void OnAnimatorMove()
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.AnimatorMove();
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (TryGetCurrentActions(out List<ActionTask> actions))
            {
                foreach (ActionTask action in actions)
                    action.AnimatorIK(layerIndex);
            }
        }

        #endregion
    }
}
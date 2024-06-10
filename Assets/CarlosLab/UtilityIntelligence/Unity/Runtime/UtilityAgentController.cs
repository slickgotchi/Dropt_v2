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
        private List<ActionModel> actionModels;

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
            foreach (DecisionModel decision in Asset.Decisions)
            {
                decision.Runtime.Task.Awake();
            }

            actionModels = Asset.Actions;
        }

        private void LateUpdate()
        {
            foreach (var action in actionModels)
            {
                action.Runtime.LateUpdate(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            foreach (var action in actionModels)
            {
                action.Runtime.FixedUpdate(Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Collision/Trigger 3D

        private void OnCollisionEnter(Collision collision)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.CollisionEnter(collision);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.CollisionStay(collision);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.CollisionExit(collision);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.ControllerColliderHit(hit);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.TriggerEnter(other);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.TriggerStay(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.TriggerExit(other);
            }
        }

        #endregion

        #region Collision/Trigger 2D

        private void OnCollisionEnter2D(Collision2D collision)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.CollisionEnter2D(collision);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.CollisionStay2D(collision);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.CollisionExit2D(collision);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.TriggerEnter2D(collision);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.TriggerStay2D(collision);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.TriggerExit2D(collision);
            }
        }

        #endregion

        #region Animation

        private void OnAnimatorMove()
        {
            foreach (var action in actionModels)
            {
                action.Runtime.AnimatorMove();
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            foreach (var action in actionModels)
            {
                action.Runtime.AnimatorIK(layerIndex);
            }
        }

        #endregion
    }
}
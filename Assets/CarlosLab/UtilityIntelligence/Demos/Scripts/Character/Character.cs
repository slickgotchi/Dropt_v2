#region

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

#endregion

namespace CarlosLab.UtilityIntelligence.Demos
{
    public enum Team
    {
        Cyan,
        Yellow,
        Orange
    }

    public class Character : UtilityAgentFacade
    {
        [SerializeField]
        private Team team;

        private CharacterEnergy energy;

        private CharacterHealth health;

        private NavMeshAgent navMeshAgent;

        private Rigidbody rigidBody;

        private CharacterState state;

        public Team Team => team;
        public CharacterState State => state;

        public CharacterHealth Health => health;
        public CharacterEnergy Energy => energy;
        
        private Coroutine backToNormalStateCoroutine;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            rigidBody = GetComponent<Rigidbody>();

            health = GetComponent<CharacterHealth>();
            energy = GetComponent<CharacterEnergy>();
        }

        public void OnAttacked(int damage, float force, Vector3 direction)
        {
            health.Health -= damage;
            
            navMeshAgent.enabled = false;
            rigidBody.isKinematic = false;
            
            Vector3 impulse = direction * force;
            rigidBody.AddForce(impulse, ForceMode.Impulse);
            
            state = CharacterState.Attacked;

            BackToNormalStateAfter(2.0f);
        }

        private void BackToNormalStateAfter(float delay)
        {
            if (backToNormalStateCoroutine != null)
            {
                StopCoroutine(backToNormalStateCoroutine);
                backToNormalStateCoroutine = null;
            }

            backToNormalStateCoroutine = StartCoroutine(BackToNormalStateCoroutine(delay));
        }

        private IEnumerator BackToNormalStateCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (state == CharacterState.Attacked)
            {
                navMeshAgent.enabled = true;
                rigidBody.isKinematic = true;
            
                state = CharacterState.Normal;
            }
        }
    }
}
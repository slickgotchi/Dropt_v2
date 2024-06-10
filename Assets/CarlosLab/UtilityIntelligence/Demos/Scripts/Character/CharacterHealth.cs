#region

using System;
using System.Collections;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence.Demos
{
    public class CharacterHealth : MonoBehaviour
    {
        private Character character;

        private int health = 100;

        private bool isDied;

        public event Action<int> HealthChanged;

        private void Awake()
        {
            character = GetComponent<Character>();
        }
        
        public int Health
        {
            get => health;
            set
            {
                int newHealth = value;

                if (newHealth < 0)
                    newHealth = 0;

                if (this.health == newHealth)
                    return;

                this.health = newHealth;
                HealthChanged?.Invoke(this.health);

                if (this.health == 0)
                    Die();
            }
        }

        private void Die()
        {
            if (isDied)
                return;

            isDied = true;
            character.Unregister();
            DestroyAfter(2.0f);
        }

        public void DestroyAfter(float seconds)
        {
            StartCoroutine(DestroyAfterCoroutine(seconds));
        }

        private IEnumerator DestroyAfterCoroutine(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            character.Destroy();
        }
    }
}
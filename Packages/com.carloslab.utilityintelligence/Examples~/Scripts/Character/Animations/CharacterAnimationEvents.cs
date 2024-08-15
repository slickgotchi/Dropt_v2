using System;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class CharacterAnimationEvents : MonoBehaviour
    {
        public event Action Attack;
        public void RaiseAttackEvent()
        {
            Attack?.Invoke();
        }
    }
}
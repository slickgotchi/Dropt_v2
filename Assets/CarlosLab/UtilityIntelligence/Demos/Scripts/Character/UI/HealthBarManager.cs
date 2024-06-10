#region

using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence.Demos
{
    public class HealthBarManager : MonoBehaviour
    {
        [SerializeField]
        private HealthBar healthBarPrefab;

        private CharacterHealth characterHealth;

        private HealthBar healthBar;
        private Transform worldCanvas;

        private void Awake()
        {
            worldCanvas = GameObject.Find("WorldCanvas").transform;
            characterHealth = GetComponentInParent<CharacterHealth>();
        }

        private void Start()
        {
            healthBar = Instantiate(healthBarPrefab, worldCanvas, false);
            healthBar.Init(characterHealth, transform);
        }

        private void OnDestroy()
        {
            if (healthBar != null)
                Destroy(healthBar.gameObject);
        }
    }
}
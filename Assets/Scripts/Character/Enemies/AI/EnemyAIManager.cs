using Dropt;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyAIManager : MonoBehaviour
{
    public static EnemyAIManager Instance { get; private set; }

    public float m_updateInterval = 0.1f;
    private float m_updateTimer = 0f;

    public List<EnemyAI> allEnemies = new List<EnemyAI>(); // Use List instead of array

    private void Awake()
    {
        // Ensure this is the only instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent multiple instances
        }
    }

    private void OnDestroy()
    {
        // Clear the list when this manager is destroyed
        allEnemies.Clear();
    }

    private void Update()
    {
        // check for enemydebugcanvas ai toggles
        HandleToggleDebugCanvases();

        // do update timer checks
        m_updateTimer -= Time.deltaTime;
        if (m_updateTimer > 0) return;
        m_updateTimer = m_updateInterval;

        // get players and enemies
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        var enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        // Clear the existing list and populate it with the latest enemies
        allEnemies.Clear();
        allEnemies.AddRange(enemies);

        // Cacl nearest players for enemies
        CalculateNearestPlayers(players, allEnemies);
    }

    void CalculateNearestPlayers(PlayerController[] players, List<EnemyAI> enemies)
    {
        // set all enemies distances quite large
        foreach (var enemy in enemies)
        {
            enemy.NearestPlayerDistance = 1e10f;

            // iterate through players
            foreach (var player in players)
            {
                var dist = math.distance(enemy.transform.position, player.transform.position);
                if (dist < enemy.NearestPlayerDistance)
                {
                    enemy.NearestPlayer = player.gameObject;
                    enemy.NearestPlayerDistance = dist;
                }
            }
        }
    }

    public bool IsDebugVisible = false;
    void HandleToggleDebugCanvases()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            IsDebugVisible = !IsDebugVisible;

            var debugCanvases = FindObjectsByType<EnemyAI_DebugCanvas>(FindObjectsSortMode.None);
            foreach (var dc in debugCanvases)
            {
                dc.Container.SetActive(IsDebugVisible);
            }
        }
    }
}

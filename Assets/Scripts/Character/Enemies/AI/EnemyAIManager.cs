using Dropt;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyAIManager : MonoBehaviour
{
    public static EnemyAIManager Instance { get; private set; }

    public float m_updateInterval = 0.1f;
    private float m_updateTimer = 0f;

    public List<EnemyController> allEnemyControllers = new List<EnemyController>(); // Use List instead of array

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        // Clear the list when this manager is destroyed
        allEnemyControllers.Clear();
    }

    private void Update()
    {
        // check for enemydebugcanvas ai toggles
        //HandleToggleDebugCanvases();

        // do update timer checks
        m_updateTimer -= Time.deltaTime;
        if (m_updateTimer > 0) return;
        m_updateTimer = m_updateInterval;

        // get players and enemies
        var players = Game.Instance.playerControllers.ToArray();
        var enemies = Game.Instance.enemyControllers.ToArray();

        // Clear the existing list and populate it with the latest enemies
        allEnemyControllers.Clear();
        allEnemyControllers.AddRange(enemies);

        // Cacl nearest players for enemies
        CalculateNearestPlayers(players, allEnemyControllers);
    }

    void CalculateNearestPlayers(PlayerController[] playerControllers, List<EnemyController> enemyControllers)
    {
        // set all enemies distances quite large
        foreach (var enemyController in enemyControllers)
        {
            var enemyAI = enemyController.enemyAI;
            if (enemyAI == null) continue;

            enemyAI.NearestPlayerDistance = 1e10f;

            // iterate through players
            foreach (var playerController in playerControllers)
            {
                var dist = math.distance(enemyController.transform.position, playerController.transform.position);
                if (dist < enemyAI.NearestPlayerDistance)
                {
                    enemyAI.NearestPlayer = playerController.gameObject;
                    enemyAI.NearestPlayerDistance = dist;
                }
            }
        }
    }

    public bool IsDebugVisible = false;

}

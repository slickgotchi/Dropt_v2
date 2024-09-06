using Dropt;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyAIManager : MonoBehaviour
{
    public float m_updateInterval = 0.1f;
    private float m_updateTimer = 0f;

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

    bool m_isVisibleDebugStates = false;
    void HandleToggleDebugCanvases()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            m_isVisibleDebugStates = !m_isVisibleDebugStates;

            var debugCanvases = FindObjectsByType<EnemyAI_DebugCanvas>(FindObjectsSortMode.None);
            foreach (var dc in debugCanvases)
            {
                dc.Container.SetActive(m_isVisibleDebugStates);
            }
        }
    }
}

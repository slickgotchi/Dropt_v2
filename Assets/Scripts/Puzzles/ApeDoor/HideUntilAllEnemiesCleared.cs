using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class HideUntilAllEnemiesCleared : MonoBehaviour
{
    private List<SpriteRenderer> m_spriteRenderers;
    private List<Collider2D> m_colliders;
    private List<TextMeshProUGUI> m_textMeshPros;

    private bool m_isActivated = false;

    private void Awake()
    {
        m_spriteRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        m_colliders = new List<Collider2D>(GetComponentsInChildren<Collider2D>());
        m_textMeshPros = new List<TextMeshProUGUI>(GetComponentsInChildren<TextMeshProUGUI>());  

        SetEnabled(false);
    }

    private void Update()
    {
        if (GetEnemyCount() > 0 && !m_isActivated) m_isActivated = true;

        if (!m_isActivated) return;

        if (GetEnemyCount() <= 0)
        {
            // show our things
            SetEnabled(true);
        }
    }

    void SetEnabled(bool isEnabled)
    {
        foreach (var sr in m_spriteRenderers)
        {
            sr.enabled = isEnabled;
        }

        foreach (var co in m_colliders)
        {
            co.enabled = isEnabled;
        }

        foreach (var tmp in m_textMeshPros)
        {
            tmp.enabled = isEnabled;
        }
    }

    int GetEnemyCount()
    {
        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        var lilEssence = FindObjectsByType<LilEssence>(FindObjectsSortMode.None);

        int enemyCount = enemies != null ? enemies.Length : 0;
        int lilEssenceCount = lilEssence != null ? lilEssence.Length : 0;

        return enemyCount - lilEssenceCount;
    }
}

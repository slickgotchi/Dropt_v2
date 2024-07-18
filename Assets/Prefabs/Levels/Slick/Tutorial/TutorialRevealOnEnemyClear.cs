using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRevealOnEnemyClear : MonoBehaviour
{
    public List<GameObject> ObjectsToReveal = new List<GameObject>();

    private bool m_isActivated = false;

    void HideAll()
    {
        foreach (var objToReveal in ObjectsToReveal)
        {
            objToReveal.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // find active enemies
        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        // don't do anything if this is the first time we've found enemies
        if (!m_isActivated && enemies != null && enemies.Length > 0)
        {
            Debug.Log("found enemies: " + enemies.Length);
            m_isActivated = true;
            HideAll();
        }
        if (!m_isActivated) return;

        // now we're activated, check for enemies all gone
        if (enemies == null || enemies.Length <= 0)
        {
            Debug.Log("no enemies left");
            foreach (var objToReveal in ObjectsToReveal)
            {
                objToReveal.gameObject.SetActive(true);
            }

            ObjectsToReveal.Clear();

            Destroy(gameObject);
        }
    }
}

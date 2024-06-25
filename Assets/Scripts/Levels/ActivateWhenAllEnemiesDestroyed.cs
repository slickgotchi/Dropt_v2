using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateWhenAllEnemiesDestroyed : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("enabled");
    }

    private void OnDisable()
    {
        Debug.Log("disabled");
    }
}

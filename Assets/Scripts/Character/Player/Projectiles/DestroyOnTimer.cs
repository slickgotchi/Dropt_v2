using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnTimer : MonoBehaviour
{
    public float duration = 0.3f;

    private void Update()
    {
        duration -= Time.deltaTime;

        if (duration < 0)
        {
            Destroy(gameObject);
        }
    }
}

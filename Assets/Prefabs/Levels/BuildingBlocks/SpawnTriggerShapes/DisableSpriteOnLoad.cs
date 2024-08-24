using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableSpriteOnLoad : MonoBehaviour
{
    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }
    }
}

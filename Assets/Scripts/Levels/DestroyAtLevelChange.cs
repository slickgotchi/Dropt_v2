using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAtLevelChange : MonoBehaviour
{
    // any OnDestroy style spawners should check this variable to ensure
    // they don't accidentally create something at level change
    public bool isDestroying = false;
}

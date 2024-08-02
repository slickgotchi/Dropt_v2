using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyIfClient : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Bootstrap.IsClient()) Destroy(gameObject);
    }

}

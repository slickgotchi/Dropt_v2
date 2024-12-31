using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfServer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Bootstrap.IsServer() && !Bootstrap.IsHost()) gameObject.SetActive(false);
    }

}

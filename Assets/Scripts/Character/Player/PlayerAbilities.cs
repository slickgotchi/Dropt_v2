using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    public CleaveSwing cleaveSwing;
    public Dash dash;

    private void Awake()
    {
        cleaveSwing = Instantiate(cleaveSwing);
        cleaveSwing.GetComponent<NetworkObject>().Spawn();

        dash = Instantiate(dash);
        dash.GetComponent<NetworkObject>().Spawn();
    }

    private void Update()
    {
        if (cleaveSwing != null)
        {
            if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
            {
                cleaveSwing.PerformCleaveSwing(transform.position);
            }
        }
    }
}

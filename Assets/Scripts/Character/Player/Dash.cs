using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : PlayerAbility
{
    // Start is called before the first frame update
    void Start()
    {
        TeleportDistance = 3.5f;
        CooldownDuration = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

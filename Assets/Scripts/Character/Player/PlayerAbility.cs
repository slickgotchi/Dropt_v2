using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAbility : NetworkBehaviour
{
    public int ApCost = 0;
    public float CooldownDuration = 1;
    public float SlowFactor = 1;
    public float SlowDuration = 1;
    public float TeleportDistance = 0;
}

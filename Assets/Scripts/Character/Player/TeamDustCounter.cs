using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TeamDustCounter : NetworkBehaviour
{
    [HideInInspector] public NetworkVariable<int> Count = new NetworkVariable<int>(0);
}

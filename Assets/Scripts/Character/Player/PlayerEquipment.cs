using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerEquipment : NetworkBehaviour
{
    public NetworkVariable<Wearable.NameEnum> Body;
    public NetworkVariable<Wearable.NameEnum> Face;
    public NetworkVariable<Wearable.NameEnum> Eyes;
    public NetworkVariable<Wearable.NameEnum> Head;
    public NetworkVariable<Wearable.NameEnum> RightHand;
    public NetworkVariable<Wearable.NameEnum> LeftHand;
    public NetworkVariable<Wearable.NameEnum> Pet;

    public override void OnNetworkSpawn()
    {
        LeftHand.Value = Wearable.NameEnum.Handsaw;
        RightHand.Value = Wearable.NameEnum.Pitchfork;
    }
}

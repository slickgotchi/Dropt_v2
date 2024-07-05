using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class ShieldWall : PlayerAbility
{
    [Header("ShieldWall Parameters")]
    [SerializeField] private float SpinSpeed = 1f;
    [SerializeField] private GameObject shieldWallEffectPrefab;
    [SerializeField] private Vector3 offset = new Vector3(0, 0.8f, 0);

    public override void OnNetworkSpawn()
    {
    }

    public override void OnStart()
    {
        // set local rotation/position.
        // IMPORTANT SetRotation(), SetRotationToActionDirection() and SetLocalPosition() must be used as
        // they call RPC's that sync remote clients
        SetRotation(quaternion.identity);
        SetLocalPosition(PlayerAbilityCentreOffset);

        // spawn shield effect if server
        if (IsServer)
        {
            var no_shieldEffect = Instantiate(shieldWallEffectPrefab).GetComponent<ShieldWallEffect>();
            no_shieldEffect.WearableNameEnum = ActivationWearableNameEnum;
            no_shieldEffect.SpinSpeed = SpinSpeed;
            no_shieldEffect.GetComponent<NetworkObject>().Spawn();
            no_shieldEffect.GetComponent<NetworkObject>().TrySetParent(Player);
            no_shieldEffect.transform.localPosition = offset;
        }
    }

    public override void OnUpdate()
    {

    }

    public override void OnFinish()
    {
    }
}

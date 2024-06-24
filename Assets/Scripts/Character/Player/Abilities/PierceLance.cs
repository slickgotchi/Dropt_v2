using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class PierceLance : PlayerAbility
{
    [Header("PierceLance Parameters")]
    [SerializeField] float Projection = 0f;
    [SerializeField] float DamageMultiplier = 2f;

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        Animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart()
    {
        // setup offset and rotation for tracking
        AbilityOffset = Vector3.zero;
        AbilityRotation = quaternion.identity;

        // set transform to activation rotation/position
        transform.rotation = AbilityRotation;
        transform.position = PlayerActivationState.position + AbilityOffset;

        // hide the player
        Player.GetComponent<PlayerGotchi>().SetVisible(false);
    }

    public override void OnTeleport()
    {
        Animator.Play("PierceLance");
        DebugDraw.DrawColliderPolygon(m_collider, IsServer);
        PlayAnimRemoteServerRpc("PierceLance", AbilityOffset, AbilityRotation);

        Player.GetComponent<PlayerGotchi>().SetVisible(true);
        Player.GetComponent<Animator>().Play("PlayerGotchi_PierceLance");
    }

    public override void OnUpdate()
    {
        //TrackPlayerPosition();
    }

    public override void OnFinish()
    {
        OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Pierce);
    }
}

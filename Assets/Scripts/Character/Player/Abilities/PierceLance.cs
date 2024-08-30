using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class PierceLance : PlayerAbility
{
    [Header("PierceLance Parameters")]
    [SerializeField] float Projection = 0f;

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        Animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart()
    {
        // hide the player
        Player.GetComponent<PlayerGotchi>().SetVisible(false);
    }

    public override void OnTeleport()
    {
        SetRotation(quaternion.identity);
        SetLocalPosition(PlayerAbilityCentreOffset);

        PlayAnimation("PierceLance");

        Player.GetComponent<PlayerGotchi>().SetVisible(true);
        Player.GetComponent<PlayerGotchi>().PlayAnimation("PlayerGotchi_PierceLance");
    }

    public override void OnUpdate()
    {
    }

    public override void OnFinish()
    {
        OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Pierce);
    }
}

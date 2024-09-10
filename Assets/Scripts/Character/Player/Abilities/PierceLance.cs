using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class PierceLance : PlayerAbility
{
    [Header("PierceLance Parameters")]
    [SerializeField] float Projection = 0f;
    public float HitRadius = 3.5f;

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        //Debug.Log("On NETWORK SPAWN -> PierceLance");
        Animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart()
    {
        // hide the player
        Player.GetComponent<PlayerGotchi>().SetVisible(false);

        var scale = HitRadius / 3.5f;   /// 3.5f is the base size of the lance animation
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    public override void OnTeleport()
    {
        SetRotation(quaternion.identity);
        SetLocalPosition(PlayerAbilityCentreOffset);

        //PlayAnimation("PierceLance");
        PlayAnimationWithDuration("PierceLance", ExecutionDuration);

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

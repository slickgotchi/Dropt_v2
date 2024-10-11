using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShieldWallEffect : NetworkBehaviour
{
    [SerializeField] private Animator m_animator;

    [HideInInspector] public Wearable.NameEnum WearableNameEnum;
    [HideInInspector] public float SpinSpeed;

    private float timer_s = 10f;

    private bool m_isDamageReductionSet = false;
    private float m_ogDamageReduction = 0;

    private Wearable m_wearable;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_animator.Play("ShieldWallEffect");
        m_animator.speed = SpinSpeed;

        m_wearable = WearableManager.Instance.GetWearable(WearableNameEnum);
        var sprite = WeaponSpriteManager.Instance.GetSprite(WearableNameEnum, m_wearable.AttackView);

        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    public override void OnNetworkDespawn()
    {
        transform.parent.GetComponent<NetworkCharacter>().DamageReduction.Value = m_ogDamageReduction;

        base.OnNetworkDespawn();
    }


    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        if (!IsSpawned) return;

        if (!m_isDamageReductionSet)
        {
            m_ogDamageReduction = transform.parent.GetComponent<NetworkCharacter>().DamageReduction.Value;
            transform.parent.GetComponent<NetworkCharacter>().DamageReduction.Value =
                1f * Wearable.GetRarityMultiplier(m_wearable.Rarity);
            m_isDamageReductionSet = true;
        }

        timer_s -= Time.deltaTime;
        if (timer_s <= 0)
        {
            if (IsServer) gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}

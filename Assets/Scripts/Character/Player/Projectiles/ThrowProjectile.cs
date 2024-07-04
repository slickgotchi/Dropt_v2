using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ThrowProjectile : NetworkBehaviour
{
    public SpriteRenderer bodySpriteRenderer;
    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float Distance;
    [HideInInspector] public float Duration;
    [HideInInspector] public float LobHeight = 2f;

    [HideInInspector] public Wearable.NameEnum WearableNameEnum;

    [HideInInspector] public GameObject LocalPlayer;

    [HideInInspector] public PlayerAbility.NetworkRole NetworkRole = PlayerAbility.NetworkRole.LocalClient;

    private float m_timer = 0;
    private bool m_isSpawned = false;
    private float m_speed = 1;
    private Vector3 m_finalPosition;

    public void Fire()
    {
        gameObject.SetActive(true);

        transform.rotation = Quaternion.identity;

        m_timer = Duration;
        m_isSpawned = true;
        m_speed = Distance / Duration;

        m_finalPosition = transform.position + Direction * Distance;

        GetComponent<LobArc>().Reset();
        GetComponent<LobArc>().Duration_s = Duration;
        GetComponent<LobArc>().MaxHeight = LobHeight;

        var wearable = WearableManager.Instance.GetWearable(WearableNameEnum);
        var wearablesSprite = WeaponSpriteManager.Instance.GetSprite(WearableNameEnum, wearable.AttackView);
        bodySpriteRenderer.sprite = wearablesSprite;
    }

    private void Update()
    {
        if (!m_isSpawned) return;

        m_timer -= Time.deltaTime;

        if (m_timer < 0)
        {
            transform.position = m_finalPosition;
            gameObject.SetActive(false);
        }

        transform.position += Direction * m_speed * Time.deltaTime;
    }
}

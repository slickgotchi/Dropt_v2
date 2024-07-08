using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ThrowProjectile : NetworkBehaviour
{
    public SpriteRenderer bodySpriteRenderer;

    [Header("OnProjectileFinish Prefabs")]
    public GameObject StunExplosionPrefab;
    public GameObject BlindCloudPrefab;
    public GameObject LurePrefab;

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
            if (IsServer)
            {
                var spawnObject = InstantiateThrowFinishObject(WearableNameEnum);
                if (spawnObject != null)
                {
                    spawnObject.transform.position = m_finalPosition;
                    spawnObject.GetComponent<NetworkObject>().Spawn();
                }
            }

            transform.position = m_finalPosition;
            gameObject.SetActive(false);

        }

        transform.position += Direction * m_speed * Time.deltaTime;
    }

    GameObject InstantiateThrowFinishObject(Wearable.NameEnum wearableNameEnum)
    {
        GameObject throwFinishObject = null;
        switch (wearableNameEnum)
        {
            case Wearable.NameEnum._1337Laptop:
                throwFinishObject = Instantiate(StunExplosionPrefab);
                throwFinishObject.GetComponent<StunExplosion>().Radius = 5f;
                throwFinishObject.GetComponent<StunExplosion>().StunDuration = 3f;
                break;
            case Wearable.NameEnum.BabyBottle:
                throwFinishObject = Instantiate(BlindCloudPrefab);
                throwFinishObject.GetComponent<BlindCloud>().Radius = 3f;
                throwFinishObject.GetComponent<BlindCloud>().BlindDuration = 10f;
                break;
            case Wearable.NameEnum.CandyJaar:
                throwFinishObject = Instantiate(LurePrefab);
                throwFinishObject.GetComponent<Lure>().Hp = 100;
                throwFinishObject.GetComponent<Lure>().WearableNameEnum.Value = wearableNameEnum;
                break;
            case Wearable.NameEnum.Coconut:
                throwFinishObject = Instantiate(LurePrefab);
                throwFinishObject.GetComponent<Lure>().Hp = 100;
                throwFinishObject.GetComponent<Lure>().WearableNameEnum.Value = wearableNameEnum;
                break;
            case Wearable.NameEnum.DAOEgg:
                throwFinishObject = Instantiate(LurePrefab);
                throwFinishObject.GetComponent<Lure>().Hp = 100;
                throwFinishObject.GetComponent<Lure>().WearableNameEnum.Value = wearableNameEnum;
                break;
            case Wearable.NameEnum.GameController:
                throwFinishObject = Instantiate(StunExplosionPrefab);
                throwFinishObject.GetComponent<StunExplosion>().Radius = 1.5f;
                throwFinishObject.GetComponent<StunExplosion>().StunDuration = 1.5f;
                break;
            case Wearable.NameEnum.GemstoneRing:
                throwFinishObject = Instantiate(BlindCloudPrefab);
                throwFinishObject.GetComponent<BlindCloud>().Radius = 2f;
                throwFinishObject.GetComponent<BlindCloud>().BlindDuration = 7.5f;
                break;
            case Wearable.NameEnum.Lasso:
                throwFinishObject = Instantiate(StunExplosionPrefab);
                throwFinishObject.GetComponent<StunExplosion>().Radius = 1.5f;
                throwFinishObject.GetComponent<StunExplosion>().StunDuration = 1.5f;
                break;
            case Wearable.NameEnum.PaintPalette:
                throwFinishObject = Instantiate(BlindCloudPrefab);
                throwFinishObject.GetComponent<BlindCloud>().Radius = 5f;
                throwFinishObject.GetComponent<BlindCloud>().BlindDuration = 15f;
                break;
            case Wearable.NameEnum.PixelcraftSquare:
                throwFinishObject = Instantiate(BlindCloudPrefab);
                throwFinishObject.GetComponent<BlindCloud>().Radius = 2f;
                throwFinishObject.GetComponent<BlindCloud>().BlindDuration = 5f;
                break;
            case Wearable.NameEnum.SushiPiece:
                throwFinishObject = Instantiate(LurePrefab);
                throwFinishObject.GetComponent<Lure>().Hp = 200;
                throwFinishObject.GetComponent<Lure>().WearableNameEnum.Value = wearableNameEnum;
                break;
            case Wearable.NameEnum.WalkieTalkie:
                throwFinishObject = Instantiate(StunExplosionPrefab);
                throwFinishObject.GetComponent<StunExplosion>().Radius = 2f;
                throwFinishObject.GetComponent<StunExplosion>().StunDuration = 2f;
                break;
            default: break;
        }

        return throwFinishObject;
    }

}

using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Hierarchy;

public class BallisticShot : PlayerAbility
{
    [Header("BallisticShot Parameters")]
    public float Projection = 1.5f;
    public float Distance = 8f;
    public float Duration = 1f;

    [Header("Projectile Prefab")]
    public GameObject ProjectilePrefab;

    // variables for keeping track of the spawned projectile
    private GameObject m_projectile;
    private NetworkVariable<ulong> m_projectileId = new NetworkVariable<ulong>(0);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // instantiate/spawn our projectile we'll be using when this ability activates
            // and initially set to deactivated
            m_projectile = Instantiate(ProjectilePrefab);
            m_projectile.GetComponent<NetworkObject>().Spawn();
            m_projectileId.Value = m_projectile.GetComponent<NetworkObject>().NetworkObjectId;
            m_projectile.SetActive(false);
        }
    }

    private void Update()
    {
        // ensure remote clients associate projectile with m_projectile
        if (m_projectile == null && m_projectileId.Value > 0)
        {
            m_projectile = NetworkManager.SpawnManager.SpawnedObjects[m_projectileId.Value].gameObject;
            m_projectile.SetActive(false);
        }
    }

    public override void OnStart()
    {
        // set rotatin/local position
        SetRotation(quaternion.identity);
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        // activate projectile
        ActivateProjectile(ActivationInput.actionDirection, Distance, Duration);
    }

    void ActivateProjectile(Vector3 direction, float distance, float duration)
    {
        // Local Client & Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            m_projectile.SetActive(true);
            m_projectile.transform.position =
                Player.GetComponent<PlayerPrediction>().GetInterpPositionAtTick(ActivationInput.tick) 
                + new Vector3(0,0.5f,0);
            var no_projectile = m_projectile.GetComponent<BallisticShotProjectile>();
            no_projectile.Direction = direction;
            no_projectile.Distance = distance;
            no_projectile.Duration = duration;

            var playerCharacter = Player.GetComponent<NetworkCharacter>();
            no_projectile.DamagePerHit = playerCharacter.AttackPower.Value;
            no_projectile.CriticalChance = playerCharacter.CriticalChance.Value;
            no_projectile.CriticalDamage = playerCharacter.CriticalDamage.Value;
            no_projectile.Role = IsServer ? PlayerAbility.NetworkRole.Server : PlayerAbility.NetworkRole.LocalClient;

            no_projectile.Fire();
        }

        // Server Only
        if (IsServer)
        {
            ActivateProjectileClientRpc(m_projectile.transform.position, direction, distance, duration);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateProjectileClientRpc(Vector3 startPosition, Vector3 direction, float distance, float duration)
    {
        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            m_projectile.SetActive(true);
            m_projectile.transform.position = startPosition;
            //m_projectile.transform.position = transform.position + new Vector3(0, 0.5f, 0);
            //m_projectile.transform.position =
            //    Player.GetComponent<PlayerPrediction>().GetInterpPositionAtTick(ActivationInput.tick)
            //    + new Vector3(0, 0.5f, 0);
            var no_projectile = m_projectile.GetComponent<BallisticShotProjectile>();
            no_projectile.Direction = direction;
            no_projectile.Distance = distance;
            no_projectile.Duration = duration;

            var playerCharacter = Player.GetComponent<NetworkCharacter>();
            no_projectile.DamagePerHit = playerCharacter.AttackPower.Value;
            no_projectile.CriticalChance = playerCharacter.CriticalChance.Value;
            no_projectile.CriticalDamage = playerCharacter.CriticalDamage.Value;
            no_projectile.Role = PlayerAbility.NetworkRole.RemoteClient;

            no_projectile.Fire();
        }
    }

    public override void OnUpdate()
    {

    }

    public override void OnFinish()
    {
    }
}

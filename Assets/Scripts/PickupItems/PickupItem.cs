using Unity.Netcode;
using UnityEngine;

public sealed class PickupItem : NetworkBehaviour
{
    private readonly float m_distanceForMagnet = 0.3f;
    private const float m_speed = 15f;
    private GameObject m_target;

    private void Update()
    {
        if (m_target == null) return;

        float step = m_speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, m_target.transform.position, step);

        if (!IsServer) return;

        if (GetDistanceTo(m_target) > m_distanceForMagnet) return;

        PlayerPickupItemMagnet magnet = m_target.GetComponentInChildren<PlayerPickupItemMagnet>();
        magnet?.Collect(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        gameObject.SetActive(true);
    }

    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);
        base.OnNetworkDespawn();
    }

    private void OnDisable()
    {
        m_target = null;
    }

    private float GetDistanceTo(GameObject target)
    {
        return Vector2.Distance(transform.position, target.transform.position);
    }

    public bool TryGoTo(GameObject target)
    {
        if (m_target == null)
        {
            m_target = target;
            return true;
        }

        if (GetDistanceTo(target) > GetDistanceTo(m_target))
        {
            return false;
        }

        m_target = target;
        return true;
    }

    public bool AllowToPick()
    {
        return m_target == null;
    }
}
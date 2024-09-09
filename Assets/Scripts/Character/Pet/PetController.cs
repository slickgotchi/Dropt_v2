using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System.Collections.Generic;

public class PetController : NetworkBehaviour
{
    [SerializeField] private PetSettings m_petSettings;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private LayerMask m_pickItemLayer;

    private PetStateMachine m_petStateMachine;

    private NavMeshAgent m_agent;

    private Transform m_petOwner;
    private Transform m_transform;

    public Sprite LeftSprite;
    public Sprite RightSprite;
    public Sprite UpSprite;
    public Sprite DownSprite;

    private readonly List<PickupItem> m_pickUpItemsInRadius = new List<PickupItem>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
        {
            return;
        }

        InitializeNavmeshAgent();
        m_transform = transform;
        m_petOwner = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
        m_petStateMachine = new PetStateMachine(this);
        m_petStateMachine.ChangeState(m_petStateMachine.PetFollowOwnerState);
    }

    private void InitializeNavmeshAgent()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_agent.updateRotation = false;
        m_agent.speed = m_petSettings.Speed;
        m_agent.stoppingDistance = m_petSettings.OffsetDistance;
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        m_petStateMachine.Update();
    }

    public void FollowOwnner()
    {
        _ = m_agent.SetDestination(m_petOwner.position);
    }

    public void FollowPickUpItem(Transform pickUpItem)
    {
        _ = m_agent.SetDestination(pickUpItem.position);
    }

    public void PickItem(PickupItem pickupItem)
    {
        m_pickUpItemsInRadius.Remove(pickupItem);
        if (IsServer)
        {
            pickupItem.Pick(OwnerClientId);
        }
        else
        {
            pickupItem.PickedByServerRpc(OwnerClientId);
        }
    }

    public PickupItem GetPickUpItemFromList()
    {
        return m_pickUpItemsInRadius[0];
    }

    public bool IsPetReachToDestination()
    {
        return m_agent.remainingDistance <= m_agent.stoppingDistance;
    }

    public bool IsPlayerOutOfTeleportRange()
    {
        float distanceToPlayer = Vector3.Distance(m_transform.position, m_petOwner.position);
        return distanceToPlayer > m_petSettings.TeleportDistance;
    }

    public void TeleportCloseToPlayer()
    {
        Vector3 randomDirection = Random.insideUnitSphere * m_petSettings.OffsetDistance;
        randomDirection += m_petOwner.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, m_petSettings.OffsetDistance, NavMesh.AllAreas))
        {
            m_agent.Warp(navHit.position);
        }
    }

    public void LookForPickupItems()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(new Vector2(m_transform.position.x, m_transform.position.y),
                                                            m_petSettings.ItemPickupRadius,
                                                            m_pickItemLayer);

        if (hitColliders.Length > 0)
        {
            foreach (Collider2D hitCollider in hitColliders)
            {
                PickupItem pickupItem = hitCollider.GetComponent<PickupItem>();
                if (!pickupItem.AllowToPick())
                {
                    continue;
                }

                if (!m_pickUpItemsInRadius.Contains(pickupItem))
                {
                    m_pickUpItemsInRadius.Add(pickupItem);
                }
            }
        }
    }

    public bool IsPickupItemsInRange()
    {
        return m_pickUpItemsInRadius.Count > 0;
    }

    public void ClearPicupItemList()
    {
        m_pickUpItemsInRadius.Clear();
    }

    public void SetFacingDirection()
    {
        Vector3 velocity = m_agent.velocity;
        if (velocity.magnitude > 0.1f)
        {
            ChangeSprite(Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y) ?
               (velocity.x > 0 ? Direction.Right : Direction.Left)
               : (velocity.y > 0 ? Direction.Up : Direction.Down));
        }
    }

    private void ChangeSprite(Direction direction)
    {
        if (IsServer)
        {
            SetFacingSpriteClientRpc(direction);
        }
        else
        {
            SetFacingSpriteServerRpc(direction);
        }
    }

    [ServerRpc]
    public void SetFacingSpriteServerRpc(Direction direction)
    {
        SetFacingSpriteClientRpc(direction);
    }

    [ClientRpc]
    public void SetFacingSpriteClientRpc(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                m_spriteRenderer.sprite = LeftSprite;
                break;
            case Direction.Right:
                m_spriteRenderer.sprite = RightSprite;
                break;
            case Direction.Up:
                m_spriteRenderer.sprite = UpSprite;
                break;
            case Direction.Down:
                m_spriteRenderer.sprite = DownSprite;
                break;
        }
    }
}

public enum Direction
{
    Left,
    Right,
    Up,
    Down
}
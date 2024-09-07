using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class PetController : NetworkBehaviour
{
    [SerializeField] private PetSettings m_petSettings;
    [SerializeField] private SpriteRenderer m_spriteRenderer;

    private NavMeshAgent m_agent;

    private Transform m_petOwner;
    private Transform m_transform;
    private bool m_allowToFollowOwner;

    public Sprite LeftSprite;
    public Sprite RightSprite;
    public Sprite UpSprite;
    public Sprite DownSprite;

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
        m_allowToFollowOwner = true;
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

        if (!m_allowToFollowOwner)
        {
            return;
        }

        _ = m_agent.SetDestination(m_petOwner.position);
        SetFacingDirection();
        float distanceToPlayer = Vector3.Distance(m_transform.position, m_petOwner.position);
        if (distanceToPlayer > m_petSettings.TeleportDistance)
        {
            TeleportCloseToPlayer();
        }
    }

    private void TeleportCloseToPlayer()
    {
        Vector3 randomDirection = Random.insideUnitSphere * m_petSettings.OffsetDistance;
        randomDirection += m_petOwner.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, m_petSettings.OffsetDistance, NavMesh.AllAreas))
        {
            m_agent.Warp(navHit.position);
            Debug.Log("Pet teleported to: " + navHit.position);
        }
    }

    private void SetFacingDirection()
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
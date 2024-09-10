public class PetFollowPickupItemState : PetState
{
    private PickupItem m_currentPickUpItem;

    public PetFollowPickupItemState(PetController petController, PetStateMachine petStateMachine) : base(petController, petStateMachine)
    {
    }

    public override void Enter()
    {
        m_PetController.InitializeNavMeshAgentWhenFillowPickUpItem();
        FindPickUpItemsInRangeAndFollowIt();
    }

    public override void Exit()
    {
        m_PetController.ClearPicupItemList();
    }

    public override void Update()
    {
        if (m_currentPickUpItem == null)
        {
            FindPickUpItemsInRangeAndFollowIt();
        }

        m_PetController.SetFacingDirection();
        if (m_PetController.IsPlayerOutOfTeleportRange())
        {
            m_PetController.TeleportCloseToPlayer();
            m_PetStateMachine.ChangeState(m_PetStateMachine.PetFollowOwnerState);
        }
        m_PetController.LookForPickupItems();
        if (m_PetController.IsPetReachToDestination())
        {
            m_PetController.PickItem(m_currentPickUpItem);
            FindPickUpItemsInRangeAndFollowIt();
        }
    }

    private void FindPickUpItemsInRangeAndFollowIt()
    {
        if (m_PetController.IsPickupItemsInRange())
        {
            m_currentPickUpItem = m_PetController.GetPickUpItemFromList();
            m_PetController.FollowPickUpItem(m_currentPickUpItem.transform);
        }
        else
        {
            m_PetStateMachine.ChangeState(m_PetStateMachine.PetFollowOwnerState);
        }
    }
}

using UnityEngine;

public class PetFollowOwnerState : PetState
{
    public PetFollowOwnerState(PetController petController, PetStateMachine petStateMachine) : base(petController, petStateMachine)
    {

    }

    public override void Enter()
    {
        Debug.Log("Enter Owner follow State");
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        m_PetController.FollowOwnner();
        m_PetController.SetFacingDirection();
        if (m_PetController.IsPlayerOutOfTeleportRange())
        {
            m_PetController.TeleportCloseToPlayer();
        }
        m_PetController.LookForPickupItems();

        if (m_PetController.IsPickupItemsInRange())
        {
            m_PetStateMachine.ChangeState(m_PetStateMachine.PetFollowPickupItemState);
        }
    }
}

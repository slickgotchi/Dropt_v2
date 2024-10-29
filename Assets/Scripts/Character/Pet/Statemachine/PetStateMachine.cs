using System;

public class PetStateMachine
{
    public PetState CurrentState { get; private set; }

    public readonly PetFollowOwnerState PetFollowOwnerState;
    public readonly PetDeactivateState PetDeactivateState;
    public readonly PetAttackState PetAttackState;
    //public readonly PetFollowPickupItemState PetFollowPickupItemState;

    public PetStateMachine(PetController petController)
    {
        PetFollowOwnerState = new PetFollowOwnerState(petController, this);
        PetDeactivateState = new PetDeactivateState(petController, this);
        PetAttackState = new PetAttackState(petController, this);
        //PetFollowPickupItemState = new PetFollowPickupItemState(petController, this);
    }

    public void ChangeState(PetState petState)
    {
        CurrentState?.Exit();
        CurrentState = petState;
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}

public class PetStateMachine
{
    public PetState CurrentState { get; private set; }

    public readonly PetFollowOwnerState PetFollowOwnerState;
    public readonly PetFollowPickupItemState PetFollowPickupItemState;

    public PetStateMachine(PetController petController)
    {
        PetFollowOwnerState = new PetFollowOwnerState(petController, this);
        PetFollowPickupItemState = new PetFollowPickupItemState(petController, this);
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

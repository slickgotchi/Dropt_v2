public abstract class PetState
{
    protected PetController m_PetController;
    protected PetStateMachine m_PetStateMachine;

    public PetState(PetController petController, PetStateMachine petStateMachine)
    {
        m_PetController = petController;
        m_PetStateMachine = petStateMachine;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}

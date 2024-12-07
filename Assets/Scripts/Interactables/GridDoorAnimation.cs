using UnityEngine;

public class GridDoorAnimation : MonoBehaviour, IDoorAnimation
{
    private Animator m_animator;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void OpenDoor()
    {
        m_animator.Play("ApeDoor_Open");
    }
}
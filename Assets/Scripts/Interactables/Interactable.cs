using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Interactable : NetworkBehaviour
{
    public enum Status
    {
        Inactive,
        Active,
    }
    public Status status;

    public virtual void OnStartInteraction() { }

    public virtual void OnUpdateInteraction() { }

    public virtual void OnFinishInteraction() { }
}

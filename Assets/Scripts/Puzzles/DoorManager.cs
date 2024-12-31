using System.Collections.Generic;
using UnityEngine;
using System;

public class DoorManager<T> : MonoBehaviour where T : Enum
{
    public static DoorManager<T> Instance { get; private set; }

    private readonly List<Door<T>> doors = new List<Door<T>>();
    private readonly List<DoorButton<T>> buttons = new List<DoorButton<T>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterDoor(Door<T> door)
    {
        if (!doors.Contains(door))
        {
            doors.Add(door);
        }
    }

    public void UnregisterDoor(Door<T> door)
    {
        doors.Remove(door);
    }

    public void RegisterButton(DoorButton<T> button)
    {
        if (!buttons.Contains(button))
        {
            buttons.Add(button);
        }
    }

    public void UnregisterButton(DoorButton<T> button)
    {
        buttons.Remove(button);
    }

    public List<Door<T>> GetDoors() => new List<Door<T>>(doors);
    public List<DoorButton<T>> GetButtons() => new List<DoorButton<T>>(buttons);
}


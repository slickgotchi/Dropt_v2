using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

//[ExecuteInEditMode]
public class EnemyCharacter : NetworkCharacter
{
    private void Start()
    {
        //NetworkCharacter[] networkCharacters = GetComponents<NetworkCharacter>();
        //if (networkCharacters.Length < 2)
        //{
        //    return;
        //}

        //CopyComponentValues(networkCharacters[0], networkCharacters[1]);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

    }

    void CopyComponentValues(NetworkCharacter source, NetworkCharacter target)
    {
        // Get all fields and properties of the source type
        var fields = typeof(NetworkCharacter).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var properties = typeof(NetworkCharacter).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        // Copy each field
        foreach (var field in fields)
        {
            field.SetValue(target, field.GetValue(source));
        }

        // Copy each property
        foreach (var property in properties)
        {
            if (property.CanWrite)
            {
                property.SetValue(target, property.GetValue(source));
            }
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New BuffObject", menuName = "Buffs/BuffObject")]
public class BuffObject : ScriptableObject
{
    public List<Buff> buffs = new List<Buff>();
}

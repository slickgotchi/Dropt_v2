using UnityEngine;

[CreateAssetMenu(fileName = "New JoostObject", menuName = "Joost/JoostObject")]
public class JoostObject : ScriptableObject
{
    public Sprite sprite;
    public Joost.Type joostType;
    [TextArea]
    public string description;
    public int cost;
    public BuffObject buffObject;
}

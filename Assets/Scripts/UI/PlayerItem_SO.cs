using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Player Item", menuName = "ScriptableObject/PlayerItem")]
public class PlayerItem_SO : ScriptableObject
{
    public enum PlayerItemType { Bomb, PortaHole, ZenCricket }

    public PlayerItemType playerItemType;
    public KeyCode keyCode;
    public Sprite sprite;
}

using UnityEngine;

[CreateAssetMenu(fileName = "WearableSprites", menuName = "ScriptableObjects/WearableSprites", order = 1)]
public class WearableSprites : ScriptableObject
{
    public Wearable.NameEnum Wearable;
    public Sprite FrontView;
    public Sprite RightView;
    public Sprite LeftView;
    public Sprite BackView;
}

using UnityEngine;

[CreateAssetMenu(fileName = "WearableSprite", menuName = "ScriptableObjects/WearableSprite", order = 1)]
public class WearableSprite : ScriptableObject
{
    public Wearable.NameEnum WearableNameEnum;

    public Sprite FrontView;
    // the order indicates if this sprite renders behind or in front of the players hand
    // for this particular view
    // 1 = renders behind hand, 3 = renders in front of hand
    public int FrontViewOrder = 1;  

    public Sprite RightView;
    public int RightViewOrder = 1;

    public Sprite LeftView;
    public int LeftViewOrder = 1;

    public Sprite BackView;
    public int BackViewOrder = 1;
}

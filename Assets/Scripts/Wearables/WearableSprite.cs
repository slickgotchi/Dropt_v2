using UnityEngine;

[CreateAssetMenu(fileName = "WearableSprite", menuName = "ScriptableObjects/WearableSprite", order = 1)]
public class WearableSprite : ScriptableObject
{
    [Header("Name")]
    public Wearable.NameEnum WearableNameEnum;

    [Header("Sprites")]
    public Sprite FrontView;
    public Sprite RightView;
    public Sprite LeftView;
    public Sprite BackView;

    [Header("Render Order (1 = behind hand, 0 = in front of hand)")]
    // the order indicates if this sprite renders behind or in front of the players hand
    // for this particular view
    // 1 = renders behind hand, 3 = renders in front of hand
    public int FrontViewOrder = 1;  
    public int RightViewOrder = 1;
    public int LeftViewOrder = 1;
    public int BackViewOrder = 1;

    [Header("DisplayOffset")]
    public Vector3 FrontOffset = new Vector3(0, 0.25f, 0);
    public Vector3 RightOffset = new Vector3(0, 0.25f, 0);
    public Vector3 LeftOffset = new Vector3(0, 0.25f, 0);
    public Vector3 BackOffset = new Vector3(0, 0.25f, 0);
}

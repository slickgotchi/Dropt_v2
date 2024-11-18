using UnityEngine;

[CreateAssetMenu(fileName = "SvgWearable", menuName = "ScriptableObjects/SvgWearable", order = 1)]
public class SvgWearableSO : ScriptableObject
{
    public Sprite SvgSprite;
    public Wearable.NameEnum Name;
}

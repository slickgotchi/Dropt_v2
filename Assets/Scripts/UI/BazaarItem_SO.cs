using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bazaar Item", menuName = "ScriptableObject/BazaarItem")]
public class BazaarItem_SO : ScriptableObject
{
    public enum PurchaseType { Bomb, PortaHole, HealSalveRecharge, Ecto }

    public Sprite sprite;
    public string titleText;
    public string descriptionText;
    public int ghstCost;

    public PurchaseType purchaseType;
    public int purchaseQty;
}

using System.Collections.Generic;
using UnityEngine;

public class ShieldDataContainer : MonoBehaviour
{
    private Dictionary<Wearable.NameEnum, ShieldData> m_equipedShieldDataForLeftHand = new Dictionary<Wearable.NameEnum, ShieldData>();
    private Dictionary<Wearable.NameEnum, ShieldData> m_equipedShieldDataForRightHand = new Dictionary<Wearable.NameEnum, ShieldData>();

    public bool HasShieldData(Wearable.NameEnum nameEnum, Hand hand)
    {
        return hand == Hand.Left ? m_equipedShieldDataForLeftHand.ContainsKey(nameEnum)
                                 : m_equipedShieldDataForRightHand.ContainsKey(nameEnum);
    }

    public ShieldData GetShieldData(Wearable.NameEnum nameEnum, Hand hand)
    {
        return hand == Hand.Left ? m_equipedShieldDataForLeftHand[nameEnum]
                                 : m_equipedShieldDataForRightHand[nameEnum];
    }

    public void SetShieldData(Wearable.NameEnum nameEnum, Hand hand, ShieldData shieldData)
    {
        if (hand == Hand.Left)
        {
            m_equipedShieldDataForLeftHand.Add(nameEnum, shieldData);
        }
        else
        {
            m_equipedShieldDataForRightHand.Add(nameEnum, shieldData);
        }
    }
}

public class ShieldData
{
    public readonly ShieldBlockData ShieldBlockData;
    public readonly ShieldBlockStateMachine ShieldBlockStateMachine;

    public ShieldData(ShieldBlockData shieldBlockData, ShieldBlockStateMachine shieldBlockStateMachine)
    {
        ShieldBlockData = shieldBlockData;
        ShieldBlockStateMachine = shieldBlockStateMachine;
    }
}

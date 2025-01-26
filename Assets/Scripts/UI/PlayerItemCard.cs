using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerItemCard : MonoBehaviour
{
    [Header("Assign in Scene")]
    [SerializeField] private PlayerItem_SO m_playerItem_SO;

    [Header("Assign in Prefab")]
    [SerializeField] private Image m_image;
    [SerializeField] private TextMeshProUGUI m_numberKeyText;
    [SerializeField] private TextMeshProUGUI m_remainingText;

    private void Awake()
    {
    }

    private void OnDestroy()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_playerItem_SO == null)
        {
            Debug.LogWarning("No BazaarItem_SO assigned to BazaarItemCard!");
            return;
        }

        m_image.sprite = m_playerItem_SO.sprite;

        m_numberKeyText.text = ConvertKeyCodeToString(m_playerItem_SO.keyCode);

        var playerRemainingCount = 0;
        var playerCapacityCount = 100;

        m_remainingText.text = playerRemainingCount + " / " + playerCapacityCount;
    }

    private void Update()
    {
        var localPlayerOffchainData = PlayerHUDCanvas.Instance.m_localPlayerOffchainData;
        if (localPlayerOffchainData == null) return;

        switch (m_playerItem_SO.playerItemType)
        {
            case PlayerItem_SO.PlayerItemType.Bomb:

                int bombBalance = LevelManager.Instance.IsDegenapeVillage() ?
                    localPlayerOffchainData.m_bombLiveBalance_wallet.Value :
                    localPlayerOffchainData.m_bombLiveCount_dungeon.Value;
                int bombCapacity = localPlayerOffchainData.m_bombCapacity_gotchi.Value;

                m_remainingText.text = bombBalance + " / " + bombCapacity;

                break;
            case PlayerItem_SO.PlayerItemType.PortaHole:

                int portaHoleBalance = LevelManager.Instance.IsDegenapeVillage() ?
                    localPlayerOffchainData.m_portaHoleLiveBalance_wallet.Value :
                    localPlayerOffchainData.m_portaHoleLiveCount_dungeon.Value;
                int portaHoleCapacity = localPlayerOffchainData.m_portaHoleCapacity_gotchi.Value;

                m_remainingText.text = portaHoleBalance + " / " + portaHoleCapacity;

                break;
            case PlayerItem_SO.PlayerItemType.ZenCricket:

                int zenCricketBalance = LevelManager.Instance.IsDegenapeVillage() ?
                    localPlayerOffchainData.m_zenCricketLiveBalance_wallet.Value :
                    localPlayerOffchainData.m_zenCricketLiveCount_dungeon.Value;
                int zenCricketCapacity = localPlayerOffchainData.m_zenCricketCapacity_gotchi.Value;

                m_remainingText.text = zenCricketBalance + " / " + zenCricketCapacity;

                break;
            default:
                break;
        }
    }

    public static string ConvertKeyCodeToString(KeyCode key)
    {
        string keyString = key.ToString();

        // Handle AlphaX (e.g., Alpha1 -> "1")
        if (keyString.StartsWith("Alpha"))
        {
            return keyString.Substring(5); // Remove "Alpha"
        }
        // Handle KeypadX (e.g., Keypad1 -> "1")
        else if (keyString.StartsWith("Keypad"))
        {
            return keyString.Substring(6); // Remove "Keypad"
        }
        // Handle letters (e.g., A -> "A", b -> "B")
        else if (key >= KeyCode.A && key <= KeyCode.Z)
        {
            return keyString.ToUpper(); // Ensure it's uppercase
        }

        // Default case: Return the string as-is for special keys (e.g., Space, Escape)
        return keyString;
    }

}

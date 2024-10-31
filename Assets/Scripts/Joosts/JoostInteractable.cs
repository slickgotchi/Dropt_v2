using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class JoostInteractable : Interactable
{
    public Joost.Type type = Joost.Type.Null;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    public int NumberLevels = 3;

    private Sprite m_sprite;
    private string m_name;
    private string m_description;
    private int m_cost;
    [HideInInspector] BuffObject BuffObject;

    private void Start()
    {
        Init(type);
    }

    public void Init(Joost.Type joostType)
    {
        if (JoostDataManager.Instance == null)
        {
            Debug.Log("no joost data manager");
            return;
        }

        var joostData = JoostDataManager.Instance.GetJoostData(joostType);
        m_sprite = joostData.sprite;
        m_name = AddSpacesToCamelCase(joostType.ToString());
        m_description = joostData.description;
        m_cost = joostData.cost;
        BuffObject = joostData.buffObject;

        m_spriteRenderer.sprite = m_sprite;
        //Debug.Log(m_spriteRenderer.sprite);
    }

    public override void OnTriggerStartInteraction()
    {
        JoostInteractionCanvas.Instance.Container.SetActive(true);
        JoostInteractionCanvas.Instance.Init(m_name, m_description, m_cost.ToString());
    }

    public override void OnTriggerUpdateInteraction()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Try consume joost!");
            TryAddJoostBuffServerRpc();
        }
    }

    public override void OnTriggerFinishInteraction()
    {
        JoostInteractionCanvas.Instance.Container.SetActive(false);
    }

    [Rpc(SendTo.Server)]
    void TryAddJoostBuffServerRpc()
    {
        var playerController = GetPlayerController();
        var playerDungeonData = playerController.GetComponent<PlayerOffchainData>();
        var cGhst = playerDungeonData.ectoCount_dungeon.Value;

        // check we have enough cGHST
        if (m_cost > cGhst) return;

        var levelCountedBuffObject = new GameObject();
        var levelCountedBuff = levelCountedBuffObject.AddComponent<LevelCountedBuff>();
        bool isBuffAdded = levelCountedBuff.TryInit(BuffObject, playerController.GetComponent<NetworkCharacter>(), NumberLevels);
        if (!isBuffAdded) return;

        // deduct cGHST
        playerDungeonData.ectoCount_dungeon.Value -= m_cost;
    }

    private string AddSpacesToCamelCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        System.Text.StringBuilder newText = new System.Text.StringBuilder(text.Length * 2);
        newText.Append(text[0]);

        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
            {
                newText.Append(' ');
            }
            newText.Append(text[i]);
        }

        return newText.ToString();
    }
}

namespace Joost
{
    public enum Type
    {
        Null,
        FrozenSlush, GalaxyJuice, WarmMilk, GallonOfMilk, CupOfCoffee,
        ChocolateMartini, ChocolateMilkshake, BatteryJuice, Glue,
        KeroseneKola, AbsintheForAlgernon, Frugalccino,
        DeadMansGambit, HammurabisGambit, ChocoCritCookies,
        EpoxyEcclair, KeroseneCake, FreakyFruitCake
    }
}

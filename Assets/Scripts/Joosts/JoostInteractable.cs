using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
    }

    public override void OnTriggerEnter2DInteraction()
    {
        base.OnTriggerEnter2DInteraction();

        PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
        JoostInteractionCanvas.Instance.Container.SetActive(true);
        JoostInteractionCanvas.Instance.Init(m_name, m_description, m_cost.ToString());
    }

    public override void OnTriggerExit2DInteraction()
    {
        base.OnTriggerExit2DInteraction();

        PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
        JoostInteractionCanvas.Instance.Container.SetActive(false);
    }

    public override void OnInteractHoldFinish()
    {
        TryAddJoostBuffServerRpc();
    }

    [Rpc(SendTo.Server)]
    void TryAddJoostBuffServerRpc()
    {
        TryAddJoostBufferServerRpcAsync();
    }

    private async UniTaskVoid TryAddJoostBufferServerRpcAsync()
    {
        var playerController = GetPlayerController();
        var playerDungeonData = playerController.GetComponent<PlayerOffchainData>();

        bool isSuccess = await playerDungeonData.RemoveEcto(m_cost);

        // check we have enough cGHST
        if (!isSuccess) return;

        var levelCountedBuffObject = new GameObject();
        var levelCountedBuff = levelCountedBuffObject.AddComponent<LevelCountedBuff>();
        bool isBuffAdded = levelCountedBuff.TryInit(BuffObject, playerController.GetComponent<NetworkCharacter>(), NumberLevels);
        if (!isBuffAdded) return;

        Debug.Log("Applied buff");
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

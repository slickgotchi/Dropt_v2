using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoostInteractable : Interactable
{
    public Joost.Type type = Joost.Type.Null;
    [SerializeField] private SpriteRenderer m_spriteRenderer;

    private Sprite m_sprite;
    private string m_name;
    private string m_description;
    private int m_cost;
    [HideInInspector] BuffObject BuffObject;

    private void Awake()
    {
        Init(type);
    }

    public void Init(Joost.Type joostType)
    {
        var joostData = JoostDataManager.Instance.GetJoostData(joostType);
        m_sprite = joostData.sprite;
        m_name = AddSpacesToCamelCase(joostType.ToString());
        m_description = joostData.description;
        m_cost = joostData.cost;
        BuffObject = joostData.buffObject;

        m_spriteRenderer.sprite = m_sprite;
    }

    public override void OnStartInteraction()
    {
        JoostInteractionCanvas.Instance.Container.SetActive(true);
        JoostInteractionCanvas.Instance.Init(m_name, m_description, m_cost.ToString());
    }

    public override void OnUpdateInteraction()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Try consume joost!");
        }
    }

    public override void OnFinishInteraction()
    {
        JoostInteractionCanvas.Instance.Container.SetActive(false);
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

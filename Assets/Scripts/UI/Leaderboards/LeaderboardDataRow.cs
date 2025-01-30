using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;
using UnityEngine.UI;

public class LeaderboardDataRow : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI RankText;
    [SerializeField] TextMeshProUGUI GotchiText;
    [SerializeField] TextMeshProUGUI IdText;
    [SerializeField] TextMeshProUGUI AddressText;
    [SerializeField] TextMeshProUGUI GhstText;
    //[SerializeField] TextMeshProUGUI FormationText;
    [SerializeField] TextMeshProUGUI DustText;
    [SerializeField] TextMeshProUGUI KillsText;
    [SerializeField] TextMeshProUGUI TimeText;

    [SerializeField] GameObject m_formationTrio;
    [SerializeField] GameObject m_formationDuo;
    [SerializeField] GameObject m_formationSolo;

    [SerializeField] GameObject GhstIcon;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Set(
        int rank,
        string gotchi,
        int id,
        string address,
        int ghst,
        string formation,
        int dust,
        int kills,
        int time
        )
    {
        RankText.text = rank.ToString();
        GotchiText.text = gotchi;
        IdText.text = id.ToString();
        AddressText.text = ShortenString(address);
        GhstText.text = ghst == 0 ? "-" : ghst.ToString();
        SetFormation(formation);
        DustText.text = dust.ToString("N0");
        KillsText.text = kills.ToString("N0");
        TimeText.text = ConvertTimeInSecondsToString(time);
        gameObject.SetActive(true);

        if (ghst == 0)
        {
            GhstIcon.SetActive(false);
        }
    }

    public string ShortenString(string input)
    {
        if (input.Length <= 10) // If the string is too short, return it as is
            return input;

        string firstPart = input.Substring(0, 6);
        string lastPart = input.Substring(input.Length - 4);

        return firstPart + "..." + lastPart;
    }


    private void SetFormation(string formation)
    {
        m_formationTrio.SetActive(false);
        m_formationDuo.SetActive(false);
        m_formationSolo.SetActive(false);

        if (formation == "trio") m_formationTrio.SetActive(true);
        else if (formation == "duo") m_formationDuo.SetActive(true);
        else if (formation == "solo") m_formationSolo.SetActive(true);
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    string ConvertTimeInSecondsToString(int time)
    {
        int hours = (int)math.floor((float)time / 3600);
        time -= hours * 3600;
        int mins = (int)math.floor((float)time / 60);
        time -= mins * 60;
        return $"{hours}h {mins}m {time}s";
    }
}

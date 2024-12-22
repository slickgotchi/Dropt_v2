using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;

public class LeaderboardDataRow : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI RankText;
    [SerializeField] TextMeshProUGUI GotchiText;
    [SerializeField] TextMeshProUGUI IdText;
    [SerializeField] TextMeshProUGUI AddressText;
    [SerializeField] TextMeshProUGUI GhstText;
    [SerializeField] TextMeshProUGUI FormationText;
    [SerializeField] TextMeshProUGUI DustText;
    [SerializeField] TextMeshProUGUI KillsText;
    [SerializeField] TextMeshProUGUI TimeText;

    private void Start()
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
        AddressText.text = address;
        GhstText.text = ghst.ToString();
        FormationText.text = formation;
        DustText.text = dust.ToString();
        KillsText.text = kills.ToString();
        TimeText.text = ConvertTimeInSecondsToString(time);
        gameObject.SetActive(true);
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

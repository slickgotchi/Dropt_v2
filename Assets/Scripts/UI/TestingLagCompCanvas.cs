using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestingLagCompCanvas : MonoBehaviour
{
    public static TestingLagCompCanvas Instance { get; private set; }

    public TMPro.TextMeshProUGUI posText;

    public float m = 1.2f;
    public float c = 0.29f;

    [SerializeField] private Button mUpBtn;
    [SerializeField] private Button mDownBtn;
    [SerializeField] private Button cUpBtn;
    [SerializeField] private Button cDownBtn;

    [SerializeField] private TMPro.TextMeshProUGUI mText;
    [SerializeField] private TMPro.TextMeshProUGUI cText;

    private void Awake()
    {
        mUpBtn.onClick.AddListener(() => { m += 0.1f; });
        mDownBtn.onClick.AddListener(() => { m -= 0.1f; });
        cUpBtn.onClick.AddListener(() => { c += 0.01f; });
        cDownBtn.onClick.AddListener(() => { c -= 0.01f; });

        Instance = this; 
    }

    private void Update()
    {
        // find roam shade
        var roamShades = FindObjectsByType<Dropt.EnemyAI_RoamShade>(FindObjectsSortMode.None);
        if (roamShades == null) return;

        foreach (var rs in roamShades)
        {
            posText.text = "RoamShade Pos: " + rs.transform.position.y;
        }

        mText.text = "m = " + m.ToString("F2");
        cText.text = "c = " + c.ToString("F2");
    }
}

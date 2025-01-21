using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestingLagCompCanvas : MonoBehaviour
{
    public static TestingLagCompCanvas Instance { get; private set; }

    public TMPro.TextMeshProUGUI posText;

    [HideInInspector] public float m = 1f;
    [HideInInspector] public float c = 0.25f;

    [SerializeField] private Button mUpBtn;
    [SerializeField] private Button mDownBtn;
    [SerializeField] private Button cUpBtn;
    [SerializeField] private Button cDownBtn;

    [SerializeField] private TMPro.TextMeshProUGUI mText;
    [SerializeField] private TMPro.TextMeshProUGUI cText;

    private void Awake()
    {
        mUpBtn.onClick.AddListener(() => { m += 0.01f; });
        mDownBtn.onClick.AddListener(() => { m -= 0.01f; });
        cUpBtn.onClick.AddListener(() => { c += 0.005f; });
        cDownBtn.onClick.AddListener(() => { c -= 0.005f; });

        Instance = this; 
    }

    private void Start()
    {
        m = 0.32f;
        c = 0.21f;
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

        mText.text = "m = " + m.ToString("F3");
        cText.text = "c = " + c.ToString("F3");
    }
}

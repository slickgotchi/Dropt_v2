using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Netcode;
using GotchiHub;

public class PlayerSVGs : NetworkBehaviour
{
    private NetworkVariable<int> GotchiId = new NetworkVariable<int>(-1);
    private GotchiSvgSet GotchiSvgSet;

    float k_pollInterval = 1f;
    float m_pollTimer = 1f;

    private void Start()
    {
        GotchiDataManager.Instance.onSelectedGotchi += HandleOnSelectedGotchi;
    }

    void HandleOnSelectedGotchi(int id)
    {
        if (!IsLocalPlayer) return;
        UpdateGotchiIdServerRpc(id);
    }

    [Rpc(SendTo.Server)]
    public void UpdateGotchiIdServerRpc(int gotchiId)
    {
        GotchiId.Value = gotchiId;
    }

    private void Update()
    {
        if (IsServer && !IsHost) return;

        m_pollTimer -= Time.deltaTime;
        if (m_pollTimer > 0) return;
        m_pollTimer = k_pollInterval;

        if (GotchiId.Value < 0) return;

        var newGotchiSvgs = GotchiDataManager.Instance.GetGotchiSvgsById(GotchiId.Value);

        if (newGotchiSvgs == null)
        {
            GotchiDataManager.Instance.FetchRemoteGotchiSvgsById(GotchiId.Value);
        }
        else
        {
            if (GotchiSvgSet == null || GotchiSvgSet.id != newGotchiSvgs.id)
            {
                SetBodySpriteFromDataManager(newGotchiSvgs);
                GotchiSvgSet = newGotchiSvgs;
            }
        }
    }

    bool m_isGotSvg = false;

    void SetBodySpriteFromDataManager(GotchiSvgSet gotchiSvgSet)
    {
        if (gotchiSvgSet == null) return;

        var newMaterial = GotchiDataManager.Instance.Material_Unlit_VectorGradient;

        var playerGotchi = GetComponent<PlayerGotchi>();

        playerGotchi.BodyFaceFront.GetComponent<SpriteRenderer>().sprite = GetSpriteFromSvgString(gotchiSvgSet.Front);
        playerGotchi.BodyFaceFront.GetComponent<SpriteRenderer>().material = newMaterial;

        playerGotchi.BodyFaceBack.GetComponent<SpriteRenderer>().sprite = GetSpriteFromSvgString(gotchiSvgSet.Back);
        playerGotchi.BodyFaceBack.GetComponent<SpriteRenderer>().material = newMaterial;

        playerGotchi.BodyFaceLeft.GetComponent<SpriteRenderer>().sprite = GetSpriteFromSvgString(gotchiSvgSet.Left);
        playerGotchi.BodyFaceLeft.GetComponent<SpriteRenderer>().material = newMaterial;

        playerGotchi.BodyFaceRight.GetComponent<SpriteRenderer>().sprite = GetSpriteFromSvgString(gotchiSvgSet.Right);
        playerGotchi.BodyFaceRight.GetComponent<SpriteRenderer>().material = newMaterial;

        //if (!m_isGotSvg)
        //{
        //    m_isGotSvg = true;
        //    SaveSvgToFile(GotchiDataManager.Instance.stylingGame.CustomizeSVG(gotchiSvgSet.Left), "GotchiFront.svg");
        //}
    }

    private void SaveSvgToFile(string svgContent, string fileName)
    {
        string path = Path.Combine(Application.dataPath, "Builds", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, svgContent);
        Debug.Log($"SVG saved to {path}");
    }

    private Sprite GetSpriteFromSvgString(string svgString)
    {
        // Convert SVG string to a Sprite
        return CustomSvgLoader.CreateSvgSprite(GotchiDataManager.Instance.stylingGame.CustomizeSVG(svgString), new Vector2(0.5f, 0.15f));
    }
}

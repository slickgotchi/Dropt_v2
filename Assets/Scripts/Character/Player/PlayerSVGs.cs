using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Netcode;
using GotchiHub;

public class PlayerSVGs : NetworkBehaviour
{
    private int m_gotchiId = 0;

    private void Start()
    {
    }

    private void Update()
    {
    }

    // this function called by PlayerController when id changes
    public void Init(int gotchiId)
    {
        if (!IsClient) return;

        var newGotchiSvgs = GotchiDataManager.Instance.GetGotchiSvgsById(gotchiId);
        //Debug.Log("newGotchiSvgs: " + newGotchiSvgs + " for gotchi id: " + gotchiId);
        if (newGotchiSvgs != null)
        {
            SetBodySpritesFromSvgSet(newGotchiSvgs);
            return;
        }

        var offchainGotchiData = GotchiDataManager.Instance.GetOffchainGotchiDataById(gotchiId);
        if (offchainGotchiData != null)
        {
            SetBodySpritesFromOffchainGotchiData(offchainGotchiData);
            return;
        }

        Debug.LogWarning("No gotchis svgs or sprites for id: " + gotchiId);
    }

    void SetBodySpritesFromSvgSet(GotchiSvgSet gotchiSvgSet)
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
    }

    private Sprite GetSpriteFromSvgString(string svgString)
    {
        // Convert SVG string to a Sprite
        return CustomSvgLoader.CreateSvgSprite(GotchiDataManager.Instance.stylingGame.CustomizeSVG(svgString), new Vector2(0.5f, 0.15f));
    }

    void SetBodySpritesFromOffchainGotchiData(PortalDefender.AavegotchiKit.DefaultGotchiData offchainGotchiData)
    {
        var newMaterial = GotchiDataManager.Instance.Material_Sprite_Unlit_Default;
        var playerGotchi = GetComponent<PlayerGotchi>();

        playerGotchi.BodyFaceFront.GetComponent<SpriteRenderer>().sprite = offchainGotchiData.spriteFront;
        playerGotchi.BodyFaceFront.GetComponent<SpriteRenderer>().material = newMaterial;

        playerGotchi.BodyFaceBack.GetComponent<SpriteRenderer>().sprite = offchainGotchiData.spriteBack;
        playerGotchi.BodyFaceBack.GetComponent<SpriteRenderer>().material = newMaterial;

        playerGotchi.BodyFaceLeft.GetComponent<SpriteRenderer>().sprite = offchainGotchiData.spriteLeft;
        playerGotchi.BodyFaceLeft.GetComponent<SpriteRenderer>().material = newMaterial;

        playerGotchi.BodyFaceRight.GetComponent<SpriteRenderer>().sprite = offchainGotchiData.spriteRight;
        playerGotchi.BodyFaceRight.GetComponent<SpriteRenderer>().material = newMaterial;
    }
}

using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class Consume : PlayerAbility
{
    //[Header("Consume Parameters")]


    public override void OnNetworkSpawn()
    {
    }

    public override void OnStart()
    {
        // get player to do consume animation
        var hand = ActivationInput.abilityHand;
        if (hand == Hand.Left)
        {
            Debug.Log("Play anim left");
            Player.GetComponent<PlayerGotchi>().PlayAnimation("PlayerGotchiLeftHand_Consume");
        } else
        {
            Debug.Log("Play anim right");
            Player.GetComponent<PlayerGotchi>().PlayAnimation("PlayerGotchiRightHand_Consume");
        }
    }

    public override void OnUpdate()
    {

    }

    public override void OnFinish()
    {
        Player.GetComponent<PlayerGotchi>().ResetIdleAnimation();
    }
}

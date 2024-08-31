using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem.Wrappers;

public class FixedBarkWhileTriggered : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object has the "CameraFollower" tag
        if (collision.gameObject.CompareTag("CameraFollower"))
        {
            // If the tag matches, enable the waitForContinueButton property
            GetComponentInChildren<StandardBarkUI>().waitForContinueButton = true;
            Debug.Log(GetComponentInChildren<StandardBarkUI>().waitForContinueButton);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Check if the colliding object has the "CameraFollower" tag before hiding the UI
        if (collision.gameObject.CompareTag("CameraFollower"))
        {
            GetComponentInChildren<StandardBarkUI>().Hide();
        }
    }
}

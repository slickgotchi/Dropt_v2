using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Hole_RandomLevelGroups : Interactable
{
    public List<LevelGroup> LevelGroups = new List<LevelGroup>();

    private List<GameObject> m_levels = new List<GameObject>();

    public override void OnInteractHoldFinish()
    {
        TryGoToNextLevelServerRpc(localPlayerNetworkObjectId);
    }

    public override void OnTriggerEnter2DInteraction()
    {
        base.OnTriggerEnter2DInteraction();

        PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
    }

    public override void OnTriggerExit2DInteraction()
    {
        base.OnTriggerExit2DInteraction();

        PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
    }

    [Rpc(SendTo.Server)]
    void TryGoToNextLevelServerRpc(ulong testPlayerNetworkObjectId)
    {
        if (!IsValidInteraction(testPlayerNetworkObjectId)) return;

        // see if this hole has level groups
        if (LevelGroups.Count > 0)
        {
            m_levels.Clear();

            // create random level arrangement based on LevelGroup parameters
            foreach (var lg in LevelGroups)
            {
                // shuffle the list and set randomized length
                lg.Shuffle();

                // add the levels to our m_levels list
                foreach (var lvl in lg.levels)
                {
                    m_levels.Add(lvl.gameObject);
                }
            }

            LevelManager.Instance.SetLevelList_SERVER(m_levels);
        }

        // go to next level
        LevelManager.Instance.StartTransitionToNextLevel_SERVER();
    }
}

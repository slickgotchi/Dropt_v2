using GotchiHub;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteractions : NetworkBehaviour
{
    private NetworkVariable<int> m_interactingNetworkObjectId;

    private int m_interactablesLayer;

    private GameObject m_interactableObject;
    private Interactable m_interactable;

    private float m_fHoldTimer = 0;
    private float k_fHoldtime = 0.5f;
    private float m_nextLevelCooldownTimer = 0;
    private float k_nextLevelCooldown = 3;

    private void Awake()
    {
        m_interactablesLayer = 1 << LayerMask.NameToLayer("Interactable");
        m_interactingNetworkObjectId = new NetworkVariable<int>(-1);
    }

    void Update()
    {
        if (!IsLocalPlayer) return;

        // test for any interaction hits
        var interactionHit = Physics2D.OverlapCircle(
            transform.position + new Vector3(0, 0.6f, 0),
            0.5f,
            m_interactablesLayer);
        if (interactionHit == null || !interactionHit.HasComponent<Interactable>())
        {
            // finish interactable if its active
            if (m_interactable != null && m_interactable.status == Interactable.Status.Active)
            {
                m_interactable.status = Interactable.Status.Inactive;
                m_interactable.OnFinishInteraction();
                m_interactable.playerNetworkObjectId = 0;
                m_interactable = null;
            }

            return;
        }

        // got a interact so lets process it
        m_interactable = interactionHit.GetComponent<Interactable>();

        if (m_interactable.status == Interactable.Status.Inactive)
        {
            m_interactable.status = Interactable.Status.Active;
            m_interactable.playerNetworkObjectId = GetComponent<NetworkObject>().NetworkObjectId;
            m_interactable.OnStartInteraction();
        }

        m_interactable.OnUpdateInteraction();
    }


    //private void HandleLocalEscapePortalInteractions()
    //{
    //    if (m_interactableObject.HasComponent<EscapePortal>())
    //    {
    //        InteractableUICanvas.Instance.InteractTextbox.SetActive(true);

    //        if (Input.GetKey(KeyCode.F))
    //        {
    //            m_fHoldTimer += Time.deltaTime;
    //            if (m_fHoldTimer >= k_fHoldtime && m_nextLevelCooldownTimer <= 0)
    //            {
    //                TryGoToDegenapeVillageLevelServerRpc();
    //                m_nextLevelCooldownTimer = k_nextLevelCooldown;
    //            }
    //        }
    //    }
    //}

    //private void HandleLocalWeaponSwapInteractions()
    //{
    //    if (m_interactableObject.HasComponent<WeaponSwap>())
    //    {
    //        WeaponSwapCanvas.Instance.Container.SetActive(true);
    //        WeaponSwapCanvas.Instance.Init(m_interactableObject.GetComponent<WeaponSwap>().WearableNameEnum);

    //        var wearableNameEnum = m_interactableObject.GetComponent<WeaponSwap>().WearableNameEnum;
    //        if (Input.GetMouseButtonDown(0))
    //        {
    //            var ogEquipment = GetComponent<PlayerEquipment>().LeftHand.Value;
    //            GetComponent<PlayerEquipment>().SetEquipment(PlayerEquipment.Slot.LeftHand, wearableNameEnum);
    //            m_interactableObject.GetComponent<WeaponSwap>().Init(ogEquipment);
    //        }
    //        if (Input.GetMouseButtonDown(1))
    //        {
    //            var ogEquipment = GetComponent<PlayerEquipment>().RightHand.Value;
    //            GetComponent<PlayerEquipment>().SetEquipment(PlayerEquipment.Slot.RightHand, wearableNameEnum);
    //            m_interactableObject.GetComponent<WeaponSwap>().Init(ogEquipment);
    //        }
    //    }
    //}

    //[Rpc(SendTo.Server)]
    //void TryGoToDegenapeVillageLevelServerRpc()
    //{
    //    if (m_interactableObject == null) return;
    //    if (!m_interactableObject.HasComponent<EscapePortal>()) return;
    //    if (m_nextLevelCooldownTimer > 0) return;

    //    LevelManager.Instance.GoToDegenapeVillageLevel();
    //    m_nextLevelCooldownTimer = k_nextLevelCooldown;
    //}
}

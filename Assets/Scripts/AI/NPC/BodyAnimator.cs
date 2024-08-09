using System;
using UnityEngine;
using Facing = PlayerGotchi.Facing;

namespace AI.NPC
{
    public class BodyAnimator : MonoBehaviour
    {
        [Header("Body GameObject and Side Views")] [SerializeField]
        GameObject m_bodyParent;

        [SerializeField] public GameObject BodyFaceFront;
        [SerializeField] public GameObject BodyFaceBack;
        [SerializeField] public GameObject BodyFaceLeft;
        [SerializeField] public GameObject BodyFaceRight;

        [Header("Right Hand GameObject and Side Views")] [SerializeField]
        private GameObject m_rightHandParent;

        [SerializeField] private GameObject m_rightHandFaceFront;
        [SerializeField] private GameObject m_rightHandFaceBack;
        [SerializeField] private GameObject m_rightHandFaceLeft;
        [SerializeField] private GameObject m_rightHandFaceRight;

        [Header("Left Hand GameObject and Side Views")] [SerializeField]
        private GameObject m_leftHandParent;

        [SerializeField] private GameObject m_leftHandFaceFront;
        [SerializeField] private GameObject m_leftHandFaceBack;
        [SerializeField] private GameObject m_leftHandFaceLeft;
        [SerializeField] private GameObject m_leftHandFaceRight;

        [SerializeField] private ParticleSystem m_dustParticleSystem;
        [SerializeField] private Animator m_animator;

        private float m_leftHandHideTimer = 0;
        private float m_rightHandHideTimer = 0;

        private void Start()
        {
            m_animator.Play("PlayerGotchiBody_Reset");
            m_animator.Play("PlayerGotchiLeftHand_Reset");
            m_animator.Play("PlayerGotchiRightHand_Reset");
        }

        public void UpdateDusts(bool isMoving)
        {
            if (isMoving)
            {
                if (!m_dustParticleSystem.isPlaying) m_dustParticleSystem.Play();
            }
            else
            {
                if (m_dustParticleSystem.isPlaying) m_dustParticleSystem.Stop();
            }
        }

        public void SetActiveBodyPartsFromFacing(Facing facing)
        {
            if (!ValidateCanAnimate())
            {
                return;
            }

            if (facing == Facing.Front)
            {
                // body
                BodyFaceFront.SetActive(true);
                BodyFaceBack.SetActive(false);
                BodyFaceLeft.SetActive(false);
                BodyFaceRight.SetActive(false);

                // right hand
                if (m_rightHandHideTimer < 0) m_rightHandFaceFront.SetActive(true);
                m_rightHandFaceBack.SetActive(false);
                m_rightHandFaceLeft.SetActive(false);
                m_rightHandFaceRight.SetActive(false);

                // left hand
                if (m_leftHandHideTimer < 0) m_leftHandFaceFront.SetActive(true);
                m_leftHandFaceBack.SetActive(false);
                m_leftHandFaceLeft.SetActive(false);
                m_leftHandFaceRight.SetActive(false);
            }

            if (facing == Facing.Back)
            {
                // body
                BodyFaceFront.SetActive(false);
                BodyFaceBack.SetActive(true);
                BodyFaceLeft.SetActive(false);
                BodyFaceRight.SetActive(false);

                // right hand
                m_rightHandFaceFront.SetActive(false);
                if (m_rightHandHideTimer < 0) m_rightHandFaceBack.SetActive(true);
                m_rightHandFaceLeft.SetActive(false);
                m_rightHandFaceRight.SetActive(false);

                // left hand
                m_leftHandFaceFront.SetActive(false);
                if (m_leftHandHideTimer < 0) m_leftHandFaceBack.SetActive(true);
                m_leftHandFaceLeft.SetActive(false);
                m_leftHandFaceRight.SetActive(false);
            }

            if (facing == Facing.Left)
            {
                // body
                BodyFaceFront.SetActive(false);
                BodyFaceBack.SetActive(false);
                BodyFaceLeft.SetActive(true);
                BodyFaceRight.SetActive(false);

                // right hand
                m_rightHandFaceFront.SetActive(false);
                m_rightHandFaceBack.SetActive(false);
                if (m_rightHandHideTimer < 0) m_rightHandFaceLeft.SetActive(true);
                m_rightHandFaceRight.SetActive(false);

                // left hand
                m_leftHandFaceFront.SetActive(false);
                m_leftHandFaceBack.SetActive(false);
                if (m_leftHandHideTimer < 0) m_leftHandFaceLeft.SetActive(true);
                m_leftHandFaceRight.SetActive(false);
            }

            if (facing == Facing.Right)
            {
                // body
                BodyFaceFront.SetActive(false);
                BodyFaceBack.SetActive(false);
                BodyFaceLeft.SetActive(false);
                BodyFaceRight.SetActive(true);

                // right hand
                m_rightHandFaceFront.SetActive(false);
                m_rightHandFaceBack.SetActive(false);
                m_rightHandFaceLeft.SetActive(false);
                if (m_rightHandHideTimer < 0) m_rightHandFaceRight.SetActive(true);

                // left hand
                m_leftHandFaceFront.SetActive(false);
                m_leftHandFaceBack.SetActive(false);
                m_leftHandFaceLeft.SetActive(false);
                if (m_leftHandHideTimer < 0) m_leftHandFaceRight.SetActive(true);
            }
        }

        //test 
        private bool ValidateCanAnimate()
        {
            return m_leftHandFaceFront != null && m_leftHandParent != null && m_leftHandFaceBack != null &&
                   m_leftHandFaceLeft != null && m_leftHandFaceRight &&
                   m_rightHandParent != null && m_rightHandFaceRight != null && m_rightHandFaceFront != null &&
                   m_rightHandFaceLeft != null && m_rightHandFaceRight && m_bodyParent != null;
        }

        private void Update()
        {
            m_leftHandHideTimer -= Time.deltaTime;
            m_rightHandHideTimer -= Time.deltaTime;
        }
    }
}
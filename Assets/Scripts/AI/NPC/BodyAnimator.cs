using UnityEngine;
using Facing = PlayerGotchi.Facing;

namespace AI.NPC
{
    public class BodyAnimator : MonoBehaviour
    {
        private const string kBodyIdleKey = "PlayerGotchiBody_Idle";

        [Header("Body GameObject and Side Views")] [SerializeField]
        GameObject m_bodyParent;

        [SerializeField] private GameObject m_bodyFaceFront;
        [SerializeField] private GameObject m_bodyFaceBack;
        [SerializeField] private GameObject m_bodyFaceLeft;
        [SerializeField] private GameObject m_bodyFaceRight;

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
            m_animator.Play(kBodyIdleKey);
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

        public void UpdateFacing(Facing facing)
        {
            if (!ValidateCanAnimate())
            {
                return;
            }

            ResetBodyParts();
            ResetHandParts();

            switch (facing)
            {
                case Facing.Front:
                    m_bodyFaceFront.SetActive(true);
                    SetHandParts(m_rightHandFaceFront, m_leftHandFaceFront);
                    break;
                case Facing.Back:
                    m_bodyFaceBack.SetActive(true);
                    SetHandParts(m_rightHandFaceBack, m_leftHandFaceBack);
                    break;
                case Facing.Left:
                    m_bodyFaceLeft.SetActive(true);
                    SetHandParts(m_rightHandFaceLeft, m_leftHandFaceLeft);
                    break;
                case Facing.Right:
                    m_bodyFaceRight.SetActive(true);
                    SetHandParts(m_rightHandFaceRight, m_leftHandFaceRight);
                    break;
            }
        }

        private void ResetBodyParts()
        {
            m_bodyFaceFront.SetActive(false);
            m_bodyFaceBack.SetActive(false);
            m_bodyFaceLeft.SetActive(false);
            m_bodyFaceRight.SetActive(false);
        }

        private void ResetHandParts()
        {
            m_rightHandFaceFront.SetActive(false);
            m_rightHandFaceBack.SetActive(false);
            m_rightHandFaceLeft.SetActive(false);
            m_rightHandFaceRight.SetActive(false);

            m_leftHandFaceFront.SetActive(false);
            m_leftHandFaceBack.SetActive(false);
            m_leftHandFaceLeft.SetActive(false);
            m_leftHandFaceRight.SetActive(false);
        }

        private void SetHandParts(GameObject rightHand, GameObject leftHand)
        {
            if (m_rightHandHideTimer < 0)
            {
                rightHand.SetActive(true);
            }

            if (m_leftHandHideTimer < 0)
            {
                leftHand.SetActive(true);
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
using CarlosLab.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CarlosLab.UtilityIntelligence
{
    public class DecisionInfo : MonoBehaviour
    {
        [SerializeField]
        private Image outlineImage;
        
        [SerializeField]
        private TextMeshProUGUI decisionNameText;

        private TargetFollower targetFollower;
        
        private void Awake()
        {
            targetFollower = GetComponent<TargetFollower>();
        }

        public void Init(Transform target)
        {
            targetFollower.FollowTarget(target);
            RotateTowardsCamera();
        }
        
        public void UpdateDecision(Decision decision)
        {
            decisionNameText.text = decision.Name;
        }
        
        private void RotateTowardsCamera()
        {
            var cameraForward = Camera.main.transform.forward;
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }
    }
}

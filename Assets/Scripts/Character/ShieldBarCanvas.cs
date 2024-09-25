using UnityEngine;
using UnityEngine.UI;

public class ShieldBarCanvas : MonoBehaviour
{
    [SerializeField] private Slider m_shieldSlider;
    [SerializeField] private GameObject m_shieldBar;

    public void SetProgress(float progress)
    {
        m_shieldSlider.value = progress;
    }

    public void SetVisible(bool visible)
    {
        m_shieldBar.SetActive(visible);
    }
}

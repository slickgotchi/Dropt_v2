using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PetMeterView : MonoBehaviour
{
    [SerializeField] private Slider m_progressSlider;
    [SerializeField] private Image m_petImage;

    private Sequence m_sequence;
    private RectTransform m_progressRectTransform;

    public void Activate(Sprite pet)
    {
        m_progressRectTransform = m_progressSlider.GetComponent<RectTransform>();
        m_sequence = DOTween.Sequence();
        m_sequence.Append(m_progressRectTransform.DOScale(1.1f, 0.25f).SetEase(Ease.OutQuad))
                .Append(m_progressRectTransform.DOScale(1f, 0.25f).SetEase(Ease.InQuad))
                .SetLoops(-1, LoopType.Yoyo);
        m_petImage.sprite = pet;
        m_petImage.SetNativeSize();
        SetProgress(0);
        gameObject.SetActive(true);
    }

    public void SetProgress(float progress)
    {
        m_progressSlider.value = progress;
        if (progress >= 1.0f && !DOTween.IsTweening(m_progressRectTransform))
        {
            m_sequence.Play();
        }
        else
        {
            m_sequence.Pause();
            m_progressRectTransform.localScale = Vector3.one;
        }
    }

    private void AnimateSlider()
    {
        m_sequence.Play();
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Reset()
    {
        m_sequence.Kill();
    }
}
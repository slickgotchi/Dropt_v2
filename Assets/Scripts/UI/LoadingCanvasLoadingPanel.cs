using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCanvasLoadingPanel : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI m_droppingText;
    [SerializeField] private Image m_droppingImage;

    [SerializeField] private float m_tickInterval_s = 0.3f;
    private float m_timer_s = 0;
    private int m_animFrameCounter = 0;

    private void Update()
    {
        m_timer_s += Time.deltaTime;

        while (m_timer_s > 0)
        {
            m_timer_s -= m_tickInterval_s;

            NextAnimFrame();
        }
    }

    void NextAnimFrame()
    {
        var imageLocalPos = m_droppingImage.transform.localPosition;
        if (m_animFrameCounter == 0)
        {
            imageLocalPos.y = 0;
            m_droppingImage.transform.localPosition = imageLocalPos;
            m_droppingText.text = "dropping";
        }
        else if (m_animFrameCounter == 1)
        {
            imageLocalPos.y = 1;
            m_droppingImage.transform.localPosition = imageLocalPos;
            m_droppingText.text = "dropping.";
        }
        else if (m_animFrameCounter == 2)
        {
            imageLocalPos.y = 0;
            m_droppingImage.transform.localPosition = imageLocalPos;
            m_droppingText.text = "dropping..";
        }
        else
        {
            imageLocalPos.y = 1;
            m_droppingImage.transform.localPosition = imageLocalPos;
            m_droppingText.text = "dropping...";
            m_animFrameCounter = 0;
            return;
        }

        m_animFrameCounter++;

    }
}

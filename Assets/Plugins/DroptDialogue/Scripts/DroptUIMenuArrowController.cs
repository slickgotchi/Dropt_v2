using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem.Wrappers;

public class DroptUIMenuArrowController : MonoBehaviour
{
    public List<Button> responseButtons;

    public KeyCode upKeyCode = KeyCode.W;
    public KeyCode downKeyCode = KeyCode.S;
    public KeyCode selectKeyCode = KeyCode.F;

    private int m_currentButton = 0;

    void Update()
    {
        if (Input.GetKeyDown(upKeyCode))
        {
            ScrollUp();
        }

        if (Input.GetKeyDown(downKeyCode))
        {
            ScrollDown();
        }

        if (Input.GetKeyDown(selectKeyCode))
        {
            // select
            var respBtn = responseButtons[m_currentButton];
            var standardUIResponseButton = respBtn.GetComponent<StandardUIResponseButton>();
            standardUIResponseButton.OnClick();
        }

        SetVisibleArrow(m_currentButton);
    }

    void ScrollUp()
    {
        m_currentButton -= 1;
        var activeResponseCount = GetActiveResponseButtonCount();
        if (m_currentButton < 0) m_currentButton = 0;
    }

    void ScrollDown()
    {
        m_currentButton += 1;
        var activeResponseCount = GetActiveResponseButtonCount();
        if (m_currentButton > (activeResponseCount - 1)) m_currentButton = activeResponseCount - 1;
    }

    int GetActiveResponseButtonCount()
    {
        var activeResponseCount = 0;
        foreach (var respBtn in responseButtons)
        {
            if (respBtn.gameObject.activeSelf) activeResponseCount++;
        }
        return activeResponseCount;
    }

    void SetVisibleArrow(int index)
    {
        if (index < 0 || index > (responseButtons.Count - 1)) return;

        for (int i = 0; i < responseButtons.Count; i++)
        {
            var respBtn = responseButtons[i];
            var arrowImage = respBtn.transform.Find("ArrowImage");
            if (arrowImage == null) continue;

            arrowImage.gameObject.SetActive(i == index);
        }
    }
}

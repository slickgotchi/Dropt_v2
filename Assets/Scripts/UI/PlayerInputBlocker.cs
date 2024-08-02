using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInputBlocker : MonoBehaviour
{
    public static PlayerInputBlocker Instance { get; private set; }

    public List<GameObject> clickInputBlockers = new List<GameObject>();
    public List<GameObject> moveInputBlockers = new List<GameObject>();


    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        //if (IsCursorWithinClickInputBlocker())
        //{
        //    Debug.Log("Cursor is within a click input blocker");
        //}

        //if (IsMoveInputBlockerActive())
        //{
        //    Debug.Log("A move input blocker is active");
        //}
    }

    public bool IsCursorWithinClickInputBlocker()
    {
        foreach (GameObject blocker in clickInputBlockers)
        {
            RectTransform blockerRectTransform = blocker.GetComponent<RectTransform>();
            if (blockerRectTransform.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(blockerRectTransform, Input.mousePosition, null))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsMoveInputBlockerActive()
    {
        foreach (GameObject blocker in moveInputBlockers)
        {
            if (blocker.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }
}

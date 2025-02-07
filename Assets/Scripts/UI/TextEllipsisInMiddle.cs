using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
[ExecuteAlways]  // Makes the script run in edit mode
public class TextEllipsisInMiddle : MonoBehaviour
{
    /*
    public int startCharacters = 6;  // Number of characters to show at the start
    public int endCharacters = 4;    // Number of characters to show at the end
    [TextArea]
    public string fullText;          // The full text to be shortened

    private TextMeshProUGUI tmpText;

    private void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        UpdateTextWithEllipsis();
    }

    private void OnEnable()
    {
        UpdateTextWithEllipsis();
    }

    private void OnValidate()
    {
        if (tmpText == null)
            tmpText = GetComponent<TextMeshProUGUI>();

        UpdateTextWithEllipsis();
    }

    public void UpdateTextWithEllipsis()
    {
        if (tmpText == null)
            tmpText = GetComponent<TextMeshProUGUI>();

        fullText = tmpText?.text ?? string.Empty; // Avoid null references

        if (fullText.Length > startCharacters + endCharacters)
        {
            string start = fullText.Substring(0, startCharacters);
            string end = fullText.Substring(fullText.Length - endCharacters);
            tmpText.text = $"{start}...{end}";
        }
        else
        {
            tmpText.text = fullText;
        }
    }
    */
}

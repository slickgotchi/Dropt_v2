using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
[ExecuteAlways]  // Makes the script run in edit mode
public class TextEllipsisInMiddle : MonoBehaviour
{
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
        fullText = GetComponent<TMPro.TextMeshProUGUI>().text;
        if (fullText.Length > startCharacters + endCharacters)
        {
            // Show the first `startCharacters`, then "...", then the last `endCharacters`
            string start = fullText.Substring(0, startCharacters);
            string end = fullText.Substring(fullText.Length - endCharacters);
            tmpText.text = $"{start}...{end}";
        }
        else
        {
            // If the text is short enough, display the entire text
            tmpText.text = fullText;
        }
    }
}

using TMPro;
using UnityEngine;

public class DebugCanvas : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI pingText;
    public TextMeshProUGUI playerCountText;

    private void Update()
    {
        // Update FPS
        fpsText.text = "FPS: " + Mathf.Ceil(1/Time.deltaTime).ToString();

        //pingText.text = "Ping: " + NetworkStats.Instance.Ping.ToString();

        //playerCountText.text = "Players: " + NetworkStats.Instance.ConnectedPlayers.ToString();
    }
}

using UnityEngine;

public class ClearPlayerPrefs : MonoBehaviour
{
    void Awake()
    {
        // Delete all PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("All PlayerPrefs have been deleted.");

        // Optionally, set the fullscreen mode to windowed and save this preference
        Screen.fullScreen = false;
    }
}

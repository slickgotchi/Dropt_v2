using UnityEngine;

public class FindContentSizeFitterWithMax : MonoBehaviour
{
    void Start()
    {
        var objectsWithScript = FindObjectsOfType<UnityEngine.UI.ContentSizeFitterWithMax>();
        foreach (var obj in objectsWithScript)
        {
            Debug.Log($"Found ContentSizeFitterWithMax on GameObject: {obj.gameObject.name}", obj.gameObject);
        }
    }
}

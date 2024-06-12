using UnityEngine;

public static class ComponentExtensions
{
    // Extension method to check if a GameObject has a specific component
    public static bool HasComponent<T>(this GameObject obj) where T : Component
    {
        return obj.GetComponent<T>() != null;
    }

    // Extension method to check if a Component's GameObject has a specific component
    public static bool HasComponent<T>(this Component component) where T : Component
    {
        return component.GetComponent<T>() != null;
    }
}

using System.Reflection;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class ButtonExtensions
{
    private static readonly MethodInfo IsPressedMethod =
        typeof(Selectable).GetMethod(
            "IsPressed",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

    private static readonly MethodInfo IsHighlightedMethod =
        typeof(Selectable).GetMethod(
            "IsHighlighted",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

    public static bool IsPressedDirect(this Button button)
    {
        return (bool)IsPressedMethod.Invoke(button, null);
    }

    public static bool IsHighlightedDirect(this Button button)
    {
        return (bool)IsHighlightedMethod.Invoke(button, null);
    }
    
    public static bool IsSelectedDirect(this Button button)
    {
        return EventSystem.current != null &&
               EventSystem.current.currentSelectedGameObject == button.gameObject;
    }
}

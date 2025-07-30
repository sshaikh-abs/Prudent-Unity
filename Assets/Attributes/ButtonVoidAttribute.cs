using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Place this somewhere in your project (do NOT place in an "Editor" folder)

public class ButtonInvoke : PropertyAttribute
{
    /// <summary>
    /// If you choose to display in editor or both, make sure that your script has a flag to run in editor.
    /// </summary>
    public enum DisplayIn
    {
        PlayMode,
        EditMode,
        PlayAndEditModes
    }

    public readonly string customLabel;
    public readonly string methodName;
    public readonly object methodParameter;
    public readonly DisplayIn displayIn;

    /// <summary>
    /// Add this attribute to any dummy field in order to show a button in inspector.
    /// </summary>
    /// <param name="methodName">Name of the method to call. I recommend using nameof() function to prevent typos.</param>
    /// <param name="methodParameter">Optional parameter to pass into the method.</param>
    /// <param name="displayIn">Should the button show in play mode, edit mode, or both.</param>
    /// <param name="customLabel">Optional custom label.</param>
    public ButtonInvoke(string methodName, object methodParameter = null, DisplayIn displayIn = DisplayIn.PlayMode, string customLabel = "")
    {
        this.methodName = methodName;
        this.methodParameter = methodParameter;
        this.displayIn = displayIn;
        this.customLabel = customLabel;
    }
}

#if UNITY_EDITOR
// Place this in a folder called "Editor" somewhere in your project.

[CustomPropertyDrawer(typeof(ButtonInvoke))]
public class ButtonInvokeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ButtonInvoke settings = (ButtonInvoke)attribute;
        return DisplayButton(ref settings) ? EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing : 0;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ButtonInvoke settings = (ButtonInvoke)attribute;

        if (!DisplayButton(ref settings)) return;

        string buttonLabel = (!string.IsNullOrEmpty(settings.customLabel)) ? settings.customLabel : label.text;

        if (property.serializedObject.targetObject is MonoBehaviour mb)
        {
            if (GUI.Button(position, buttonLabel))
            {
                mb.SendMessage(settings.methodName, settings.methodParameter);
            }
        }
    }

    private bool DisplayButton(ref ButtonInvoke settings)
    {
        return (settings.displayIn == ButtonInvoke.DisplayIn.PlayAndEditModes) ||
               (settings.displayIn == ButtonInvoke.DisplayIn.EditMode && !Application.isPlaying) ||
               (settings.displayIn == ButtonInvoke.DisplayIn.PlayMode && Application.isPlaying);
    }
}
#endif
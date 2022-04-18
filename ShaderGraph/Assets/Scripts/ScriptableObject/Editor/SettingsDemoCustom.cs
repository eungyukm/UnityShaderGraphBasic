using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SettingsDemo))]
public class SettingsDemoCustom : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var settings = serializedObject.FindProperty("settingType");

        EditorGUILayout.PropertyField(settings);
    }
}

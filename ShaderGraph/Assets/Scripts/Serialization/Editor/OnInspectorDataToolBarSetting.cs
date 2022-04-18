using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OnInspectorDataToolBar))]
public class OnInspectorDataToolBarSetting : Editor
{
    public override void OnInspectorGUI()
    {
        var toolModeProperty = serializedObject.FindProperty("toolMode");
        toolModeProperty.enumValueIndex = GUILayout.Toolbar(toolModeProperty.enumValueIndex, toolModeProperty.enumDisplayNames);
        
        switch (toolModeProperty.enumValueIndex)
        {
            case 0:
                var mainTexture = serializedObject.FindProperty("mainTexture");
                EditorGUILayout.PropertyField(mainTexture, new GUIContent("Main Texture"));
                break;
            
            case 1:
                EditorGUILayout.HelpBox("THIS IS DEBUG MODE!!", MessageType.Info); 
                break;
            
            case 2:
                var toolValue = serializedObject.FindProperty("toolValue");
                EditorGUILayout.PropertyField(toolValue, true);
                break;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}